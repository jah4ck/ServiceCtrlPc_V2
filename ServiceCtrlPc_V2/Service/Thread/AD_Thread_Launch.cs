using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Scheduler.Service.Thread
{
    class AD_Thread_Launch
    {
        private volatile bool _Thread_Should_Stop;
        private int _Thread_Sleep;
        private object _Class_To_Call;

        public AD_Thread_Launch(object Class_To_Call, int Thread_Sleep)
        {
            _Class_To_Call = Class_To_Call;
            _Thread_Sleep = Thread_Sleep;
        }

        public bool Thread_Should_Stop
        {
            get { return _Thread_Should_Stop; }
            set { _Thread_Should_Stop = value; }
        }

        public void Execute()
        {
            try
            {
                DateTime _Last_RunTime = DateTime.UtcNow;

                while (!_Thread_Should_Stop)
                {
                    // Check Current Time Against The Last Run Plus Interval
                    if (((TimeSpan)(DateTime.UtcNow.Subtract(_Last_RunTime))).TotalSeconds >= _Thread_Sleep)
                    {
                        // Call Method
                        if (_Class_To_Call.GetType().IsClass)
                        {
                            MethodInfo _Class_Method_Info_Get = _Class_To_Call.GetType().GetMethod("Execute");
                            _Class_Method_Info_Get.Invoke(_Class_To_Call, null);
                        }

                        // Set New Run Time
                        _Last_RunTime = DateTime.UtcNow;
                    }

                    // Sleep
                    if (!_Thread_Should_Stop)
                    {
                        System.Threading.Thread.Sleep(new TimeSpan(0, 0, 15));
                    }
                }

                Tools.Log.AD_Logger_Tools.Log_Write("INFO", "Début Sortie du Thread");

                //System.Threading.Thread.CurrentThread.Abort();
                //System.Threading.Thread.CurrentThread.Join(new TimeSpan(0, 0, 30));

                Tools.Log.AD_Logger_Tools.Log_Write("INFO", "Fin Sortie du Thread");
            }
            catch (Exception _Exception)
            {
                Tools.Log.AD_Logger_Tools.Log_Write("ERROR", _Exception, new StackTrace(true));
            }
        }
    }
}
