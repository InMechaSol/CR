using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace ccLib_netCore
{
    public class MsgStruct
    {
        public string ModuleNameFrom { set; get; }
        public string ModuleNameTo { set; get; }
        public string Message { set; get; }
    }
    public class UniverseCommunicator
    {
        public ConcurrentQueue<MsgStruct> ExtMsgQueue = new ConcurrentQueue<MsgStruct>();
        
        // In -> to the Universe
        List<string> InputMessagesList = new List<string>();
        // Out -> from the Universe
        List<string> OutputMessagesList = new List<string>();

        public int lastMessageIndexDisplayed { private set; get; } = 0;
        public int inputMessagesCount { private set; get; } = 0;

        public string OutputMessage2Display()
        {
            if (OutputMessagesList.Count > lastMessageIndexDisplayed)
                return OutputMessagesList[lastMessageIndexDisplayed++];
            else
                return null;
        }
        public void InputMessage2Communicate(string msgString)
        {
            InputMessagesList.Add(msgString);
            inputMessagesCount = InputMessagesList.Count;
        }
        public void EnqueMsgString(string msgIn)
        {
            MsgStruct thisMsg = new MsgStruct();
            thisMsg.Message = msgIn;
            thisMsg.ModuleNameFrom = "";
            thisMsg.ModuleNameTo = "";
            ExtMsgQueue.Enqueue(thisMsg);
        }
        public void cyclicManageMsgQueue()
        {
            if(!ExtMsgQueue.IsEmpty)
            {
                MsgStruct thisMsg;
                if (ExtMsgQueue.TryDequeue(out thisMsg))
                {
                    OutputMessagesList.Add($"{DateTime.Now.ToString("u")}:{""}:{""}:{thisMsg.Message}\n");
                }
            }
        }
    }
}
