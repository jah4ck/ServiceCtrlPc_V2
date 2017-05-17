using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Text;

namespace Scheduler.Tools.Process
{
    class AD_Process_Info_Tools
    {
        CounterSample _startSample;
        PerformanceCounter _Performance_Counter;

        public Process_Info Get_Process_Info_By_PID()
        { 
            Process_Info _Process_Info = new Process_Info();

            return _Process_Info;
        }

        /// Creates a per-process CPU meter instance tied to the current process.
        public AD_Process_Info_Tools()
        {
            String instancename = Get_Current_Process_Instance_Name();
            _Performance_Counter = new PerformanceCounter("Process","% Processor Time", instancename, true);
            Reset_Counter();
        }

        /// Creates a per-process CPU meter instance tied to a specific process.
        public AD_Process_Info_Tools(int pid)
        {
            String instancename = Get_Process_Instance_Name(pid);
            _Performance_Counter = new PerformanceCounter("Process","% Processor Time", instancename, true);
            Reset_Counter();
        }

        /// Resets the internal counter. All subsequent calls to GetCpuUtilization() will 
        /// be relative to the point in time when you called ResetCounter(). This 
        /// method can be call as often as necessary to get a new baseline for 
        /// CPU utilization measurements.
        public void Reset_Counter()
        {
            _startSample = _Performance_Counter.NextSample();
        }

        /// Returns this process's CPU utilization since the last call to ResetCounter().
        public double Get_Cpu_Used()
        {
            CounterSample curr = _Performance_Counter.NextSample();

            double diffValue = curr.RawValue - _startSample.RawValue;
            double diffTimestamp = curr.TimeStamp100nSec - _startSample.TimeStamp100nSec;

            double _Cpu_Used = ((diffValue / diffTimestamp) * 100) / Environment.ProcessorCount;
            return _Cpu_Used;
        }

        private static string Get_Current_Process_Instance_Name()
        {
            System.Diagnostics.Process proc = System.Diagnostics.Process.GetCurrentProcess();
            int pid = proc.Id;
            return Get_Process_Instance_Name(pid);
        }

        private static string Get_Process_Instance_Name(int pid)
        {
            PerformanceCounterCategory _Performance_Counter_Category = new PerformanceCounterCategory("Process");

            string[] instances = _Performance_Counter_Category.GetInstanceNames();
            foreach (string instance in instances)
            {
                using (PerformanceCounter cnt = new PerformanceCounter("Process", "ID Process", instance, true))
                {
                int val = (int) cnt.RawValue;
                if (val == pid)
                {
                   return instance;
                }
                }
            }
            throw new Exception("Could not find performance counter " + 
              "instance name for current process. This is truly strange ...");
        }

        public static void Kill(string Process, Int64 Timeout, bool With_Children, bool With_TaskKill)
        {
            try
            {
                Int32 _Process_Pid = 0;

                if (Int32.TryParse((string)Process, out _Process_Pid) == false)
                {
                    Tools.Log.AD_Logger_Tools.Log_Write("ERROR", "Conversion To Int32 Value = " + Process + " Impossible !!!");
                }
                else
                {
                    if (With_TaskKill == true)
                    {
                        Tools.Log.AD_Logger_Tools.Log_Write("WARN", "Kill Process " + Process + " With Taskkill ...");

                        string _TaskKill_Exe = @"C:\Windows\System32\TaskKill.Exe";

                        Tools.Process.AD_Process_Tools _Kill_Process = new Tools.Process.AD_Process_Tools();

                        _Kill_Process.Process_Exec(_TaskKill_Exe, " /F /T /PID " + _Process_Pid, null, null, 0, Timeout, 1000, true, true, true, true);

                        if (_Kill_Process.Process_Exec_ExitCode == 0)
                        {
                            Tools.Log.AD_Logger_Tools.Log_Write("WARN", "Process " + Process + " Killed");
                        }
                        else
                        {
                            Tools.Log.AD_Logger_Tools.Log_Write("ERROR", "Kill Process " + Process + " Impossible !!!");
                        }
                    }
                    else
                    {
                        ManagementObjectSearcher _Process_ID_Search = new ManagementObjectSearcher("Select * From Win32_Process Where ParentProcessID=" + _Process_Pid);
                        ManagementObjectCollection _Process_ID_List = _Process_ID_Search.Get();

                        foreach (ManagementObject _Process_ID in _Process_ID_List)
                        {
                            Tools.Log.AD_Logger_Tools.Log_Write("WARN", "Kill Process : Id = " + _Process_ID["ProcessID"] + " / Name = " + _Process_ID["Name"] + " / ExecutablePath = " + _Process_ID["ExecutablePath"] + " / CommandLine = " + _Process_ID["CommandLine"] + " ...");

                            Kill(Convert.ToString(_Process_ID["ProcessID"]), 90000, true, false);

                            Tools.Log.AD_Logger_Tools.Log_Write("WARN", "Process Killed : Id = " + _Process_ID["ProcessID"] + " / Name = " + _Process_ID["Name"] + " / ExecutablePath = " + _Process_ID["ExecutablePath"] + " / CommandLine = " + _Process_ID["CommandLine"]);
                        }

                        try
                        {
                            System.Diagnostics.Process _Process = System.Diagnostics.Process.GetProcessById(_Process_Pid);

                            _Process.Kill();

                            Tools.Log.AD_Logger_Tools.Log_Write("WARN", "Process Killed");
                        }
                        catch (Exception _Exception)
                        {
                            Tools.Log.AD_Logger_Tools.Log_Write("ERROR", "Kill Process Impossible : " + _Exception.Message.ToString() + " !!!");
                        }
                    }
                }
            }
            catch (Exception _Exception)
            {
                Tools.Log.AD_Logger_Tools.Log_Write("ERROR", _Exception, new StackTrace(true));
            }
        }
    
    }
    class Process_Info
    {
        public string Name;
        public int ID;
        public bool IsService;
        public string CpuUsage;
        public long OldCpuUsage;
    }
}
