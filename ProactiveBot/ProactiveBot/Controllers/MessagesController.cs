using Microsoft.Azure;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Teams.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using ProactiveBot.Models;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace ProactiveBot
{
    [BotAuthentication]
    public class MessagesController
        : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            if (activity.Type == ActivityTypes.Message)
            {
                await Conversation.SendAsync(activity, () => new Dialogs.RootDialog());
            }
            else
            {
                await HandleSystemMessageAsync(activity);
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        public HttpResponseMessage Options()
        {
            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        private async Task<Activity> HandleSystemMessageAsync(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
                var teamsChannelData = message.GetChannelData<TeamsChannelData>();
                if (teamsChannelData != null)
                {
                    var eventtype = teamsChannelData.EventType;
                    if (eventtype == "teamMemberAdded")
                    {
                        foreach (var memberAdded in message.MembersAdded)
                        {
                            var teamsUserInfo = new TeamsUserInfo();

                            // Why does the bot do this???
                            if (memberAdded.Name == null)
                            {
                                // Assume it's the bot?
                                teamsUserInfo = new TeamsUserInfo()
                                {
                                    RowKey = "jwproactivebot",
                                    PartitionKey = "jwproactivebot",
                                    Id = memberAdded.Id,
                                    Name = "jwproactivebot",
                                    Type = "bot",
                                };
                            }
                            else
                            {
                                // Assume it's a user added to the channel?
                                teamsUserInfo = new TeamsUserInfo()
                                {
                                    RowKey = memberAdded.Name,
                                    PartitionKey = memberAdded.Name,
                                    Id = memberAdded.Id,
                                    Name = memberAdded.Name,
                                    Type = "user",
                                };
                            }

                            await SaveToStorageAsync("TeamsUsers", teamsUserInfo);

                            var tenantInformation = new TenantInformation()
                            {
                                RowKey = "BotTenant",
                                PartitionKey = "BotTenant",
                                TenantId = teamsChannelData.Tenant.Id,
                            };
                            await SaveToStorageAsync("TenantInfo", tenantInformation);
                        }
                    }
                }
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }

        private async Task SaveToStorageAsync<T>(string tableReference, T item)
            where T : TableEntity
        {
            var cloudStorageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageAccount"));
            var cloudTableClient = cloudStorageAccount.CreateCloudTableClient();
            var cloudTable = cloudTableClient.GetTableReference(tableReference);
            await cloudTable.CreateIfNotExistsAsync();

            var insertOrReplaceOperation = TableOperation.InsertOrReplace(item);
            await cloudTable.ExecuteAsync(insertOrReplaceOperation);
        }
    }
}