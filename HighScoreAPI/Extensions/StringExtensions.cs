using Microsoft.AspNetCore.Mvc.Abstractions;
using Newtonsoft.Json;

namespace HighScoreAPI.Extensions;

public static class StringExtensions
{
    public static object DeserializeJsonToBodyType(this string jsonString, ActionDescriptor descriptor, RouteValueDictionary routeValues)
    {
        Type bodyType = typeof(object);

        foreach (ParameterDescriptor parameter in descriptor.Parameters)
        {
            if (routeValues.TryGetValue(parameter.Name, out object? value))
                continue;

            bodyType = parameter.ParameterType;
            break;
        }

        return JsonConvert.DeserializeObject(jsonString, bodyType);
    }
}
