using System;

namespace BluePrism.ApplicationManager.DDE
{

    /// Project  : Application Manager
    /// Class    : clsDDE
    /// <summary>
    /// Encapsulates a DDE element.
    /// </summary>
    public class clsDDEElement : IDisposable
    {
        /// <summary>
        /// The DDE Client owning this element.
        /// </summary>
        private clsDDEClient mParent;

        /// <summary>
        /// Handle to the string naming the DDE topic of interest.
        /// </summary>
        private IntPtr mTopicHandle;

        /// <summary>
        /// Handle to the string naming the DDE Item of interest.
        /// </summary>
        private IntPtr mItemHandle;


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="ParentDDEClient">Owning DDE client.</param>
        /// <param name="TopicName">The topic name of interest. Must not be null/empty.</param>
        /// <param name="ItemName">The item name of interest. May be null/empty.</param>
        public clsDDEElement(clsDDEClient ParentDDEClient,string TopicName,string ItemName)
        {
            mParent=ParentDDEClient;
            if(!string.IsNullOrEmpty(TopicName))
                mTopicHandle=mParent.GetStringHandle(TopicName);
            else
                throw new ArgumentNullException(nameof(TopicName));
            if(!string.IsNullOrEmpty(ItemName))
                mItemHandle=mParent.GetStringHandle(ItemName);
        }

        /// <summary>
        /// Gets the string value of the element.
        /// </summary>
        /// <returns>Returns the string value, or throws an exception.</returns>
        public string getTextValue()
        {
            if(mItemHandle==IntPtr.Zero)
                throw new InvalidOperationException("Cannot perform getText on a DDE element with no Item Name");

            UInt32 hConv=0;
            try
            {
                hConv=clsDDE.DdeConnect(mParent.mInstance,mParent.mServerHandle,mTopicHandle,IntPtr.Zero);
                if(hConv!=0)
                {
                    int dummy=0;
                    Int32 Trans=clsDDE.DdeClientTransaction(null,0,hConv,mItemHandle,clsDDE.CLIPFORMAT.CF_TEXT,clsDDE.DdeTransaction.XTYP_REQUEST,clsDDE.TransactionTimeout,ref dummy);
                    if(Trans!=0)
                    {
                        //First call returns the length of the data
                        Int32 len=clsDDE.DdeGetData(Trans,null,0,0);
                        if(len!=0)
                        {
                            // Grab the data.
                            byte[] bytes=new byte[len];
                            Int32 RetVal=clsDDE.DdeGetData(Trans,bytes,len,0);
                            if(RetVal!=0)
                            {
                                //The return value often ends with \r\n\0. We trim this
                                string RetString=System.Text.Encoding.ASCII.GetString(bytes);
                                if(!String.IsNullOrEmpty(RetString)&&RetString.EndsWith("\r\n\0"))
                                {
                                    RetString=RetString.Substring(0,RetString.Length-3);
                                }
                                return RetString;
                            }
                            else
                            {
                                clsDDE.DoError(mParent.mInstance,"Failed to retrieve data");
                                return string.Empty;
                            }
                        }
                        else
                        {
                            clsDDE.DoError(mParent.mInstance,"DDE reports data has zero length");
                            return string.Empty;
                        }
                    }
                    else
                    {
                        clsDDE.DoError(mParent.mInstance,"Failed to inititiate DDE transaction");
                        return string.Empty;
                    }
                }
                else
                {
                    clsDDE.DoError(mParent.mInstance,"Failed to inititiate DDE conversation");
                    return string.Empty;
                }
            }
            finally
            {
                if(hConv!=0)
                {
                    clsDDE.DdeDisconnect(hConv);
                }
            }
        }


