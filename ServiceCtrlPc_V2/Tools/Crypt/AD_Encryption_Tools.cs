using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Scheduler.Tools.Crypt
{
    class AD_Encryption_Tools
    {
        private static readonly byte[] _Encryption_Default_Key = new byte[] { 34, 77, 23, 23, 43, 73, 23, 43, 2, 64, 23, 67, 34, 12, 32, 5, 3, 64, 75, 23, 54, 23, 12, 52 };
        private static readonly byte[] _Encryption_Default_IV = new byte[] { 2, 45, 123, 22, 66, 234, 45, 34 };

        public static byte[] Encryption_Default_Key
        {
            get { return _Encryption_Default_Key; }
        }

        public static byte[] Encryption_Default_IV
        {
            get { return _Encryption_Default_IV; }
        }

        public static string Encrypt_String(string String_To_Encrypt, byte[] Encryption_Key, byte[] Encryption_IV)
        {
            byte[] _Byte_Encrypt = null;

            try
            {
                MemoryStream _MemoryStream = new MemoryStream();
                CryptoStream _CryptoStream = new CryptoStream(_MemoryStream, new TripleDESCryptoServiceProvider().CreateEncryptor(Encryption_Key, Encryption_IV), CryptoStreamMode.Write);

                byte[] _String_To_Encrypt = new ASCIIEncoding().GetBytes(String_To_Encrypt);

                _CryptoStream.Write(_String_To_Encrypt, 0, _String_To_Encrypt.Length);
                _CryptoStream.FlushFinalBlock();

                _Byte_Encrypt = _MemoryStream.ToArray();

                _CryptoStream.Close();
                _MemoryStream.Close();
            }
            catch (Exception _Exception)
            {
                Tools.Log.AD_Logger_Tools.Log_Write("ERROR", _Exception, new StackTrace(true));
            }

            return Convert.ToBase64String(_Byte_Encrypt);
        }

        public static string Decrypt_String(string String_To_Decrypt, byte[] Encryption_Key, byte[] Encryption_IV)
        {
            byte[] _Byte_Decrypt = null;

            try
            {
                byte[] _Byte_To_Decrypt = Convert.FromBase64String(String_To_Decrypt);

                MemoryStream _MemoryStream = new MemoryStream(_Byte_To_Decrypt);
                CryptoStream _CryptoStream = new CryptoStream(_MemoryStream, new TripleDESCryptoServiceProvider().CreateDecryptor(Encryption_Key, Encryption_IV), CryptoStreamMode.Read);

                _Byte_Decrypt = new byte[_Byte_To_Decrypt.Length];

                _CryptoStream.Read(_Byte_Decrypt, 0, _Byte_Decrypt.Length);
            }
            catch (Exception _Exception)
            {
                Tools.Log.AD_Logger_Tools.Log_Write("ERROR", _Exception, new StackTrace(true));
            }

            return new ASCIIEncoding().GetString(_Byte_Decrypt);
        }

        public static void Encrypt_File(byte[] bKey, byte[] bIv, string pathPlainTextFile, string pathCypheredTextFile)
        {
            // Place la clé de déchiffrement dans un tableau d'octets
            //byte[] key = Encoding.UTF8.GetBytes(strKey);

            // Place le vecteur d'initialisation dans un tableau d'octets
            //byte[] iv = Encoding.UTF8.GetBytes(strIv);

            FileStream fsCypheredFile = new FileStream(pathCypheredTextFile, FileMode.Create);

            RijndaelManaged rijndael = new RijndaelManaged();
            rijndael.Mode = CipherMode.CBC;
            rijndael.Key = bKey;
            rijndael.IV = bIv;

            ICryptoTransform aesEncryptor = rijndael.CreateEncryptor();

            CryptoStream cs = new CryptoStream(fsCypheredFile, aesEncryptor, CryptoStreamMode.Write);

            FileStream fsPlainTextFile = new FileStream(pathPlainTextFile, FileMode.OpenOrCreate);

            int data;

            while ((data = fsPlainTextFile.ReadByte()) != -1)
            {
                cs.WriteByte((byte)data);
            }

            fsPlainTextFile.Close();
            fsPlainTextFile.Dispose();
            cs.Close();
            cs.Dispose();
            fsCypheredFile.Close();
            fsCypheredFile.Dispose();
        }

        public static void Decrypt_File(byte[] bKey, byte[] bIv, string pathCypheredTextFile, string pathPlainTextFile)
        {
            // Place la clé de déchiffrement dans un tableau d'octets
            //byte[] key = Encoding.UTF8.GetBytes(strKey);

            // Place le vecteur d'initialisation dans un tableau d'octets
            //byte[] iv = Encoding.UTF8.GetBytes(strIv);

            // Filestream of the new file that will be decrypted.   
            FileStream fsCrypt = new FileStream(pathPlainTextFile, FileMode.Create);

            RijndaelManaged rijndael = new RijndaelManaged();
            rijndael.Mode = CipherMode.CBC;
            rijndael.Key = bKey;
            rijndael.IV = bIv;

            ICryptoTransform aesDecryptor = rijndael.CreateDecryptor();

            CryptoStream cs = new CryptoStream(fsCrypt, aesDecryptor, CryptoStreamMode.Write);

            // FileStream of the file that is currently encrypted.    
            FileStream fsIn = new FileStream(pathCypheredTextFile, FileMode.OpenOrCreate);

            int data;

            while ((data = fsIn.ReadByte()) != -1)
                cs.WriteByte((byte)data);
            cs.Close();
            cs.Dispose();
            fsIn.Close();
            fsIn.Dispose();
            fsCrypt.Close();
            fsCrypt.Dispose();
        }
    }
}
