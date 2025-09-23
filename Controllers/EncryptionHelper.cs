using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

public class EncryptionHelper
{
    private static readonly byte[] Key = Encoding.UTF8.GetBytes("1234567890123456"); // Clave de 16 bytes (AES-128)
    private static readonly byte[] IV = Encoding.UTF8.GetBytes("6543210987654321");  // Vector de inicialización de 16 bytes
     
    public static string Encriptar(string texto)
    {
        using (Rijndael rijndael = Rijndael.Create())
        {
            rijndael.Key = Key;
            rijndael.IV = IV;

            using (MemoryStream memoryStream = new MemoryStream())
            using (CryptoStream cryptoStream = new CryptoStream(memoryStream, rijndael.CreateEncryptor(), CryptoStreamMode.Write))
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(texto);
                cryptoStream.Write(inputBytes, 0, inputBytes.Length);
                cryptoStream.FlushFinalBlock();

                return Convert.ToBase64String(memoryStream.ToArray());
            }
        }
    }
     
    public static string Desencriptar(string textoEncriptado)
    {
        byte[] inputBytes = Convert.FromBase64String(textoEncriptado);

        using (Rijndael rijndael = Rijndael.Create())
        {
            rijndael.Key = Key;
            rijndael.IV = IV;

            using (MemoryStream memoryStream = new MemoryStream(inputBytes))
            using (CryptoStream cryptoStream = new CryptoStream(memoryStream, rijndael.CreateDecryptor(), CryptoStreamMode.Read))
            {
                byte[] outputBytes = new byte[inputBytes.Length];
                int bytesRead = cryptoStream.Read(outputBytes, 0, outputBytes.Length);

                return Encoding.UTF8.GetString(outputBytes, 0, bytesRead);
            }
        }
    }
}
