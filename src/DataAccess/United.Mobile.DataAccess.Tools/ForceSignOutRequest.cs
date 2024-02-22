namespace United.Mobile.DataAccess.Tools
{
    internal class ForceSignOutRequest
    {
        public string TransactionId { get; set; }
        public string TableName { get; set; }
        public string SecondaryKey { get; set; }
    }
}