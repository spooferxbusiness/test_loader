using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;


class misc
{
    public static string Encrypt(string text, string key)
    {
        byte[] plaintextBytes = Encoding.UTF8.GetBytes(text);
        byte[] keyBytes = Encoding.UTF8.GetBytes(key);
        byte[] encryptedBytes = new byte[plaintextBytes.Length];

        for (int i = 0; i < plaintextBytes.Length; i++)
        {
            encryptedBytes[i] = (byte)(plaintextBytes[i] ^ keyBytes[i % keyBytes.Length]);
        }

        return Convert.ToBase64String(encryptedBytes);
    }

    public static string GenerateString()
    {
        return (new Random().Next() * 0xDEADBEEF).ToString("X");
    }


    public static void ClearString(ref string str)
    {
        str = misc.GenerateString();
        string[] buffs = { };

        ClearMem();

        for (int i = 0; i < new Random().Next(100, 150); i++)
        {
            buffs.Append(Encrypt(GenerateString(), GenerateString())); // this will add random encrypted strings in memory
            buffs.Append(GenerateString()); // this will add random strings in memory

        }

    }

    public static string getMD5Hash(string file)
    {
        var md5 = MD5.Create();
        byte[] bytes = md5.ComputeHash(File.ReadAllBytes(file));
        string buffer = string.Empty;

        foreach (byte b in bytes)
        {
            buffer += b.ToString("x2");
        }

        return buffer;
    }


    public static DateTime UnixTimeToDateTime(long unixtime)
    {
        DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Local);
        try
        {
            dtDateTime = dtDateTime.AddHours(unixtime).ToLocalTime();
        }
        catch
        {
            dtDateTime = DateTime.MaxValue;
        }
        return dtDateTime;
    }

    public static void ClearBytes(ref byte[] bytes)
    {
        bytes = GenerateRandomBytes(new Random().Next(1,10));
        List<byte[]> buffs = new List<byte[]>();

        ClearMem();

        for (int i = 0; i < new Random().Next(100, 150); i++)
        {
            buffs.Add(Encoding.UTF8.GetBytes(GenerateString()));
        }

    }

    public static byte[] XorEncrypt(byte[] inputBytes, string key)
    {
        byte[] keyBytes = Encoding.UTF8.GetBytes(key);
        byte[] encryptedBytes = new byte[inputBytes.Length];
        for (int i = 0; i < inputBytes.Length; i++)
        {
            encryptedBytes[i] = (byte)(inputBytes[i] ^ keyBytes[i % keyBytes.Length]);
        }
        return encryptedBytes;
    }

    public static byte[] XorDecrypt(byte[] encryptedBytes, string key)
    {
        byte[] keyBytes = Encoding.UTF8.GetBytes(key);
        byte[] decryptedBytes = new byte[encryptedBytes.Length];
        for (int i = 0; i < encryptedBytes.Length; i++)
        {
            decryptedBytes[i] = (byte)(encryptedBytes[i] ^ keyBytes[i % keyBytes.Length]);
        }
        return decryptedBytes;
    }


    public static void ClearMem()
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
    }

    public static byte[] GenerateRandomBytes(int length)
    {
        byte[] randomBytes = new byte[length];

        using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
        {
            rng.GetBytes(randomBytes);
        }

        return randomBytes;
    }

    public static string[] DAD(string[] auth_data)
    {
        string[] buffer = new string[auth_data.Length];

        // Keys for decryption
        string[] resources = {
        loader_fra.Properties.Resources.name_e_key,
        loader_fra.Properties.Resources.owner_e_key,
        loader_fra.Properties.Resources.secret_e_key,
        loader_fra.Properties.Resources.version_e_key
    };

        for (int i = 0; i < auth_data.Length; i++)
        {
            buffer[i] = misc.Decrypt(auth_data[i], resources[i]); // Decrypt using the corresponding key
        }

        return buffer;
    }

    public static string Decrypt(string encryptedBase64Text, string key)
    {
        byte[] encryptedBytes = Convert.FromBase64String(encryptedBase64Text);
        byte[] keyBytes = Encoding.UTF8.GetBytes(key);
        byte[] decryptedBytes = new byte[encryptedBytes.Length];

        for (int i = 0; i < encryptedBytes.Length; i++)
        {
            decryptedBytes[i] = (byte)(encryptedBytes[i] ^ keyBytes[i % keyBytes.Length]);
        }

        return Encoding.UTF8.GetString(decryptedBytes);
    }

    public static string base64_enc(string input)
    {
        byte[] plainTextBytes = System.Text.Encoding.UTF8.GetBytes(input);
        string encodedText = System.Convert.ToBase64String(plainTextBytes);
        return encodedText;
    }

    public static string base64_dec(string input)
    {
        byte[] encodedBytes = System.Convert.FromBase64String(input);
        string plainText = System.Text.Encoding.UTF8.GetString(encodedBytes);
        return plainText;
    }
}
