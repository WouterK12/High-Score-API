namespace HighScoreAPI.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequiresAdminKeyAttribute : Attribute
{
}
