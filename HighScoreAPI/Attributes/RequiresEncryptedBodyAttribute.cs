namespace HighScoreAPI.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequiresEncryptedBodyAttribute : Attribute
{
}
