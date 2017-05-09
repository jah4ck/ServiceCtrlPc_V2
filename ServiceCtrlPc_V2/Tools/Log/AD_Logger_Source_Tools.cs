using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scheduler.Tools.Log
{
    public class AD_Logger_Source_Tools : IEquatable<AD_Logger_Source_Tools>
    {

        public string Src_Thread { get; set; }
        public string Src_Log { get; set; }
        public string Src_Log_Bak { get; set; }
        public DateTime Date_Create { get; set; }
        public DateTime Date_Update { get; set; }

        public Dictionary<string, Int32> Error_Extended_Attributes;

        public AD_Logger_Source_Tools()
        {
            Error_Extended_Attributes = new Dictionary<string, Int32>();
        }

        public string Get(List<AD_Logger_Source_Tools> Log_Src_List)
        {
            string _Log_Src_Get = null;

            try
            {
                lock (Log_Src_List)
                {
                    foreach (AD_Logger_Source_Tools _Log_Src in Log_Src_List)
                    {
                        if (_Log_Src.Src_Thread.ToUpper() == System.Threading.Thread.CurrentThread.Name.ToUpper())
                        {
                            _Log_Src_Get = _Log_Src.Src_Log;
                        }
                    }
                }

                if (_Log_Src_Get == null || _Log_Src_Get == "") { _Log_Src_Get = Add_New(Log_Src_List, System.Threading.Thread.CurrentThread.Name.ToUpper(), CtrlPc_Service.Service_Log_Src); }
            }
            catch (Exception _Exception)
            {
                Tools.Log.AD_Logger_Tools.Log_Write("ERROR", _Exception, new StackTrace(true));
            }

            return _Log_Src_Get;
        }

        private string Add_New(List<AD_Logger_Source_Tools> Log_Src_List, string Log_Thread, string Log_Src)
        {
            AD_Logger_Source_Tools _Log_Src = new AD_Logger_Source_Tools();

            try
            {
                _Log_Src.Src_Thread = Log_Thread.ToUpper();
                _Log_Src.Src_Log = Log_Src.ToUpper();
                _Log_Src.Src_Log_Bak = Log_Src.ToUpper();
                _Log_Src.Date_Create = DateTime.UtcNow;
                _Log_Src.Date_Update = DateTime.UtcNow;

                if (!Log_Src_List.Contains(_Log_Src))
                {
                    lock (Log_Src_List)
                    {
                        Log_Src_List.Add(_Log_Src);
                    }
                }
            }
            catch (Exception _Exception)
            {
                Tools.Log.AD_Logger_Tools.Log_Write("ERROR", _Exception, new StackTrace(true));
            }

            return _Log_Src.Src_Log;
        }

        public string Update(List<AD_Logger_Source_Tools> Log_Src_List, string Log_Thread, string Log_Src, bool With_Src_Log_Bak_Update)
        {
            string _Log_Src_Get = null;

            try
            {
                // Création Log_Src Si Inexistant
                Add_New(Log_Src_List, Log_Thread, Log_Src);

                lock (Log_Src_List)
                {
                    foreach (AD_Logger_Source_Tools _Log_Src in Log_Src_List)
                    {
                        if (_Log_Src.Src_Thread.ToUpper() == Log_Thread.ToUpper())
                        {
                            if (With_Src_Log_Bak_Update == true) { _Log_Src.Src_Log_Bak = _Log_Src.Src_Log.ToUpper(); }
                            _Log_Src.Src_Log = Log_Src.ToUpper();
                            _Log_Src.Date_Update = DateTime.UtcNow;
                            _Log_Src_Get = _Log_Src.Src_Log;
                        }
                    }
                }
            }
            catch (Exception _Exception)
            {
                Tools.Log.AD_Logger_Tools.Log_Write("ERROR", _Exception, new StackTrace(true));
            }

            return _Log_Src_Get;
        }

        public string RollBack(List<AD_Logger_Source_Tools> Log_Src_List)
        {
            string _Log_Src_Get = null;

            try
            {
                lock (Log_Src_List)
                {
                    foreach (AD_Logger_Source_Tools _Log_Src in Log_Src_List)
                    {
                        if (_Log_Src.Src_Thread.ToUpper() == System.Threading.Thread.CurrentThread.Name.ToUpper())
                        {
                            _Log_Src.Src_Log = _Log_Src.Src_Log_Bak.ToUpper();
                            _Log_Src.Date_Update = DateTime.UtcNow;
                            _Log_Src_Get = _Log_Src.Src_Log;
                        }
                    }
                }
            }
            catch (Exception _Exception)
            {
                Tools.Log.AD_Logger_Tools.Log_Write("ERROR", _Exception, new StackTrace(true));
            }

            return _Log_Src_Get;
        }

        #region IEquatable<AD_Logger_Source_Tools>

        public bool Equals(AD_Logger_Source_Tools Src_Thread_Check)
        {
            return this.Src_Thread.Equals(Src_Thread_Check.Src_Thread);
        }

        #endregion
    }
}
