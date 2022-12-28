using System.ComponentModel;
using Microsoft.Extensions.FileProviders;

namespace YuzuDelivery.TemplateEngines.Handlebars.Settings;

public class HandlebarsSettings
{
    private const string DefaultTemplatesPath = "./Yuzu/_templates";
    private const string DefaultHandlebarsFileExtension = ".hbs";

    [DefaultValue(DefaultTemplatesPath)]
    public string TemplatesPath { get; set; } = DefaultTemplatesPath;

    [DefaultValue(DefaultHandlebarsFileExtension)]
    public string HandlebarsFileExtension { get; set; } = DefaultHandlebarsFileExtension;

    public IFileProvider TemplatesFileProvider { get; set; } = null!;
}
