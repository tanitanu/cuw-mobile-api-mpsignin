using Microsoft.AspNetCore.Identity;

namespace United.Common.Helper
{
    public interface IHashedPin
    {
        string HashPinGeneration(string userpin);

        PasswordVerificationResult VerifyHashedPin(string hashedPin, string providedPin);
    }
}
