using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.ComponentModel;
using System.Diagnostics;

namespace ccLib_netCore
{
    public class ExtProcCmdStruct
    {
        [Category("External Process Command")]
        [Description("Path to Working Directory for Execution")]
        [DisplayName("workingDirString")]
        public string workingDirString { set; get; } = "";
        [Category("External Process Command")]
        [Description("Path to Process Executable")]
        [DisplayName("cmdString")]
        public string cmdString { set; get; } = "";
        [Category("External Process Command")]
        [Description("Command Line Argument to Process")]
        [DisplayName("cmdArguments")]
        public string cmdArguments { set; get; } = "";
        [Category("External Process Command")]
        [Description("Time (ms) to Wait for Process Execution to Complete")]
        [DisplayName("timeOutms")]
        public int timeOutms { set; get; } = 5000;
        [Category("External Process Results")]
        [Description("Results of Process Execution")]
        [DisplayName("outANDerrorResults")]
        public string outANDerrorResults { set; get; } = "";
    }
    public class ExtProcManager
    {
        ExecutionSystem exeSysLink;
        Process procLink = null;
        List<string> stdOutStrings = new List<string>();
        List<string> stdErrStrings = new List<string>();
        AutoResetEvent outputWaitHandle;
        AutoResetEvent errorWaitHandle;
        public ExtProcManager(ExecutionSystem exeSysLinkIn)
        {
            exeSysLink = exeSysLinkIn;
        }

        public void executeCMDS(List<ExtProcCmdStruct> CmdListIn)
        {
            if(procLink==null)
            {
                foreach(ExtProcCmdStruct currentCmdLink in CmdListIn)
                {
                    currentCmdLink.outANDerrorResults = "";
                    stdOutStrings.Clear();
                    stdErrStrings.Clear();
                    outputWaitHandle = new AutoResetEvent(false);
                    errorWaitHandle = new AutoResetEvent(false);
                    procLink = new Process();
                    procLink.StartInfo.UseShellExecute = false;
                    procLink.StartInfo.CreateNoWindow = true;
                    procLink.StartInfo.RedirectStandardOutput = true;
                    procLink.StartInfo.RedirectStandardError = true;
                    procLink.StartInfo.WorkingDirectory = currentCmdLink.workingDirString;
                    procLink.StartInfo.FileName = currentCmdLink.cmdString;
                    procLink.StartInfo.Arguments = currentCmdLink.cmdArguments;
                    procLink.OutputDataReceived += ProcLink_OutputDataReceived;
                    procLink.ErrorDataReceived += ProcLink_ErrorDataReceived;
                    exeSysLink.uComms.EnqueMsgString($"Starting {procLink.StartInfo.FileName} from {procLink.StartInfo.WorkingDirectory} with args= {procLink.StartInfo.Arguments}");
                    procLink.Start();
                    procLink.BeginOutputReadLine();
                    procLink.BeginErrorReadLine();
                    if (
                    procLink.WaitForExit(currentCmdLink.timeOutms) &&
                    outputWaitHandle.WaitOne(currentCmdLink.timeOutms) &&
                    errorWaitHandle.WaitOne(currentCmdLink.timeOutms)
                        )
                    {
                        exeSysLink.uComms.EnqueMsgString($"Finished:Success {procLink.StartInfo.FileName} from {procLink.StartInfo.WorkingDirectory} with args= {procLink.StartInfo.Arguments}");

                        if (stdErrStrings.Count > 0)
                            currentCmdLink.outANDerrorResults += "err:\n";
                        foreach (string s in stdErrStrings)
                            currentCmdLink.outANDerrorResults += s;
                        if (stdOutStrings.Count > 0)
                            currentCmdLink.outANDerrorResults += "out:\n";
                        foreach (string s in stdOutStrings)
                            currentCmdLink.outANDerrorResults += s;
                    }
                    else
                    {
                        exeSysLink.uComms.EnqueMsgString($"Finished:Timeout {procLink.StartInfo.FileName} from {procLink.StartInfo.WorkingDirectory} with args= {procLink.StartInfo.Arguments}");
                        currentCmdLink.outANDerrorResults += "Timed Out: No Results";
                    }
                        

                    procLink.Close();
                    procLink = null;
                }                  
            }
        }

        private void ProcLink_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data == null)
                errorWaitHandle.Set();
            else
                stdErrStrings.Add(e.Data);
        }

        private void ProcLink_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data == null)
            outputWaitHandle.Set();
            else
            stdOutStrings.Add(e.Data);
        }
    }
}
