using Microsoft.WindowsAzure.Storage.Table;

namespace ProactiveBot.Models
{
    public class TeamsUserInfo
        : TableEntity
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Type { get; set; }
    }
}