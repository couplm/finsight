namespace Jellyfin.Plugin.Finsight;

using System;
using System.Collections.Generic;
using System.Globalization;
using Jellyfin.Plugin.Finsight.Configuration;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;

public class Plugin : BasePlugin<PluginConfiguration>, IHasWebPages
{
    public Plugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer)
        : base(applicationPaths, xmlSerializer)
    {
        Instance = this;
    }

    public static Plugin? Instance { get; private set; }

    public override string Name => "Finsight";

    public override Guid Id => Guid.Parse("16cc3e3a-d475-47b2-8412-75e2eaf55ebe");

    public IEnumerable<PluginPageInfo> GetPages()
    {
        return
        [
            new PluginPageInfo
            {
                Name = this.Name,
                EmbeddedResourcePath = string.Format(CultureInfo.InvariantCulture, "{0}.Configuration.configPage.html", GetType().Namespace),
                EnableInMainMenu = true,
            },
            new PluginPageInfo
            {
                Name = "Artists",
                EmbeddedResourcePath = string.Format(CultureInfo.InvariantCulture, "{0}.Pages.artists.html", GetType().Namespace),
            },
        ];
    }
}
