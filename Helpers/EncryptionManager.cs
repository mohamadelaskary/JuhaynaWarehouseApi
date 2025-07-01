using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace GBSWarehouse
{
    public class EncryptionManager
    {
        const string Password = "gdsd3!2gNgb6#";
        #region Decrypt
        public static string Decrypt(string msg)
        {
            RijndaelManaged rijndaelCipherText = new RijndaelManaged();
            msg = msg.Replace("-12sw-", "+");
            var cipherText = Convert.FromBase64String(msg.Trim());
            var rijndaelIv = new byte[16];
            var rijndaelKey = new byte[16];

            GenrateKeyIv(ref rijndaelKey, ref rijndaelIv, Password);
            var decryptor = rijndaelCipherText.CreateDecryptor(rijndaelKey, rijndaelIv);
            var mSdecrypt = new MemoryStream(cipherText);
            var cSdecrypt = new CryptoStream(mSdecrypt, decryptor, CryptoStreamMode.Read);

            var plainTextBytes = new byte[cipherText.Length];
            var decryptedCount = cSdecrypt.Read(plainTextBytes, 0, plainTextBytes.Length);

            mSdecrypt.Close();
            cSdecrypt.Close();
            var plainText = Encoding.Unicode.GetString(plainTextBytes, 0, decryptedCount);
            return plainText;

        }
        #endregion //Decrypt

        #region GenrateKeyIV
        private static void GenrateKeyIv(ref byte[] rijndaelKey, ref byte[] rijndaelIv, string password)
        {
            var encryptedRijndaelKey = new byte[16];
            var salt = Encoding.Unicode.GetBytes(password);

            var psdDrvByt = new PasswordDeriveBytes(password, salt);
            //Derives Rijndael key from a password and salt
            rijndaelKey = psdDrvByt.CryptDeriveKey("RC2", "MD5", 128, new byte[8]);
            rijndaelIv = psdDrvByt.GetBytes(16);

            var classId = Encoding.Unicode.GetBytes("DE96C164-2CF5-484d-A7C6-7758FEDBCE8C"); ;

            for (int i = 0; i < rijndaelKey.Length; i = i + 2)
            {
                encryptedRijndaelKey[i] = rijndaelKey[i];
                encryptedRijndaelKey[i + 1] = classId[i];
            }
            rijndaelKey = encryptedRijndaelKey;
        }
        #endregion //GenrateKeyIV

        #region Encrypt
        public static string Encrypt(string msg)
        {
            var rijndaelPlainText = new RijndaelManaged();
            var plainText = Encoding.Unicode.GetBytes(msg);

            var rijndaelIv = new byte[16];
            var rijndaelKey = new byte[16];


            GenrateKeyIv(ref rijndaelKey, ref rijndaelIv, Password);
            ICryptoTransform encryptor = rijndaelPlainText.CreateEncryptor(rijndaelKey, rijndaelIv);
            MemoryStream mSencrypt = new MemoryStream();
            CryptoStream cSencrypt = new CryptoStream(mSencrypt, encryptor, CryptoStreamMode.Write);

            cSencrypt.Write(plainText, 0, plainText.Length);
            cSencrypt.FlushFinalBlock();

            var cipherTextBytes = mSencrypt.ToArray();
            mSencrypt.Close();
            cSencrypt.Close();
            var cipherText = Convert.ToBase64String(cipherTextBytes);
            cipherText = cipherText.Replace("+", "-12sw-");
            return cipherText;
        }
        #endregion //Encrypt
    }
}