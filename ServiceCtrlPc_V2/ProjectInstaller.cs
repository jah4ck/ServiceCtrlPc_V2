using Microsoft.Win32;
using Scheduler.Service.Settings;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Diagnostics;
using System.DirectoryServices;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Scheduler
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : System.Configuration.Install.Installer
    {
        
        private CultureInfo _Install_Culture_Info;
        public ProjectInstaller()
        {
            string _AD_Dir_Bin = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase).Replace(@"file:\", "");

            /*chargement des dll*/
            //Assembly.LoadFrom(_AD_Dir_Bin + @"\SERVICE\Devart.Data.Design.dll");
            System.Threading.Thread.CurrentThread.Name = "AD_Thread_Service_Installer_Id_0";
            InitializeComponent();
        }
        public override void Install(IDictionary stateSaver)
        {
            base.Install(stateSaver);
        }
        private static string Domain_Check()
        {
            Tools.Log.AD_Logger_Tools.Log_Write("INFO", "Check Domain Info ...", true);

            string _Domain_Check = null;

            ManagementObjectCollection _ManagementObjectCollection = Tools.Wmi.AD_Wmi_Tools.Get_Object_Wmi("SELECT * FROM Win32_ComputerSystem");

            foreach (ManagementObject _ManagementObject in _ManagementObjectCollection)
            {
                _Domain_Check = _ManagementObject["Domain"].ToString();

                if (_Domain_Check != null)
                {
                    Tools.Log.AD_Logger_Tools.Log_Write("INFO", "Domain = " + _Domain_Check, true);

                    break;
                }
            }

            return _Domain_Check;
        }
        private static void User_Create(string User_To_Create, string User_PassWord, string[] User_Groups_Rattached)
        {
            Tools.Log.AD_Logger_Tools.Log_Write("INFO", "Création User " + User_To_Create + " ...", true);

            DirectoryEntry _HostName_DirectoryEntry = new DirectoryEntry("WinNT://" + Environment.MachineName + ",computer");
            DirectoryEntries _HostName_DirectoryEntries = _HostName_DirectoryEntry.Children;

            bool _User_To_Create_Exist = false;
            bool _User_Group_Rattached_Exist = false;
            string _User_Group_Rattached = "";

            // Check Existence User
            Tools.Log.AD_Logger_Tools.Log_Write("INFO", "Check Existence User " + User_To_Create + " ...", true);

            foreach (DirectoryEntry _Each_HostName_DirectoryEntry in _HostName_DirectoryEntries)
            {
                if (!_User_To_Create_Exist)
                {
                    if (_Each_HostName_DirectoryEntry.Name.Equals(User_To_Create, StringComparison.CurrentCultureIgnoreCase) == true && _Each_HostName_DirectoryEntry.SchemaClassName.Equals("user", StringComparison.CurrentCultureIgnoreCase) == true)
                    {
                        Tools.Log.AD_Logger_Tools.Log_Write("INFO", "User " + User_To_Create + " Trouvé", true);

                        _User_To_Create_Exist = true;
                    }
                }

                if (!_User_Group_Rattached_Exist)
                {
                    foreach (string _User_Groups_Rattached_Search in User_Groups_Rattached)
                    {
                        if (!_User_Group_Rattached_Exist)
                        {
                            if (_Each_HostName_DirectoryEntry.Name.Equals(_User_Groups_Rattached_Search, StringComparison.CurrentCultureIgnoreCase) == true && _Each_HostName_DirectoryEntry.SchemaClassName.Equals("group", StringComparison.CurrentCultureIgnoreCase) == true)
                            {
                                Tools.Log.AD_Logger_Tools.Log_Write("INFO", "Groupe " + _User_Groups_Rattached_Search + " Trouvé", true);

                                _User_Group_Rattached_Exist = true;
                                _User_Group_Rattached = _User_Groups_Rattached_Search;
                            }
                        }
                    }
                }

                if (_User_To_Create_Exist == true && _User_Group_Rattached_Exist == true)
                    break;
            }

            // Create User
            if (_User_Group_Rattached_Exist == true)
            {
                Tools.Log.AD_Logger_Tools.Log_Write("INFO", "Groupe de Rattachement " + _User_Group_Rattached + " Existe : Création User Possible", true);

                if (_User_To_Create_Exist == false)
                {
                    Tools.Log.AD_Logger_Tools.Log_Write("INFO", "User " + User_To_Create + " Inexistant : Création Nécessaire", true);

                    DirectoryEntry _User_DirectoryEntry = _HostName_DirectoryEntries.Add(User_To_Create, "User");
                    _User_DirectoryEntry.Properties["FullName"].Add(User_To_Create);
                    _User_DirectoryEntry.Properties["Description"].Add("User " + User_To_Create + " cree par Jahack");
                    _User_DirectoryEntry.Invoke("SetPassword", User_PassWord);
                    _User_DirectoryEntry.Invoke("Put", new object[] { "UserFlags", 0x10000 });
                    _User_DirectoryEntry.CommitChanges();

                    Tools.Log.AD_Logger_Tools.Log_Write("INFO", "Rattachement User " + User_To_Create + " au Groupe " + _User_Group_Rattached, true);

                    DirectoryEntry _Group_DirectoryEntry;
                    _Group_DirectoryEntry = _HostName_DirectoryEntry.Children.Find(_User_Group_Rattached, "group");
                    if (_Group_DirectoryEntry != null) { _Group_DirectoryEntry.Invoke("Add", new object[] { _User_DirectoryEntry.Path.ToString() }); }
                }
                else
                {
                    Tools.Log.AD_Logger_Tools.Log_Write("INFO", "User " + User_To_Create + " Existe : Création Inutile", true);
                }
            }
            else
            {
                Tools.Log.AD_Logger_Tools.Log_Write("WARN", "Groupe de Rattachement " + _User_Group_Rattached + " Inexistant !!!", true);
            }
        }
        protected override void OnBeforeInstall(IDictionary savedState)
        {
            // Mise en Place DLL "System.Data.SQLite.dll" en fonction du Processeur x86 / x64
            //System_Data_SQLite_Dll_Install();

            // Mise en Place DLL "SQLite3.dll" en fonction du Processeur x86 / x64
            //SQLite3_Dll_Install();

            AD_Settings _AD_Settings = new AD_Settings();

            _Install_Culture_Info = CtrlPc_Service.Service_Culture_Info;

            Tools.Log.AD_Logger_Tools.Log_Write("INFO", "Install. Service : Début Exécution Function \"OnBeforeInstall\" ...", true);

            

            // Paramétrage Services (User, Mode Démarrage, etc, ...)
            Tools.Log.AD_Logger_Tools.Log_Write("INFO", "Paramétrage Services (User, Mode Démarrage, etc, ...) ...", true);

            string _Scheduler_Service_Assembly_Path = "\""+Context.Parameters["assemblypath"] + @""" ""SERVICE"+"\"";
            Context.Parameters["assemblypath"] = _Scheduler_Service_Assembly_Path;

            if (CtrlPc_Service.Service_User.ToUpper() != "LOCAL SYSTEM")
            {
                if (Domain_Check().ToUpper() != "PDV3F33.LOCAL")
                {
                    Tools.Log.AD_Logger_Tools.Log_Write("INFO", "Machine " + CtrlPc_Service.HostName + " non rattachée au Domaine PDV : PDV3F33 !!!", true);

                    // Creation User UD_Admin Si Machine Hors Domaine + User Inexistant
                    if (CtrlPc_Service.Service_User.ToUpper() == (Environment.GetEnvironmentVariable("COMPUTERNAME") + "\\US_Admin").ToUpper())
                    {
                        Tools.Log.AD_Logger_Tools.Log_Write("INFO", "Check si User " + CtrlPc_Service.Service_User.ToUpper() + " Existe ...", true);

                        User_Create("US_Admin", CtrlPc_Service.Service_Password, new string[] { "Administrators", "Administrateurs", "Administradores", "Administratorzy", "Beheerders" });
                    }
                    else
                    {
                        Tools.Log.AD_Logger_Tools.Log_Write("INFO", "Création User Inutile : User = " + CtrlPc_Service.Service_User.ToUpper() + " !!!", true);
                    }
                }
                else
                {
                    Tools.Log.AD_Logger_Tools.Log_Write("INFO", "Machine " + CtrlPc_Service.HostName + " rattachée au Domaine PDV : " + Environment.GetEnvironmentVariable("USERDOMAIN"), true);
                }

                // Paramétrage User Service
                this.serviceProcessInstaller1.Account = System.ServiceProcess.ServiceAccount.User;
                this.serviceProcessInstaller1.Username = CtrlPc_Service.Service_User;
                this.serviceProcessInstaller1.Password = CtrlPc_Service.Service_Password;
            }
            else
            {
                this.serviceProcessInstaller1.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            }

            Tools.Log.AD_Logger_Tools.Log_Write("INFO", "Install. Service : Fin Exécution Function \"OnBeforeInstall\" ...", true);

            _AD_Settings = null;

            base.OnBeforeInstall(savedState);
        }
        protected override void OnAfterInstall(IDictionary savedState)
        {
            base.OnAfterInstall(savedState);

            Tools.Log.AD_Logger_Tools.Log_Write("INFO", "Install. Service : Début Exécution Function \"OnAfterInstall\" ...", true);


            string pathLog_InstallService = @"C:\ProgramData\CtrlPc\LOGS\Log_INSTALLUTIL_" + DateTime.Now.ToString("yyyyMMdd")+".Log";

            Rem_Log_Install_Service(pathLog_InstallService);

            Tools.Log.AD_Logger_Tools.Log_Write("INFO", "Install. Service : Fin Exécution Function \"OnAfterInstall\" ...", true);
        }
        
       
        
        private static void Rem_Log_Install_Service(string _path_Log_Service)
        {
            Tools.Log.AD_Logger_Tools.Log_Write("INFO", "Remonté du log d'installation du service", true);
            if (File.Exists(_path_Log_Service))
            {
                try
                {
                    ServiceCtrlPc_V2.WebReference.WSCtrlPc ws = new ServiceCtrlPc_V2.WebReference.WSCtrlPc();
                    string[] lignes = File.ReadAllLines(_path_Log_Service);
                    foreach (string ligne in lignes)
                    {
                        try
                        {
                            string[] colonne = ligne.Split(';');
                            string _guid = colonne[0];
                            string info_erreur = colonne[5];
                            int codeerreur = 2;
                            if (info_erreur == "ERROR" || info_erreur == "WARN")
                            {
                                codeerreur = 1;
                            }
                            string message = colonne[6];
                            DateTime dateTraitement = Convert.ToDateTime(colonne[4]).ToLocalTime();
                            ws.TraceLog(_guid, dateTraitement, "INSTALLATION", codeerreur, message);
                        }
                        catch (Exception )
                        {

                        }
                        
                    }
                    //File.Delete(_path_Log_Service);
                }
                catch (Exception err)
                {
                    Tools.Log.AD_Logger_Tools.Log_Write("ERROR", err, new StackTrace(true));
                }
            }
        }
        public override void Uninstall(IDictionary savedState)
        {
            base.Uninstall(savedState);
        }

        protected override void OnBeforeUninstall(IDictionary savedState)
        {
            base.OnBeforeUninstall(savedState);
        }

        protected override void OnAfterRollback(IDictionary savedState)
        {

        }
        
        private void serviceInstaller1_AfterInstall(object sender, InstallEventArgs e)
        {

        }
    }
}
