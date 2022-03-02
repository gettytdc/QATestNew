using BPServer;
using System;
using System.Collections;
using System.Runtime.Remoting.Contexts;
using System.Runtime.Remoting.Messaging;
using System.Text;

namespace BluePrism.BPServer.ServerBehaviours
{
    /// <summary>
    /// Class requireed as part of the detatiled logging of remoting messages.
    /// This receives the messages.
    /// </summary>
    internal class clsDynamicMessageSink : IDynamicMessageSink
    {
        private clsBPServer mServer;

        public clsDynamicMessageSink(clsBPServer parent)
        {
            mServer = parent;
        }

        private string MessageInfo(IMessage msg)
        {
            StringBuilder sb = new StringBuilder();
            Type msgType = msg.GetType();
            sb.Append(msgType.ToString());
            IDictionaryEnumerator e = msg.Properties.GetEnumerator();
            while (e.MoveNext())
            {
                string k = (string)e.Key;
                if (k == "__MethodName")
                {
                    sb.Append(String.Format(" {0}", e.Value));
                }
                else if (k == "__TypeName")
                {
                    // No need for this - always something like:
                    //    "BluePrism.AutomateAppCore.clsServer, AutomateAppCore, Version=4.2.999.999, Culture=neutral, PublicKeyToken=null"
                    // sb.Append(String.Format(" TypeName:{0}", e.Value));
                }
            }
            return sb.ToString();
        }

        public void ProcessMessageStart(IMessage reqMsg, bool bClientSide, bool bAsync)
        {
            mServer.OnVerbose("Messaging: Start " + MessageInfo(reqMsg));
        }

        public void ProcessMessageFinish(IMessage replyMsg, bool bClientSide, bool bAsync)
        {
            // No need for these at the moment, there will always be a finish corresponding
            // to each start.
            //mServer.OnVerbose("Messaging: Finish " + MessageInfo(replyMsg));
        }
    }


}
