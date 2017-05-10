using Scheduler.Service.Thread;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Scheduler
{
    public partial class CtrlPc_Service : ServiceBase
    {
        public CtrlPc_Service()
        {
            InitializeComponent();
        }
        #region variable
        public static CultureInfo Service_Culture_Info { get; set; }
        public static bool Service_OnLine { get; set; }
        public static string AD_Dir_Logs { get; set; }
        public static string AD_Dir_Data { get; set; }
        public static long Service_Security_Mode_Log_Size_Max { get; set; }
        public static long Service_Security_Mode_Log_Data_Size_Max { get; set; }
        public static string HostName { get; set; }
        public static string guid { get; set; }
        public static IPAddress IPV4_Address { get; set; }
        public static bool Service_Actif { get; set; }
        private Int32 _Sleep_Time_Task { get; set; }
        private Int32 _Service_TimeOut_Stop { get; set; }
        private Int32 _Sleep_Time_End_Task { get; set; }
        public static string Service_User { get; set; }
        public static string Service_Password { get; set; }

        public static ServiceCtrlPc_V2.WebReference.WSCtrlPc ws = new ServiceCtrlPc_V2.WebReference.WSCtrlPc();

        public static System.Threading.ThreadPriority Service_ThreadPriority = System.Threading.ThreadPriority.Lowest;

        public static string Service_Logger_Application = "SERVICE";
        public static string Service_Logger_Log_Module = "LOG";
        public static string Service_Logger_Alert_Module = "ALERTE";
        public static volatile string Service_State = "UNKNOWN";
        public static Int32 Service_Logger_ID = 0;
        public static string Codehex = Environment.GetEnvironmentVariable("GuidScheduler", EnvironmentVariableTarget.Machine);

        Service.Settings.AD_Settings _AD_Settings;
        System.Threading.Thread[] _Service_Threads_Worker;
        Service.Thread.AD_Thread_Launch[] _Service_Threads_Worker_Arr;

        // Variables Indiquant le State des Différents Threads : WAIT_ON / WAIT_OFF / ON / OFF
        public static volatile string Service_Thread_Schedule_State = "UNKNOWN";
        public static volatile string Service_Thread_Heartbeat_State = "UNKNOWN";
        public static volatile string Service_Thread_Task2_State = "UNKNOWN";

        public static volatile bool Service_Thread_Schedule_Running = false;
        public static volatile bool Service_Thread_Heartbeat_Running = false;
        public static volatile bool Service_Thread_Task2_Running = false;

        private List<object> _Thread_Service_List = new List<object>() {
                                                                            new Service.Thread.Schedule.AD_Thread_Schedule(),
                                                                            new Service.Thread.Heartbeat.AD_Thread_Heartbeat(),
                                                                            new Service.Thread.task2.AD_Thread_Task2(),
                                                                       };

        /*gestion log*/
        public static Tools.Log.AD_Logger_Source_Tools Service_Log = new Tools.Log.AD_Logger_Source_Tools();
        public static volatile List<Tools.Log.AD_Logger_Source_Tools> Service_Log_List = new List<Tools.Log.AD_Logger_Source_Tools>();
        public static string Service_Log_Src = CtrlPc_Service.Service_Log.Update(CtrlPc_Service.Service_Log_List, System.Threading.Thread.CurrentThread.Name.ToUpper(), "ALL_" + System.Guid.NewGuid().ToString(), false);
        #endregion

        protected override void OnStart(string[] args)
        {
            System.Threading.Thread.CurrentThread.Name = "AD_Thread_Service_Start_Id_0";
            this.RequestAdditionalTime(240000);
            
            Service_Start_Init();
            
            System.Threading.Thread.CurrentThread.CurrentCulture = CtrlPc_Service.Service_Culture_Info;

            // Démarrage des <> Threads
            Thread _Thread_Service_Start = new System.Threading.Thread(new ParameterizedThreadStart(Service_Start));
            _Thread_Service_Start = new System.Threading.Thread(new ParameterizedThreadStart(Service_Start));
            _Thread_Service_Start.Name = "AD_Thread_Service_Start_Id_0";
            _Thread_Service_Start.CurrentCulture = CtrlPc_Service.Service_Culture_Info;
            _Thread_Service_Start.Start("SERVICE");
            _Thread_Service_Start.Priority = CtrlPc_Service.Service_ThreadPriority;

            this.ExitCode = 0;
        }
        private void Service_Start_Init()
        {
            _AD_Settings = new Service.Settings.AD_Settings();
            _Sleep_Time_Task = 30000;
            _Service_TimeOut_Stop = 30000;
            _Sleep_Time_End_Task = 30000;

            Assembly.GetExecutingAssembly().GetName().CultureInfo = CtrlPc_Service.Service_Culture_Info;
            System.Threading.Thread.CurrentThread.CurrentCulture = CtrlPc_Service.Service_Culture_Info;

            Tools.Log.AD_Logger_Tools.Log_Write("WARN", "Initialisation Client : " + CtrlPc_Service.HostName);

        }

        private void Service_Start(object Start_Mode)
        {
            try
            {
                //calcul du sleep de démarrage 
                Int32 _Der_Car_IP = Convert.ToInt32(CtrlPc_Service.IPV4_Address.ToString().Substring((CtrlPc_Service.IPV4_Address.ToString().Length - 1), 1));
                Int32 _Current_Minutes = DateTime.UtcNow.Minute;
                Int32 _Sleep_Time_Before_Start = (((((((int)Math.Truncate((decimal)(_Current_Minutes) / (decimal)(10))) + 1) * 10) + _Der_Car_IP)) - _Current_Minutes + 10) * 60000;
                Int32 _Thread_Sleep_Time = 10;
                DateTime _Service_Actif_Check_DateTime = DateTime.UtcNow;

                Tools.Log.AD_Logger_Tools.Log_Write("INFO", "Début Démarrage Service en Mode " + Start_Mode + " ...", true);
                Tools.Log.AD_Logger_Tools.Log_Write("INFO", "Wait Service Actif ...");

                while (Service_State == "UNKNOWN")
                {
                    if (CtrlPc_Service.Service_Actif == true)
                    {
                        Service_State="STARTING";
                        break;
                    }
                    else
                    {
                        if (((TimeSpan)(DateTime.UtcNow.Subtract(_Service_Actif_Check_DateTime))).TotalSeconds >= 120)
                        {
                            Tools.Log.AD_Logger_Tools.Log_Write("INFO", "Check Service Actif ...");

                            Service_Actif = true;

                            _Service_Actif_Check_DateTime = DateTime.UtcNow;
                        }
                    }

                    System.Threading.Thread.Sleep(30000);
                }

                // Démarrage des <> Threads
                if (CtrlPc_Service.Service_Actif == true)
                {
                    Tools.Log.AD_Logger_Tools.Log_Write("INFO", "Service Actif : Démarrage Complet", true);

                    _Service_Threads_Worker_Arr = new Service.Thread.AD_Thread_Launch[_Thread_Service_List.Count];
                    _Service_Threads_Worker = new System.Threading.Thread[_Thread_Service_List.Count];

                    for (int i = 0; i < _Thread_Service_List.Count; i++)
                    {
                        // Create an Object
                        string _Thread_Sleep_Time_Name = _Thread_Service_List[i].GetType().Name.ToString().Replace(".Service.Thread.AD_Thread", "").Replace("AD_Thread_", "Task_");

                        _Thread_Sleep_Time = Convert.ToInt32(120);

                        _Service_Threads_Worker_Arr[i] = new AD_Thread_Launch(_Thread_Service_List[i], _Thread_Sleep_Time);

                        // Set Properties on the Object
                        _Service_Threads_Worker_Arr[i].Thread_Should_Stop = false;

                        // Create a Thread and Attach to the Object
                        System.Threading.ThreadStart _Thread_To_Start = new System.Threading.ThreadStart(_Service_Threads_Worker_Arr[i].Execute);
                        _Service_Threads_Worker[i] = new System.Threading.Thread(_Thread_To_Start);
                        _Service_Threads_Worker[i].CurrentCulture = CtrlPc_Service.Service_Culture_Info;
                        _Service_Threads_Worker[i].Name = _Thread_Service_List[i].GetType().Name.ToString().Replace(".Service.Thread.AD_Thread", "") + "_Id_" + i.ToString();
                        Tools.Log.AD_Logger_Tools.Log_Write("INFO", "Initialisation Thread " + _Service_Threads_Worker[i].Name.ToString() + " avec Valeur Sleep Time = " + _Thread_Sleep_Time + "");
                    }

                    // Start Threads
                    for (int i = 0; i < _Thread_Service_List.Count; i++)
                    {
                        if (_Service_Threads_Worker[i] != null)
                        {
                            // Temporisation entre chaque Thread
                            System.Threading.Thread.Sleep(_Sleep_Time_Task);

                            // Start Thread
                            Tools.Log.AD_Logger_Tools.Log_Write("INFO", "Démarrage Thread " + _Service_Threads_Worker[i].Name + " ...", true);
                            _Service_Threads_Worker[i].Start();
                            _Service_Threads_Worker[i].Priority = CtrlPc_Service.Service_ThreadPriority;
                        }
                    }

                    Service_State="STARTED";

                    Service_Thread_Schedule_State = "WAIT_ON";
                    Service_Thread_Heartbeat_State = "WAIT_ON";
                    Service_Thread_Task2_State = "WAIT_ON";
                }
                else
                {
                    Tools.Log.AD_Logger_Tools.Log_Write("WARN", "Service Inactif : Démarrage Minimun", true);
                }

            }
            catch (Exception _Exception)
            {

                Tools.Log.AD_Logger_Tools.Log_Write("ERROR", _Exception, new StackTrace(true));
            }
            finally
            {
                if (CtrlPc_Service.Service_Actif == true)
                {
                    // Log Settings Service
                    Tools.Log.AD_Logger_Tools.Log_Write("INFO", "Settings Service :");
                    Tools.Log.AD_Logger_Tools.Log_Write("INFO", _AD_Settings.To_String());
                }

                Tools.Log.AD_Logger_Tools.Log_Write("INFO", "Fin Démarrage Service", true);
            }
        }
        protected override void OnStop()
        {
            System.Threading.Thread.CurrentThread.Name = "AD_Thread_Service_Stop_Id_0";

            // Initialisation Unattended Errors
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(Tools.Log.AD_Logger_Tools.Log_Write_Unhandled_Exception);

            // Initialisation Regional Settings
            System.Threading.Thread.CurrentThread.CurrentCulture = CtrlPc_Service.Service_Culture_Info;

            // TimeOut Stop Service
            this.RequestAdditionalTime(_Service_TimeOut_Stop);

            // Stop Service
            Service_Stop(_AD_Settings);

            // Code Retour
            this.ExitCode = 0;
        }
        private void Service_Stop(object Service_AD_Settings)
        {
            Tools.Log.AD_Logger_Tools.Log_Write("INFO", "Début Arret Service", true);

            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(Tools.Log.AD_Logger_Tools.Log_Write_Unhandled_Exception);

            try
            {
                Service_State="STOPPING";

                Service_Thread_Schedule_State = "WAIT_OFF";
                Service_Thread_Heartbeat_State = "WAIT_OFF";
                Service_Thread_Task2_State = "WAIT_OFF";

                System.Threading.Thread.Sleep(30000);

                if (CtrlPc_Service.Service_Actif == true)
                {
                    if (_Service_Threads_Worker != null)
                    {
                        for (int i = 0; i < _Service_Threads_Worker.Length; i++)
                        {
                            if (_Service_Threads_Worker[i] != null)
                            {
                                Tools.Log.AD_Logger_Tools.Log_Write("INFO", "Etat Thread Avant Join " + _Service_Threads_Worker[i].Name.ToString() + " " + _Service_Threads_Worker[i].ThreadState.ToString());

                                // set flag to stop worker thread
                                _Service_Threads_Worker_Arr[i].Thread_Should_Stop = true;

                                if (((_Service_Threads_Worker[i] != null) && (_Service_Threads_Worker[i].IsAlive)) || (_Service_Threads_Worker[i].ThreadState.ToString().ToUpper() == "RUNNING") || (_Service_Threads_Worker[i].ThreadState.ToString().ToUpper() == "WAITSLEEPJOIN") || (_Service_Threads_Worker[i].ThreadState.ToString().ToUpper() == "SUSPENDED") || (_Service_Threads_Worker[i].ThreadState.ToString().ToUpper() == "SUSPENDREQUESTED"))
                                {

                                    Tools.Log.AD_Logger_Tools.Log_Write("INFO", "Attente Fin Thread (Max " + _Sleep_Time_End_Task + "s) ...");
                                    _Service_Threads_Worker[i].Join(new TimeSpan(0, 0, _Sleep_Time_End_Task));
                                    Tools.Log.AD_Logger_Tools.Log_Write("INFO", "Fin Attente Fin Thread (Max " + _Sleep_Time_End_Task + "s) ...");
                                }

                                Tools.Log.AD_Logger_Tools.Log_Write("INFO", "Etat Thread Après Join " + _Service_Threads_Worker[i].Name.ToString() + " " + _Service_Threads_Worker[i].ThreadState.ToString());

                                if (((_Service_Threads_Worker[i] != null) && (_Service_Threads_Worker[i].IsAlive)) || (_Service_Threads_Worker[i].ThreadState.ToString().ToUpper() == "RUNNING") || (_Service_Threads_Worker[i].ThreadState.ToString().ToUpper() == "WAITSLEEPJOIN") || (_Service_Threads_Worker[i].ThreadState.ToString().ToUpper() == "SUSPENDED") || (_Service_Threads_Worker[i].ThreadState.ToString().ToUpper() == "SUSPENDREQUESTED"))
                                {
                                    Tools.Log.AD_Logger_Tools.Log_Write("ERROR", "Non Stoppé donc Abort");

                                    _Service_Threads_Worker[i].Abort();

                                    Tools.Log.AD_Logger_Tools.Log_Write("ERROR", "Etat Thread Après Abort " + _Service_Threads_Worker[i].Name.ToString() + " " + _Service_Threads_Worker[i].ThreadState.ToString());
                                }
                            }
                        }
                    }
                    else
                    {
                        Tools.Log.AD_Logger_Tools.Log_Write("INFO", "Aucun Thread Démarré !!!", true);
                    }
                }
                else
                {
                    System.Threading.Thread.Sleep(30000);
                }

                Service_State="STOPPED";
            }
            catch (Exception _Exception)
            {
                Tools.Log.AD_Logger_Tools.Log_Write("ERROR", _Exception, new StackTrace(true));
            }

            finally
            {
                Tools.Log.AD_Logger_Tools.Log_Write("INFO", "Fin Arret Service", true);
            }

            AppDomain.CurrentDomain.UnhandledException -= new UnhandledExceptionEventHandler(Tools.Log.AD_Logger_Tools.Log_Write_Unhandled_Exception);
        }
    }
}
