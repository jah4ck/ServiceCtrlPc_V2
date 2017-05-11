using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Scheduler.Tools.Download
{
    public class AD_Download_File
    {
        public static void Download_File(string _rep, string _nameFile)
        {
            string uri = CtrlPc_Service.Link_To_Download + _nameFile;
            string dest = @"C:\ProgramData\CtrlPc\" + _rep + @"\" + _nameFile;
            WebClient webClient = new WebClient();
            webClient.DownloadFile(new Uri(uri), dest);
        }
    }
}