        /// <summary>
        /// Sets the string value of the element.
        /// </summary>
        /// <param name="Value">The new value to be set.</param>
        public void setTextValue(string Value)
        {
            if(mItemHandle==IntPtr.Zero)
                throw new InvalidOperationException("Cannot perform setText on a DDE element with no Item Name");

            UInt32 hConv=0;
            try
            {
                hConv=clsDDE.DdeConnect(mParent.mInstance,mParent.mServerHandle,mTopicHandle,IntPtr.Zero);
                if(hConv!=0)
                {
                    //Prepare our data being sent
                    byte[] stringbytes=System.Text.Encoding.ASCII.GetBytes(Value);
                    byte[] bytestosend=new byte[stringbytes.Length+1];
                    stringbytes.CopyTo(bytestosend,0);
                    bytestosend[bytestosend.Length-1]=0;

                    int dummy=0;
                    Int32 Trans=clsDDE.DdeClientTransaction(bytestosend,bytestosend.Length,hConv,mItemHandle,clsDDE.CLIPFORMAT.CF_TEXT,clsDDE.DdeTransaction.XTYP_POKE,clsDDE.TransactionTimeout,ref dummy);
                    if(Trans!=0)
                    {
                        return;
                    }
                    else
                    {
                        clsDDE.DoError(mParent.mInstance,"Failed to complete DDE POKE transaction");
                        return;
                    }
                }
                else
                {
                    clsDDE.DoError(mParent.mInstance,"Failed to inititiate DDE conversation");
                    return;
                }
            }
            finally
            {
                if(hConv!=0)
                {
                    clsDDE.DdeDisconnect(hConv);
                }
            }

        }

        /// <summary>
        /// Checks to see if a conversation can be initiated with this element's
        /// Server/Topic pair.
        /// </summary>
        /// <returns>Returns true if a conversation can be successfully established,
        /// false otherwise.</returns>
        public bool checkTopicAvailable()
        {
            UInt32 hConv=0;
            try
            {
                hConv = clsDDE.DdeConnect(mParent.mInstance, mParent.mServerHandle, mTopicHandle, IntPtr.Zero);
                return (hConv != 0);
            }
            finally
            {
                if (hConv != 0)
                {
                    clsDDE.DdeDisconnect(hConv);
                }
            }
        }


        /// <summary>
        /// Executes the command encapsulated by the DDE element.
        /// Throws an exception on error
        /// </summary>
        /// <param name="data">The data to send, as part of the command excution.
        /// <para>If false, then the return value of the API call is not
        /// checked. Allows a workaround for badly behaved applications.
        /// Use false if unsure.</para>
        /// </param>
        public void excecuteCommand(string data,bool noCheck)
        {
            //This command is allowable when ItemName is blank

            UInt32 hConv=0;
            try
            {
                hConv=clsDDE.DdeConnect(mParent.mInstance,mParent.mServerHandle,mTopicHandle,IntPtr.Zero);
                if(hConv!=0)
                {
                    if(data==null)
                        data="";

                    byte[] bytestosend=null;
                    byte[] bytes=System.Text.Encoding.ASCII.GetBytes(data);
                    bytestosend=new byte[bytes.Length+1];
                    bytes.CopyTo(bytestosend,0);
                    bytestosend[bytestosend.Length-1]=0;

                    // PW: some applications behave badly (eg IE7). They
                    // seem to do the command anyway, but not return true.
                    // Hence the nocheck parameter.
                    int transid = 0;
                    Int32 Trans=clsDDE.DdeClientTransaction(bytestosend, bytestosend.Length, hConv, 
                                                             IntPtr.Zero, clsDDE.CLIPFORMAT.CF_TEXT, 
                                                             clsDDE.DdeTransaction.XTYP_EXECUTE, 
                                                             clsDDE.TransactionTimeout, ref transid);
                    if(noCheck||(Trans!=0))
                    {
                        //success
                        return;
                    }
                    else
                    {
                        clsDDE.DoError(mParent.mInstance,"Failed to inititiate DDE transaction");
                    }
                }
                else
                {
                    clsDDE.DoError(mParent.mInstance,"Failed to inititiate DDE conversation");
                }
            }
            finally
            {
                if(hConv!=0)
                {
                    clsDDE.DdeDisconnect(hConv);
                }
            }
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool _disposed;

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            //Nothing to do - strings, etc. get cleaned up by parent client
            _disposed = true;
        }

        ~clsDDEElement()
        {
            Dispose(false);
        }



    }
}
