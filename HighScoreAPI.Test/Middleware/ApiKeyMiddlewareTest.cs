﻿using HighScoreAPI.Attributes;
using HighScoreAPI.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Configuration;
using Moq;

namespace HighScoreAPI.Test.Middleware;

[TestClass]
public class ApiKeyMiddlewareTest
{
    private Mock<IRequestMock> _requestMock = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        _requestMock = new Mock<IRequestMock>(MockBehavior.Strict);
        _requestMock.Setup(r => r.Next(It.IsAny<HttpContext>()))
                    .Returns(Task.CompletedTask);
    }

    [TestMethod]
    public async Task InvokeAsync_NoXAPIKeyHeader_ApiKeyMiddleware_SetsResponse400()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var configuration = new ConfigurationBuilder().Build();

        var sut = new ApiKeyMiddleware(_requestMock.Object.Next, configuration);

        // Act
        await sut.InvokeAsync(context);

        // Assert
        Assert.AreEqual(StatusCodes.Status400BadRequest, context.Response.StatusCode);
        Assert.AreEqual("text/plain", context.Response.ContentType);
        _requestMock.Verify(r => r.Next(It.IsAny<HttpContext>()), Times.Never);
    }

    [TestMethod]
    public async Task InvokeAsync_AdminKeyAttribute_CorrectAdminXAPIKeyHeader_ApiKeyMiddleware_CallsNext()
    {
        // Arrange
        var context = new DefaultHttpContext();

        var andminKeyAttribute = new RequiresAdminKeyAttribute();
        var endpointMetadataCollection = new EndpointMetadataCollection(andminKeyAttribute);
        var endpoint = new Endpoint(null, endpointMetadataCollection, null);
        var endpointFeatureMock = new Mock<IEndpointFeature>(MockBehavior.Strict);
        endpointFeatureMock.SetupProperty(ef => ef.Endpoint, endpoint);
        context.Features.Set(endpointFeatureMock.Object);

        string correctKey = "correct key";
        context.Request.Headers.Add(HeaderNames.XAPIKey, correctKey);
        var configuration = BuildXAPIKeyConfiguration(correctKey, correctKey);

        var sut = new ApiKeyMiddleware(_requestMock.Object.Next, configuration);

        // Act
        await sut.InvokeAsync(context);

        // Assert
        _requestMock.Verify(n => n.Next(context), Times.Once);
    }

    [TestMethod]
    public async Task InvokeAsync_AdminKeyAttribute_InvalidXAPIKeyHeader_ApiKeyMiddleware_SetsResponse401()
    {
        // Arrange
        var context = new DefaultHttpContext();

        var andminKeyAttribute = new RequiresAdminKeyAttribute();
        var endpointMetadataCollection = new EndpointMetadataCollection(andminKeyAttribute);
        var endpoint = new Endpoint(null, endpointMetadataCollection, null);
        var endpointFeatureMock = new Mock<IEndpointFeature>(MockBehavior.Strict);
        endpointFeatureMock.SetupProperty(ef => ef.Endpoint, endpoint);
        context.Features.Set(endpointFeatureMock.Object);

        string correctKey = "correct key";
        context.Request.Headers.Add(HeaderNames.XAPIKey, "invalid key");
        IConfigurationRoot configuration = BuildXAPIKeyConfiguration(correctKey, correctKey);

        var sut = new ApiKeyMiddleware(_requestMock.Object.Next, configuration);

        // Act
        await sut.InvokeAsync(context);

        // Assert
        Assert.AreEqual(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
        Assert.AreEqual("text/plain", context.Response.ContentType);
        _requestMock.Verify(r => r.Next(It.IsAny<HttpContext>()), Times.Never);
    }

    [TestMethod]
    public async Task InvokeAsync_InvalidXAPIKeyHeader_ApiKeyMiddleware_SetsResponse401()
    {
        // Arrange
        var context = new DefaultHttpContext();
        string correctKey = "correct key";
        context.Request.Headers.Add(HeaderNames.XAPIKey, "invalid key");
        IConfigurationRoot configuration = BuildXAPIKeyConfiguration(correctKey, correctKey);

        var sut = new ApiKeyMiddleware(_requestMock.Object.Next, configuration);

        // Act
        await sut.InvokeAsync(context);

        // Assert
        Assert.AreEqual(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
        Assert.AreEqual("text/plain", context.Response.ContentType);
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

        var sut = new ApiKeyMiddleware(_requestMock.Object.Next, configuration);

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
