using BluePrism.AutomateAppCore;
using BluePrism.BPCoreLib.Collections;
using BPServer;
using System.Collections.Generic;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Services;

namespace BluePrism.BPServer.ServerBehaviours
{
    /// <summary>
    /// Class that implements the ITrackingHandler interface, allowing us to keep
    /// track of objects that are instantiated on behalf of clients.
    /// </summary>
    internal class TrackingHandler : ITrackingHandler
    {
        private clsSet<clsServer> mClients;
        private object mClientsLock = new object();
        private clsBPServer mServer;

        /// <summary>
        /// Create a new instance of the tracking handler.
        /// </summary>
        /// <param name="parent">The parent clsBPServer instance.</param>
        public TrackingHandler(clsBPServer parent)
        {
            mServer = parent;
            mClients = new clsSet<clsServer>();
        }

        public int GetConnectedClients()
        {
            lock (mClientsLock)
            {
                return mClients.Count;
            }
        }

        public List<clsServer> GetClients()
        {
            lock (mClientsLock)
            {
                List<clsServer> retval = new List<clsServer>(mClients);
                return retval;
            }
        }

        public void MarshaledObject(object obj, ObjRef or)
        {
            if (obj is clsServer)
            {
                lock (mClientsLock)
                {
                    // This event can fire twice - see bug #6689
                    if (mClients.Add((clsServer)obj))
                        mServer.OnVerbose("Tracking: An instance of clsServer was marshaled.");
                }
            }
        }

        public void UnmarshaledObject(object obj, ObjRef or)
        {
            if (obj is clsServer)
            {
                mServer.OnVerbose("Tracking: An instance of clsServer was unmarshaled.");
            }
        }

        public void DisconnectedObject(object obj)
        {
            if (obj is clsServer)
            {
                mServer.OnVerbose("Tracking: An instance of clsServer was disconnected.");
                lock (mClientsLock)
                {
                    mClients.Remove((clsServer)obj);
                }
            }
        }

    }


}
