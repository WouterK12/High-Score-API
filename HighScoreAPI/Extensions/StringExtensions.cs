using Microsoft.AspNetCore.Mvc.Abstractions;
using Newtonsoft.Json;

namespace HighScoreAPI.Extensions;

public static class StringExtensions
{
    public static object DeserializeJsonToBodyType(this string jsonString, ActionDescriptor descriptor)
    {
        Type bodyType = typeof(object);

        foreach (ParameterDescriptor parameter in descriptor.Parameters)
        {
            if (descriptor.RouteValues.TryGetValue(parameter.Name, out string? value))
                continue;

            bodyType = parameter.ParameterType;
            break;
        }

        return JsonConvert.DeserializeObject(jsonString, bodyType);
    }
}
