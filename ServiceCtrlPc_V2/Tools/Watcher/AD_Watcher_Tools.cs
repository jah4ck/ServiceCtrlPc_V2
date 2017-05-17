using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading;

namespace Scheduler.Tools.Watcher
{
    public class AD_Watcher_Tools
    {
        //voir dans une autre branche pk ça plante en attendant on scan le répertoire toutes les 15s
        private List<FileSystemWatcher> listFileSystemWatcher;

        private static FileSystemWatcher fileWatcher = null;
        [PermissionSet(SecurityAction.Demand, Name="FullTrust")]
        public AD_Watcher_Tools(string _directory)
        {
            
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(Tools.Log.AD_Logger_Tools.Log_Write_Unhandled_Exception);
            try
            {
                Tools.Log.AD_Logger_Tools.Log_Write("INFO", "Initialisation du watcher pour le répertoire : "+_directory);
                StartFileSystemWatcher(_directory);
                Tools.Log.AD_Logger_Tools.Log_Write("INFO", "Initialisation du watcher terminé pour le répertoire : " + _directory);
            }
            catch (Exception _err)
            {
                Tools.Log.AD_Logger_Tools.Log_Write("ERROR", _err, new StackTrace(true));
            }
        }
        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        private void StartFileSystemWatcher(string _directory)
        {
            //this.listFileSystemWatcher = new List<FileSystemWatcher>();
            //DirectoryInfo dir = new DirectoryInfo(_directory);
            //if (dir.Exists)
            //{
            //    //FileSystemWatcher fileWatcher = new FileSystemWatcher();
            //    fileWatcher = new FileSystemWatcher()
            //    {
            //        Path = dir.FullName,
            //        NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.CreationTime,
            //        Filter = "*.*"
            //    };
            //    //fileWatcher.Filter = "*.exe";
            //    // fileWatcher.Path = _directory;
            //    //fileWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.CreationTime;

            //    //fileWatcher.Changed += new FileSystemEventHandler(newFile);
            //    fileWatcher.Changed += (senderObj, fileSysArgs) => newFile(senderObj, fileSysArgs);
            //    fileWatcher.Created += (senderObj, fileSysArgs) => newFile(senderObj, fileSysArgs);
            //    fileWatcher.Disposed += (senderObj, fileSysArgs) => newFileEvent(senderObj, fileSysArgs);
            //    fileWatcher.Error += (senderObj, fileSysArgs) => newFileerr(senderObj,fileSysArgs);
            //    fileWatcher.Deleted += (senderObj, fileSysArgs) => newFile(senderObj, fileSysArgs);
            //    fileWatcher.Renamed += (senderObj, fileSysArgs) => newFile(senderObj, fileSysArgs);
            //    fileWatcher.EnableRaisingEvents = true;
            //    listFileSystemWatcher.Add(fileWatcher);

            //}

            fileWatcher = new FileSystemWatcher();
            fileWatcher.Path = @"C:\ProgramData\CtrlPc\UPDATE\";
            fileWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.CreationTime | NotifyFilters.FileName;
            fileWatcher.Created += new FileSystemEventHandler(OnChanged);

            fileWatcher.EnableRaisingEvents = true;

        }
        private static void OnChanged(object source, FileSystemEventArgs e)
        {

            fileWatcher.EnableRaisingEvents = false;
            FileInfo newFile = new FileInfo(e.FullPath);
            long sizefirst = newFile.Length;
            Thread.Sleep(2000);
            while (sizefirst != newFile.Length)
            {
                sizefirst = newFile.Length;

                Thread.Sleep(2000);
            }
            if (sizefirst == newFile.Length)
            {
                Thread.Sleep(1000);
                try
                {
                    File.Delete(e.FullPath);
                }
                catch (Exception err)
                {
                    StreamWriter toto = new StreamWriter(@"C:\TEMP\LOGERR_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".log");
                    toto.WriteLine(err.Message);
                    toto.Close();
                }
                
            }
            
            Thread.Sleep(1000);
            fileWatcher.EnableRaisingEvents = true;

        }
        public void newFileEvent(object source, EventArgs e)
        {
            Tools.Log.AD_Logger_Tools.Log_Write("INFO", "event arg");
            Tools.Log.AD_Logger_Tools.Log_Write("INFO", e.ToString());
        }
        public void newFileerr(object source, ErrorEventArgs e)
        {
            Tools.Log.AD_Logger_Tools.Log_Write("INFO", "errorevent");
            Tools.Log.AD_Logger_Tools.Log_Write("INFO", e.ToString());
        }
        public void newFile(object source, FileSystemEventArgs e)
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(Tools.Log.AD_Logger_Tools.Log_Write_Unhandled_Exception);
            try
            {
                Tools.Log.AD_Logger_Tools.Log_Write("INFO", "Déclenchement de l'évènement AD_Watcher_Tools");
                Tools.Log.AD_Logger_Tools.Log_Write("INFO", "Présence fichier : " + e.FullPath + "\t" + e.ChangeType);
                foreach (string file in Directory.GetFiles(@"C:\ProgramData\CtrlPc\UPDATE"))
                {
                    File.Delete(file);
                }
                Service.Update.AD_Update_Service_Task.Update_Service_Task();
            }
            catch (Exception _err)
            {
                File.Create(@"C:\TEMP\ERR.FLG");
                Tools.Log.AD_Logger_Tools.Log_Write("ERROR", _err, new StackTrace(true));
            }
            
        }
    }
}
