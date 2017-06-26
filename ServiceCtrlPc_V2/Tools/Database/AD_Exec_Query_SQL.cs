using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;

namespace ServiceCtrlPc_V2.Tools.Database
{
    public class AD_Exec_Query_SQL
    {
        public static DataSet AD_ExecQuery(string dataSouce, string Query_Command, string Query_String, int Query_TimeOut)
        {

            SQLiteConnectionStringBuilder _Connection_StringBuilder = new SQLiteConnectionStringBuilder();
            _Connection_StringBuilder.BusyTimeout = 300000;
            _Connection_StringBuilder.DataSource = dataSouce;
            _Connection_StringBuilder.DefaultTimeout = 300;
            _Connection_StringBuilder.FailIfMissing = true;
            _Connection_StringBuilder.LegacyFormat = false;
            _Connection_StringBuilder.Pooling = true;
            _Connection_StringBuilder.CacheSize = 4000;
            _Connection_StringBuilder.PageSize = 65536;
            _Connection_StringBuilder.SyncMode = SynchronizationModes.Full;

            DataSet DB_DataSet = new DataSet();


            using (SQLiteConnection myconnection = new SQLiteConnection(_Connection_StringBuilder.ConnectionString))
            {
                myconnection.Open();
                Console.WriteLine(myconnection.State);
                using (SQLiteTransaction mytransaction = myconnection.BeginTransaction())
                {
                    SQLiteCommand DB_Command;
                    DB_Command = new SQLiteCommand(Query_String, myconnection, mytransaction);
                    DB_Command.CommandType = CommandType.Text;
                    DB_Command.CommandTimeout = Query_TimeOut;
                    using (SQLiteCommand mycommand = DB_Command)
                    {

                        using (SQLiteDataAdapter DB_DataAdapter = new SQLiteDataAdapter(mycommand))
                        {
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
                                        Console.WriteLine(_SQLiteException.Message);
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
                                        Console.WriteLine(_SQLiteException.Message);
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
                                        Console.WriteLine(_SQLiteException.Message);
                                    }

                                    break;

                                default:
                                    break;
                            }
                            DB_DataAdapter.Dispose();
                        }
                        DB_Command.Dispose();
                        if (Query_Command.ToUpper() != "SELECT") { DB_DataSet = null; }
                    }
                    mytransaction.Commit();
                }
            }
            return DB_DataSet;
        }
    }
}
