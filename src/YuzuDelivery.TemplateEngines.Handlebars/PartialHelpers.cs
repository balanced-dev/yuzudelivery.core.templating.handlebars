using System;
using System.Collections.Generic;
using System.Linq;
using HandlebarsDotNet;
using HandlebarsDotNet.Compiler;
using YuzuDelivery.Core;

namespace YuzuDelivery.TemplateEngines.Handlebars;

public static class PartialHelpers
{
    public static bool IsSimple(this Type type)
    {
        return type.IsPrimitive
               || type == typeof(string);
    }

    public static Dictionary<string, object>? GetDataModel(Arguments parameters, dynamic context = null)
    {
        var paramType = parameters[1].GetType();
        var properties = new Dictionary<string, object>();

        if (parameters[1] is Dictionary<string, object>)
        {
            properties = parameters[1] as Dictionary<string, object>;
        }
        else
        {
            //we can't support modifiers and hashParameters on generic types
            //check if namespace is null since anonymous types count as generic
            if (paramType.IsSimple() || paramType.IsArray || (paramType.IsGenericType && paramType.Namespace != null))
            {
                return null;
            }

            if (paramType != typeof(HashParameterDictionary))
            {
                properties = paramType.GetProperties().ToDictionary(
                    property => StringExtensions.FirstCharacterToLower(property.Name),
                    property => property.GetValue(parameters[1]));
            }
            // when context is implicitly given
            else if (context != null)
            {
                properties = ((object)context).GetType().GetProperties().ToDictionary(
                    property => StringExtensions.FirstCharacterToLower(property.Name),
                    property => property.GetValue(context));
            }
        }

        var modifiers = parameters.Where((source, index) => index > 1)
                                  .Where(x => x != null && x.GetType().IsSimple())
                                  .Select(x => x.ToString()).ToList();
        if (modifiers.Any())
        {
            if (properties.Any(property => property.Key == "_modifiers" && property.Value == null))
            {
                properties.Remove("_modifiers");
            }

            if (!properties.ContainsKey("_modifiers"))
            {
                properties.Add("_modifiers", modifiers);
            }
        }

        if (!(parameters[parameters.Length - 1] is HashParameterDictionary hashParameterDictionary))
            return properties;

        foreach (var parameter in hashParameterDictionary)
        {
            if(properties.ContainsKey(parameter.Key))
            {
                properties[parameter.Key] = parameter.Value;
            }
            else
            {
                properties.Add(parameter.Key, parameter.Value);
            }
        }

        return properties;
    }
}
