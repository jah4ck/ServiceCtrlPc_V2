using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Scheduler.Tools.Log
{
    class AD_Logger_Tools
    {
        # region "Partie GET /SET <> Variables"

        
        private static string _Log_File;
        private static string _Log_File_Alert;
        private static string _Log_File_Log;

        #endregion

        public AD_Logger_Tools()
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = CtrlPc_Service.Service_Culture_Info;
        }

        public static void Log_Write(string Log_Evt, string Log_Message)
        {
            Log_Write("", Log_Evt, Log_Message, false);
        }

        public static void Log_Write(string Log_Evt, string Log_Message, bool With_Log_STDOUT)
        {
            Log_Write("", Log_Evt, Log_Message, With_Log_STDOUT);
        }

        public static void Log_Write(string Log_GUID, string Log_Evt, string Log_Message, bool With_Log_STDOUT)
        {
            // Initialisation des <> Variables
            string _Log_Src = CtrlPc_Service.Service_Log.Get(CtrlPc_Service.Service_Log_List);

            string _Log_Entete_Begin = Environment.GetEnvironmentVariable("GuidScheduler", EnvironmentVariableTarget.Machine) + ";" + Environment.GetEnvironmentVariable("COMPUTERNAME")+";"+CtrlPc_Service.Version_Service + ";" + _Log_Src + ";" + DateTime.UtcNow.ToString();
            string _Log_Entete_End = " (localtime = " + DateTime.Now.ToString() + ")";

            string _Log_Alert_Entete_Begin = CtrlPc_Service.Service_Logger_Application + ";" + CtrlPc_Service.Service_Logger_Alert_Module + ";" + _Log_Src;

            Log_Culture_Info_Param();
            Log_File_Param(Log_GUID);

            // Check Message Length => Split en Lot de 3500 Caractères
            Log_Message = Log_Message_Bloc(Log_Message, 3500);

            // Print Message To Screen
            Log_Print(Log_Message, With_Log_STDOUT);

            # region "LOG / ALERT"

            if ((Log_Evt.ToUpper() == "LOG" || Log_Evt.ToUpper() == "ALERT") && Log_Size_Check(_Log_File_Log, "DATA") == true)
            {
                string _Log_File_Name = _Log_File_Log;

                if (Log_Evt.ToUpper() == "ALERT") { _Log_File_Name = _Log_File_Alert; }

                // Split Message en Ligne
                string[] _Log_Message_Split = Log_Message.Split(new string[] { System.Environment.NewLine, "\r", "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

                // Ecriture Log en Mode Append / Partagé
                using (StreamWriter _Log_File_Name_Writer = new StreamWriter(File.Open(_Log_File_Name, FileMode.Append, FileAccess.Write, FileShare.ReadWrite | FileShare.Delete)))
                {
                    foreach (string _Log_Message_Split_Line in _Log_Message_Split)
                    {
                        if (_Log_Message_Split_Line.Trim() != "" && Log_Size_Check(_Log_File_Log, "DATA") == true)
                        {
                            _Log_File_Name_Writer.WriteLine(_Log_Message_Split_Line.Trim() + ";" + DateTime.UtcNow.ToString());
                        }
                    }

                    _Log_File_Name_Writer.Close();
                    _Log_File_Name_Writer.Dispose();
                }
            }

            # endregion

            # region "INFO / WARN / ERROR"

            if ((Log_Evt.ToUpper() == "INFO" || Log_Evt.ToUpper().Contains("_INFO") || Log_Evt.ToUpper() == "WARN" || Log_Evt.ToUpper() == "ERROR") && (Log_Size_Check(_Log_File, "LOG") == true))
            {
                // Split Message en Ligne
                string[] _Log_Message_Split = Log_Message.Split(new string[] { System.Environment.NewLine, "\r", "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

                // Ecriture Log en Mode Append / Partagé
                using (StreamWriter _Log_File_Writer = new StreamWriter(File.Open(_Log_File, FileMode.Append, FileAccess.Write, FileShare.ReadWrite | FileShare.Delete)))
                {
                    foreach (string _Log_Message_Split_Line in _Log_Message_Split)
                    {
                        if (_Log_Message_Split_Line.Trim() != "" && Log_Size_Check(_Log_File, "LOG") == true)
                        {
                            _Log_File_Writer.WriteLine(_Log_Entete_Begin + ";" + Log_Evt.ToUpper() + ";" + _Log_Message_Split_Line.Trim().Replace(";", "") + _Log_Entete_End);
                        }
                    }

                    _Log_File_Writer.Close();
                    _Log_File_Writer.Dispose();
                }
            }

            #endregion

            
        }

        
        public static void Log_Write(string Log_Evt, Exception Exception, StackTrace StackTrace)
        {
            if (Exception.InnerException != null)
            {
                if (Exception.InnerException.Message != null) { Log_Write(Log_Evt.ToUpper().ToString(), "InnerException Message = " + Exception.InnerException.Message); }
            }

            Log_Write(Log_Evt.ToUpper().ToString(), "Détail de l'Erreur : " + System.Environment.NewLine + "Source = " + Exception.Source.ToString() + System.Environment.NewLine + "Message = " + Exception.Message.ToString() + System.Environment.NewLine + "StackTrace = " + Exception.StackTrace.ToString());

            for (int i = 0; i < StackTrace.FrameCount; i++)
            {
                StackFrame _StackTrace = StackTrace.GetFrame(i);
                MethodBase _StackTrace_MethodBase = _StackTrace.GetMethod();
                string k = _StackTrace_MethodBase.Name.ToString();
                string p = _StackTrace.GetFileLineNumber().ToString();
                string j = _StackTrace.GetFileName();
            }
        }

        public static void Log_Write_Unhandled_Exception(object sender, UnhandledExceptionEventArgs e)
        {
            Exception _Exception = (Exception)e.ExceptionObject;

            Log_Write("ERROR", "Unhandled Exception :" + System.Environment.NewLine + "Source = " + _Exception.Source.ToString() + System.Environment.NewLine + "Message = " + _Exception.Message.ToString() + System.Environment.NewLine + "StackTrace = " + _Exception.StackTrace.ToString());
        }

        private static void Log_Print(string Log_Message, bool With_Log_STDOUT)
        {
            if (With_Log_STDOUT == true)
            {
                Console.WriteLine(Log_Message);
            }
        }

        private static void Log_Culture_Info_Param()
        {
            // Initialisation du Format Date / Heure pour les Logs
            CtrlPc_Service.Service_Culture_Info = new CultureInfo("en-US");
            CtrlPc_Service.Service_Culture_Info.DateTimeFormat.DateSeparator = "-";
            CtrlPc_Service.Service_Culture_Info.DateTimeFormat.ShortDatePattern = "yyyy-MM-dd";
            CtrlPc_Service.Service_Culture_Info.DateTimeFormat.LongTimePattern = "HH:mm:ss";

            System.Threading.Thread.CurrentThread.CurrentCulture = CtrlPc_Service.Service_Culture_Info;
        }

        private static void Log_File_Param(string Log_GUID)
        {
            try
            {
                string _Log_Dir = (Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase).Replace(@"file:\", "")).ToUpper().Replace("\\BIN", "\\LOGS");
                string _Data_Dir = (Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase).Replace(@"file:\", "")).ToUpper().Replace("\\BIN", "\\DATA");

                string _Log_File_Base = (new FileInfo(Assembly.GetEntryAssembly().GetName().CodeBase.ToString().Replace("file:///", "")).Name.ToUpper().Replace(".EXE", "").Replace(".DLL", "").ToString());

                // Initialisation <> Variables
                if (CtrlPc_Service.AD_Dir_Logs != null) { _Log_Dir = CtrlPc_Service.AD_Dir_Logs; }
                if (CtrlPc_Service.AD_Dir_Data != null) { _Data_Dir = CtrlPc_Service.AD_Dir_Data; }

                // Création Dir si Inexistant
                if (System.IO.Directory.Exists(_Log_Dir) == false)
                {
                    System.IO.Directory.CreateDirectory(_Log_Dir);
                }

                if (System.IO.Directory.Exists(_Data_Dir) == false)
                {
                    System.IO.Directory.CreateDirectory(_Data_Dir);
                }

                // Init. Log_File
                _Log_File = (_Log_Dir + "\\Log_" + _Log_File_Base + "_" + Log_GUID + "_" + DateTime.UtcNow.ToString("yyyyMMdd") + ".Log").Replace("__", "_");

                // Init. Log_File_Alert
                _Log_File_Alert = (_Data_Dir + "\\Alert_" + _Log_File_Base + "_" + Log_GUID + "_" + DateTime.UtcNow.ToString("yyyyMMdd_HH") + ".Log").Replace("__", "_");

                // Init. Log_File_Log
                _Log_File_Log = (_Data_Dir + "\\Log_" + _Log_File_Base + "_" + Log_GUID + "_" + DateTime.UtcNow.ToString("yyyyMMdd_HH") + ".Log").Replace("__", "_");
            }
            catch (Exception _Exception)
            {
                Log_Write("ERROR", _Exception, new StackTrace(true));
            }
        }

        private static bool Log_Size_Check(string Log_File, string Log_Type)
        {
            long _Log_Size_Max = 0;

            if (Log_Type.ToUpper() == "LOG") { _Log_Size_Max = CtrlPc_Service.Service_Security_Mode_Log_Size_Max; }
            if (Log_Type.ToUpper() == "DATA") { _Log_Size_Max = CtrlPc_Service.Service_Security_Mode_Log_Data_Size_Max; }

            FileInfo _Log_File_Info = new FileInfo(Log_File);

            if (_Log_File_Info.Exists == true)
            {
                if (_Log_File_Info.Length < _Log_Size_Max)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return true;
            }
        }

        public static string Log_Message_Bloc(string Log_Message, Int32 Message_Bloc_Length)
        {
            string _Log_Message_New = "";

            string[] _Log_Message_Split = Log_Message.Split(new string[] { System.Environment.NewLine, "\r", "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string _Log_Message_Split_Line in _Log_Message_Split)
            {
                string _Log_Message_Line = _Log_Message_Split_Line.Trim();

                Int32 _Log_Message_New_Nb_Bloc = _Log_Message_Line.Length / Message_Bloc_Length;

                for (int _Bloc = 0; _Bloc <= _Log_Message_New_Nb_Bloc; _Bloc++)
                {
                    string _Log_Message_New_Bloc = "";

                    if (_Bloc != _Log_Message_New_Nb_Bloc)
                    {
                        _Log_Message_New_Bloc = _Log_Message_Line.Substring(_Bloc * Message_Bloc_Length, Message_Bloc_Length);
                    }
                    else
                    {
                        _Log_Message_New_Bloc = _Log_Message_Line.Substring(_Bloc * Message_Bloc_Length, _Log_Message_Line.Length - (_Bloc * Message_Bloc_Length));
                    }

                    if (_Log_Message_New == "")
                    {
                        _Log_Message_New = _Log_Message_New_Bloc;
                    }
                    else
                    {
                        _Log_Message_New = _Log_Message_New + System.Environment.NewLine + _Log_Message_New_Bloc;
                    }
                }
            }

            return _Log_Message_New;
        }
    }
}
