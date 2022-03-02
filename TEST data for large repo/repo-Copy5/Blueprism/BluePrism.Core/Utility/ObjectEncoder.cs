using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Linq;
using System.Reflection;

namespace BluePrism.Core.Utility
{
    public static class ObjectEncoder
    {
        public static string ObjectToBase64String<T>(T obj) where T : class
        {
            if (obj is null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, obj);
                return Convert.ToBase64String(ms.ToArray());
            }
        }

        public static T Base64StringToObject<T>(string base64String) where T : class
        {
            if (string.IsNullOrEmpty(base64String))
            {
                throw new ArgumentException($"'{nameof(base64String)}' cannot be null or empty", nameof(base64String));
            }

            var bytes = Convert.FromBase64String(base64String);
            using (var ms = new MemoryStream(bytes, 0, bytes.Length))
            {
                ms.Write(bytes, 0, bytes.Length);
                ms.Position = 0;
                var binaryFormatter = new BinaryFormatter()
                {
                    Binder = new BinaryFormatterWhiteList()
                };
                var deserializedObject = binaryFormatter.Deserialize(ms);
                if(!(deserializedObject is T))
                {
                    throw new InvalidOperationException($"Unable to cast to type {typeof(T)}");
                }
                return deserializedObject as T;
            }
        }
    }

    internal sealed class BinaryFormatterWhiteList : SerializationBinder
    {
        private readonly string[] _whiteList = {"System.Guid",
                                                "BluePrism.AutomateProcessCore.clsProcessValue",
                                                "BluePrism.AutomateAppCore.clsSessionVariable",
                                                "BluePrism.Core.UnitTests.Utility.ObjectEncoderTests+TestObject",
                                                "BluePrism.Common.Security.SafeString"};

        public override Type BindToType(string assemblyName, string typeName)
        {
            if(!_whiteList.Any(t=>t==typeName ))
            {
                throw new SerializationException($"Only type { string.Join(",",_whiteList.Select(s=>s.ToString()).ToArray())} are allowed");
            }
            return Assembly.Load(assemblyName).GetType(typeName);
        }
    }
}
