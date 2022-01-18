using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Diagnostics;

namespace ccLib_netCore
{

    public class ExecutionSystem
    {
        public int sysTickDuration { set; get; } = 10;

        Timer timer1;
        BackgroundWorker backgroundWorker1;

        public ExtProcManager ThirdPartyTools; 
        public UniverseCommunicator uComms;

        List<ComputeModule> exeSysModules;

        public ExecutionSystem(List<ComputeModule> exeSysModulesin)
        {
            exeSysModules = exeSysModulesin;
            foreach (ComputeModule cm in exeSysModules)
            {
                cm.exeSysLink = this;
            }
            timer1 = new Timer(new TimerCallback(timer1_Tick));
            timer1.Change(0, sysTickDuration);

            backgroundWorker1 = new BackgroundWorker();
            backgroundWorker1.WorkerReportsProgress = true;
            backgroundWorker1.WorkerSupportsCancellation = true;
            backgroundWorker1.DoWork += BackgroundWorker1_DoWork;
            ThirdPartyTools = new ExtProcManager();
        }

        private void BackgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            foreach (ComputeModule cm in exeSysModules)
                cm.Execute();
        }

        private void timer1_Tick(object sender)
        {
            foreach (ComputeModule cm in exeSysModules)
                cm.SysTick();

            if (!backgroundWorker1.IsBusy)
                backgroundWorker1.RunWorkerAsync();
        }

    }
}
