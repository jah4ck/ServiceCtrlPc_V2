using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Scheduler.Tools.Reflection
{
    class AD_Reflection_Tools
    {
        public static PropertyInfo[] Get_Variables_Properties(Type Objet_To_Do, bool Sorted)
        {
            PropertyInfo[] _PropertyInfos;

            _PropertyInfos = Objet_To_Do.GetProperties(BindingFlags.Public | BindingFlags.Static);

            if (Sorted)
            {
                // Sort Pproperties by Name
                Array.Sort(_PropertyInfos, delegate (PropertyInfo _PropertyInfo_1, PropertyInfo _PropertyInfo_2)
                { return _PropertyInfo_1.Name.CompareTo(_PropertyInfo_2.Name); });
            }

            return _PropertyInfos;
        }
    }
}
