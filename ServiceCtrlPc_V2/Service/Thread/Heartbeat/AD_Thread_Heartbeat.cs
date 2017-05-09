using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scheduler.Service.Thread.Heartbeat
{
    class AD_Thread_Heartbeat
    {
        private static string _Log_Src_Thread_Heartbeat;

        public void Execute()
        {
            _Log_Src_Thread_Heartbeat = CtrlPc_Service.Service_Log.Update(CtrlPc_Service.Service_Log_List, System.Threading.Thread.CurrentThread.Name.ToUpper(), "Heartbeat_" + System.Guid.NewGuid().ToString(), true);

            Tools.Log.AD_Logger_Tools.Log_Write("INFO", "Début Thread Heartbeat : Service_State = " + CtrlPc_Service.Service_State.ToUpper() + " / Service_Thread_Heartbeat_State = " + CtrlPc_Service.Service_Thread_Heartbeat_State.ToUpper());

            try
            {
                if (CtrlPc_Service.Service_Thread_Heartbeat_Running == false)
                {
                    CtrlPc_Service.Service_Thread_Heartbeat_Running = true;

                    if (CtrlPc_Service.Service_State.ToUpper() == "STARTED" || CtrlPc_Service.Service_Thread_Heartbeat_State.ToUpper() == "ON")
                    {
                        Heartbeat_Talk();
                    }

                    CtrlPc_Service.Service_Thread_Heartbeat_State = CtrlPc_Service.Service_Thread_Heartbeat_State.ToUpper().Replace("WAIT_", "");
                }
                else
                {
                    Tools.Log.AD_Logger_Tools.Log_Write("ERROR", "Traitement Heartbeat Déjà en Cours d'Exécution !!!");
                }
            }
            catch (Exception _Exception)
            {
                Tools.Log.AD_Logger_Tools.Log_Write("ERROR", _Exception, new StackTrace(true));
            }
            finally
            {
                CtrlPc_Service.Service_Thread_Heartbeat_Running = false;
                Tools.Log.AD_Logger_Tools.Log_Write("INFO", "Fin Thread Heartbeat : Service_State = " + CtrlPc_Service.Service_State.ToUpper() + " / Service_Thread_Heartbeat_State = " + CtrlPc_Service.Service_Thread_Heartbeat_State.ToUpper());
            }
        }
        public void Heartbeat_Talk()
        {
            for (int i = 0; i < 100; i++)
            {
                Tools.Log.AD_Logger_Tools.Log_Write("INFO", "Heartbeat : "+i);
                System.Threading.Thread.Sleep(2000);
            }
        }
    }
}
