namespace Jellyfin.Plugin.Finsight;

using MediaBrowser.Controller;
using MediaBrowser.Controller.Plugins;
using Microsoft.Extensions.DependencyInjection;

public class PluginServiceRegistrator : IPluginServiceRegistrator
{
    public void RegisterServices(IServiceCollection serviceCollection, IServerApplicationHost applicationHost)
    {
        serviceCollection.AddSingleton<Services.IStatsService, Services.StatsService>();
        serviceCollection.AddSingleton<Data.Repository.IStatsRepository, Data.Repository.StatsRepository>();
    }
}
