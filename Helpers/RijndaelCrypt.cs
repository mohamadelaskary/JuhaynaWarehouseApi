using System;
using System.Security.Cryptography;
using System.Text;

namespace GBSWarehouse
{
    public class RijndaelCrypt
    {
        #region Private/Protected Member Variables
        private readonly ICryptoTransform _decryptor;
        private readonly ICryptoTransform _encryptor;
        private readonly byte[] IV = Encoding.UTF8.GetBytes("gdzr3!2gNga,mj5#");
        private readonly byte[] _password;
        private readonly RijndaelManaged _cipher;
        #endregion

        #region Private/Protected Properties
        public ICryptoTransform Decryptor { get { return _decryptor; } }
        private ICryptoTransform Encryptor { get { return _encryptor; } }
        #endregion
        #region Constructor
        public RijndaelCrypt(string password = "gdzr3!2gNga,mj5#")
        {
            //Encode digest
            MD5CryptoServiceProvider md5 = new();
            _password = md5.ComputeHash(Encoding.ASCII.GetBytes(password));

            //Initialize objects
            _cipher = new RijndaelManaged();
            _decryptor = _cipher.CreateDecryptor(_password, IV);
            _encryptor = _cipher.CreateEncryptor(_password, IV);

        }
        #endregion
        #region Public Methods
        public string Decrypt(string text)
        {
            try
            {
                byte[] input = Convert.FromBase64String(text);

                var newClearData = Decryptor.TransformFinalBlock(input, 0, input.Length);
                return Encoding.ASCII.GetString(newClearData);
            }
            catch (ArgumentException ae)
            {
                Console.WriteLine("inputCount uses an invalid value or inputBuffer has an invalid offset length. " + ae);
                return null;
            }
            catch (ObjectDisposedException oe)
            {
                Console.WriteLine("The object has already been disposed." + oe);
                return null;
            }


        }
        public string Encrypt(string text)
        {
            try
            {
                var buffer = Encoding.ASCII.GetBytes(text);
                return Convert.ToBase64String(Encryptor.TransformFinalBlock(buffer, 0, buffer.Length));
            }
            catch (ArgumentException ae)
            {
                Console.WriteLine("inputCount uses an invalid value or inputBuffer has an invalid offset length. " + ae);
                return null;
            }
            catch (ObjectDisposedException oe)
            {
                Console.WriteLine("The object has already been disposed." + oe);
                return null;
            }
        }
        #endregion
    }
}