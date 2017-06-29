using ServiceCtrlPc_V2.Tools.Database;
using ServiceCtrlPc_V2.Tools.Heure;
using System;
using System.Collections.Generic;
using System.Data;
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

            //SynchroHeure MySynchroHeure = new SynchroHeure();.
            Tools.Log.AD_Logger_Tools.Log_Write("INFO", "Récupération Heure");
            DateTime dateTraitement = SynchroHeure.GetNetworkTime();

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

                try
                {
                    Tools.Log.AD_Logger_Tools.Log_Write("INFO", "Maj de la BDD");
                    DataSet myDataSetConnexion = new DataSet();
                    myDataSetConnexion = AD_Exec_Query_SQL.AD_ExecQuery(CtrlPc_Service.AD_Sqlite_DataSource, "SELECT", "SELECT count(ID_Connexion) as compteur FROM Connexion WHERE strftime('%Y%m%d', Date_Debut) = strftime('%Y%m%d', datetime('" + dateTraitement.ToString("yyyy-MM-dd HH:mm:ss") + "'))", 300);

                    string resultat = myDataSetConnexion.Tables["Table"].Rows[0]["compteur"].ToString();
                    int compteur=0;
                    bool check = Int32.TryParse(resultat, out compteur);
                    if (check)
                    {
                        if (compteur > 0)
                        {
                            Tools.Log.AD_Logger_Tools.Log_Write("INFO", "Date de début à la date du jour trouvé");
                            Tools.Log.AD_Logger_Tools.Log_Write("INFO", "Mise à jour de la date de fin");
                            AD_Exec_Query_SQL.AD_ExecQuery(CtrlPc_Service.AD_Sqlite_DataSource, "UPDATE", "UPDATE Connexion SET Date_Fin = (datetime('" + dateTraitement.ToString("yyyy-MM-dd HH:mm:ss") + "')),Temp_Activite=((julianday(datetime('" + dateTraitement.ToString("yyyy-MM-dd HH:mm:ss") + "')) - julianday(Date_Debut))*1440) WHERE ID_Connexion = (SELECT ID_Connexion FROM Connexion WHERE strftime('%Y%m%d', Date_Debut) = strftime('%Y%m%d', datetime('" + dateTraitement.ToString("yyyy-MM-dd HH:mm:ss") + "')) ORDER BY Date_Debut DESC LIMIT 1)", 300);
                        }
                        else
                        {
                            Tools.Log.AD_Logger_Tools.Log_Write("INFO", "Date de début à la date du jour non trouvé");
                            Tools.Log.AD_Logger_Tools.Log_Write("INFO", "Création d'une nouvelle ligne");
                            AD_Exec_Query_SQL.AD_ExecQuery(CtrlPc_Service.AD_Sqlite_DataSource, "INSERT", "INSERT INTO Connexion (Date_Debut) VALUES (datetime('" + dateTraitement.ToString("yyyy-MM-dd HH:mm:ss") + "'))", 300);
                        }
                    }
                    
                    Tools.Log.AD_Logger_Tools.Log_Write("INFO", "Maj de la BDD terminée");
                }
                catch (Exception _Exception)
                {
                    Tools.Log.AD_Logger_Tools.Log_Write("ERROR", _Exception, new StackTrace(true));
                }


            }
            catch (Exception err)
            {
                Tools.Log.AD_Logger_Tools.Log_Write("ERROR", err, new StackTrace(true));
            }
        }
    }
}
