using HighScoreAPI.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Moq;

namespace HighScoreAPI.Test.Middleware;

[TestClass]
public class ApiKeyMiddlewareTest
{
    private ApiKeyMiddleware _sut = null!;
    private Mock<IRequestMock> _requestMock = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        _requestMock = new Mock<IRequestMock>(MockBehavior.Strict);
        _requestMock.Setup(r => r.Next(It.IsAny<HttpContext>()))
                   .Returns(Task.CompletedTask);
        _sut = new ApiKeyMiddleware(_requestMock.Object.Next);
    }

    [TestMethod]
    public async Task InvokeAsync_ApiKeyMiddleware_NoXAPIKeyHeader_SetsResponse400()
    {
        // Arrange
        var context = new DefaultHttpContext();

        // Act
        await _sut.InvokeAsync(context);

        // Assert
        Assert.AreEqual(StatusCodes.Status400BadRequest, context.Response.StatusCode);
        Assert.AreEqual("text/plain", context.Response.ContentType);
        _requestMock.Verify(r => r.Next(It.IsAny<HttpContext>()), Times.Never);
    }

    [TestMethod]
    public async Task InvokeAsync_ApiKeyMiddleware_InvalidXAPIKeyHeader_SetsResponse401()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var myConfiguration = new Dictionary<string, string>() { { HeaderNames.XAPIKey, "correct key" } };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(myConfiguration)
            .Build();
        var serviceProviderMock = new Mock<IServiceProvider>(MockBehavior.Strict);
        serviceProviderMock.Setup(sp => sp.GetService(It.IsAny<Type>()))
                           .Returns(configuration);
        context.RequestServices = serviceProviderMock.Object;
        context.Request.Headers.Add(HeaderNames.XAPIKey, "invalid key");

        // Act
        await _sut.InvokeAsync(context);

        // Assert
        Assert.AreEqual(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
        Assert.AreEqual("text/plain", context.Response.ContentType);
        _requestMock.Verify(r => r.Next(It.IsAny<HttpContext>()), Times.Never);
    }

    [TestMethod]
    public async Task InvokeAsync_ApiKeyMiddleware_CorrectXAPIKeyHeader_CallsNext()
    {
        // Arrange
        var context = new DefaultHttpContext();
        string correctKey = "correct key";
        var myConfiguration = new Dictionary<string, string>() { { HeaderNames.XAPIKey, correctKey } };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(myConfiguration)
            .Build();
        var serviceProviderMock = new Mock<IServiceProvider>(MockBehavior.Strict);
        serviceProviderMock.Setup(sp => sp.GetService(It.IsAny<Type>()))
                           .Returns(configuration);
        context.RequestServices = serviceProviderMock.Object;
        context.Request.Headers.Add(HeaderNames.XAPIKey, correctKey);

        // Act
        await _sut.InvokeAsync(context);

        // Assert
        _requestMock.Verify(n => n.Next(context), Times.Once);
    }
}
