using Microsoft.WindowsAzure.Storage.Table;

namespace ProactiveBot.Models
{
    public class TenantInformation
        : TableEntity
    {
        public string TenantId { get; set; }
    }
}