using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ccLib_netCore
{
   
    public abstract class ComputeModule
    {
        public bool NoAlarmsNoWarnings { set; get; } = false;
        public ExecutionSystem exeSysLink;
        public void Execute()
        {
            try
            {
                Setup();
            }
            catch
            {

            }
            finally
            {
                for(; ; )
                {
                    try
                    {
                        if (!NoAlarmsNoWarnings)
                            Setup();
                        else
                            Loop();
                    }
                    catch
                    {

                    }
                    finally
                    {
                        System.Threading.Thread.Sleep(exeSysLink.sysTickDuration);
                    }
                }
            }

        }
        public abstract void SysTick();
        protected abstract void Loop();
        protected abstract void Setup();
    }

}
