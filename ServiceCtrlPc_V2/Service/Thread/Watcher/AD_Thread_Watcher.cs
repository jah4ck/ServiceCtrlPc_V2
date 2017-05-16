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

        public void Execute()
        {
            _Log_Src_Thread_Watcher = CtrlPc_Service.Service_Log.Update(CtrlPc_Service.Service_Log_List, System.Threading.Thread.CurrentThread.Name.ToUpper(), "Watcher_" + System.Guid.NewGuid().ToString(), true);

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
                if (Directory.Exists(rep))
                {
                    string[] files = Directory.GetFiles(rep);
                    if (files.Length>0)
                    {

                    }
                }
            }
            System.Threading.Thread.Sleep(2000);

        }
    }
}
