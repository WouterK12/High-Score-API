using HighScoreAPI.Attributes;
using HighScoreAPI.Middleware;
using HighScoreAPI.Middleware.Workers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Configuration;
using Moq;

namespace HighScoreAPI.Test.Middleware;

[TestClass]
public class ApiKeyMiddlewareTest
{
    private Mock<IRequestMock> _requestMock = null!;
    private Mock<IRequestWriter> _requestWriterMock = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        _requestMock = new Mock<IRequestMock>(MockBehavior.Strict);
        _requestMock.Setup(r => r.Next(It.IsAny<HttpContext>()))
                    .Returns(Task.CompletedTask);
        _requestWriterMock = new Mock<IRequestWriter>(MockBehavior.Strict);
    }

    [TestMethod]
    public async Task InvokeAsync_NoXAPIKeyHeader_ApiKeyMiddleware_CallsRequestWriter()
    {
        // Arrange
        _requestWriterMock.Setup(rw => rw.WriteBadRequestAsync(It.IsAny<HttpContext>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        var context = new DefaultHttpContext();
        var configuration = new ConfigurationBuilder().Build();

        var sut = new ApiKeyMiddleware(_requestMock.Object.Next, _requestWriterMock.Object, configuration);

        // Act
        await sut.InvokeAsync(context);

        // Assert
        _requestWriterMock.Verify(rw => rw.WriteBadRequestAsync(context, "X-API-Key is required"), Times.Once);
        _requestMock.Verify(r => r.Next(It.IsAny<HttpContext>()), Times.Never);
    }

    [TestMethod]
    public async Task InvokeAsync_AdminKeyAttribute_CorrectAdminXAPIKeyHeader_ApiKeyMiddleware_CallsNext()
    {
        // Arrange
        var context = new DefaultHttpContext();

        var adminKeyAttribute = new RequiresAdminKeyAttribute();
        var endpointMetadataCollection = new EndpointMetadataCollection(adminKeyAttribute);
        var endpoint = new Endpoint(null, endpointMetadataCollection, null);
        var endpointFeatureMock = new Mock<IEndpointFeature>(MockBehavior.Strict);
        endpointFeatureMock.SetupProperty(ef => ef.Endpoint, endpoint);
        context.Features.Set(endpointFeatureMock.Object);

        string correctKey = "correct key";
        context.Request.Headers.Add(HeaderNames.XAPIKey, correctKey);
        var configuration = BuildXAPIKeyConfiguration(correctKey, correctKey);

        var sut = new ApiKeyMiddleware(_requestMock.Object.Next, _requestWriterMock.Object, configuration);

        // Act
        await sut.InvokeAsync(context);

        // Assert
        _requestMock.Verify(n => n.Next(context), Times.Once);
    }

    [TestMethod]
    public async Task InvokeAsync_AdminKeyAttribute_InvalidXAPIKeyHeader_ApiKeyMiddleware_CallsRequestWriter()
    {
        // Arrange
        _requestWriterMock.Setup(rw => rw.WriteUnauthorizedAsync(It.IsAny<HttpContext>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        var context = new DefaultHttpContext();

        var adminKeyAttribute = new RequiresAdminKeyAttribute();
        var endpointMetadataCollection = new EndpointMetadataCollection(adminKeyAttribute);
        var endpoint = new Endpoint(null, endpointMetadataCollection, null);
        var endpointFeatureMock = new Mock<IEndpointFeature>(MockBehavior.Strict);
        endpointFeatureMock.SetupProperty(ef => ef.Endpoint, endpoint);
        context.Features.Set(endpointFeatureMock.Object);

        string correctKey = "correct key";
        context.Request.Headers.Add(HeaderNames.XAPIKey, "invalid key");
        IConfigurationRoot configuration = BuildXAPIKeyConfiguration(correctKey, correctKey);

        var sut = new ApiKeyMiddleware(_requestMock.Object.Next, _requestWriterMock.Object, configuration);

        // Act
        await sut.InvokeAsync(context);

        // Assert
        _requestWriterMock.Verify(rw => rw.WriteUnauthorizedAsync(context, It.IsAny<string>()), Times.Once);
        _requestMock.Verify(r => r.Next(It.IsAny<HttpContext>()), Times.Never);
    }

    [TestMethod]
    public async Task InvokeAsync_InvalidXAPIKeyHeader_ApiKeyMiddleware_CallsRequestWriter()
    {
        // Arrange
        _requestWriterMock.Setup(rw => rw.WriteUnauthorizedAsync(It.IsAny<HttpContext>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        var context = new DefaultHttpContext();
        string correctKey = "correct key";
        context.Request.Headers.Add(HeaderNames.XAPIKey, "invalid key");
        IConfigurationRoot configuration = BuildXAPIKeyConfiguration(correctKey, correctKey);

        var sut = new ApiKeyMiddleware(_requestMock.Object.Next, _requestWriterMock.Object, configuration);

        // Act
        await sut.InvokeAsync(context);

        // Assert
        _requestWriterMock.Verify(rw => rw.WriteUnauthorizedAsync(context, It.IsAny<string>()), Times.Once);
        _requestMock.Verify(r => r.Next(It.IsAny<HttpContext>()), Times.Never);
    }

    [TestMethod]
    public async Task InvokeAsync_CorrectXAPIKeyHeader_ApiKeyMiddleware_CallsNext()
    {
        // Arrange
        var context = new DefaultHttpContext();
        string correctKey = "correct key";
        context.Request.Headers.Add(HeaderNames.XAPIKey, correctKey);
        var configuration = BuildXAPIKeyConfiguration(correctKey, "other key");

        var sut = new ApiKeyMiddleware(_requestMock.Object.Next, _requestWriterMock.Object, configuration);

        // Act
        await sut.InvokeAsync(context);

        // Assert
        _requestMock.Verify(n => n.Next(context), Times.Once);
    }

    private static IConfigurationRoot BuildXAPIKeyConfiguration(string clientKey, string adminKey)
    {
        var myConfiguration = new Dictionary<string, string>()
        {
            { HeaderNames.XAPIKey + ":Client", clientKey },
            { HeaderNames.XAPIKey + ":Admin", adminKey }
        };
        return new ConfigurationBuilder()
            .AddInMemoryCollection(myConfiguration)
            .Build();
    }
}
