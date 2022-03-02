using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
// ReSharper disable InconsistentNaming

namespace BluePrism.Core.HttpConfiguration.Interop
{
    /// <summary>
    /// Handles unmanaged calls to httpapi.dll functions
    /// </summary>
    internal class HttpApiWrapper : IDisposable
    {
        private static readonly HTTPAPI_VERSION Version = new HTTPAPI_VERSION(1, 0);
        private bool _initialised = false;

        #region Constants 

        private const uint HTTP_INITIALIZE_CONFIG = 0x00000002;

        #endregion
        
        /// <summary>
        /// Ensures that HttpTerminate httpapi function has been called to 
        /// initialise resources associated with this process
        /// </summary>
        private void EnsureInitialised()
        {
            if (!_initialised)
            {
                uint retVal = HttpInitialize(Version, HTTP_INITIALIZE_CONFIG, IntPtr.Zero);
                ThrowIfError(retVal);
                _initialised = true;
            }
        }

        /// <summary>
        /// If httpapi has been initialised, calls HttpTerminate httpapi function to
        /// clean up any resources associated with this process
        /// </summary>
        private void Terminate()
        {
            if (_initialised)
            {
                HttpTerminate(HTTP_INITIALIZE_CONFIG, IntPtr.Zero);
                _initialised = false;
            }
        }

        /// <summary>
        /// Coordinates multiple calls to the HttpQueryServiceConfiguration function
        /// using the HttpServiceConfigQueryNext query type and returns a sequence 
        /// of results mapped from the pOutputConfigInfo output parameter from each 
        /// call. 
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="configId">The type of configuration being queried</param>
        /// <param name="createInput">A function to create the config information passed to 
        /// HttpQueryServiceConfiguration</param>
        /// <param name="mapResult">A function to create values based on the pOutputConfigInfo
        /// structs returned by the HttpQueryServiceConfiguration function</param>
        /// <returns></returns>
        public List<TResult> QueryMany<TInput, TOutput, TResult>(HTTP_SERVICE_CONFIG_ID configId, 
            Func<uint, TInput> createInput, Func<TOutput, TResult> mapResult)
            where TOutput : struct
        {
            EnsureInitialised();

            var results = new List<TResult>();
            
            // Call in a loop until no more items. The token parameter is incremented as
            // we loop, which controls which item in the list of matching values is returned
            uint token = 0;
            uint returnCode;
            do
            {
                var input = createInput(token);
                IntPtr inputPointer = Marshal.AllocCoTaskMem(
                    Marshal.SizeOf(input.GetType()));
                Marshal.StructureToPtr(input, inputPointer, false);
                IntPtr outputPointer = IntPtr.Zero;

                try
                {
                    int returnLength = 0;
                    int inputConfigInfoSize = Marshal.SizeOf(input);
                    returnCode = HttpQueryServiceConfiguration(IntPtr.Zero,
                        configId,
                        inputPointer,
                        inputConfigInfoSize,
                        outputPointer,
                        returnLength,
                        out returnLength,
                        IntPtr.Zero);
                    if (returnCode == ErrorCodes.ERROR_NO_MORE_ITEMS)
                        break;
                    if (returnCode == ErrorCodes.ERROR_INSUFFICIENT_BUFFER)
                    {
                        // Call is valid, but we didn't know size of output struct
                        // We now know returnLength so we can call it again
                        outputPointer = Marshal.AllocCoTaskMem(returnLength);
                        try
                        {
                            returnCode = HttpQueryServiceConfiguration(IntPtr.Zero,
                                configId,
                                inputPointer,
                                inputConfigInfoSize,
                                outputPointer,
                                returnLength,
                                out returnLength,
                                IntPtr.Zero);

                            ThrowIfError(returnCode);

                            // Map output struct to result type
                            var output = (TOutput) Marshal.PtrToStructure(
                                outputPointer, typeof(TOutput));
                            var resultItem = mapResult(output);
                            results.Add(resultItem);
                            token++;
                        }
                        finally
                        {
                            Marshal.FreeCoTaskMem(outputPointer);
                        }
                    }
                    else
                    {
                        ThrowIfError(returnCode);
                    }
                }
                finally
                {
                    Marshal.FreeCoTaskMem(inputPointer);
                }
            } while (returnCode == ErrorCodes.NOERROR);

            return results;
        }

