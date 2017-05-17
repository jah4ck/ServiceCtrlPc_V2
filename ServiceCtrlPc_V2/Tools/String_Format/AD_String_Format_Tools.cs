using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security;
using System.Text;

namespace Scheduler.Tools.String_Format
{
    class AD_String_Format_Tools
    {
        public static SecureString String_To_SecureString(string String_To_Convert)
        {
            SecureString _SecureString_Result = new SecureString();

            char[] passwordChars = String_To_Convert.ToCharArray();

            foreach (char c in passwordChars)
            {
                _SecureString_Result.AppendChar(c);
            }

            return _SecureString_Result;
        }
    }
}
