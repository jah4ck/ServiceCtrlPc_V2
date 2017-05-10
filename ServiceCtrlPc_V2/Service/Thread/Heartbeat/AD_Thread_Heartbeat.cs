using ServiceCtrlPc_V2.Tools.Heure;
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

            SynchroHeure MySynchroHeure = new SynchroHeure();
            DateTime dateTraitement = DateTime.Now;
            try
            {
                Tools.Log.AD_Logger_Tools.Log_Write("INFO", "Récupération Heure");
                dateTraitement = MySynchroHeure.GetNetworkTime();
            }
            catch (Exception err)
            {
                Tools.Log.AD_Logger_Tools.Log_Write("ERROR", err, new StackTrace(true));
                dateTraitement = DateTime.Now;
            }

            try
            {
                Tools.Log.AD_Logger_Tools.Log_Write("INFO", "Intérogation WS");
                string stop = CtrlPc_Service.ws.GetHeartbeat(CtrlPc_Service.guid, dateTraitement);
                if (stop=="1")
                {
                    Tools.Log.AD_Logger_Tools.Log_Write("INFO", "Demande d'arrêt de l'ordinateur");
                }
                if (stop=="2")
                {
                    Tools.Log.AD_Logger_Tools.Log_Write("WARN", "Erreur lors du contrôle d'arrêt");
                }
                

            }
            catch (Exception err)
            {
                Tools.Log.AD_Logger_Tools.Log_Write("ERROR", err, new StackTrace(true));
            }
        }
    }
}
