using HandlebarsDotNet;

namespace YuzuDelivery.TemplateEngines.Handlebars.Helpers;

public class Enum
{
    public static void Register()
    {
        HandlebarsDotNet.Handlebars.RegisterHelper("enum", (writer, context, parameters) =>
        {
            writer.WriteSafeString(EnumResolver.Convert(parameters[0]));
        });
    }
}
