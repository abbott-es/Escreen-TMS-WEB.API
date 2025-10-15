using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WEB.UTILITY.Security.ISecurity
{
    public interface IRsaEncryptionService
    {
        string Encrypt(string plainText);
        string Decrypt(string base64EncryptedText);
        (string encrypted, string signature) EncryptWithSignature(string plainText);
        string DecryptWithSignature(string base64EncryptedText, string signature);
    }
}
