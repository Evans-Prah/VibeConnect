using System.Security.Cryptography;
using System.Text;

namespace VibeConnect.Auth.Module.Utilities;

public static class PasswordHelper
{
    private const int KeySize = 64;
    private const int Iterations = 350000;
    private static readonly HashAlgorithmName HashAlgorithm = HashAlgorithmName.SHA512;
    
    public static string HashPassword(string password, out byte[] salt)
    {
        salt = GenerateRandomSalt();

        var hash = Rfc2898DeriveBytes.Pbkdf2(Encoding.UTF8.GetBytes(password), salt, Iterations, HashAlgorithm,
            KeySize);

        return Convert.ToHexString(hash);
    }

    public static bool VerifyPassword(string password, byte[] hash, byte[] salt)
    {
        var hashToCompare = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithm, KeySize);
        
        //FixedTimeEquals: This is necessary because hackers can infer information about the internal state of our system based on execution time variations
        //allowing them to potentially guess the correct password. These are called timing side-channel exploits.

        return CryptographicOperations.FixedTimeEquals(hashToCompare, hash);
    }

    private static byte[] GenerateRandomSalt()
    {
        var salt = RandomNumberGenerator.GetBytes(KeySize);
        return salt;
    }
    
    public static string GenerateRandomToken()
    {
        return Convert.ToHexString(RandomNumberGenerator.GetBytes(KeySize));
    }
    
    public static string GenerateUnique2FactorCode()
    {
        // Get the current timestamp as a long integer (milliseconds since Unix epoch).
        long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        // Generate a random number between 10000 and 999999 (6-digit random number).
        int randomPart = new Random().Next(10000, 999999);

        // Combine the timestamp and random number.
        var uniqueCode = timestamp + randomPart.ToString();

        // Ensure the code is exactly 6 digits long (trim or pad with leading zeros if necessary).
        uniqueCode = uniqueCode.Substring(uniqueCode.Length - 6);

        return uniqueCode;
    }
    
    public static string HashTwoFactorCode(string code)
    {
        using SHA256 sha256 = SHA256.Create();
        byte[] codeBytes = Encoding.UTF8.GetBytes(code);
        byte[] hashBytes = sha256.ComputeHash(codeBytes);
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
    }

}