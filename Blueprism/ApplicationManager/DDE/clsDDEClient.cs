using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace BluePrism.ApplicationManager.DDE
{

    /// Project  : Application Manager
    /// Class    : clsDDE
    /// <summary>
    /// Encapsulates a DDE client.
    /// </summary>
    public class clsDDEClient : IDisposable
    {
        /// <summary>
        /// The handle returned by the DDE API to this communication instance.
        /// </summary>
        internal UInt32 mInstance;

        /// <summary>
        /// Handle to the string naming the server of interest.
        /// </summary>
        internal IntPtr mServerHandle;

        /// <summary>
        /// Important to manage reference to this callback, because don't want
        /// garbage collector to hoover it up whilst the remote API might
        /// still use this.
        private clsDDE.DdeCallback mCallback;


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="ServerName">The name of the server of interest.</param>
        public clsDDEClient(string ServerName)
        {
            mAllStrings=new Dictionary<string,IntPtr>();

            mCallback=new clsDDE.DdeCallback(OurDdeCallback);
            clsDDE.DMLERR RetVal=(clsDDE.DMLERR)clsDDE.DdeInitialize(ref mInstance,mCallback,(Int32)clsDDE.AfCmd.APPCMD_CLIENTONLY,0);
            switch(RetVal)
            {
                case clsDDE.DMLERR.DMLERR_NO_ERROR:
                    mServerHandle=this.GetStringHandle(ServerName);
                    return;

                //All other possibilities are errors
                case clsDDE.DMLERR.DMLERR_DLL_USAGE:
                    throw new InvalidOperationException("Failure from DdeInitialize - "+((clsDDE.DMLERR)RetVal).ToString());
                case clsDDE.DMLERR.DMLERR_SYS_ERROR:
                    throw new InvalidOperationException("Failure from DdeInitialize - "+((clsDDE.DMLERR)RetVal).ToString());
                case clsDDE.DMLERR.DMLERR_INVALIDPARAMETER:
                    throw new InvalidOperationException("Failure from DdeInitialize - "+((clsDDE.DMLERR)RetVal).ToString());
                default:
                    throw new InvalidOperationException("Unrecognised return value from DdeInitialize - "+RetVal.ToString());
            }
        }


        /// <summary>
        /// The DDE callback function.
        /// </summary>
        internal IntPtr OurDdeCallback(clsDDE.DdeTransaction uType,Int32 uFmt,UInt32 hConv,Int32 hsz1,Int32 hsz2,Int32 hData,IntPtr dwData1,IntPtr dwData2)
        {
            return IntPtr.Zero;
        }

        /// <summary>
        /// Gets a list of servers, topics, items available.
        /// </summary>
        /// <returns>Returns a list of servers, topics and available items, one
        /// triplet per line.</returns>
        public string BrowseServers()
        {
            IntPtr SystemHandle=GetStringHandle("System");

            IntPtr ConvList=IntPtr.Zero;
            try
            {
                //This gets us a list of all servers which support the system topic
                ConvList=clsDDE.DdeConnectList(mInstance,IntPtr.Zero,SystemHandle,IntPtr.Zero,IntPtr.Zero);
                if(ConvList!=IntPtr.Zero)
                {
                    IntPtr Conv=IntPtr.Zero;

                    //We generate a list of servers, which supports a number of topics.
                    //Inside each server/topic pair is a list of supported items
                    Dictionary<string,Dictionary<string,List<string>>> List=new Dictionary<string,Dictionary<string,List<string>>>();

                    // For each conversation in our list, we can query the items
                    // "Topics" and "TopicItemList", which should be supported
                    // by the system topic.
                    do
                    {
                        Conv=clsDDE.DdeQueryNextServer(ConvList,Conv);
                        if(Conv!=IntPtr.Zero)
                        {
                            //We query the conv info to discover the name of the server
                            clsDDE.CONVINFO ci=new clsDDE.CONVINFO();
                            ci.cb=System.Convert.ToUInt32(Marshal.SizeOf(ci));
                            UInt32 Retval=clsDDE.DdeQueryConvInfo(Conv,clsDDE.QID_SYNC,ref ci);

                            if(Retval!=0)
                            {
                                //Get handle to server name
                                IntPtr hServer=ci.hszSvcPartner;
                                if(hServer==IntPtr.Zero)
                                {
                                    hServer=ci.hszServiceReq;
                                }
                                if(hServer==IntPtr.Zero)
                                {
                                    continue;
                                }

                                //We have now found server. Add the items it supports under
                                //the system topic
                                var ServerName = clsDDE.ReadString(mInstance,hServer);
                                var SysItemsRead = GetElementTextValue(ServerName, "System", "SysItems");

                                Dictionary<string,List<string>> Topics=new Dictionary<string,List<string>>();
                                if(SysItemsRead!=null)
                                {
                                    string[] TempSysItems=SysItemsRead.Replace("\0","").Split('\t');
                                    List<string> SysItems=new List<string>();
                                    foreach(string s in TempSysItems)
                                    {
                                        SysItems.Add(s);
                                    }
                                    Topics.Add("System",SysItems);
                                }

                                //Now we query the topics supported under this server
                                var TopicListRead = GetElementTextValue(ServerName, "System", "Topics");

                                if(TopicListRead!=null)
                                {
                                    string[] SplitTopics=TopicListRead.Replace("\0","").Split('\t');
                                    foreach(string s in SplitTopics)
                                    {
                                        var ItemListRead = GetElementTextValue(ServerName, s, "TopicItemList");
                                        List<string> Items=new List<string>();
                                        if(ItemListRead!=null)
                                        {
                                            string[] SplitItems=ItemListRead.Replace("\0","").Split('\t');
                                            foreach(string item in SplitItems)
                                            {
                                                Items.Add(item);
                                            }
                                        }
                                        if(Topics.ContainsKey(s))
                                        {
                                            foreach(string item in Items)
                                            {
                                                if(!Topics[s].Contains(item))
                                                    Topics[s].Add(item);
                                            }
                                        }
                                        else
                                        {
                                            Topics.Add(s,Items);
                                        }
                                    }

                                    List.Add(ServerName,Topics);
                                }
                            }
                            else
                            {
                                clsDDE.DoError(mInstance,"Didn't get the info for this conv");
                            }
                        }

                    }
                    while(Conv!=IntPtr.Zero);


                    //Finally we generate some output
                    System.Text.StringBuilder Output=new StringBuilder();
                    foreach(KeyValuePair<string,Dictionary<string,List<string>>> kvpServerTopics in List)
                    {
                        foreach(KeyValuePair<string,List<string>> kvpTopicItems in kvpServerTopics.Value)
                        {
                            if(kvpTopicItems.Value.Count>0)
                            {
                                foreach(string item in kvpTopicItems.Value)
                                {
                                    Output.Append("Server: "+kvpServerTopics.Key+", Topic: "+kvpTopicItems.Key+", Item: "+item+"\n");
                                }
                            }
                            else
                            {
                                Output.Append("Server: "+kvpServerTopics.Key+", Topic: "+kvpTopicItems.Key+", Items: (none)\n");
                            }
                        }
                    }
                    return Output.ToString();
                }
                else
                {
                    clsDDE.DoError(mInstance,"Failed to inititiate DDE conversation list");
                    return string.Empty;
                }
            }
            finally
            {
                if(ConvList!=IntPtr.Zero)
                {
                    clsDDE.DdeDisconnectList(ConvList);
                }
            }
        }

        private string GetElementTextValue(string serverName, string topicName, string itemName)
        {
            try
            {
                using (var client = new clsDDEClient(serverName))
                using (var element = new clsDDEElement(client, topicName, itemName))
                {
                    return element.getTextValue();
                }
            }
            catch (Exception ex)
            {
                //swallow the error. This is probably because failed to
                //initiate conversation. This could simply be that the
                //topic is a document, and the server won't list all the 
                //items available on the document
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                return null;
            }
        }


        /// <summary>
        /// Gets a handle to the supplied string, either by looking up its handle if
        /// already registered, or by registering it.
        /// </summary>
        /// <param name="Value">The string of interest.</param>
        /// <returns>Returns a handle to the supplied string.</returns>
        internal IntPtr GetStringHandle(string Value)
        {
            IntPtr Handle=IntPtr.Zero;
            if(!mAllStrings.ContainsKey(Value))
            {
                Handle=RegisterString(Value);
            }
            else
            {
                Handle=mAllStrings[Value];
            }
            return Handle;
        }

        /// <summary>
        /// A lookup of strings/handles.
        /// </summary>
        private Dictionary<string,IntPtr> mAllStrings;

        /// <summary>
        /// Registers the supplied string
        /// </summary>
        /// <param name="Value">The string to be registered.</param>
        /// <returns>Returns the handle to the registered string.</returns>
        private IntPtr RegisterString(string Value)
        {
            IntPtr NewHandle;
            byte[] stringbytes=System.Text.Encoding.ASCII.GetBytes(Value);
            byte[] bytestosend=new byte[stringbytes.Length+1];
            stringbytes.CopyTo(bytestosend,0);
            bytestosend[bytestosend.Length-1]=0;
            NewHandle=clsDDE.DdeCreateStringHandle(mInstance,bytestosend,clsDDE.CodePages.CP_WINANSI);
            if(NewHandle==IntPtr.Zero)
                throw new InvalidOperationException("Failed to register string handle for value "+Value);
            else
                mAllStrings.Add(Value,NewHandle);
            return NewHandle;
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool _disposed;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (mAllStrings != null)
                    {
                        try
                        {
                            foreach (KeyValuePair<string, IntPtr> pair in mAllStrings)
                            {
                                //The string itself is the key, the handle is the value
                                clsDDE.DdeFreeStringHandle(mInstance, pair.Value);
                            }
                            mAllStrings.Clear();
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine("Exception disposing of DDE Client - " + ex.ToString());
                        }
                    }
                    if (mInstance != 0)
                    {
                        clsDDE.DdeUninitialize(mInstance);
                        mInstance = 0;
                        mCallback = null;
                    }
                }
            }
            _disposed = true;
        }

        ~clsDDEClient()
        {
            Dispose(false);
        }


    }
}
