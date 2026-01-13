using System.Security.Cryptography;
using System.Text;

namespace Medical.API.Services;

/// <summary>
/// 数据加密服务
/// </summary>
public interface IEncryptionService
{
    /// <summary>
    /// 加密字符串
    /// </summary>
    string Encrypt(string plainText);

    /// <summary>
    /// 解密字符串
    /// </summary>
    string Decrypt(string cipherText);

    /// <summary>
    /// 哈希密码
    /// </summary>
    string HashPassword(string password);

    /// <summary>
    /// 验证密码
    /// </summary>
    bool VerifyPassword(string password, string hashedPassword);
}

/// <summary>
/// 数据加密服务实现
/// </summary>
public class EncryptionService : IEncryptionService
{
    private readonly string _encryptionKey;
    private readonly byte[] _key;
    private readonly byte[] _iv;

    public EncryptionService(IConfiguration configuration)
    {
        _encryptionKey = configuration["Encryption:Key"] ?? "MedicalSystemEncryptionKey2024!@#$%^&*()";
        using var sha256 = SHA256.Create();
        _key = sha256.ComputeHash(Encoding.UTF8.GetBytes(_encryptionKey));
        _iv = _key.Take(16).ToArray();
    }

    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            return string.Empty;

        using var aes = Aes.Create();
        aes.Key = _key;
        aes.IV = _iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var encryptor = aes.CreateEncryptor();
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

        return Convert.ToBase64String(encryptedBytes);
    }

    public string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText))
            return string.Empty;

        try
        {
            using var aes = Aes.Create();
            aes.Key = _key;
            aes.IV = _iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using var decryptor = aes.CreateDecryptor();
            var cipherBytes = Convert.FromBase64String(cipherText);
            var decryptedBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);

            return Encoding.UTF8.GetString(decryptedBytes);
        }
        catch
        {
            return string.Empty;
        }
    }

    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    public bool VerifyPassword(string password, string hashedPassword)
    {
        return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
    }
}

