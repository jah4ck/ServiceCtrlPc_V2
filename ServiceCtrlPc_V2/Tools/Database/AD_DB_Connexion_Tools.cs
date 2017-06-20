using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Scheduler.Tools.Database
{
    public class AD_DB_Connexion_Tools : IEquatable<AD_DB_Connexion_Tools>
    {
        private string _log_src_DB_logger = CtrlPc_Service.Service_Log_Src.ToUpper();

        public string Name { get; set; }
        public SQLiteConnection Connexion { get; set; }
        public DateTime Date_Deb { get; set; }
        public DateTime Date_End { get; set; }
        public System.Data.ConnectionState Connexion_Last_State { get; set; }

        public Dictionary<string, Int32> Error_Extended_Attributes;

        public AD_DB_Connexion_Tools()
        {
            Error_Extended_Attributes = new Dictionary<string, Int32>();
        }

        private AD_DB_Connexion_Tools Get(List<AD_DB_Connexion_Tools> DB_Connexion_List, string DB_Connexion_Name)
        {
            AD_DB_Connexion_Tools _DB_Connexion_Get = new AD_DB_Connexion_Tools();

            try
            {
                // Copy DB_Connexion_List
                lock (DB_Connexion_List)
                {
                    foreach (AD_DB_Connexion_Tools _DB_Connexion in DB_Connexion_List)
                    {
                        if (_DB_Connexion.Connexion.DefaultTypeName.ToUpper() == DB_Connexion_Name.ToUpper())
                        {
                            _DB_Connexion_Get = _DB_Connexion;
                        }
                    }
                }
            }
            catch (Exception _Exception)
            {
                Tools.Log.AD_Logger_Tools.Log_Write("ERROR", _Exception, new StackTrace(true));
            }

            return _DB_Connexion_Get;
        }

        private SQLiteConnection Add_New(List<AD_DB_Connexion_Tools> DB_Connexion_List, string DB_Connexion_Name)
        {
            AD_DB_Connexion_Tools _DB_Connexion = new AD_DB_Connexion_Tools();

            try
            {
                _DB_Connexion.Name = DB_Connexion_Name.ToUpper();

                if (!DB_Connexion_List.Contains(_DB_Connexion))
                {
                    _DB_Connexion.Connexion = new SQLiteConnection();
                    _DB_Connexion.Connexion.DefaultTypeName = DB_Connexion_Name.ToUpper();
                    _DB_Connexion.Date_Deb = DateTime.UtcNow;
                    _DB_Connexion.Date_End = DateTime.UtcNow;
                    _DB_Connexion.Connexion_Last_State = _DB_Connexion.Connexion.State;

                    lock (DB_Connexion_List)
                    {
                        DB_Connexion_List.Add(_DB_Connexion);
                    }

                    CtrlPc_Service.AD_Sqlite_DB_Connexion_Totale++;
                }
                else
                {
                    _DB_Connexion = Get(DB_Connexion_List, DB_Connexion_Name.ToUpper());
                }
            }
            catch (Exception _Exception)
            {
                Tools.Log.AD_Logger_Tools.Log_Write("ERROR", _Exception, new StackTrace(true));
            }

            return _DB_Connexion.Connexion;
        }

        public SQLiteConnection Update(List<AD_DB_Connexion_Tools> DB_Connexion_List, string DB_Connexion_Name)
        {
            SQLiteConnection _DB_Connexion_Get = null;

            try
            {
                if (DB_Connexion_Name != null && DB_Connexion_Name != "")
                {
                    lock (DB_Connexion_List)
                    {
                        // Création DB_Connexion Si Inexistante
                        _DB_Connexion_Get = Add_New(DB_Connexion_List, DB_Connexion_Name);

                        foreach (AD_DB_Connexion_Tools _DB_Connexion in DB_Connexion_List)
                        {
                            if (_DB_Connexion.Name.ToUpper() == _DB_Connexion_Get.DefaultTypeName.ToUpper())
                            {
                                _DB_Connexion.Date_End = DateTime.UtcNow;
                                _DB_Connexion.Connexion_Last_State = _DB_Connexion_Get.State;
                            }
                        }
                    }
                }
                else
                {
                    Tools.Log.AD_Logger_Tools.Log_Write("ERROR", "Valeur DB_Connexion_Name Incorrecte !!!");
                }
            }
            catch (Exception _Exception)
            {
                Tools.Log.AD_Logger_Tools.Log_Write("ERROR", _Exception, new StackTrace(true));
            }

            return _DB_Connexion_Get;
        }

        public void Delete_Expire(List<AD_DB_Connexion_Tools> DB_Connexion_List)
        {
            try
            {
                lock (DB_Connexion_List)
                {
                    // Update All DB Connexion
                    CtrlPc_Service.AD_Sqlite_DB_Connexion_Active = 0;

                    foreach (AD_DB_Connexion_Tools _DB_Connexion in DB_Connexion_List)
                    {
                        // Actualise Nb Connexion(s) Active(s)
                        if (_DB_Connexion.Connexion.State != System.Data.ConnectionState.Closed && _DB_Connexion.Connexion.State == System.Data.ConnectionState.Broken)
                        {
                            CtrlPc_Service.AD_Sqlite_DB_Connexion_Active++;
                        }
                        if (_DB_Connexion.Connexion_Last_State != _DB_Connexion.Connexion.State)
                        {
                            _DB_Connexion.Date_End = DateTime.UtcNow;
                            _DB_Connexion.Connexion_Last_State = _DB_Connexion.Connexion.State;
                        }
                    }

                    // Copy DB_Connexion_List
                    List<AD_DB_Connexion_Tools> _DB_Connexion_List_Copy = new List<AD_DB_Connexion_Tools>(DB_Connexion_List);

                    // Parcours DB_Connexion_List_Copy
                    foreach (AD_DB_Connexion_Tools _DB_Connexion_Copy in _DB_Connexion_List_Copy)
                    {
                        Int32 _TimeSpan_Keep_Open = 1800;
                        Int32 _TimeSpan_Keep_Closed = 300;

                        TimeSpan _DB_Connexion_TimeSpan = DateTime.UtcNow - _DB_Connexion_Copy.Date_End;

                        // Recyclage DB Connexion Open
                        if ((_DB_Connexion_Copy.Connexion_Last_State != System.Data.ConnectionState.Closed && _DB_Connexion_Copy.Connexion_Last_State != System.Data.ConnectionState.Broken) && _DB_Connexion_TimeSpan.TotalSeconds > _TimeSpan_Keep_Open)
                        {
                            Delete(DB_Connexion_List, _DB_Connexion_Copy.Name.ToUpper());
                        }

                        // Recyclage DB Connexion Closed
                        if ((_DB_Connexion_Copy.Connexion_Last_State == System.Data.ConnectionState.Closed || _DB_Connexion_Copy.Connexion_Last_State == System.Data.ConnectionState.Broken) && _DB_Connexion_TimeSpan.TotalSeconds > _TimeSpan_Keep_Closed)
                        {
                            Delete(DB_Connexion_List, _DB_Connexion_Copy.Name.ToUpper());
                        }
                    }

                    _DB_Connexion_List_Copy.Clear();
                    _DB_Connexion_List_Copy = null;
                }
            }
            catch (Exception _Exception)
            {
                Tools.Log.AD_Logger_Tools.Log_Write("ERROR", _Exception, new StackTrace(true));
            }
        }

        public void Delete(List<AD_DB_Connexion_Tools> DB_Connexion_List, string DB_Connexion_Name)
        {
            try
            {
                DB_Connexion_List.ForEach(delegate (AD_DB_Connexion_Tools _DB_Connexion)
                {
                    if (_DB_Connexion.Name == DB_Connexion_Name.ToUpper())
                    {
                        Tools.Log.AD_Logger_Tools.Log_Write("INFO", ToString("Recycle DB Connexion Expirée : ", _DB_Connexion));

                        if (_DB_Connexion.Connexion.State != System.Data.ConnectionState.Closed && _DB_Connexion.Connexion.State != System.Data.ConnectionState.Broken)
                        {
                            _DB_Connexion.Connexion.Close();
                            _DB_Connexion.Connexion.Dispose();
                            _DB_Connexion = null;
                        }

                        lock (DB_Connexion_List)
                        {
                            DB_Connexion_List.Remove(_DB_Connexion);
                        }
                    }
                });
            }
            catch (Exception _Exception)
            {
                Tools.Log.AD_Logger_Tools.Log_Write("ERROR", _Exception, new StackTrace(true));
            }
        }

        public string ToString(string DB_Connexion_Action, AD_DB_Connexion_Tools DB_Connexion)
        {
            return string.Format("DB Connexion Infos = {0} / " +
                                   "Nom DB_Connexion = {1} / " +
                                   "DB Connexion Début = {2} / " +
                                   "DB Connexion Fin = {3} / " +
                                   "DB Connexion State = {4} / ",
                                   DB_Connexion_Action.Trim(),
                                   DB_Connexion.Name.ToString(),
                                   DB_Connexion.Date_Deb.ToString(),
                                   DB_Connexion.Date_End.ToString(),
                                   DB_Connexion.Connexion_Last_State.ToString());
        }

        #region IEquatable<AD_DB_Logger_Connexion_Tools>

        public bool Equals(AD_DB_Connexion_Tools DB_Connexion_Name_Check)
        {
            return this.Name.Equals(DB_Connexion_Name_Check.Name);
        }

        #endregion
    }
}
