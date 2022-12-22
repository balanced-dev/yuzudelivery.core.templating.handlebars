using System;
using System.Linq;
using HandlebarsDotNet;
using YuzuDelivery.Core;

namespace YuzuDelivery.TemplateEngines.Handlebars.Helpers;

public class DynPartial
{
    public static void Register()
    {
        HandlebarsDotNet.Handlebars.RegisterHelper("dynPartial", (writer, context, parameters) =>
        {
            var @ref = string.Empty;

            if (parameters[0].GetType() != typeof(UndefinedBindingResult))
            {
                @ref = parameters[0].ToString();
            }

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
                throw new Exception($"dynPartial error : Partial not found for ref: '{@ref}'");
            }
            if (parameters.Length > 1)
            {
                template(writer.CreateWrapper(), PartialHelpers.GetDataModel(parameters, context.Value) ?? parameters[1]);
            }
            else
            {
                template(writer.CreateWrapper(), context.Value);
            }
        });
    }
}
