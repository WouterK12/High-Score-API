using HighScoreAPI.Attributes;
using HighScoreAPI.DAL.DataMappers;
using HighScoreAPI.Middleware;
using HighScoreAPI.Middleware.Workers;
using HighScoreAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Moq;
using System.Text;

namespace HighScoreAPI.Test.Middleware;

[TestClass]
public class EncryptionMiddlewareTest
{
    private Mock<IRequestMock> _requestMock = null!;
    private Mock<IRequestWriter> _requestWriterMock = null!;
    private Mock<IProjectDataMapper> _projectDataMapperMock = null!;

    private const string KeyBase64 = "HbrBX/TckMpgKFKZbLQsJkkfE3bUKJ1JuD2CPZbxt48=";
    private const string VectorBase64 = "YYmZfhLrt858GSKU/U6Siw==";
    private const string CipherText = "lto17w6Y3bKc0xJbKEg9iiWlHD2hNmPISili+kWhMKBIB/PE0ceU+Qk9yUjzJC1b";

    [TestInitialize]
    public void TestInitialize()
    {
        _requestMock = new Mock<IRequestMock>(MockBehavior.Strict);
        _requestMock.Setup(r => r.Next(It.IsAny<HttpContext>()))
                    .Returns(Task.CompletedTask);
        _requestWriterMock = new Mock<IRequestWriter>(MockBehavior.Strict);
        _projectDataMapperMock = new Mock<IProjectDataMapper>(MockBehavior.Strict);
    }

    [TestMethod]
    public async Task InvokeAsync_NoEncryptedBodyAttribute_EncryptionMiddleware_DoesNotDecryptBody()
    {
        // Arrange
        var context = new DefaultHttpContext();

        using var stream = new MemoryStream();
        byte[] body = Encoding.UTF8.GetBytes("Body of the request");
        stream.Write(body, 0, body.Length);
        stream.Seek(0, SeekOrigin.Begin);
        context.Request.Body = stream;

        var sut = new EncryptionMiddleware(_requestMock.Object.Next, _requestWriterMock.Object, _projectDataMapperMock.Object);

        // Act
        await sut.InvokeAsync(context);

        // Assert
        using var reader = new StreamReader(context.Request.Body);
        var bodyString = reader.ReadToEnd();
        Assert.AreEqual("Body of the request", bodyString);
        _requestMock.Verify(n => n.Next(context), Times.Once);
    }

