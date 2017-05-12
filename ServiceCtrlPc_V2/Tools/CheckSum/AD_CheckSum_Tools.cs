using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Scheduler.Tools.CheckSum
{
    class AD_CheckSum_Tools
    {
        public static string GetFile_CheckSum(string HashType, string File_To_Check)
        {
            string _File_CheckSum = null;
            System.Security.Cryptography.MD5CryptoServiceProvider _Md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            System.Security.Cryptography.SHA1CryptoServiceProvider _Sha1 = new System.Security.Cryptography.SHA1CryptoServiceProvider();
            System.Text.StringBuilder _Hash = new System.Text.StringBuilder();

            try
            {
                if (System.IO.File.Exists(File_To_Check))
                {
                    FileStream _FileStream = new FileStream(File_To_Check, FileMode.Open, FileAccess.Read, FileShare.Read, 8192);

                    switch (HashType.ToUpper())
                    {
                        case "MD5":
                            _Md5.ComputeHash(_FileStream);

                            foreach (byte a in _Md5.Hash)
                            {
                                _Hash.Append(a.ToString("x2"));
                            }

                            break;

                        case "SHA1":
                            _Sha1.ComputeHash(_FileStream);

                            foreach (byte a in _Sha1.Hash)
                            {
                                _Hash.Append(a.ToString("x2"));
                            }

                            break;
                    }

                    _FileStream.Close();
                    _FileStream.Dispose();
                    _FileStream = null;

                    _File_CheckSum = _Hash.ToString();

                    Tools.Log.AD_Logger_Tools.Log_Write("INFO", "CheckSum File " + File_To_Check + " = " + _File_CheckSum + " pour HashType = " + HashType + "");

                    return _File_CheckSum.ToLower();
                }
                else
                {
                    Tools.Log.AD_Logger_Tools.Log_Write("ERROR", "File " + File_To_Check + " Inexistant !!!");
                }
            }
            catch (Exception _Exception)
            {
                Tools.Log.AD_Logger_Tools.Log_Write("ERROR", _Exception, new StackTrace(true));
            }
            finally
            {
                _Md5.Clear();
                _Sha1.Clear();

                _Md5 = null;
                _Sha1 = null;

                _Hash.Remove(0, _Hash.Length);
                _Hash = null;
            }

            return _File_CheckSum;
        }

        public static bool IsValid_File_CheckSum(string File_To_Check)
        {
            bool _CheckSum_Return = false;

            try
            {
                Tools.Log.AD_Logger_Tools.Log_Write("INFO", "Contrôle CheckSum File " + File_To_Check + " ...");

                FileInfo _File_To_Check_FileInfo = new FileInfo(File_To_Check);

                string[] _File_To_Check_Split = File_To_Check.Split(new string[] { "_" }, StringSplitOptions.None);

                string _File_To_Check_CheckSum = _File_To_Check_Split[_File_To_Check_Split.Length - 1].ToString().ToUpper().Replace(_File_To_Check_FileInfo.Extension.ToUpper(), "");

                Tools.Log.AD_Logger_Tools.Log_Write("INFO", "Valeur CheckSum Récupéré = " + _File_To_Check_CheckSum + " dans File " + File_To_Check);

                if (_File_To_Check_CheckSum.Length == 32)
                {
                    if (_File_To_Check_CheckSum == Tools.CheckSum.AD_CheckSum_Tools.GetFile_CheckSum("MD5", File_To_Check).ToUpper())
                    {
                        Tools.Log.AD_Logger_Tools.Log_Write("INFO", "CheckSum File " + File_To_Check + " Is Valid");

                        _CheckSum_Return = true;
                    }
                    else
                    {
                        Tools.Log.AD_Logger_Tools.Log_Write("ERROR", "CheckSum File " + File_To_Check + " Is Not Valid");
                    }
                }
                else
                {
                    Tools.Log.AD_Logger_Tools.Log_Write("INFO", "Valeur CheckSum Récupéré = " + _File_To_Check_CheckSum + " Incorrecte !!!");
                }

                _File_To_Check_FileInfo = null;
            }
            catch (Exception _Exception)
            {
                Tools.Log.AD_Logger_Tools.Log_Write("ERROR", _Exception, new StackTrace(true));
            }

            return _CheckSum_Return;
        }

        public static string GetString_CheckSum(string HashType, string String_Src)
        {
            string _String_CheckSum = null;
            System.Security.Cryptography.MD5CryptoServiceProvider _Md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            System.Security.Cryptography.SHA1CryptoServiceProvider _Sha1 = new System.Security.Cryptography.SHA1CryptoServiceProvider();
            System.Text.StringBuilder _Hash = new System.Text.StringBuilder();

            try
            {
                byte[] _Input_Bytes = System.Text.Encoding.ASCII.GetBytes(String_Src);

                switch (HashType.ToUpper())
                {
                    case "MD5":
                        _Md5.ComputeHash(_Input_Bytes);

                        foreach (byte a in _Md5.Hash)
                        {
                            _Hash.Append(a.ToString("x2"));
                        }

                        break;

                    case "SHA1":
                        _Sha1.ComputeHash(_Input_Bytes);

                        foreach (byte a in _Sha1.Hash)
                        {
                            _Hash.Append(a.ToString("x2"));
                        }

                        break;
                }

                _String_CheckSum = _Hash.ToString();
                _Input_Bytes = null;

                return _String_CheckSum.ToLower();
            }
            catch (Exception _Exception)
            {
                Tools.Log.AD_Logger_Tools.Log_Write("ERROR", _Exception, new StackTrace(true));
            }
            finally
            {
                _Md5.Clear();
                _Sha1.Clear();

                _Md5 = null;
                _Sha1 = null;

                _Hash.Remove(0, _Hash.Length);
                _Hash = null;
            }

            return _String_CheckSum;
        }
    }
}
