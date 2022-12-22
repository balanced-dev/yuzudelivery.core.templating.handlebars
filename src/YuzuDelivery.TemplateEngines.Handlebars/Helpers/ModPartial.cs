using System;
using System.Linq;
using YuzuDelivery.Core;

namespace YuzuDelivery.TemplateEngines.Handlebars.Helpers;

public class ModPartial
{
    /*
    var partial = Handlebars.partials[path];
            if (typeof partial !== 'function') {
                partial = Handlebars.compile(partial);
            }
            return partial(context);
*/

    public static void Register()
    {
        HandlebarsDotNet.Handlebars.RegisterHelper("modPartial", (writer, context, parameters) =>
        {
            if (parameters.Length < 3)
            {
                throw new Exception(
                    "Handlebars modifier partial should have 3 parameters; partial name, content and modifier");
            }

            var @ref = string.Empty;

            if (parameters[0] != null)
                @ref = parameters[0].ToString();

            if (@ref == string.Empty)
            {
                var vmType = parameters[1].GetType();
                if (vmType.IsArray)
                {
                    vmType = vmType.GetElementType()!;
                }

                if (vmType.IsGenericType)
                {
                    vmType = vmType.GetGenericArguments().FirstOrDefault();
                }

                @ref = vmType.GetBlockName();
            }

            var templates =  HandlebarsDotNet.Handlebars.Configuration.RegisteredTemplates;

            if (!templates.TryGetValue(@ref.RemoveFirstForwardSlash(), out var template))
            {
                throw new Exception($"Handlebars modifier partial cannot find partial {parameters[0]}");
            }

            template(writer.CreateWrapper(), PartialHelpers.GetDataModel(parameters, context) ?? parameters[1]);
        });
    }
}
