using HighScoreServer.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace HighScoreAPI.Test.Extensions;

[TestClass]
public class ServiceCollectionExtensionsTest
{
    [TestMethod]
    public void AddSwaggerGenWithApiKeyHeader_AddsSwaggerGen()
    {
        // Arrange
        var sut = new ServiceCollection();

        // Act
        var result = sut.AddSwaggerGenWithApiKeyHeader();

        // Assert
        Assert.AreEqual(sut, result);

        Assert.IsTrue(result.Any(s => s.ServiceType == typeof(ISwaggerProvider) &&
                                      s.ImplementationType == typeof(SwaggerGenerator) &&
                                      s.Lifetime == ServiceLifetime.Transient));
    }
}
