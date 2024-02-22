namespace United.Mobile.Model.Common
{
    public class SQLDBRequest
    {
        public string SessionId { get; set; }
       
        public string TransactionId { get; set; }

        public dynamic Data { get; set; }
    }
}
