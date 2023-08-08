using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace MacroBot.Core.Crypto;

public static class Signature
{
    public static bool VerifySignature(string? signature1, string? signature2)
    {
        if (string.IsNullOrEmpty(signature1) || string.IsNullOrEmpty(signature2))
        {
            return false;
        }

        var hash1 = Convert.FromBase64String(signature1);
        var hash2 = Convert.FromBase64String(signature2);

        if (hash1.Length != hash2.Length)
        {
            return false;
        }

        return !hash1.Where((t, i) => t != hash2[i]).Any();
    }
    
    public static async Task<string> GenerateSignatureFromRequest(HttpRequest request, string secret)
    {
        request.EnableBuffering();
        if (string.IsNullOrEmpty(secret))
        {
            throw new ArgumentException("Secret cannot be null or empty.", nameof(secret));
        }

        if (request.Body.CanSeek)
        {
            request.Body.Seek(0, SeekOrigin.Begin);
        }

        using var reader = new StreamReader(
            request.Body,
            encoding: Encoding.UTF8,
            detectEncodingFromByteOrderMarks: false,
            leaveOpen: true);
        
        var body = await reader.ReadToEndAsync();
        
        request.Body.Position = 0;
        
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(body));
        
        return BitConverter.ToString(hash).ToLower().Replace("-", "");
    }

}