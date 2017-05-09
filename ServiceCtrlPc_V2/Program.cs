using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace Scheduler
{
    static class Program
    {
        /// <summary>
        /// Point d'entrée principal de l'application.
        /// </summary>
        static void Main(string[] args)
        {
            System.Diagnostics.Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.BelowNormal;
            System.Diagnostics.Process.GetCurrentProcess().ProcessorAffinity = new IntPtr(1);

            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(Tools.Log.AD_Logger_Tools.Log_Write_Unhandled_Exception);

            Assembly.GetExecutingAssembly().GetName().CultureInfo = new System.Globalization.CultureInfo("en-US");
            Assembly.GetExecutingAssembly().GetName().CultureInfo.DateTimeFormat.DateSeparator = "-";
            Assembly.GetExecutingAssembly().GetName().CultureInfo.DateTimeFormat.ShortDatePattern = "yyyy-MM-dd";
            Assembly.GetExecutingAssembly().GetName().CultureInfo.DateTimeFormat.LongTimePattern = "HH:mm:ss";

            System.Threading.Thread.CurrentThread.Name = "AD_Thread_Service_Id_0";
            System.Threading.Thread.CurrentThread.CurrentCulture = Assembly.GetExecutingAssembly().GetName().CultureInfo;
            System.Threading.Thread.CurrentThread.Priority = System.Threading.ThreadPriority.Lowest;


            try
            {
                if (args.Length != 0)
                {
                    if (args[0].ToUpper() == "SERVICE")
                    {
                        ServiceBase[] ServicesToRun;

                        ServicesToRun = new ServiceBase[]
                        {
                           new CtrlPc_Service()
                        };

                        ServiceBase.Run(ServicesToRun);
                    }
                    else
                    {
                        //(new Scheduler_Service()).Start_Mode(args);
                    }
                }
            }
            catch (Exception _Exception)
            {
                Tools.Log.AD_Logger_Tools.Log_Write("ERROR", _Exception, new StackTrace(true));
            }

            AppDomain.CurrentDomain.UnhandledException -= new UnhandledExceptionEventHandler(Tools.Log.AD_Logger_Tools.Log_Write_Unhandled_Exception);
        }
    }
}
