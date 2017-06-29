using Scheduler;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Scheduler.Service.Thread.Watcher
{
    class AD_Thread_Watcher
    {
        private static string _Log_Src_Thread_Watcher;
        private static string _Log_Src_Thread_Alert;
        private static string __Log_Src_Thread_Watcher_Guid;

        public void Execute()
        {
            __Log_Src_Thread_Watcher_Guid = System.Guid.NewGuid().ToString();
            _Log_Src_Thread_Watcher = CtrlPc_Service.Service_Log.Update(CtrlPc_Service.Service_Log_List, System.Threading.Thread.CurrentThread.Name.ToUpper(), "Watcher_" + __Log_Src_Thread_Watcher_Guid, true);

            Scheduler.Tools.Log.AD_Logger_Tools.Log_Write("INFO", "Début Thread Watcher : Service_State = " + CtrlPc_Service.Service_State.ToUpper() + " / Service_Thread_Watcher_State = " + CtrlPc_Service.Service_Thread_Watcher_State.ToUpper());

            try
            {
                if (CtrlPc_Service.Service_Thread_Watcher_Running == false)
                {
                    CtrlPc_Service.Service_Thread_Watcher_Running = true;

                    if (CtrlPc_Service.Service_State.ToUpper() == "STARTED" || CtrlPc_Service.Service_Thread_Watcher_State.ToUpper() == "ON")
                    {
                        Watcher_Talk();
                    }

                    CtrlPc_Service.Service_Thread_Watcher_State = CtrlPc_Service.Service_Thread_Watcher_State.ToUpper().Replace("WAIT_", "");
                }
                else
                {
                    Tools.Log.AD_Logger_Tools.Log_Write("ERROR", "Traitement Watcher Déjà en Cours d'Exécution !!!");
                }
            }
            catch (Exception _Exception)
            {
                Tools.Log.AD_Logger_Tools.Log_Write("ERROR", _Exception, new StackTrace(true));
            }
            finally
            {
                CtrlPc_Service.Service_Thread_Watcher_Running = false;
                Tools.Log.AD_Logger_Tools.Log_Write("INFO", "Fin Thread Watcher : Service_State = " + CtrlPc_Service.Service_State.ToUpper() + " / Service_Thread_Watcher_State = " + CtrlPc_Service.Service_Thread_Watcher_State.ToUpper());
            }
        }
        public void Watcher_Talk()
        {
            Tools.Log.AD_Logger_Tools.Log_Write("INFO", "Exécution Thread Watcher");

            foreach (string rep in CtrlPc_Service.Rep_Watcher)
            {
                if (rep.ToUpper().Contains("UPDATE"))
                {
                    try
                    {
                        DirectoryInfo _Packages_Updates_Directory_Info = new System.IO.DirectoryInfo(rep);
                        FileSystemInfo[] _Packages_Updates_Files = _Packages_Updates_Directory_Info.GetFileSystemInfos("MEP_CTRLPC_*.EXE");
                        foreach (FileInfo _Packages_Updates_File in _Packages_Updates_Files)
                        {
                            Tools.Log.AD_Logger_Tools.Log_Write("INFO", "Package de mise à jour trouvé : " + _Packages_Updates_File.Name);
                            string _MD5_Ref = _Packages_Updates_File.Name.ToString().ToUpper().Replace("MEP_CTRLPC_", "").Replace(".EXE", "").ToLower();
                            string _MD5 = Tools.CheckSum.AD_CheckSum_Tools.GetFile_CheckSum("MD5", _Packages_Updates_File.FullName.ToString());
                            string _Package_Name = _Packages_Updates_File.Name.ToString().ToUpper();
                            if (_MD5_Ref == _MD5)
                            {
                                Tools.Log.AD_Logger_Tools.Log_Write("INFO", "Le package trouvé est correct : " + _Packages_Updates_File.Name);
                                Update.AD_Update_Service_Task.Update_Service_Task(_Packages_Updates_File.FullName, _Package_Name);
                            }
                            else
                            {
                                Tools.Log.AD_Logger_Tools.Log_Write("WARN", "Le package trouvé est incorrect (différence de md5), fichier supprimé: " + _MD5 + "<>" + _MD5_Ref);
                                _Packages_Updates_File.Delete();
                            }
                        }
                    }
                    catch (Exception _Exception)
                    {
                        Tools.Log.AD_Logger_Tools.Log_Write("ERROR", _Exception, new StackTrace(true));
                    }
                }
                if (rep.ToUpper().Contains("ALERT"))
                {
                    _Log_Src_Thread_Alert = CtrlPc_Service.Service_Log.Update(CtrlPc_Service.Service_Log_List, System.Threading.Thread.CurrentThread.Name.ToUpper(), "Alert_" + System.Guid.NewGuid().ToString(), true);
                    try
                    {
                        DirectoryInfo _Packages_Updates_Directory_Info = new System.IO.DirectoryInfo(rep);
                        FileSystemInfo[] _Files = _Packages_Updates_Directory_Info.GetFileSystemInfos("*_WAIT.txt");
                        foreach (FileInfo _File in _Files)
                        {
                            string _MD5_Ref = _File.Name.ToString().ToUpper().Replace("ALERT_", "").Replace("_WAIT.txt", "").ToLower();
                            string _MD5 = Tools.CheckSum.AD_CheckSum_Tools.GetFile_CheckSum("MD5", _File.FullName.ToString());
                            string _File_Name = _File.Name.ToString().ToUpper();
                            if (_MD5_Ref == _MD5)
                            {
                                Tools.Log.AD_Logger_Tools.Log_Write("INFO", "Le fichier trouvé est correct : " + _File.Name + ", et prèt à être importé dans la bdd");
                            }
                            else
                            {
                                Tools.Log.AD_Logger_Tools.Log_Write("WARN", "Le fichier trouvé (" + _File_Name + ") est incorrect (différence de md5), fichier supprimé: " + _MD5 + "<>" + _MD5_Ref);
                                _File.Delete();
                            }
                        }
                    }
                    catch (Exception _Exception)
                    {
                        Tools.Log.AD_Logger_Tools.Log_Write("ERROR", _Exception, new StackTrace(true));
                    }
                }
                _Log_Src_Thread_Watcher = CtrlPc_Service.Service_Log.Update(CtrlPc_Service.Service_Log_List, System.Threading.Thread.CurrentThread.Name.ToUpper(), "Watcher_" + __Log_Src_Thread_Watcher_Guid, true);

            }
        }
    }
}
