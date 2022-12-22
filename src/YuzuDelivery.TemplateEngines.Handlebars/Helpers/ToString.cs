using HandlebarsDotNet;
using Newtonsoft.Json;

namespace YuzuDelivery.TemplateEngines.Handlebars.Helpers;

public class ToString
{
    public static void Register()
    {
        HandlebarsDotNet.Handlebars.RegisterHelper("toString", (writer, context, parameters) =>
        {
            writer.WriteSafeString(JsonConvert.SerializeObject(parameters[0]));
        });
    }
}
