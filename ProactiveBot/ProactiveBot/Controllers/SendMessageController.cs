using Microsoft.Azure;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Teams;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json.Linq;
using ProactiveBot.Models;
using System;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace ProactiveBot.Controllers
{
    public class SendMessageController
        : ApiController
    {
        public async Task<HttpResponseMessage> Post()
        {
            var connectorClientUrl = "https://smba.trafficmanager.net/amer/";
            MicrosoftAppCredentials.TrustServiceUrl(connectorClientUrl, DateTime.Now.AddDays(7));

            var account = new MicrosoftAppCredentials(ConfigurationManager.AppSettings["MicrosoftAppId"], ConfigurationManager.AppSettings["MicrosoftAppPassword"]);
            var connectorClient = new ConnectorClient(new Uri(connectorClientUrl), account);

            // Recipient id will look like 29:15WNNgxnHVyknEFMW4gUtEcNYciMmr7ZIsWHKxNnxLX7BVGuy8ukM6ieeDzPGWEzBnLPl-8bZQCDPnPevO0-Npg
            // Recipient name will look like Justin Wendlandt
            // This will come from "conversationUpdate" and be stored in table storage?
            var user = await ReadFromStorageAsync<TeamsUserInfo>("TeamsUsers", "Justin Wendlandt", "Justin Wendlandt");
            var recipientId = user.Id;
            var recipientName = user.Name;

            // From id will be the bot one like 28:a97169b8-25b8-479a-8cbb-6030ae331f13
            // From Name will be the bot name like jwproactivebot
            // This will come from "conversationUpdate" and be stored in table storage?
            var bot = await ReadFromStorageAsync<TeamsUserInfo>("TeamsUsers", "jwproactivebot", "jwproactivebot");
            var fromId = bot.Id;
            var fromName = bot.Name;

            // This will be something like 453b8921-be44-49ec-bd3c-87414db10cb2
            // Can get this from "conversationUpdate" in MessageController by using message.GetChannelData<TeamsChannelData>().Tenant
            var tenantInformation = await ReadFromStorageAsync<TenantInformation>("TenantInfo", "BotTenant", "BotTenant");
            var tenantId = tenantInformation.TenantId;

            var initialActivity = new Activity()
            {
                Type = ActivityTypes.Message,
                ServiceUrl = connectorClientUrl,
                Recipient = new ChannelAccount()
                {
                    Id = recipientId,
                    Name = recipientName,
                },
                From = new ChannelAccount()
                {
                    Id = fromId,
                    Name = fromName,
                },
                ChannelData = JObject.Parse(@"{ ""tenant"": { ""id"": """ + tenantId + @""" }}"),
            };

            // Create or get existing chat conversation with user
            var response = connectorClient.Conversations.CreateOrGetDirectConversation(initialActivity.From, initialActivity.Recipient, initialActivity.GetTenantId());

            // Construct the message to post to conversation
            var activity = new Activity()
            {
                Text = "This is a proactive message!",
                Type = ActivityTypes.Message,
                Conversation = new ConversationAccount
                {
                    Id = response.Id
                },
            };

            // Post the message to chat conversation with user
            await connectorClient.Conversations.SendToConversationAsync(activity, response.Id);

            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        private async Task<T> ReadFromStorageAsync<T>(string tableReference, string partitionKey, string rowKey)
            where T : TableEntity
        {
            var cloudStorageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageAccount"));
            var cloudTableClient = cloudStorageAccount.CreateCloudTableClient();
            var cloudTable = cloudTableClient.GetTableReference(tableReference);
            await cloudTable.CreateIfNotExistsAsync();

            var retrieveOperation = TableOperation.Retrieve<T>(partitionKey, rowKey);
            var tableResult = await cloudTable.ExecuteAsync(retrieveOperation);
            return tableResult.Result as T;
        }
    }
}