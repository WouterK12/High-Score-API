using HighScoreAPI.Encryption;

namespace HighScoreAPI.Test.Encryption;

[TestClass]
public class AesOperationTest
{
    private const string KeyBase64 = "HbrBX/TckMpgKFKZbLQsJkkfE3bUKJ1JuD2CPZbxt48=";

    [TestMethod]
    public void GenerateKeyBase64_AesOperation_ReturnsExpected()
    {
        // Act
        string result = AesOperation.GenerateKeyBase64();

        // Assert
        Assert.IsFalse(string.IsNullOrWhiteSpace(result));
    }

    [TestMethod]
    public void EncryptString_AesOperation_ReturnsExpected()
    {
        // Arrange
        string jsonString = "{ \"username\": \"user\", \"score\": 10 }";

        // Act
        var result = AesOperation.EncryptData(jsonString, KeyBase64, out string vectorBase64);

        // Assert
        var decryptedString = AesOperation.DecryptData(result, KeyBase64, vectorBase64);
        Assert.AreEqual("{ \"username\": \"user\", \"score\": 10 }", decryptedString);
    }

    [TestMethod]
    public void DecryptString_AesOperation_ReturnsExpected()
    {
        // Arrange
        string jsonString = "{ \"username\": \"user\", \"score\": 10 }";
        string cipherText = AesOperation.EncryptData(jsonString, KeyBase64, out string vectorBase64);

        // Act
        var result = AesOperation.DecryptData(cipherText, KeyBase64, vectorBase64);

        // Assert
        Assert.AreEqual("{ \"username\": \"user\", \"score\": 10 }", result);
    }
}