    [TestMethod]
    public async Task InvokeAsync_EncryptedBodyAttribute_NoAESVectorHeader_EncryptionMiddleware_CallsRequestWriter()
    {
        // Arrange
        _requestWriterMock.Setup(rw => rw.WriteBadRequestAsync(It.IsAny<HttpContext>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var context = new DefaultHttpContext();
        var bodyType = typeof(HighScore);
        AddEncryptedBodyAttributeEndpointFeature(context, bodyType);

        var sut = new EncryptionMiddleware(_requestMock.Object.Next, _requestWriterMock.Object, _projectDataMapperMock.Object);

        // Act
        await sut.InvokeAsync(context);

        // Assert
        _requestWriterMock.Verify(rw => rw.WriteBadRequestAsync(It.IsAny<HttpContext>(), "AES-Vector is required"), Times.Once);
        _requestMock.Verify(n => n.Next(context), Times.Never);
    }

    [TestMethod]
    public async Task InvokeAsync_EncryptedBodyAttribute_AESVectorHeader_EncryptionMiddleware_CallsNextWithDecryptedBody()
    {
        // Arrange
        _projectDataMapperMock.Setup(d => d.GetProjectByNameAsync(It.IsAny<string>()))
            .ReturnsAsync(new Project() { AesKeyBase64 = KeyBase64 });

        var context = new DefaultHttpContext();
        var bodyType = typeof(HighScore);
        AddEncryptedBodyAttributeEndpointFeature(context, bodyType);

        using var stream = new MemoryStream();
        byte[] body = Encoding.UTF8.GetBytes(CipherText);
        stream.Write(body, 0, body.Length);
        stream.Seek(0, SeekOrigin.Begin);
        context.Request.Body = stream;

        context.Request.Headers.Add(HeaderNames.AESVector, VectorBase64);

        var sut = new EncryptionMiddleware(_requestMock.Object.Next, _requestWriterMock.Object, _projectDataMapperMock.Object);

        // Act
        await sut.InvokeAsync(context);

        // Assert
        using var ms = (MemoryStream)context.Request.Body;
        var bodyString = Encoding.UTF8.GetString(ms.ToArray());
        Assert.AreEqual("{ \"username\": \"user\", \"score\": 10 }", bodyString);
        _requestMock.Verify(n => n.Next(context), Times.Once);
    }

    [TestMethod]
    public async Task InvokeAsync_EncryptedBodyAttribute_AESVectorHeader_ProjectDataMapperReturnsNull_EncryptionMiddleware_CallsRequestWriter()
    {
        // Arrange
        _projectDataMapperMock.Setup(d => d.GetProjectByNameAsync(It.IsAny<string>()))
            .ReturnsAsync(() => null!);
        _requestWriterMock.Setup(rw => rw.WriteBadRequestAsync(It.IsAny<HttpContext>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var context = new DefaultHttpContext();
        var bodyType = typeof(HighScore);
        AddEncryptedBodyAttributeEndpointFeature(context, bodyType);

        using var stream = new MemoryStream();
        byte[] body = Encoding.UTF8.GetBytes(CipherText);
        stream.Write(body, 0, body.Length);
        stream.Seek(0, SeekOrigin.Begin);
        context.Request.Body = stream;

        context.Request.Headers.Add(HeaderNames.AESVector, VectorBase64);

        var sut = new EncryptionMiddleware(_requestMock.Object.Next, _requestWriterMock.Object, _projectDataMapperMock.Object);

        // Act
        await sut.InvokeAsync(context);

        // Assert
        _requestWriterMock.Verify(rw => rw.WriteBadRequestAsync(
                It.IsAny<HttpContext>(),
                "Project with name \"Smuggling-Pirates\" could not be found."),
            Times.Once);
        _requestMock.Verify(n => n.Next(context), Times.Never);
    }

    [TestMethod]
    public async Task InvokeAsync_EncryptedBodyAttribute_AESVectorHeader_IncorrectBodyType_EncryptionMiddleware_CallsRequestWriter()
    {
        // Arrange
        _requestWriterMock.Setup(rw => rw.WriteBadRequestAsync(It.IsAny<HttpContext>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        _projectDataMapperMock.Setup(d => d.GetProjectByNameAsync(It.IsAny<string>()))
            .ReturnsAsync(new Project() { AesKeyBase64 = KeyBase64 });

        var context = new DefaultHttpContext();
        var bodyType = typeof(string);
        AddEncryptedBodyAttributeEndpointFeature(context, bodyType);

        using var stream = new MemoryStream();
        byte[] body = Encoding.UTF8.GetBytes(CipherText);
        stream.Write(body, 0, body.Length);
        stream.Seek(0, SeekOrigin.Begin);
        context.Request.Body = stream;

        context.Request.Headers.Add(HeaderNames.AESVector, VectorBase64);

        var sut = new EncryptionMiddleware(_requestMock.Object.Next, _requestWriterMock.Object, _projectDataMapperMock.Object);

        // Act
        await sut.InvokeAsync(context);

        // Assert
        _requestWriterMock.Verify(rw => rw.WriteBadRequestAsync(It.IsAny<HttpContext>(), "The requested body could not be deserialized to the desired type."), Times.Once);
        _requestMock.Verify(n => n.Next(context), Times.Never);
    }

    [TestMethod]
    public async Task InvokeAsync_EncryptedBodyAttribute_AESVectorHeader_NoRouteValues_EncryptionMiddleware_CallsRequestWriter()
    {
        // Arrange
        _requestWriterMock.Setup(rw => rw.WriteBadRequestAsync(It.IsAny<HttpContext>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        _projectDataMapperMock.Setup(d => d.GetProjectByNameAsync(It.IsAny<string>()))
            .ReturnsAsync(new Project() { AesKeyBase64 = KeyBase64 });

        var context = new DefaultHttpContext();
        var bodyType = typeof(HighScore);
        bool withRouteValues = false;
        AddEncryptedBodyAttributeEndpointFeature(context, bodyType, withRouteValues);

        using var stream = new MemoryStream();
        byte[] body = Encoding.UTF8.GetBytes(CipherText);
        stream.Write(body, 0, body.Length);
        stream.Seek(0, SeekOrigin.Begin);
        context.Request.Body = stream;

        context.Request.Headers.Add(HeaderNames.AESVector, VectorBase64);

        var sut = new EncryptionMiddleware(_requestMock.Object.Next, _requestWriterMock.Object, _projectDataMapperMock.Object);

        // Act
        await sut.InvokeAsync(context);

        // Assert
        _requestWriterMock.Verify(rw => rw.WriteBadRequestAsync(It.IsAny<HttpContext>(), "Route Value \"projectName\" must be set."), Times.Once);
        _requestMock.Verify(n => n.Next(context), Times.Never);
    }

    private static void AddEncryptedBodyAttributeEndpointFeature(DefaultHttpContext context, Type bodyType, bool withRouteValues = true)
    {
        var encryptedBodyAttribute = new RequiresEncryptedBodyAttribute();
        var endpointMetadataCollection = new EndpointMetadataCollection(encryptedBodyAttribute, BuildActionDescriptorMock(bodyType, withRouteValues));
        var endpoint = new Endpoint(null, endpointMetadataCollection, null);
        var endpointFeatureMock = new Mock<IEndpointFeature>(MockBehavior.Strict);
        endpointFeatureMock.SetupProperty(ef => ef.Endpoint, endpoint);
        context.Features.Set(endpointFeatureMock.Object);
    }

    private static ActionDescriptor BuildActionDescriptorMock(Type bodyType, bool withRouteValues)
    {
        var descriptor = new ActionDescriptor();

        if (withRouteValues)
            descriptor.RouteValues.Add("projectName", "Smuggling-Pirates");

        var parameter = new ParameterDescriptor { Name = "highScoreToAdd", ParameterType = bodyType };
        descriptor.Parameters = new List<ParameterDescriptor> { parameter };

        return descriptor;
    }
}
