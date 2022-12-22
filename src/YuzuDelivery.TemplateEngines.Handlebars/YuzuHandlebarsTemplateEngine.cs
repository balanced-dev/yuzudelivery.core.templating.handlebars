using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using YuzuDelivery.Core;

namespace YuzuDelivery.TemplateEngines.Handlebars;

// ReSharper disable once UnusedType.Global -- Used downstream
public class YuzuHandlebarsTemplateEngine : IYuzuTemplateEngine
{
    private readonly IDictionary<string, HandlebarsDotNet.HandlebarsTemplate<object, string>> _cache =
        new Dictionary<string, HandlebarsDotNet.HandlebarsTemplate<object, string>>();

    static YuzuHandlebarsTemplateEngine()
    {
        Helpers.IfCond.Register();
        Helpers.Array.Register();
        Helpers.Enum.Register();
        Helpers.DynPartial.Register();
        Helpers.ModPartial.Register();
        Helpers.ToString.Register();
        Helpers.ToLowerCase.Register();
        Helpers.PictureSource.Register();
    }

    public YuzuHandlebarsTemplateEngine(IYuzuConfiguration config)
    {
        foreach (var location in config.TemplateLocations ?? new List<ITemplateLocation>())
        {
            ProcessTemplates(new DirectoryInfo(location.Path), location);
        }
    }

    public string Render(string templateName, object model)
    {
        if (!_cache.TryGetValue(templateName, out var template))
        {
            throw new TemplateNotFound(templateName);
        }

        return template(model);
    }

    public class TemplateNotFound : ApplicationException
    {
        public TemplateNotFound(string templateName)
            : base($"Unknown template: '{templateName}'")
        {
        }
    }

    private void ProcessTemplates(DirectoryInfo directory, ITemplateLocation location)
    {
        if (location.SearchSubDirectories)
        {
            foreach (var d in directory.GetDirectories())
            {
                ProcessTemplates(d, location);
            }
        }

        foreach (var f in directory.GetFiles().Where(x => x.Extension == ".hbs"))
        {
            if (location.RegisterAllAsPartials)
            {
                RegisterPartial(f);
            }

            AddCompiledTemplates(f);
        }
    }

    private void RegisterPartial(FileInfo f)
    {
        using var reader = new StringReader(File.ReadAllText(f.FullName));

        var compiled = HandlebarsDotNet.Handlebars.Compile(reader);
        var templateName = Path.GetFileNameWithoutExtension(f.Name);

        HandlebarsDotNet.Handlebars.RegisterTemplate(templateName, compiled);
    }

    private void AddCompiledTemplates(FileInfo f)
    {
        var source = File.ReadAllText(f.FullName);
        var compiled = HandlebarsDotNet.Handlebars.Compile(source);
        _cache.Add(Path.GetFileNameWithoutExtension(f.Name), compiled);
    }
}
