using System;
using System.Linq;

namespace YuzuDelivery.TemplateEngines.Handlebars.Helpers;

public class IfCond
{
    public static void Register()
    {
        HandlebarsDotNet.Handlebars.RegisterHelper("ifCond", (writer, options, context, parameters) =>
        {
            if (parameters.Length != 3)
                options.Inverse(writer, context);
            else
            {
                var param0 = string.Empty;
                var param1 = string.Empty;
                var param2 = string.Empty;

                try { param0 = EnumResolver.Convert(parameters[0])?.ToString(); } catch { }
                try { param1 = parameters[1].ToString(); } catch { }
                try { param2 = EnumResolver.Convert(parameters[2])?.ToString(); } catch { }

                if (checkCondition(param0, param1, param2))
                    options.Template(writer, context);
                else
                    options.Inverse(writer, context);
            }
        });
    }

    private static bool checkCondition(string v1, string operators, string v2)
    {
        switch (operators)
        {
            case "==":
                return (v1 == v2);
            case "===":
                return (v1 == v2);
            case "!==":
                return (v1 != v2);
        }

        double v1AsInt = 0, v2AsInt = 0;
        double.TryParse(v1, out v1AsInt);
        double.TryParse(v2, out v2AsInt);

        switch (operators)
        {
            case "<":
                return (v1AsInt < v2AsInt);
            case "<=":
                return (v1AsInt <= v2AsInt);
            case ">":
                return (v1AsInt > v2AsInt);
            case ">=":
                return (v1AsInt >= v2AsInt);
        }

        if (isBoolString(v1) && isBoolString(v2))
        {
            bool v1AsBool = false, v2AsBool = false;
            bool.TryParse(v1, out v1AsBool);
            bool.TryParse(v2, out v2AsBool);

            switch (operators)
            {
                case "&&":
                    return (v1AsBool && v2AsBool);
                case "||":
                    return (v1AsBool || v2AsBool);
            }
        }
        else
        {
            var v1AsString = v1?.ToString();
            var v2AsString = v2?.ToString();

            switch (operators)
            {
                case "&&":
                    return (!string.IsNullOrEmpty(v1) && !string.IsNullOrEmpty(v2));
                case "||":
                    return (!string.IsNullOrEmpty(v1) || !string.IsNullOrEmpty(v2));
            }
        }


        return false;
    }

    private static bool isBoolString(string value)
    {
        string[] values = { null, string.Empty, "True", "False", "true", "false", "0", "1", "-1" };
        return values.Any(x => x == value);
    }

}
