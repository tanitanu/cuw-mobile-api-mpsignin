namespace United.Mobile.MPSignInTool.Models
{
    public class DynamoUserModel
    {
        public string UserId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string PasswordHash { get; set; }
        public string  Role { get; set; }
    }
}