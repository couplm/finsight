namespace Jellyfin.Plugin.Finsight.Configuration;

using MediaBrowser.Model.Plugins;

public enum SomeOptions
{
    OneOption,

    AnotherOption,
}

public class PluginConfiguration : BasePluginConfiguration
{
    public PluginConfiguration()
    {
        // set default options here
        this.Options = SomeOptions.AnotherOption;
        this.TrueFalseSetting = true;
        this.AnInteger = 2;
        this.AString = "string";
    }

    public bool TrueFalseSetting { get; set; }

    public int AnInteger { get; set; }

    public string AString { get; set; }

    public SomeOptions Options { get; set; }
}