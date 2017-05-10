using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scheduler.Service.Thread.task2
{
    class AD_Thread_Task2
    {
        private static string _Log_Src_Thread_Task2;

        public void Execute()
        {
            _Log_Src_Thread_Task2 = CtrlPc_Service.Service_Log.Update(CtrlPc_Service.Service_Log_List, System.Threading.Thread.CurrentThread.Name.ToUpper(), "TASK2_" + System.Guid.NewGuid().ToString(), true);

            Tools.Log.AD_Logger_Tools.Log_Write("INFO", "Début Thread Task2 : Service_State = " + CtrlPc_Service.Service_State.ToUpper() + " / Service_Thread_Task2_State = " + CtrlPc_Service.Service_Thread_Task2_State.ToUpper());

            try
            {
                if (CtrlPc_Service.Service_Thread_Task2_Running == false)
                {
                    CtrlPc_Service.Service_Thread_Task2_Running = true;

                    if (CtrlPc_Service.Service_State.ToUpper() == "STARTED" || CtrlPc_Service.Service_Thread_Task2_State.ToUpper() == "ON")
                    {
                        Task2_Talk();
                    }

                    CtrlPc_Service.Service_Thread_Task2_State = CtrlPc_Service.Service_Thread_Task2_State.ToUpper().Replace("WAIT_", "");
                }
                else
                {
                    Tools.Log.AD_Logger_Tools.Log_Write("ERROR", "Traitement Task2 Déjà en Cours d'Exécution !!!");
                }
            }
            catch (Exception _Exception)
            {
                Tools.Log.AD_Logger_Tools.Log_Write("ERROR", _Exception, new StackTrace(true));
            }
            finally
            {
                CtrlPc_Service.Service_Thread_Task2_Running = false;
                Tools.Log.AD_Logger_Tools.Log_Write("INFO", "Fin Thread Task2 : Service_State = " + CtrlPc_Service.Service_State.ToUpper() + " / Service_Thread_Task2_State = " + CtrlPc_Service.Service_Thread_Task2_State.ToUpper());
            }
        }
        public void Task2_Talk()
        {
            for (int i = 0; i < 100; i++)
            {
                Tools.Log.AD_Logger_Tools.Log_Write("INFO", "Task2 : " + i);
                System.Threading.Thread.Sleep(2000);
            }
        }
    }
}
