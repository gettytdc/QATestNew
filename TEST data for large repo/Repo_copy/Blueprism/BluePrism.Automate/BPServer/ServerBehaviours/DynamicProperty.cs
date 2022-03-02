using BPServer;
using System;
using System.Runtime.Remoting.Contexts;

namespace BluePrism.BPServer.ServerBehaviours
{
    /// <summary>
    /// Class requireed as part of the detatiled logging of remoting messages.
    /// The only useful thing this does is return a clsDynamicMessageSink
    /// instance when asked to.
    /// </summary>
    internal class DynamicProperty : IDynamicProperty, IContributeDynamicSink
    {
        private clsBPServer mServer;

        public DynamicProperty(clsBPServer parent)
        {
            mServer = parent;
        }

        //          public clsDynamicProperty(MarshalByRefObject obj, Context ctx)
        //          {
        //          }

        public String Name
        {
            get
            {
                return "BPServer.clsDynamicProperty";
            }
        }

        public IDynamicMessageSink GetDynamicSink()
        {
            return new clsDynamicMessageSink(mServer);
        }
    }

}
