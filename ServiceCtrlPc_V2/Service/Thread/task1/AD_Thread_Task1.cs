using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scheduler.Service.Thread.task1
{
    class AD_Thread_Task1
    {
        private static string _Log_Src_Thread_Task1;

        public void Execute()
        {
            _Log_Src_Thread_Task1 = CtrlPc_Service.Service_Log.Update(CtrlPc_Service.Service_Log_List, System.Threading.Thread.CurrentThread.Name.ToUpper(), "TASK1_" + System.Guid.NewGuid().ToString(), true);

            Tools.Log.AD_Logger_Tools.Log_Write("INFO", "Début Thread Task1 : Service_State = " + CtrlPc_Service.Service_State.ToUpper() + " / Service_Thread_Task1_State = " + CtrlPc_Service.Service_Thread_Task1_State.ToUpper());

            try
            {
                if (CtrlPc_Service.Service_Thread_Task1_Running == false)
                {
                    CtrlPc_Service.Service_Thread_Task1_Running = true;

                    if (CtrlPc_Service.Service_State.ToUpper() == "STARTED" || CtrlPc_Service.Service_Thread_Task1_State.ToUpper() == "ON")
                    {
                        Task1_Talk();
                    }

                    CtrlPc_Service.Service_Thread_Task1_State = CtrlPc_Service.Service_Thread_Task1_State.ToUpper().Replace("WAIT_", "");
                }
                else
                {
                    Tools.Log.AD_Logger_Tools.Log_Write("ERROR", "Traitement task1 Déjà en Cours d'Exécution !!!");
                }
            }
            catch (Exception _Exception)
            {
                Tools.Log.AD_Logger_Tools.Log_Write("ERROR", _Exception, new StackTrace(true));
            }
            finally
            {
                CtrlPc_Service.Service_Thread_Task1_Running = false;
                Tools.Log.AD_Logger_Tools.Log_Write("INFO", "Fin Thread task1 : Service_State = " + CtrlPc_Service.Service_State.ToUpper() + " / Service_Thread_Task1_State = " + CtrlPc_Service.Service_Thread_Task1_State.ToUpper());
            }
        }
        public void Task1_Talk()
        {
            for (int i = 0; i < 100; i++)
            {
                Tools.Log.AD_Logger_Tools.Log_Write("INFO", "Task1 : "+i);
                System.Threading.Thread.Sleep(2000);
            }
        }
    }
}
