using System.ComponentModel;
using Microsoft.Extensions.FileProviders;

namespace YuzuDelivery.TemplateEngines.Handlebars.Settings;

public class HandlebarsSettings
{
    private const string DefaultPartialPrefix = "par";
    private const string DefaultTemplatesPath = "./Yuzu/_templates";
    private const string DefaultHandlebarsFileExtensions = ".hbs";

    [DefaultValue(DefaultPartialPrefix)]
    public string PartialPrefix { get; set; } = DefaultPartialPrefix;

    [DefaultValue(DefaultTemplatesPath)]
    public string TemplatesPath { get; set; } = DefaultTemplatesPath;

    [DefaultValue(DefaultHandlebarsFileExtensions)]
    public string HandlebarsFileExtension { get; set; } = DefaultHandlebarsFileExtensions;

    public IFileProvider TemplatesFileProvider { get; set; } = null!;
}
