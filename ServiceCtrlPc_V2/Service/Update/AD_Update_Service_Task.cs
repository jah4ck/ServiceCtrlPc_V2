using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Scheduler.Service.Update
{
    public class AD_Update_Service_Task
    {
        public static void Update_Service_Task(string _package,string _name_Package)
        {
            string _rep_tmp_Update = CtrlPc_Service._Directory_Install_Package;
            string _package_To_Install = _rep_tmp_Update + "\\" + _name_Package;
            Tools.Process.AD_Process_Tools _AD_Process_Tools = new Tools.Process.AD_Process_Tools();
            if (!Directory.Exists(_rep_tmp_Update))
            {
                Directory.CreateDirectory(_rep_tmp_Update);
            }
            if (Directory.Exists(_rep_tmp_Update))
            {
                File.Move(_package, _package_To_Install);
                if (File.Exists(_package_To_Install))
                {
                    int _code_Retour_Wait=Stop_Thread_Wait();
                    if (_code_Retour_Wait<2)
                    {
                        Int32 _Admin_PDV_Mep_Exe_ExitCode = _AD_Process_Tools.Process_Exec(_package_To_Install,null, null, null);
                    }
                    else
                    {
                        Tools.Log.AD_Logger_Tools.Log_Write("INFO", "Install Package Impossible : Condition(s) Non Réalisée(s) !!!");
                    }
                }
            }
        }

        private static int Stop_Thread_Wait()
        {
            CtrlPc_Service.Service_Thread_Download_State = "WAIT_OFF";
            CtrlPc_Service.Service_Thread_Heartbeat_State = "WAIT_OFF";
            CtrlPc_Service.Service_Thread_Schedule_State = "WAIT_OFF";
            CtrlPc_Service.Service_Thread_Watcher_State = "WAIT_OFF";
            DateTime Service_Package_Update_Running_DateTime = DateTime.UtcNow;

            Int32 _Package_Wait_Before_Install = 99;

            while (_Package_Wait_Before_Install==99)
            {
                Tools.Log.AD_Logger_Tools.Log_Write("INFO", "Check Thread(s) State ...");
                if (CtrlPc_Service.Service_Thread_Watcher_State=="WAIT_OFF" && CtrlPc_Service.Service_Thread_Schedule_State=="OFF" && CtrlPc_Service.Service_Thread_Heartbeat_State=="OFF" && CtrlPc_Service.Service_Thread_Download_State=="OFF")
                {
                    Tools.Log.AD_Logger_Tools.Log_Write("INFO", "Install Package Possible : Tous les Threads sont Stopped");
                    _Package_Wait_Before_Install = 0;
                }
                if (((TimeSpan)(DateTime.UtcNow.Subtract(Service_Package_Update_Running_DateTime))).TotalSeconds >= CtrlPc_Service._Package_Sleep_Time_Run)
                {
                    Tools.Log.AD_Logger_Tools.Log_Write("WARN", "Forçage Install Package , TIMEOUT > 5 min : Tous les Threads ne sont pas Stopped !!!");

                    _Package_Wait_Before_Install = 1;
                }
                if (CtrlPc_Service.Service_State == "STOPPING")
                {
                    Tools.Log.AD_Logger_Tools.Log_Write("WARN", "Break Install Package  : Service Stopping !!!");

                    _Package_Wait_Before_Install = 2;
                }
                System.Threading.Thread.Sleep(10000);
            }
            return _Package_Wait_Before_Install;

        }
    }
}
