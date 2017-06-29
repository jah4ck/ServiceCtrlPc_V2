using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CtrlPc_Write_Trace
{
    public class AD_Write_Trace
    {
        private static string path= @"C:\ProgramData\CtrlPc";

        public static string Write_Trace(string _eventLog,string _statut, string _application, string _module, string _dateKey, string _data, string _dateTraitement)
        {
            if (_eventLog.ToUpper() == "LOG" || _eventLog.ToUpper() == "ALERT" || _eventLog.ToUpper() == "DATA")
            {

                string guidStation = Registry.GetValue(@"HKEY_USERS\.DEFAULT\Software\CtrlPc\Version", "GUID", "123456789ABCDEF").ToString();
                string _Path_File = path + "\\" + _eventLog.ToUpper();
                if (!Directory.Exists(_Path_File))
                {
                    Directory.CreateDirectory(_Path_File);
                }
                
                string _Log_File_Name = _Path_File+"\\"+Guid.NewGuid().ToString().ToUpper()+".txt";
                string _entete = guidStation + "|" + _application + "|" + _module + "|" + _dateKey+"|"+_statut;
                // Split Message en Ligne
                string[] _Log_Message_Split = _data.Split(new string[] { System.Environment.NewLine, "\r", "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

                // Ecriture Log en Mode Append / Partagé
                using (StreamWriter _Log_File_Name_Writer = new StreamWriter(File.Open(_Log_File_Name, FileMode.Append, FileAccess.Write, FileShare.ReadWrite | FileShare.Delete)))
                {
                    foreach (string _Log_Message_Split_Line in _Log_Message_Split)
                    {
                        if (_Log_Message_Split_Line.Trim() != "")
                        {
                            _Log_File_Name_Writer.WriteLine(_entete+"|"+ _Log_Message_Split_Line+"|"+_dateTraitement);
                        }
                    }

                    _Log_File_Name_Writer.Close();
                    _Log_File_Name_Writer.Dispose();
                }
                return _Log_File_Name;
            }
            else
            {
                return "N/A";
            }
        }
    }
}
