using System.Security.Cryptography;
using System.Text;

namespace RestBar.Helpers;

/// <summary>RFC 6238 TOTP (HMAC-SHA1, 30s step, 6 digits).</summary>
public static class TotpHelper
{
    public static string GenerateSecret(int bytes = 20)
    {
        var raw = RandomNumberGenerator.GetBytes(bytes);
        return Base32Encode(raw);
    }

    public static string GetProvisioningUri(string email, string secret, string issuer = "RestBar")
    {
        var label = Uri.EscapeDataString($"{issuer}:{email}");
        var iss = Uri.EscapeDataString(issuer);
        return $"otpauth://totp/{label}?secret={secret}&issuer={iss}&digits=6&period=30";
    }

    public static bool VerifyCode(string secret, string code, int window = 1)
    {
        if (string.IsNullOrWhiteSpace(secret) || string.IsNullOrWhiteSpace(code)) return false;
        code = code.Trim().Replace(" ", "");
        if (code.Length != 6 || !code.All(char.IsDigit)) return false;

        var key = Base32Decode(secret);
        var timestep = DateTimeOffset.UtcNow.ToUnixTimeSeconds() / 30;
        for (var w = -window; w <= window; w++)
        {
            if (ComputeTotp(key, timestep + w) == code) return true;
        }
        return false;
    }

    private static string ComputeTotp(byte[] key, long counter)
    {
        var counterBytes = BitConverter.GetBytes(System.Net.IPAddress.HostToNetworkOrder(counter));
        using var hmac = new HMACSHA1(key);
        var hash = hmac.ComputeHash(counterBytes);
        var offset = hash[^1] & 0x0F;
        var binary = ((hash[offset] & 0x7F) << 24)
                     | ((hash[offset + 1] & 0xFF) << 16)
                     | ((hash[offset + 2] & 0xFF) << 8)
                     | (hash[offset + 3] & 0xFF);
        return (binary % 1_000_000).ToString("D6");
    }

    private static string Base32Encode(byte[] data)
    {
        const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
        var sb = new StringBuilder((data.Length * 8 + 4) / 5);
        int buffer = 0, bitsLeft = 0;
        foreach (var b in data)
        {
            buffer = (buffer << 8) | b;
            bitsLeft += 8;
            while (bitsLeft >= 5)
            {
                sb.Append(alphabet[(buffer >> (bitsLeft - 5)) & 31]);
                bitsLeft -= 5;
            }
        }
        if (bitsLeft > 0) sb.Append(alphabet[(buffer << (5 - bitsLeft)) & 31]);
        return sb.ToString();
    }

    private static byte[] Base32Decode(string input)
    {
        const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
        input = input.Trim().Replace(" ", "").Replace("=", "").ToUpperInvariant();
        var output = new List<byte>(input.Length * 5 / 8);
        int buffer = 0, bitsLeft = 0;
        foreach (var c in input)
        {
            var val = alphabet.IndexOf(c);
            if (val < 0) continue;
            buffer = (buffer << 5) | val;
            bitsLeft += 5;
            if (bitsLeft >= 8)
            {
                output.Add((byte)((buffer >> (bitsLeft - 8)) & 0xFF));
                bitsLeft -= 8;
            }
        }
        return output.ToArray();
    }
}
