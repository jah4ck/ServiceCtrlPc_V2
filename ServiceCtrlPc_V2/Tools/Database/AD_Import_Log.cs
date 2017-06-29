using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Scheduler.Tools.Database
{
    public class AD_Import_Log
    {
        public static void Import_File(string file)
        {
            if (File.Exists(file))
            {
                FileInfo infoFichier = new FileInfo(file);
                Tools.Log.AD_Logger_Tools.Log_Write("INFO", "Fichier trouvé : "+infoFichier.Name);

            }
        }
        private void Import_Alert(string file)
        {

        }
        private void Import_Log(string file)
        {

        }
        private void Import_Data(string file)
        {

        }
    }
}
