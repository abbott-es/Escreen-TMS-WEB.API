using System;
using System.Security.Cryptography;
using System.Text;
using WEB.UTILITY.Logger;

namespace WEB.UTILITY.Security
{
    public class RsaEncryptionService : IRsaEncryptionService
    {
        private readonly RSA _rsa;
        private readonly RSASignaturePadding _defaultPadding = RSASignaturePadding.Pss;
        private readonly IAppLogger<RsaEncryptionService> _logger;

        public RsaEncryptionService(RsaKeyManager keyManager, IAppLogger<RsaEncryptionService> logger)
        {
            _rsa = keyManager?.Rsa ?? throw new ArgumentNullException(nameof(keyManager), "Key manager cannot be null.");
            _logger = logger;
        }

        public string Encrypt(string plainText)
        {
            return ExecuteCrypto(() =>
            {
                byte[] data = ToBytes(plainText);
                byte[] encrypted = _rsa.Encrypt(data, RSAEncryptionPadding.OaepSHA256);
                return ToBase64(encrypted);
            }, "Encryption failed.");
        }

        public string Decrypt(string base64EncryptedText)
        {
            return ExecuteCrypto(() =>
            {
                byte[] encrypted = FromBase64(base64EncryptedText);
                byte[] decrypted = _rsa.Decrypt(encrypted, RSAEncryptionPadding.OaepSHA256);
                return ToString(decrypted);
            }, "Decryption failed.");
        }

        public (string encrypted, string signature) EncryptWithSignature(string plainText)
        {
            return ExecuteCrypto(() =>
            {
                byte[] data = ToBytes(plainText);
                byte[] encrypted = _rsa.Encrypt(data, RSAEncryptionPadding.OaepSHA256);
                string signature = SignData(plainText);
                return (ToBase64(encrypted), signature);
            }, "Encryption with signature failed.");
        }

        public string DecryptWithSignature(string base64EncryptedText, string signature)
        {
            return ExecuteCrypto(() =>
            {
                byte[] encrypted = FromBase64(base64EncryptedText);
                byte[] decrypted = _rsa.Decrypt(encrypted, RSAEncryptionPadding.OaepSHA256);
                string plainText = ToString(decrypted);

                if (!VerifySignature(plainText, signature))
                {
                    _logger.LogError(new CryptographicException("Signature verification failed."), "Invalid signature");
                    throw new CryptographicException("Signature verification failed.");
                }

                return plainText;
            }, "Decryption with signature failed.");
        }

        private string SignData(string plainText)
        {
            return ExecuteCrypto(() =>
            {
                byte[] data = ToBytes(plainText);
                byte[] signature = _rsa.SignData(data, HashAlgorithmName.SHA256, _defaultPadding);
                return ToBase64(signature);
            }, "Signing failed.");
        }

        private bool VerifySignature(string plainText, string base64Signature)
        {
            try
            {
                byte[] data = ToBytes(plainText);
                byte[] signature = FromBase64(base64Signature);
                return _rsa.VerifyData(data, signature, HashAlgorithmName.SHA256, _defaultPadding);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Signature verification failed: {ex.Message}");
                return false;
            }
        }

        private static byte[] ToBytes(string input) => Encoding.UTF8.GetBytes(input);
        private static string ToString(byte[] data) => Encoding.UTF8.GetString(data);
        private static string ToBase64(byte[] data) => Convert.ToBase64String(data);
        private static byte[] FromBase64(string base64) => Convert.FromBase64String(base64);

        private T ExecuteCrypto<T>(Func<T> action, string errorMessage)
        {
            try
            {
                return action();
            }
            catch (FormatException ex)
            {
                _logger.LogError(ex, $"Invalid format: {ex.Message}");
                throw new ArgumentException("Invalid input format.", ex);
            }
            catch (CryptographicException ex)
            {
                _logger.LogError(ex, $"{errorMessage}: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unexpected error: {ex.Message}");
                throw new CryptographicException(errorMessage, ex);
            }
        }
    }
}
