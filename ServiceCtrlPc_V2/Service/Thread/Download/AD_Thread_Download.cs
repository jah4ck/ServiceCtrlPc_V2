using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scheduler.Service.Thread.Download
{
    class AD_Thread_Download
    {
        private static string _Log_Src_Thread_Download;

        public void Execute()
        {
            _Log_Src_Thread_Download = CtrlPc_Service.Service_Log.Update(CtrlPc_Service.Service_Log_List, System.Threading.Thread.CurrentThread.Name.ToUpper(), "Download_" + System.Guid.NewGuid().ToString(), true);

            Tools.Log.AD_Logger_Tools.Log_Write("INFO", "Début Thread Download : Service_State = " + CtrlPc_Service.Service_State.ToUpper() + " / Service_Thread_Download_State = " + CtrlPc_Service.Service_Thread_Download_State.ToUpper());

            try
            {
                if (CtrlPc_Service.Service_Thread_Download_Running == false)
                {
                    CtrlPc_Service.Service_Thread_Download_Running = true;

                    if (CtrlPc_Service.Service_State.ToUpper() == "STARTED" || CtrlPc_Service.Service_Thread_Download_State.ToUpper() == "ON")
                    {
                        Download_Talk();
                    }

                    CtrlPc_Service.Service_Thread_Download_State = CtrlPc_Service.Service_Thread_Download_State.ToUpper().Replace("WAIT_", "");
                }
                else
                {
                    Tools.Log.AD_Logger_Tools.Log_Write("ERROR", "Traitement Download Déjà en Cours d'Exécution !!!");
                }
            }
            catch (Exception _Exception)
            {
                Tools.Log.AD_Logger_Tools.Log_Write("ERROR", _Exception, new StackTrace(true));
            }
            finally
            {
                CtrlPc_Service.Service_Thread_Download_Running = false;
                Tools.Log.AD_Logger_Tools.Log_Write("INFO", "Fin Thread Download : Service_State = " + CtrlPc_Service.Service_State.ToUpper() + " / Service_Thread_Download_State = " + CtrlPc_Service.Service_Thread_Download_State.ToUpper());
            }
        }
        public void Download_Talk()
        {
            TimeSpan timer = DateTime.Now.ToUniversalTime() - CtrlPc_Service.Flag_ThreadDownload;
            Tools.Log.AD_Logger_Tools.Log_Write("INFO", "Dernière exécution du Thread Download il y a : " + timer.TotalSeconds);
            if (timer.TotalSeconds > CtrlPc_Service.Time_Flag_ThreadDownload)
            {
                Tools.Log.AD_Logger_Tools.Log_Write("INFO", "Nouvelle exécution du Thread Download");
                Download_Talk_Exec();
            }

        }
        public void Download_Talk_Exec()
        {
            try
            {
                CtrlPc_Service.Flag_ThreadDownload = DateTime.Now.ToUniversalTime();
                Tools.Log.AD_Logger_Tools.Log_Write("INFO", "Appel du WS --> GetDownloadFile(" + CtrlPc_Service.guid + "," + DateTime.Now + ")");
                string FileDownload = CtrlPc_Service.ws.GetDownloadFile(CtrlPc_Service.guid, DateTime.Now);
                if (FileDownload.Length > 0)
                {
                    string[] LstFileDownload = FileDownload.Split(new string[] { System.Environment.NewLine, "\r", "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string ligne in LstFileDownload)
                    {
                        Tools.Log.AD_Logger_Tools.Log_Write("INFO", "Téléchargement de " + ligne);
                        try
                        {
                            string[] colonne = ligne.Split(new Char[] { ';' });
                            Tools.Download.AD_Download_File.Download_File(colonne[0], colonne[1]);
                            Tools.Log.AD_Logger_Tools.Log_Write("INFO", "Téléchargement terminé de " + ligne);
                            //mise a jour de la bdd via ws

                            Tools.Log.AD_Logger_Tools.Log_Write("INFO", "Appel du WSS --> SetDownloadFile(" + CtrlPc_Service.guid + "," + DateTime.Now + "," + colonne[0] + "," + colonne[1] + ")");
                            CtrlPc_Service.ws.SetDownloadFile(CtrlPc_Service.guid, DateTime.Now, colonne[0], colonne[1]);
                        }
                        catch (Exception _err)
                        {
                            Tools.Log.AD_Logger_Tools.Log_Write("ERROR", _err, new StackTrace(true));
                        }
                        
                    }
                }
                else
                {
                    Tools.Log.AD_Logger_Tools.Log_Write("INFO", "Aucun fichier à télécharger ");
                }
            }
            catch (Exception _err)
            {
                Tools.Log.AD_Logger_Tools.Log_Write("ERROR", _err, new StackTrace(true));
            }

        }
    }
}
