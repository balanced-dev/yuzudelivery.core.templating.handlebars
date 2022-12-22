using HandlebarsDotNet;

namespace YuzuDelivery.TemplateEngines.Handlebars.Helpers;

public class Array
{
    public static void Register()
    {
        HandlebarsDotNet.Handlebars.RegisterHelper("array", (writer, options, context, parameters) =>
        {
            writer.WriteSafeString(parameters[0]);
        });
    }
}
