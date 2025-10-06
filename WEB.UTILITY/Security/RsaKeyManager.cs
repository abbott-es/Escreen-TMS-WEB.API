using System.Security.Cryptography;

namespace WEB.UTILITY.Security
{

    public sealed class RsaKeyManager
    {
        private static readonly Lazy<RsaKeyManager> _instance = new(() => new RsaKeyManager());

        public static RsaKeyManager Instance => _instance.Value;

        public RSA Rsa { get; private set; }

        private RsaKeyManager()
        {
            Rsa = RSA.Create(2048);
        }

        public void LoadPublicKey(string base64PublicKey)
        {
            Rsa.ImportRSAPublicKey(Convert.FromBase64String(base64PublicKey), out _);
        }

        public void LoadPrivateKey(string base64PrivateKey)
        {
            Rsa.ImportRSAPrivateKey(Convert.FromBase64String(base64PrivateKey), out _);
        }
    }

}