using HighScoreAPI.Middleware.Workers;
using Microsoft.AspNetCore.Http;

namespace HighScoreAPI.Test.Middleware.Workers;

[TestClass]
public class RequestWriterTest
{
    [TestMethod]
    public async Task WriteBadRequestAsync_Message_RequestWriter_WritesBadRequestWithMessage()
    {
        // Arrange
        string message = "That's a Bad Request!";
        var context = new DefaultHttpContext();
        var sut = new RequestWriter();

        // Act
        await sut.WriteBadRequestAsync(context, message);

        // Assert
        Assert.AreEqual(StatusCodes.Status400BadRequest, context.Response.StatusCode);
        Assert.AreEqual("text/plain", context.Response.ContentType);
    }

    [TestMethod]
    public async Task WriteBadRequestAsync_NoMessage_RequestWriter_WritesBadRequestWithDefaultMessage()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var sut = new RequestWriter();

        // Act
        await sut.WriteBadRequestAsync(context);

        // Assert
        Assert.AreEqual(StatusCodes.Status400BadRequest, context.Response.StatusCode);
        Assert.AreEqual("text/plain", context.Response.ContentType);
    }

    [TestMethod]
    public async Task WriteUnauthorizedAsync_Message_RequestWriter_WritesUnauthorizedWithMessage()
    {
        // Arrange
        string message = "You're Unauthorized!";
        var context = new DefaultHttpContext();
        var sut = new RequestWriter();

        // Act
        await sut.WriteUnauthorizedAsync(context, message);

        // Assert
        Assert.AreEqual(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
        Assert.AreEqual("text/plain", context.Response.ContentType);
    }

    [TestMethod]
    public async Task WriteUnauthorizedAsync_NoMessage_RequestWriter_WritesUnauthorizedWithDefaultMessage()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var sut = new RequestWriter();

        // Act
        await sut.WriteUnauthorizedAsync(context);

        // Assert
        Assert.AreEqual(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
        Assert.AreEqual("text/plain", context.Response.ContentType);
    }
}
