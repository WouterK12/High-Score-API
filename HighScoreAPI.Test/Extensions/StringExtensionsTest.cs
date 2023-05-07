using HighScoreAPI.Extensions;
using HighScoreAPI.Models;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using System.Xml.Linq;

namespace HighScoreAPI.Test.Extensions;

[TestClass]
public class StringExtensionsTest
{
    [TestMethod]
    public void DeserializeJsonToBodyType_StringExtensions_ReturnsExpected()
    {
        // Arrange
        string jsonString = "{ \"username\": \"K03N\", \"score\": 423 }";
        var parameterWithBodyType = new ParameterDescriptor() { Name = "highScoreToAdd", ParameterType = typeof(HighScore) };
        var descriptor = new ActionDescriptor
        {
            Parameters = new List<ParameterDescriptor>
            {
               new() { Name = "projectName", ParameterType = typeof(string) },
               parameterWithBodyType,
               new() { Name = "dummyObject", ParameterType = typeof(object) }
            },
            RouteValues = new Dictionary<string, string?>()
            {
                { "projectName", "Smuggling-Pirates" }
            }
        };

        // Act
        var result = StringExtensions.DeserializeJsonToBodyType(jsonString, descriptor);

        // Assert
        Assert.IsInstanceOfType(result, typeof(HighScore));
        var highScore = result as HighScore;
        Assert.AreEqual("K03N", highScore.Username);
        Assert.AreEqual(423, highScore.Score);
    }
}
