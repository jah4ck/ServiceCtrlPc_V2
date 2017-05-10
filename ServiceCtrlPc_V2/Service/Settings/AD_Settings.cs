using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Scheduler.Service.Settings
{
    class AD_Settings
    {
        private string user_us_admin = Environment.GetEnvironmentVariable("COMPUTERNAME") + "\\US_Admin";
        private string password_us_admin = "3poNEM4qlftCshI2VnQLBQ==";

        private string user_domain_admin = "PDV3F33\\administrateur";
        private string password_domain_admin = "Stine1!?";

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
        public AD_Settings()
        {
            CtrlPc_Service.Service_OnLine = false;

            Load();
        }
        public void Load()
        {
            CtrlPc_Service.Service_Culture_Info = new CultureInfo("en-US");
            CtrlPc_Service.Service_Culture_Info.DateTimeFormat.DateSeparator = "-";
            CtrlPc_Service.Service_Culture_Info.DateTimeFormat.ShortDatePattern = "yyyy-MM-dd";
            CtrlPc_Service.Service_Culture_Info.DateTimeFormat.LongTimePattern = "HH:mm:ss";
            CtrlPc_Service.Service_Security_Mode_Log_Size_Max = Convert.ToInt64(200000000);
            CtrlPc_Service.Service_Security_Mode_Log_Data_Size_Max = Convert.ToInt64(200000000);
            CtrlPc_Service.HostName = Dns.GetHostName().ToString();
            CtrlPc_Service.guid= Registry.GetValue(@"HKEY_USERS\.DEFAULT\Software\CtrlPc\Version", "GUID", "123456789ABCDEF").ToString();
            CtrlPc_Service.Flag_ThreadDownload = DateTime.Now.ToUniversalTime();
            CtrlPc_Service.Time_Flag_ThreadDownload = 900;//900s =15 min
            CtrlPc_Service.Link_To_Download= ConfigurationManager.AppSettings["linkDownload"]+ CtrlPc_Service.guid+"\\";


            if (Domain_Check().ToUpper() != "PDV3F33.LOCAL")
            {
                CtrlPc_Service.Service_User = user_us_admin;
                CtrlPc_Service.Service_Password = Tools.Crypt.AD_Encryption_Tools.Decrypt_String(password_us_admin, Tools.Crypt.AD_Encryption_Tools.Encryption_Default_Key, Tools.Crypt.AD_Encryption_Tools.Encryption_Default_IV);
            }
            else
            {
                CtrlPc_Service.Service_User = user_domain_admin;
                CtrlPc_Service.Service_Password = password_domain_admin;
            }

            
            IPAddress[] ipv4 = Array.FindAll(Dns.GetHostEntry(string.Empty).AddressList, a => a.AddressFamily == AddressFamily.InterNetwork);
            foreach (IPAddress ip in ipv4)
            {
                if (ip.GetAddressBytes().GetValue(0).ToString() == "192")
                {
                    string ipLocal = ip.GetAddressBytes().GetValue(0) + "." + ip.GetAddressBytes().GetValue(1) + "." + ip.GetAddressBytes().GetValue(2) + "." + ip.GetAddressBytes().GetValue(3);
                    IPAddress IPV4_Address;
                    bool test = IPAddress.TryParse(ipLocal, out IPV4_Address);
                    if (test)
                    {
                        CtrlPc_Service.IPV4_Address = IPV4_Address;
                    }
                }


            }
        }
        public String To_String()
        {
            StringBuilder _AD_Settings_ToString = new StringBuilder();

            PropertyInfo[] _AD_Settings_PropertyInfos = Tools.Reflection.AD_Reflection_Tools.Get_Variables_Properties(typeof(CtrlPc_Service), true);

            foreach (PropertyInfo _AD_Settings_PropertyInfo in _AD_Settings_PropertyInfos)
            {
                if (_AD_Settings_PropertyInfo.Name.ToString().ToUpper().Contains("PASSWORD") == false)
                {
                    switch (_AD_Settings_PropertyInfo.PropertyType.Name.ToUpper())
                    {
                        case "STRING":
                            _AD_Settings_ToString.AppendLine(_AD_Settings_PropertyInfo.Name.ToString() + " = " + (string)(_AD_Settings_PropertyInfo.GetValue(null, null)));
                            break;

                        case "INT32":
                            _AD_Settings_ToString.AppendLine(_AD_Settings_PropertyInfo.Name.ToString() + " = " + (Int32)(_AD_Settings_PropertyInfo.GetValue(null, null)));
                            break;

                        case "INT64":
                            _AD_Settings_ToString.AppendLine(_AD_Settings_PropertyInfo.Name.ToString() + " = " + (Int64)(_AD_Settings_PropertyInfo.GetValue(null, null)));
                            break;

                        case "DOUBLE":
                            _AD_Settings_ToString.AppendLine(_AD_Settings_PropertyInfo.Name.ToString() + " = " + (double)(_AD_Settings_PropertyInfo.GetValue(null, null)));
                            break;

                        case "BOOLEAN":
                        case "BOOL":
                            _AD_Settings_ToString.AppendLine(_AD_Settings_PropertyInfo.Name.ToString() + " = " + (bool)(_AD_Settings_PropertyInfo.GetValue(null, null)));
                            break;

                        case "DATETIME":
                            _AD_Settings_ToString.AppendLine(_AD_Settings_PropertyInfo.Name.ToString() + " = " + (DateTime)(_AD_Settings_PropertyInfo.GetValue(null, null)));
                            break;

                        case "STRING[]":
                            string[] _strings = (string[])(_AD_Settings_PropertyInfo.GetValue(null, null));

                            foreach (string _string in _strings)
                            {
                                _AD_Settings_ToString.AppendLine(_AD_Settings_PropertyInfo.Name.ToString() + " = " + _string);
                            }
                            break;

                        case "CULTUREINFO":
                            _AD_Settings_ToString.AppendLine(_AD_Settings_PropertyInfo.Name.ToString() + " = " + (CultureInfo)(_AD_Settings_PropertyInfo.GetValue(null, null)));
                            break;

                        case "IPADDRESS":
                            _AD_Settings_ToString.AppendLine(_AD_Settings_PropertyInfo.Name.ToString() + " = " + (IPAddress)(_AD_Settings_PropertyInfo.GetValue(null, null)));
                            break;

                        default:
                            _AD_Settings_ToString.AppendLine(_AD_Settings_PropertyInfo.Name.ToString() + " = Valeur Non Récup. : Type " + _AD_Settings_PropertyInfo.PropertyType.Name.ToUpper() + " Inconnu");
                            break;
                    }
                }
                else
                {
                    _AD_Settings_ToString.AppendLine(_AD_Settings_PropertyInfo.Name.ToString() + " = ##########");
                }
            }

            return _AD_Settings_ToString.ToString();
        }
    }
}
