using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace Scheduler.Tools.Wmi
{
    class AD_Wmi_Tools
    {
        public static ManagementObjectCollection Get_Object_Wmi(string Wmi_Query_Object)
        {
            ManagementObjectCollection _ManagementObjectCollection = null;

            try
            {
                ManagementScope _ManagementScope = new ManagementScope();

                ObjectQuery _Wql_ObjectQuery = new ObjectQuery(Wmi_Query_Object);

                ManagementObjectSearcher searcher = new ManagementObjectSearcher(_ManagementScope, _Wql_ObjectQuery);

                _ManagementObjectCollection = searcher.Get();
            }
            catch (Exception _Exception)
            {
                Tools.Log.AD_Logger_Tools.Log_Write("ERROR", _Exception, new StackTrace(true));
            }

            return _ManagementObjectCollection;
        }

        public static string Dir_To_Dir_Wmi(string Dir_To_Do)
        {
            string _Dir_Wmi = null;

            try
            {
                _Dir_Wmi = Dir_To_Do.Replace(Dir_To_Do.Substring(0, 2), "");
                _Dir_Wmi = _Dir_Wmi + @"\";
            }
            catch (Exception _Exception)
            {
                Tools.Log.AD_Logger_Tools.Log_Write("ERROR", _Exception, new StackTrace(true));
            }

            return _Dir_Wmi;
        }
    }
}
