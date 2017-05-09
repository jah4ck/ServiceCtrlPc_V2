﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scheduler.Service.Thread.Schedule
{
    class AD_Thread_Schedule
    {
        private static string _Log_Src_Thread_Schedule;

        public void Execute()
        {
            _Log_Src_Thread_Schedule = CtrlPc_Service.Service_Log.Update(CtrlPc_Service.Service_Log_List, System.Threading.Thread.CurrentThread.Name.ToUpper(), "Schedule_" + System.Guid.NewGuid().ToString(), true);

            Tools.Log.AD_Logger_Tools.Log_Write("INFO", "Début Thread Schedule : Service_State = " + CtrlPc_Service.Service_State.ToUpper() + " / Service_Thread_Schedule_State = " + CtrlPc_Service.Service_Thread_Schedule_State.ToUpper());

            try
            {
                if (CtrlPc_Service.Service_Thread_Schedule_Running == false)
                {
                    CtrlPc_Service.Service_Thread_Schedule_Running = true;

                    if (CtrlPc_Service.Service_State.ToUpper() == "STARTED" || CtrlPc_Service.Service_Thread_Schedule_State.ToUpper() == "ON")
                    {
                        Schedule_Talk();
                    }

                    CtrlPc_Service.Service_Thread_Schedule_State = CtrlPc_Service.Service_Thread_Schedule_State.ToUpper().Replace("WAIT_", "");
                }
                else
                {
                    Tools.Log.AD_Logger_Tools.Log_Write("ERROR", "Traitement Schedule Déjà en Cours d'Exécution !!!");
                }
            }
            catch (Exception _Exception)
            {
                Tools.Log.AD_Logger_Tools.Log_Write("ERROR", _Exception, new StackTrace(true));
            }
            finally
            {
                CtrlPc_Service.Service_Thread_Schedule_Running = false;
                Tools.Log.AD_Logger_Tools.Log_Write("INFO", "Fin Thread Schedule : Service_State = " + CtrlPc_Service.Service_State.ToUpper() + " / Service_Thread_Schedule_State = " + CtrlPc_Service.Service_Thread_Schedule_State.ToUpper());
            }
        }
        public void Schedule_Talk()
        {
            for (int i = 0; i < 100; i++)
            {
                Tools.Log.AD_Logger_Tools.Log_Write("INFO", "Schedule : " + i);
                System.Threading.Thread.Sleep(2000);
            }
        }
    }
}
