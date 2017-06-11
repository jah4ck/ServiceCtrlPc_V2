using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Scheduler.Tools.Process
{
    class AD_Process_Tools
    {
        System.Diagnostics.Process Process_Execute = new System.Diagnostics.Process();

        private bool _With_Log_Info;
        private bool _Process_Exec_Killed = false;

        public string Process_Exec_Thread_Name;
        public string Process_Exec_Log_Name;

        public string Process_Exec_STDOUT;
        public string Process_Exec_STDERR;
        public int Process_Exec_ExitCode;
        public DateTime Process_Exec_StartTime;
        public DateTime Process_Exec_EndTime;

        public AD_Process_Tools()
        {

        }

        public Int32 Process_Exec(string Process_To_Call, string Process_Args, string Process_User, string Process_Pwd, int Type_Window, double TimeOut, int TimeSleep, bool With_Kill, bool With_Log_STDOUT, bool With_Log_STDERR, bool With_Log_Info)
        {
            _With_Log_Info = With_Log_Info;

            System.Threading.Thread.CurrentThread.CurrentCulture = CtrlPc_Service.Service_Culture_Info;

            try
            {
                if (!Directory.Exists(CtrlPc_Service.AD_Dir_Tmp))
                {
                    Directory.CreateDirectory(CtrlPc_Service.AD_Dir_Tmp);
                }
                Process_Exec_Thread_Name = System.Threading.Thread.CurrentThread.Name;
                Process_Exec_Log_Name = CtrlPc_Service.Service_Log.Get(CtrlPc_Service.Service_Log_List);

                if (With_Log_Info == true) { Tools.Log.AD_Logger_Tools.Log_Write("INFO", "Process " + Process_To_Call + " /" + Process_Args); }

                // Réinitialisation <> Variables
                Process_Exec_STDOUT = null;
                Process_Exec_STDERR = null;
                Process_Exec_ExitCode = 99;
                Process_Exec_StartTime = DateTime.Parse("1900-01-01 00:00:00");
                Process_Exec_EndTime = DateTime.Parse("1900-01-01 00:00:00");

                ProcessStartInfo Process_Execute_Info = new ProcessStartInfo(Process_To_Call);
                if (Process_Args != null) { Process_Execute_Info.Arguments = Process_Args; }

                // 
                Process_Execute_Info.UseShellExecute = false;
                Process_Execute_Info.CreateNoWindow = true;
                Process_Execute.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                Process_Execute_Info.WorkingDirectory = CtrlPc_Service.AD_Dir_Tmp;

                Process_Execute_Info.RedirectStandardOutput = With_Log_STDOUT;
                Process_Execute_Info.RedirectStandardError = With_Log_STDERR;
                if (With_Log_STDOUT == true) { Process_Execute_Info.StandardErrorEncoding = Encoding.Default; }
                if (With_Log_STDERR == true) { Process_Execute_Info.StandardOutputEncoding = Encoding.Default; }

                if (Process_User != null && Process_User != "") { Process_Execute_Info.UserName = Process_User; };
                if (Process_Pwd != null && Process_Pwd != "") { Process_Execute_Info.Password = Tools.String_Format.AD_String_Format_Tools.String_To_SecureString(Process_Pwd); };

                Process_Execute.StartInfo = Process_Execute_Info;

                // Evts
                Process_Execute.EnableRaisingEvents = true;
                Process_Execute.OutputDataReceived += new DataReceivedEventHandler(Process_Exec_Read_Output);
                Process_Execute.ErrorDataReceived += new DataReceivedEventHandler(Process_Exec_Read_Error);
                Process_Execute.Exited += new System.EventHandler(this.Process_Exec_Ended);

                Process_Execute.Start();

                Process_Exec_StartTime = Process_Execute.StartTime.ToUniversalTime();

                if (!Process_Execute.HasExited)
                {
                    Tools.Log.AD_Logger_Tools.Log_Write("INFO", "Try to Set PriorityClass + ProcessorAffinity ...");

                    try
                    {
                        Process_Execute.PriorityClass = ProcessPriorityClass.BelowNormal;
                        Process_Execute.ProcessorAffinity = new IntPtr(1);

                        Tools.Log.AD_Logger_Tools.Log_Write("INFO", "Set PriorityClass + ProcessorAffinity To Low OK");
                    }
                    catch (Exception _Exception)
                    {
                        Tools.Log.AD_Logger_Tools.Log_Write("INFO", "Set PriorityClass + ProcessorAffinity To Low Impossible : " + _Exception.Message);
                    }
                }

                // 
                if (With_Log_STDOUT == true)
                {
                    Process_Execute.BeginOutputReadLine();
                }

                if (With_Log_STDERR == true)
                {
                    Process_Execute.BeginErrorReadLine();
                }

                //
                while (!Process_Execute.HasExited)
                {
                    if (Process_Exec_StartTime.AddMilliseconds(TimeOut) < DateTime.UtcNow)
                    {
                        if (With_Kill == true & !(Process_Execute.HasExited))
                        {
                            _Process_Exec_Killed = true;

                            System.Threading.Thread.Sleep(TimeSleep);

                            if (With_Log_Info == true) { Tools.Log.AD_Logger_Tools.Log_Write("ERROR", "Kill Process " + Process_To_Call + " / " + Process_Args + " : TimeOut Expiré !!!"); }

                            Tools.Process.AD_Process_Info_Tools.Kill(Process_Execute.Id.ToString(), 90000, true, true);

                            if (With_Log_Info == true) { Tools.Log.AD_Logger_Tools.Log_Write("ERROR", "Process " + Process_To_Call + " / " + Process_Args + " Killé : TimeOut Expiré !!!"); }

                            if (Process_Execute != null)
                            {
                                Tools.Log.AD_Logger_Tools.Log_Write("INFO", "Refresh Process " + Process_To_Call + " / " + Process_Args + " suite Kill ...");

                                Process_Execute.Refresh();

                                Tools.Log.AD_Logger_Tools.Log_Write("INFO", "Wait 60 Secondes suite Kill Process " + Process_To_Call + " / " + Process_Args + " ...");

                                Process_Execute.WaitForExit(60000);

                                if (!(Process_Execute.HasExited))
                                {
                                    Tools.Log.AD_Logger_Tools.Log_Write("INFO", "Force Kill Process " + Process_To_Call + " / " + Process_Args + " car Not Ended suite Kill ...");

                                    Process_Execute.Kill();
                                }
                            }
                        }
                        break;
                    }

                    System.Threading.Thread.Sleep(TimeSleep);
                }

                Process_Execute.WaitForExit();

                Process_Exec_ExitCode = Process_Execute.ExitCode;
                Process_Exec_EndTime = Process_Execute.ExitTime.ToUniversalTime();

                Process_Execute.Close();
            }
            catch (Exception _Exception)
            {
                Tools.Log.AD_Logger_Tools.Log_Write("ERROR", _Exception, new StackTrace(true));
            }
            finally
            {
                Tools.Log.AD_Logger_Tools.Log_Write("INFO", "Code Retour Exécution = " + Process_Exec_ExitCode + "");
            }

            return Process_Exec_ExitCode;
        }

        private void Process_Exec_Ended(object sender, System.EventArgs e)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = CtrlPc_Service.Service_Culture_Info;

            if (String.IsNullOrEmpty(System.Threading.Thread.CurrentThread.Name))
            {
                System.Threading.Thread.CurrentThread.Name = Process_Exec_Thread_Name + "_PROCESS_ENDED";
                CtrlPc_Service.Service_Log.Update(CtrlPc_Service.Service_Log_List, System.Threading.Thread.CurrentThread.Name, Process_Exec_Log_Name, false);
            }

            System.Threading.Thread.Sleep(10000);
        }

        private void Process_Exec_Read_Output(object sender, DataReceivedEventArgs Output_Line)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = CtrlPc_Service.Service_Culture_Info;

            try
            {
                if (String.IsNullOrEmpty(System.Threading.Thread.CurrentThread.Name))
                {
                    System.Threading.Thread.CurrentThread.Name = Process_Exec_Thread_Name + "_PROCESS_STDOUT";
                    CtrlPc_Service.Service_Log.Update(CtrlPc_Service.Service_Log_List, System.Threading.Thread.CurrentThread.Name, Process_Exec_Log_Name, false);
                }

                Process_Exec_Write_Output(Output_Line.Data);
            }
            catch (Exception _Exception)
            {
                Tools.Log.AD_Logger_Tools.Log_Write("ERROR", _Exception, new StackTrace(true));
            }
        }

        private void Process_Exec_Read_Error(object sender, DataReceivedEventArgs Error_Line)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = CtrlPc_Service.Service_Culture_Info;

            try
            {
                if (String.IsNullOrEmpty(System.Threading.Thread.CurrentThread.Name))
                {
                    System.Threading.Thread.CurrentThread.Name = Process_Exec_Thread_Name + "_PROCESS_STDERR";
                    CtrlPc_Service.Service_Log.Update(CtrlPc_Service.Service_Log_List, System.Threading.Thread.CurrentThread.Name, Process_Exec_Log_Name, false);
                }

                Process_Exec_Write_Error(Error_Line.Data);
            }
            catch (Exception _Exception)
            {
                Tools.Log.AD_Logger_Tools.Log_Write("ERROR", _Exception, new StackTrace(true));
            }
        }

        private void Process_Exec_Write_Output(string Process_Exec_Read_Output)
        {
            try
            {
                if (Process_Exec_Read_Output != null && _With_Log_Info == true)
                {
                    if (Process_Exec_Read_Output.Trim() != "")
                    {
                        if (Process_Exec_STDOUT == null || Process_Exec_STDOUT == "")
                        {
                            Process_Exec_STDOUT = Process_Exec_Read_Output;
                        }
                        else
                        {
                            Process_Exec_STDOUT = Process_Exec_STDOUT + "\n" + Process_Exec_Read_Output;
                        }

                        Tools.Log.AD_Logger_Tools.Log_Write("INFO", Process_Exec_Read_Output);
                    }
                }
            }
            catch (Exception _Exception)
            {
                Tools.Log.AD_Logger_Tools.Log_Write("ERROR", _Exception, new StackTrace(true));
            }
        }

        private void Process_Exec_Write_Error(string Process_Exec_Read_Error)
        {
            try
            {
                if (Process_Exec_Read_Error != null && _With_Log_Info == true)
                {
                    if (Process_Exec_Read_Error.Trim() != "")
                    {
                        if (Process_Exec_STDERR == null || Process_Exec_STDERR == "")
                        {
                            Process_Exec_STDERR = Process_Exec_Read_Error;
                        }
                        else
                        {
                            Process_Exec_STDERR = Process_Exec_STDERR + "\n" + Process_Exec_Read_Error;
                        }

                        Tools.Log.AD_Logger_Tools.Log_Write("ERROR", Process_Exec_Read_Error);
                    }
                }
            }
            catch (Exception _Exception)
            {
                Tools.Log.AD_Logger_Tools.Log_Write("ERROR", _Exception, new StackTrace(true));
            }
        }

        public void Process_Exec_Reset()
        {
            try
            {
                Process_Exec_STDOUT = null;
                Process_Exec_STDERR = null;
            }
            catch (Exception _Exception)
            {
                Tools.Log.AD_Logger_Tools.Log_Write("ERROR", _Exception, new StackTrace(true));
            }
        }

        public void Process_Exec_Dispose()
        {
            Process_Exec_Reset();
            Process_Execute.Dispose();
        }

        public int Process_Exec(string Process_To_Call, string Process_Args, string Process_User, string Process_Pwd)
        {
            try
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = CtrlPc_Service.Service_Culture_Info;

                Tools.Log.AD_Logger_Tools.Log_Write("INFO", "Process " + Process_To_Call + " / " + Process_Args);

                Process_Exec_ExitCode = 99;

                ProcessStartInfo Process_Execute_Info = new ProcessStartInfo(Process_To_Call);
                if (Process_Args != null) { Process_Execute_Info.Arguments = Process_Args; }

                Process_Execute_Info.CreateNoWindow = true;
                Process_Execute.StartInfo.WindowStyle = ProcessWindowStyle.Normal;

                if (Process_User != null) { Process_Execute_Info.UserName = Process_User; };
                if (Process_Pwd != null) { Process_Execute_Info.Password = Tools.String_Format.AD_String_Format_Tools.String_To_SecureString(Process_Pwd); };

                Process_Execute.StartInfo = Process_Execute_Info;
                Process_Execute.Start();

                if (!Process_Execute.HasExited)
                {
                    Tools.Log.AD_Logger_Tools.Log_Write("INFO", "Try to Set PriorityClass + ProcessorAffinity ...");

                    try
                    {
                        Process_Execute.PriorityClass = ProcessPriorityClass.BelowNormal;
                        Process_Execute.ProcessorAffinity = new IntPtr(1);

                        Tools.Log.AD_Logger_Tools.Log_Write("INFO", "Set PriorityClass + ProcessorAffinity To Low OK");
                    }
                    catch (Exception _Exception)
                    {
                        Tools.Log.AD_Logger_Tools.Log_Write("INFO", "Set PriorityClass + ProcessorAffinity To Low Impossible : " + _Exception.Message);
                    }
                }
            }
            catch (Exception _Exception)
            {
                Tools.Log.AD_Logger_Tools.Log_Write("ERROR", _Exception, new StackTrace(true));
            }

            return Process_Exec_ExitCode;
        }
    }
}
