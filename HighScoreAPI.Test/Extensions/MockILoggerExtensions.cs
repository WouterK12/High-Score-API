using Microsoft.Extensions.Logging;
using Moq;

namespace HighScoreServer.Test.Extensions;

// https://adamstorr.azurewebsites.net/blog/mocking-ilogger-with-moq
internal static class MockILoggerExtensions
{
    public static Mock<ILogger<T>> Verify<T>(this Mock<ILogger<T>> logger, LogLevel expectedLogLevel, string expectedMessage, Func<Times> times)
    {
        Func<object, Type, bool> state = (v, t) => v.ToString().CompareTo(expectedMessage) == 0;

        logger.Verify(
            l => l.Log(
                It.Is<LogLevel>(l => l == expectedLogLevel),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => state(v, t)),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            times);

        return logger;
    }
}
