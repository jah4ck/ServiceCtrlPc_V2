using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Scheduler.Tools.Database
{
    class AD_Sqlite_DB_Tools
    {
        public static SQLiteConnection AD_SQLite_DB_Connexion(string DB_Action)
        {
            return AD_SQLite_DB_Connexion(DB_Action, CtrlPc_Service.AD_Sqlite_DataSource, 0);
        }

        public static SQLiteConnection AD_SQLite_DB_Connexion(string DB_Action, string DB_DataSource, Int32 DB_Busy_Retry)
        {
            SQLiteConnection _DB_Connexion = CtrlPc_Service.Service_DB_Connexion.Update(CtrlPc_Service.Service_DB_Connexion_List, System.Threading.Thread.CurrentThread.Name);

            try
            {
                System.Data.ConnectionState _DB_Connexion_State = AD_SQLite_DBConnexion_GetState(_DB_Connexion);

                if (_DB_Connexion_State != System.Data.ConnectionState.Closed && _DB_Connexion_State != System.Data.ConnectionState.Broken && _DB_Connexion_State != System.Data.ConnectionState.Open)
                {
                    Tools.Log.AD_Logger_Tools.Log_Write("ERROR", "Tentative " + DB_Action + " Database : State Database = " + _DB_Connexion_State.ToString());
                }

                switch (DB_Action.ToUpper())
                {
                    case "OPEN":
                        if (_DB_Connexion_State == System.Data.ConnectionState.Closed || _DB_Connexion_State == System.Data.ConnectionState.Broken)
                        {

                            SQLiteConnectionStringBuilder _Connection_StringBuilder = new SQLiteConnectionStringBuilder();
                            //_Connection_StringBuilder.BusyTimeout = 300000;
                            //_Connection_StringBuilder.ConnectionTimeout = 300;
                            //_Connection_StringBuilder.DataSource = DB_DataSource;
                            //_Connection_StringBuilder.DefaultCommandTimeout = 300;
                            //_Connection_StringBuilder.Encryption = EncryptionMode.None;
                            //_Connection_StringBuilder.FailIfMissing = true;
                            //_Connection_StringBuilder.JournalMode = JournalMode.Default;
                            //_Connection_StringBuilder.Locking = LockingMode.Normal;
                            //_Connection_StringBuilder.LegacyFileFormat = false;
                            //_Connection_StringBuilder.MinPoolSize = 0;
                            //_Connection_StringBuilder.MaxPoolSize = 100;
                            //_Connection_StringBuilder.Pooling = true;
                            //_Connection_StringBuilder.ReadUncommitted = true;
                            //_Connection_StringBuilder.CacheSize = 4000;
                            //_Connection_StringBuilder.PageSize = 8192;
                            ////_Connection_StringBuilder.Synchronous = SynchronizationMode.Full;
                            ////_Connection_StringBuilder.CheckpointFullFSync = true;
                            ////_Connection_StringBuilder.TransactionScopeLocal = true;
                            ////_Connection_StringBuilder.AutoVacuum = AutoVacuumMode.Full;

                            _Connection_StringBuilder.BusyTimeout = 300000;
                            _Connection_StringBuilder.DataSource = DB_DataSource;
                            _Connection_StringBuilder.DefaultTimeout = 300;
                            _Connection_StringBuilder.FailIfMissing = true;
                            _Connection_StringBuilder.LegacyFormat = false;
                            _Connection_StringBuilder.Pooling = true;
                            _Connection_StringBuilder.CacheSize = 4000;
                            _Connection_StringBuilder.PageSize = 65536;
                            _Connection_StringBuilder.SyncMode = SynchronizationModes.Full;

                            _DB_Connexion.ConnectionString = _Connection_StringBuilder.ConnectionString;

                            try
                            {
                                _DB_Connexion.Open();
                            }
                            catch (SQLiteException _SQLiteException)
                            {
                                int codeErreur;
                                bool valid = Int32.TryParse(SQLiteErrorCode.Busy.ToString(), out codeErreur);
                                if (valid)
                                {
                                    if (_SQLiteException.ErrorCode == codeErreur && DB_Busy_Retry <= CtrlPc_Service.AD_Sqlite_DB_Busy_Retry)
                                    {
                                        if (DB_Busy_Retry > (CtrlPc_Service.AD_Sqlite_DB_Busy_Retry / 2))
                                        {
                                            Tools.Log.AD_Logger_Tools.Log_Write("WARN", DB_Action + " " + DB_DataSource + " Impossbible sur Thread " + System.Threading.Thread.CurrentThread.Name + " : " + _SQLiteException.Message + " (Tentative N° " + DB_Busy_Retry + ")");
                                        }

                                        System.Threading.Thread.Sleep(1000);

                                        AD_SQLite_DB_Connexion(DB_Action, DB_DataSource, (DB_Busy_Retry + 1));
                                    }
                                    else
                                    {
                                        Tools.Log.AD_Logger_Tools.Log_Write("ERROR", _SQLiteException, new StackTrace(true));
                                    }
                                }
                                
                            }
                        }
                        break;

                    case "CLOSE":
                        if (_DB_Connexion_State != System.Data.ConnectionState.Closed && _DB_Connexion_State != System.Data.ConnectionState.Broken)
                        {
                            _DB_Connexion.Close();
                            _DB_Connexion.Dispose();
                        }
                        break;

                    default:
                        break;
                }

                CtrlPc_Service.Service_DB_Connexion.Update(CtrlPc_Service.Service_DB_Connexion_List, _DB_Connexion.DefaultTypeName);
            }
            catch (Exception _Exception)
            {
                Tools.Log.AD_Logger_Tools.Log_Write("ERROR", _Exception, new StackTrace(true));
            }
            finally
            {

            }

            return _DB_Connexion;
        }

        public static System.Data.ConnectionState AD_SQLite_DBConnexion_GetState(SQLiteConnection DB_Connexion)
        {
            return DB_Connexion.State;
        }

        public static DataSet AD_SQLite_DS_Run_Query(string Data_Source, string Query_Command, string Query_String, int Query_TimeOut, bool With_Close, bool With_Transaction)
        {
            SQLiteConnection DB_Connexion = DB_Connexion = AD_SQLite_DB_Connexion("OPEN", Data_Source, 0);
            DataSet DB_DataSet = new DataSet();
            SQLiteTransaction DB_Transaction = null;
            SQLiteCommand DB_Command;

            try
            {
                AD_Sqlite_DB_Query_Log("++");

                if (With_Transaction == true && Query_Command.ToUpper() != "SELECT")
                {
                    DB_Transaction = AD_Sqlite_DB_Transaction(DB_Connexion.BeginTransaction(), "BEGIN", "INFO", "Run Query");
                }

                DB_Command = new SQLiteCommand(Query_String, DB_Connexion, DB_Transaction);

                DB_Command.CommandType = CommandType.Text;
                DB_Command.CommandTimeout = Query_TimeOut;
                SQLiteDataAdapter DB_DataAdapter = new SQLiteDataAdapter(DB_Command);

                switch (Query_Command.ToUpper())
                {
                    case "SELECT":
                        DB_DataAdapter.Fill(DB_DataSet);
                        break;

                    case "UPDATE":
                        DB_DataAdapter.UpdateCommand = DB_Command;
                        DB_DataAdapter.UpdateCommand.CommandTimeout = Query_TimeOut;

                        try
                        {
                            DB_DataAdapter.UpdateCommand.ExecuteNonQuery();
                        }
                        catch (SQLiteException _SQLiteException)
                        {
                            int codeErreur;
                            bool valid = Int32.TryParse(SQLiteErrorCode.Busy.ToString(), out codeErreur);
                            if (valid)
                            {
                                if (_SQLiteException.ErrorCode == codeErreur)
                                {
                                    Tools.Log.AD_Logger_Tools.Log_Write("WARN", Query_Command + " Impossbible sur Thread " + System.Threading.Thread.CurrentThread.Name + " : " + _SQLiteException.Message);

                                    System.Threading.Thread.Sleep(5000);

                                    DB_DataAdapter.UpdateCommand.ExecuteNonQuery();
                                }
                                else
                                {
                                    AD_Sqlite_DB_Query_Error(Query_String, _SQLiteException);
                                }
                            }
                        }

                        break;

                    case "INSERT":
                        DB_DataAdapter.InsertCommand = DB_Command;
                        DB_DataAdapter.InsertCommand.CommandTimeout = Query_TimeOut;

                        try
                        {
                            DB_DataAdapter.InsertCommand.ExecuteNonQuery();
                        }
                        catch (SQLiteException _SQLiteException)
                        {
                            int codeErreur;
                            bool valid = Int32.TryParse(SQLiteErrorCode.Busy.ToString(), out codeErreur);
                            if (valid)
                            {
                                if (_SQLiteException.ErrorCode == codeErreur)
                                {
                                    Tools.Log.AD_Logger_Tools.Log_Write("WARN", Query_Command + " Impossbible : " + _SQLiteException.Message);

                                    System.Threading.Thread.Sleep(5000);

                                    DB_DataAdapter.UpdateCommand.ExecuteNonQuery();
                                }
                                else
                                {
                                    AD_Sqlite_DB_Query_Error(Query_String, _SQLiteException);
                                }
                            }
                        }

                        break;

                    case "DELETE":
                        DB_DataAdapter.DeleteCommand = DB_Command;
                        DB_DataAdapter.DeleteCommand.CommandTimeout = Query_TimeOut;

                        try
                        {
                            DB_DataAdapter.DeleteCommand.ExecuteNonQuery();
                        }
                        catch (SQLiteException _SQLiteException)
                        {
                            int codeErreur;
                            bool valid = Int32.TryParse(SQLiteErrorCode.Busy.ToString(), out codeErreur);
                            if (valid)
                            {
                                if (_SQLiteException.ErrorCode == codeErreur)
                                {
                                    Tools.Log.AD_Logger_Tools.Log_Write("WARN", Query_Command + " Impossbible : " + _SQLiteException.Message);

                                    System.Threading.Thread.Sleep(5000);

                                    DB_DataAdapter.UpdateCommand.ExecuteNonQuery();
                                }
                                else
                                {
                                    AD_Sqlite_DB_Query_Error(Query_String, _SQLiteException);
                                }
                            }
                        }

                        break;

                    default:
                        break;
                }

                if (With_Transaction == true && Query_Command.ToUpper() != "SELECT")
                {
                    AD_Sqlite_DB_Transaction(DB_Transaction, "COMMIT", "INFO", "Run Query");
                }

                DB_DataSet.Dispose();
                DB_DataAdapter.Dispose();
                DB_Command.Dispose();
                if (Query_Command.ToUpper() != "SELECT") { DB_DataSet = null; }
                DB_Command = null;

                if (With_Close == true)
                {
                    if (With_Transaction == true && Query_Command.ToUpper() != "SELECT") { DB_Transaction.Dispose(); }

                    DB_Transaction = null;
                    AD_SQLite_DB_Connexion("CLOSE", Data_Source, 0);
                }

                return DB_DataSet;
            }
            catch (Exception _Exception)
            {
                Tools.Log.AD_Logger_Tools.Log_Write("ERROR", _Exception, new StackTrace(true));
                Tools.Log.AD_Logger_Tools.Log_Write("ERROR", "QUERY = " + Query_String.Replace("|", "PIPE"));

                if (With_Transaction == true)
                {
                    AD_Sqlite_DB_Transaction(DB_Transaction, "ROLLBACK", "ERROR", "Run Query");
                    DB_Transaction.Dispose();
                    DB_Transaction = null;
                    DB_Connexion.Close();
                    DB_Connexion.Dispose();
                }

                return null;
            }
            finally
            {
                AD_Sqlite_DB_Query_Log("--");
            }
        }

        public static SQLiteDataReader AD_SQLite_DR_Run_Query(string Data_Source, string Query_String, int Query_TimeOut, bool With_Close)
        {
            SQLiteDataReader DB_DataReader = null;

            try
            {
                AD_Sqlite_DB_Query_Log("++");

                SQLiteConnection DB_Connexion = AD_SQLite_DB_Connexion("OPEN", Data_Source, 0);

                SQLiteCommand DB_Command;

                DB_Command = new SQLiteCommand(Query_String, DB_Connexion);

                DB_Command.CommandType = CommandType.Text;
                DB_Command.CommandTimeout = Query_TimeOut;

                SQLiteDataAdapter DB_DataAdapter = new SQLiteDataAdapter(DB_Command);

                DB_DataReader = DB_Command.ExecuteReader(CommandBehavior.Default);

                DB_DataAdapter.Dispose();
                DB_Command.Dispose();
                DB_DataAdapter = null;
                DB_Command = null;
            }
            catch (Exception _Exception)
            {
                Tools.Log.AD_Logger_Tools.Log_Write("ERROR", _Exception, new StackTrace(true));
                Tools.Log.AD_Logger_Tools.Log_Write("ERROR", "QUERY = " + Query_String.Replace("|", "PIPE"));
            }
            finally
            {
                AD_Sqlite_DB_Query_Log("--");
            }

            return DB_DataReader;
        }

        public static Int32 AD_SQLite_Bulk_Run_Query(string Data_Source, string Query_String, List<string> Query_Data_Fieds, string Query_Data_Fieds_Separator, object Query_Data_Src)
        {
            StreamReader _Query_Data_Src = null;
            Int32 _Nb_Records_Insert = 0;

            try
            {
                AD_Sqlite_DB_Query_Log("++");

                // Conversion StringBuilder en StreamReader si pas le cas
                if (Query_Data_Src.GetType().FullName.ToUpper() == "System.Text.StringBuilder".ToUpper())
                {
                    _Query_Data_Src = new StreamReader(new MemoryStream(Encoding.ASCII.GetBytes(Query_Data_Src.ToString())));
                }
                else
                {
                    _Query_Data_Src = (StreamReader)Query_Data_Src;
                }

                using (SQLiteConnection DB_Connexion = AD_SQLite_DB_Connexion("OPEN", Data_Source, 0))
                {
                    using (var DB_Transaction = AD_Sqlite_DB_Transaction(DB_Connexion.BeginTransaction(), "BEGIN", "INFO", "Bulk Query"))
                    {
                        using (SQLiteCommand DB_Command = DB_Connexion.CreateCommand())
                        {
                            DB_Command.CommandType = CommandType.Text;
                            DB_Command.CommandText = Query_String.ToUpper();
                            DB_Command.Prepare();

                            using (_Query_Data_Src)
                            {
                                String _ReadLine;

                                while (!_Query_Data_Src.EndOfStream)
                                {
                                    _ReadLine = _Query_Data_Src.ReadLine();

                                    string[] _ReadLine_Split = _ReadLine.Split(new string[] { "" + Query_Data_Fieds_Separator + "" }, StringSplitOptions.None);

                                    List<SQLiteParameter> _SQLiteParameters_List = new List<SQLiteParameter> { };

                                    for (int i = 0; i <= Query_Data_Fieds.Count - 1; i++)
                                    {
                                        SQLiteParameter _SQLiteParameter = new SQLiteParameter();

                                        _SQLiteParameter.ParameterName = "@" + Query_Data_Fieds[i].ToUpper();
                                        _SQLiteParameter.Value = _ReadLine_Split[i];
                                        _SQLiteParameters_List.Add(_SQLiteParameter);
                                    }

                                    DB_Command.Parameters.AddRange(_SQLiteParameters_List.ToArray());

                                    DB_Command.ExecuteNonQuery();

                                    DB_Command.Parameters.Clear();

                                    _Nb_Records_Insert++;
                                }
                            }
                            
                            AD_Sqlite_DB_Transaction(DB_Transaction, "COMMIT", "INFO", "Bulk Query");
                            DB_Command.Dispose();
                        }

                        DB_Transaction.Dispose();
                    }
                }

                return _Nb_Records_Insert;
            }
            catch (Exception _Exception)
            {
                Tools.Log.AD_Logger_Tools.Log_Write("ERROR", _Exception, new StackTrace(true));
                Tools.Log.AD_Logger_Tools.Log_Write("ERROR", "QUERY = " + Query_String.Replace("|", "PIPE"));
            }
            finally
            {
                _Query_Data_Src.Close();
                _Query_Data_Src.Dispose();
                _Query_Data_Src = null;

                AD_SQLite_DB_Connexion("CLOSE", Data_Source, 0);

                AD_Sqlite_DB_Query_Log("--");
            }

            return _Nb_Records_Insert;
        }

        private static void AD_Sqlite_DB_Connexion_Log(string Log_Type)
        {
            switch (Log_Type.ToUpper())
            {
                case "++":
                    CtrlPc_Service.AD_Sqlite_DB_Connexion_Active++;
                    CtrlPc_Service.AD_Sqlite_DB_Connexion_Totale++;
                    break;

                case "--":
                    CtrlPc_Service.AD_Sqlite_DB_Connexion_Active--;
                    break;
            }
        }

        private static void AD_Sqlite_DB_Query_Log(string Log_Type)
        {
            switch (Log_Type.ToUpper())
            {
                case "++":
                    CtrlPc_Service.AD_Sqlite_DB_Query_Active++;
                    CtrlPc_Service.AD_Sqlite_DB_Query_Totale++;
                    break;

                case "--":
                    CtrlPc_Service.AD_Sqlite_DB_Query_Active--;
                    break;
            }
        }

        public static SQLiteTransaction AD_Sqlite_DB_Transaction(SQLiteTransaction DB_Transaction, string DB_Transaction_Command, string DB_Transaction_Msg_Type, string DB_Transaction_Msg)
        {
            try
            {
                switch (DB_Transaction_Command.ToUpper())
                {
                    case "BEGIN":
                        Tools.Log.AD_Logger_Tools.Log_Write(DB_Transaction_Msg_Type, DB_Transaction_Msg + " : Begin Transaction ...");

                        CtrlPc_Service.AD_Sqlite_DB_Transaction_Active++;
                        CtrlPc_Service.AD_Sqlite_DB_Transaction_Totale++;

                        break;

                    case "COMMIT":
                        Tools.Log.AD_Logger_Tools.Log_Write(DB_Transaction_Msg_Type, DB_Transaction_Msg + " : Commit Transaction ...");

                        DB_Transaction.Commit();

                        CtrlPc_Service.AD_Sqlite_DB_Transaction_Commited++;
                        CtrlPc_Service.AD_Sqlite_DB_Transaction_Active--;

                        break;

                    case "ROLLBACK":
                        Tools.Log.AD_Logger_Tools.Log_Write(DB_Transaction_Msg_Type, DB_Transaction_Msg + " : Rollback Transaction !!!");

                        DB_Transaction.Rollback();

                        CtrlPc_Service.AD_Sqlite_DB_Transaction_Rollbacked++;
                        CtrlPc_Service.AD_Sqlite_DB_Transaction_Active--;

                        break;
                }
            }
            catch (Exception _Exception)
            {
                Tools.Log.AD_Logger_Tools.Log_Write("ERROR", _Exception, new StackTrace(true));
            }

            return DB_Transaction;
        }

        private static void AD_Sqlite_DB_Query_Error(string Query_String, SQLiteException SQLite_Exception)
        {
            try
            {
                switch (SQLite_Exception.ErrorCode)
                {
                    case 19:
                        Tools.Log.AD_Logger_Tools.Log_Write("ERROR", SQLite_Exception.ErrorCode.ToString() + " : " + SQLite_Exception.Message);
                        Tools.Log.AD_Logger_Tools.Log_Write("ERROR", "Query = " + Query_String);
                        break;

                    default:
                        Tools.Log.AD_Logger_Tools.Log_Write("ERROR", SQLite_Exception, new StackTrace(true));
                        break;
                }

            }
            catch (Exception _Exception)
            {
                Tools.Log.AD_Logger_Tools.Log_Write("ERROR", _Exception, new StackTrace(true));
            }
        }

    }
}
