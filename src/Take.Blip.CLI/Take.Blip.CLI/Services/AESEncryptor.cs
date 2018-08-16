using System;
using System.Security.Cryptography;
using System.Text;

namespace Take.BlipCLI.Services
{
    public class AESEncryptor
    {
        private const string K = "tZcsgH/V9MGXSq5cq54VfQ==";
        private const string I4 = "erH1hKWw2rkDi1mCZXuN5g==";

        private readonly ICryptoTransform _encryptorTransformer;
        private readonly ICryptoTransform _decryptorTransformer;

        public AESEncryptor()
        {
            var key = Convert.FromBase64String(K);
            var iv = Convert.FromBase64String(I4);
            
            _encryptorTransformer = Aes.Create().CreateEncryptor(key, iv);
            _decryptorTransformer = Aes.Create().CreateDecryptor(key, iv);
        }

        public string Decrypt(string encryptedText)
        {
            byte[] encrypted2 = Convert.FromBase64String(encryptedText);
            var result = _decryptorTransformer.TransformFinalBlock(encrypted2, 0, encrypted2.Length);

            return Encoding.UTF8.GetString(result);
        }

        public string Encrypt(string text)
        {
            var bytes = Encoding.UTF8.GetBytes(text);
            var encrypted = _encryptorTransformer.TransformFinalBlock(bytes, 0, bytes.Length);
            return Convert.ToBase64String(encrypted);
        }
    }
}
