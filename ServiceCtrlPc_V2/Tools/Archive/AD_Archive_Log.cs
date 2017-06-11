using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Scheduler.Tools.Archive
{
    public class AD_Archive_Log
    {
        public static void Archive_Log()
        {
            string _Log_Dir = (Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase).Replace(@"file:\", "")).ToUpper().Replace("\\BIN", "\\LOGS");
            string _Script_Dir = (Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase).Replace(@"file:\", "")).ToUpper().Replace("\\BIN", "\\SCRIPT");
            string _Archive_Dir = (Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase).Replace(@"file:\", "")).ToUpper().Replace("\\BIN", "\\ARCHIVE");
            string _ArchiveTMP_Dir = (Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase).Replace(@"file:\", "")).ToUpper().Replace("\\BIN", "\\ARCHIVE\\TMP");
            string _Log_File_Base = (new FileInfo(Assembly.GetEntryAssembly().GetName().CodeBase.ToString().Replace("file:///", "")).Name.ToUpper().Replace(".EXE", "").Replace(".DLL", "").ToString());
            string _Log_File = ("Log_" + _Log_File_Base + "_").Replace("__", "_");
            string[] lstFileLog = Directory.GetFiles(_Log_Dir, "*" + _Log_File +"*");
            string[] lstArchive = Directory.GetFiles(_Archive_Dir,"*.zip");
            //string _Command_Archive = _Script_Dir + "\\7z.exe u -r " + _Archive_Dir + "\\Archive_" + DateTime.Now.ToString("yyyyMMdd") + ".zip " + _ArchiveTMP_Dir + "\\*";

            Tools.Log.AD_Logger_Tools.Log_Write("INFO", "Vérification des temps de concervation des archives");
            Tools.Log.AD_Logger_Tools.Log_Write("INFO", "Nombre d'archive possédées : " + lstArchive.Length);

            if (lstArchive.Length>1)
            {
                foreach (string archive in lstArchive)
                {
                    Tools.Log.AD_Logger_Tools.Log_Write("INFO", "Archive : "+ archive);
                    FileInfo infoArchive = new FileInfo(archive);
                    TimeSpan timerArchive = DateTime.Now.ToUniversalTime() - infoArchive.LastWriteTimeUtc;
                    Tools.Log.AD_Logger_Tools.Log_Write("INFO", "Dernière écriture : " + infoArchive.LastWriteTimeUtc);
                    if (timerArchive.TotalDays>CtrlPc_Service.Time_Flag_Histo_Archive)
                    {
                        Tools.Log.AD_Logger_Tools.Log_Write("INFO", "Suppression de l'archive : " + archive);
                        File.Delete(archive);
                    }
                }
            }

            if (lstFileLog.Length>1)
            {
                Tools.Log.AD_Logger_Tools.Log_Write("INFO", "Il y a " +lstFileLog.Length + " fichier susceptible d'être archivés");
                foreach (string _File_Log in lstFileLog)
                {
                    string date = _File_Log.Replace(_Log_Dir + "\\" + _Log_File, "").Replace(".Log", "");
                    DateTime date_File;
                    bool tryconvertDate = DateTime.TryParseExact(date, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out date_File);
                    if (tryconvertDate)
                    {
                        if (date_File<DateTime.Now.ToUniversalTime().AddHours(-24))
                        {
                            Tools.Log.AD_Logger_Tools.Log_Write("INFO", "Ce fichier doit être archivé : "+_File_Log);
                            if (!Directory.Exists(_ArchiveTMP_Dir))
                            {
                                Directory.CreateDirectory(_ArchiveTMP_Dir);
                            }
                            if (Directory.Exists(_ArchiveTMP_Dir))
                            {
                                Tools.Log.AD_Logger_Tools.Log_Write("INFO", "Déplacement du fichier "+_File_Log+" vers "+ _ArchiveTMP_Dir + "\\" + _Log_File + date + ".Log");
                                File.Move(_File_Log, _ArchiveTMP_Dir + "\\"+_Log_File+date+".Log");
                            }
                        }
                    }
                    else
                    {
                        Tools.Log.AD_Logger_Tools.Log_Write("ERROR", "Conversion de la date impossible : "+ date);
                    }
                }
                //Archivage
                Tools.Log.AD_Logger_Tools.Log_Write("INFO", "Début de l'archivage des fichiers");
                if (File.Exists(_Script_Dir + "\\7z.exe"))
                {
                    Tools.Process.AD_Process_Tools _AD_Process_Tools = new Tools.Process.AD_Process_Tools();
                    _AD_Process_Tools.Process_Exec(_Script_Dir + "\\7z.exe", " u -r " + _Archive_Dir + "\\Archive_" + DateTime.Now.ToString("yyyyMMdd") + ".zip " + _ArchiveTMP_Dir + "\\*", null, null, 0, 300000, 10000, true, true, true, true);
                    string md5 = Tools.CheckSum.AD_CheckSum_Tools.GetFile_CheckSum("MD5", _Archive_Dir + "\\Archive_" + DateTime.Now.ToString("yyyyMMdd") + ".zip ");
                    File.Move(_Archive_Dir + "\\Archive_" + DateTime.Now.ToString("yyyyMMdd") + ".zip ", _Archive_Dir + "\\Archive_"+md5+"_"+ DateTime.Now.ToString("yyyyMMdd") + ".zip ");
                    Tools.Log.AD_Logger_Tools.Log_Write("INFO", "Code Retour Archive = " + _AD_Process_Tools.Process_Exec_ExitCode);
                    if (_AD_Process_Tools.Process_Exec_ExitCode == 0 || _AD_Process_Tools.Process_Exec_ExitCode == 1000)
                    {
                        Tools.Log.AD_Logger_Tools.Log_Write("INFO", "Archivage réussi");
                        Tools.Log.AD_Logger_Tools.Log_Write("INFO", "Suppression des fichiers de log");
                        string[] file_Log_Archive_TMP = Directory.GetFiles(_ArchiveTMP_Dir);
                        foreach (string _File_To_Delete in file_Log_Archive_TMP)
                        {
                            Tools.Log.AD_Logger_Tools.Log_Write("INFO", "Suppression : "+_File_To_Delete);
                            File.Delete(_File_To_Delete);
                        }
                    }
                    else
                    {
                        Tools.Log.AD_Logger_Tools.Log_Write("ERROR", "Problème lors de l'archivage !!! Code Erreur : "+ _AD_Process_Tools.Process_Exec_ExitCode);
                    }

                    _AD_Process_Tools.Process_Exec_Dispose();
                    _AD_Process_Tools = null;
                }
                else
                {
                    Tools.Log.AD_Logger_Tools.Log_Write("ERROR", "Archivage impossible, 7Zip est introuvable : "+ _Script_Dir + "\\7z.exe");
                }
                Tools.Log.AD_Logger_Tools.Log_Write("INFO", "Fin de l'archivage des fichiers");

            }
            //déplacement des fichiers de log

        }
    }
}
