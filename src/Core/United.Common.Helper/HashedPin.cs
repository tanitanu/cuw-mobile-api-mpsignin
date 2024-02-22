using Microsoft.AspNetCore.Identity;

namespace United.Common.Helper
{
    public class HashedPin : IHashedPin
    {
        public string HashPinGeneration(string userpin)
        {
            return BCrypt.Net.BCrypt.HashPassword(userpin);
        }

        public PasswordVerificationResult VerifyHashedPin(string hashedPin, string providedPin)
        {
            var isValid = BCrypt.Net.BCrypt.Verify(providedPin, hashedPin);

            return isValid ? PasswordVerificationResult.Success : PasswordVerificationResult.Failed;
        }
    }
}