        /// <summary>
        /// Calls the HttpSetServiceConfiguration function within httpapi.dll with
        /// the specified configuration data. If a configuration already exists for the
        /// specified key it will be deleted using the HttpDeleteServiceConfiguration
        /// function and replaced with the new configuration.
        /// </summary>
        /// <param name="configId">The type of configuration being set</param>
        /// <param name="input">The input supplied to the HttpSetServiceConfiguration function</param>
        public void Set(HTTP_SERVICE_CONFIG_ID configId, object input)
        {
            EnsureInitialised();

            var inputPointer = Marshal.AllocCoTaskMem(Marshal.SizeOf(input.GetType()));
            Marshal.StructureToPtr(input, inputPointer, false);

            try
            {
                uint returnCode = HttpSetServiceConfiguration(IntPtr.Zero,
                    configId,
                    inputPointer,
                    Marshal.SizeOf(input),
                    IntPtr.Zero);

                if (returnCode == ErrorCodes.ERROR_ALREADY_EXISTS)
                {
                    // Configuration that already exists needs to be deleted
                    // then recreated
                    returnCode = HttpDeleteServiceConfiguration(IntPtr.Zero,
                        configId,
                        inputPointer,
                        Marshal.SizeOf(input),
                        IntPtr.Zero);

                    ThrowIfError(returnCode);

                    returnCode = HttpSetServiceConfiguration(IntPtr.Zero,
                        configId,
                        inputPointer,
                        Marshal.SizeOf(input),
                        IntPtr.Zero);

                    ThrowIfError(returnCode);
                }
                else
                {
                    ThrowIfError(returnCode);
                }
            }
            finally
            {
                Marshal.FreeCoTaskMem(inputPointer);
            }
        }

        /// <summary>
        /// Calls the HttpDeleteServiceConfiguration function within httpapi.dll with
        /// the specified configuration data. 
        /// </summary>
        /// <param name="configId">The type of configuration being deleted</param>
        /// <param name="input">The input supplied to the HttpDeleteServiceConfiguration function</param>
        public void Delete(HTTP_SERVICE_CONFIG_ID configId, object input)
        {
            EnsureInitialised();

            var inputPointer = Marshal.AllocCoTaskMem(Marshal.SizeOf(input.GetType()));
            Marshal.StructureToPtr(input, inputPointer, false);

            try
            {
                uint returnCode = HttpDeleteServiceConfiguration(IntPtr.Zero,
                    configId,
                    inputPointer,
                    Marshal.SizeOf(input),
                    IntPtr.Zero);

                ThrowIfError(returnCode);
            }
            finally
            {
                Marshal.FreeCoTaskMem(inputPointer);
            }
        }

        /// <summary>
        /// Throws a Win32Exception if the result code is not NOERROR
        /// </summary>
        /// <param name="result"></param>
        private static void ThrowIfError(uint result)
        {
            if (result != ErrorCodes.NOERROR)
            {
                throw new Win32Exception(Convert.ToInt32(result));
            }
        }

        public void Dispose()
        {
            Terminate();
            GC.SuppressFinalize(this);  
        }
    
        #region Imports 

        [DllImport("httpapi.dll", SetLastError = true)]
        static extern uint HttpInitialize(HTTPAPI_VERSION version, 
            uint flags,
            IntPtr pReserved);

        [DllImport("httpapi.dll", SetLastError = true)]
        static extern uint HttpSetServiceConfiguration(
            IntPtr serviceIntPtr,
            HTTP_SERVICE_CONFIG_ID configId,
            IntPtr pConfigInformation,
            int configInformationLength,
            IntPtr pOverlapped);

        [DllImport("httpapi.dll", SetLastError = true)]
        static extern uint HttpDeleteServiceConfiguration(
            IntPtr serviceIntPtr,
            HTTP_SERVICE_CONFIG_ID configId,
            IntPtr pConfigInformation,
            int configInformationLength,
            IntPtr pOverlapped);

        [DllImport("httpapi.dll", SetLastError = true)]
        static extern uint HttpTerminate(
            uint Flags,
            IntPtr pReserved);

        [DllImport("httpapi.dll", SetLastError = true)]
        static extern uint HttpQueryServiceConfiguration(
            IntPtr serviceIntPtr,
            HTTP_SERVICE_CONFIG_ID configId,
            IntPtr pInputConfigInfo,
            int inputConfigInfoLength,
            IntPtr pOutputConfigInfo,
            int outputConfigInfoLength,
            [Optional]
            out int pReturnLength,
            IntPtr pOverlapped);

        #endregion
    }
}