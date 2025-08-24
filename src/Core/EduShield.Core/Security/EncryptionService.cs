using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using EduShield.Core.Configuration;

namespace EduShield.Core.Security;

/// <summary>
/// Service for encrypting and decrypting sensitive data using AES encryption
/// </summary>
public interface IEncryptionService
{
    /// <summary>
    /// Encrypts a string value
    /// </summary>
    /// <param name="plainText">Text to encrypt</param>
    /// <returns>Encrypted string in base64 format</returns>
    string Encrypt(string plainText);

    /// <summary>
    /// Decrypts an encrypted string value
    /// </summary>
    /// <param name="cipherText">Encrypted text in base64 format</param>
    /// <returns>Decrypted plain text</returns>
    string Decrypt(string cipherText);

    /// <summary>
    /// Encrypts a decimal value
    /// </summary>
    /// <param name="value">Decimal value to encrypt</param>
    /// <returns>Encrypted string in base64 format</returns>
    string EncryptDecimal(decimal value);

    /// <summary>
    /// Decrypts an encrypted decimal value
    /// </summary>
    /// <param name="cipherText">Encrypted text in base64 format</param>
    /// <returns>Decrypted decimal value</returns>
    decimal DecryptDecimal(string cipherText);
}

public class EncryptionService : IEncryptionService
{
    private readonly byte[] _key;
    private readonly byte[] _iv;

    public EncryptionService(IOptions<AuthenticationConfiguration> authConfig)
    {
        // Use JWT secret key as encryption key (first 32 bytes)
        var secretKey = authConfig.Value.Jwt.SecretKey;
        var keyBytes = Encoding.UTF8.GetBytes(secretKey);
        
        // Ensure key is exactly 32 bytes for AES-256
        if (keyBytes.Length < 32)
        {
            Array.Resize(ref keyBytes, 32);
        }
        else if (keyBytes.Length > 32)
        {
            Array.Resize(ref keyBytes, 32);
        }
        
        _key = keyBytes;
        
        // Generate a fixed IV from the key for consistency
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(_key);
        _iv = hash.Take(16).ToArray(); // Use first 16 bytes as IV
    }

    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            return plainText;

        using var aes = Aes.Create();
        aes.Key = _key;
        aes.IV = _iv;

        using var encryptor = aes.CreateEncryptor();
        using var msEncrypt = new MemoryStream();
        using var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
        using var swEncrypt = new StreamWriter(csEncrypt);

        swEncrypt.Write(plainText);
        swEncrypt.Flush();
        csEncrypt.FlushFinalBlock();

        return Convert.ToBase64String(msEncrypt.ToArray());
    }

    public string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText))
            return cipherText;

        try
        {
            using var aes = Aes.Create();
            aes.Key = _key;
            aes.IV = _iv;

            using var decryptor = aes.CreateDecryptor();
            using var msDecrypt = new MemoryStream(Convert.FromBase64String(cipherText));
            using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
            using var srDecrypt = new StreamReader(csDecrypt);

            return srDecrypt.ReadToEnd();
        }
        catch (Exception)
        {
            // Return original value if decryption fails
            return cipherText;
        }
    }

    public string EncryptDecimal(decimal value)
    {
        return Encrypt(value.ToString("F2"));
    }

    public decimal DecryptDecimal(string cipherText)
    {
        var decrypted = Decrypt(cipherText);
        if (decimal.TryParse(decrypted, out var result))
        {
            return result;
        }
        
        // Return 0 if parsing fails
        return 0m;
    }
}
