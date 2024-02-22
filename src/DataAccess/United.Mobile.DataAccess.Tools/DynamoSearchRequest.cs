namespace United.Mobile.DataAccess.Tools
{
    public class DynamoSearchRequest
    {
        public string TransactionId { get; set; }
        public string TableName { get; set; }
        public string SecondaryKey { get; set; }
    }
}