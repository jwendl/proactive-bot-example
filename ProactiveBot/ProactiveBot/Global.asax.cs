using Autofac;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Connector;
using System.Web.Http;

namespace ProactiveBot
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            var store = new InMemoryDataStore();

            Conversation.UpdateContainer(
                       builder =>
                       {
                           builder
                                  .RegisterType<InMemoryDataStore>()
                                  .Keyed<IBotDataStore<BotData>>(typeof(ConnectorStore));

                           builder.Register(c => new CachingBotDataStore(store,
                                    CachingBotDataStoreConsistencyPolicy.ETagBasedConsistency))
                                    .As<IBotDataStore<BotData>>()
                                    .AsSelf()
                                    .SingleInstance();
                       });

            GlobalConfiguration.Configure(WebApiConfig.Register);
        }
    }
}
