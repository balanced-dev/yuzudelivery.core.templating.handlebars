using HandlebarsDotNet;

namespace YuzuDelivery.TemplateEngines.Handlebars.Helpers;

public class ToLowerCase
{
    public static void Register()
    {
        HandlebarsDotNet.Handlebars.RegisterHelper("toLowerCase", (writer, context, parameters) =>
        {
            writer.WriteSafeString(parameters[0].ToString().ToLower());
        });
    }
}
