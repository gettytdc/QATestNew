using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.ServiceModel.Channels;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace BluePrism.UnitTesting.TestSupport
{
    /// <summary>
    /// Utility class for service contract methods and such
    /// </summary>
    public static class ServiceUtil
    {
        /// <summary>
        /// The types supported (automatically) by the data contract serializer.
        /// Note that this does not include primitives - they are supported by the
        /// serializer, but frankly it's a lot easier to test if the type is
        /// primitive (<see cref="Type.IsPrimitive"/>) than it is for me to list all
        /// the different primitive types here.
        /// Technically, it will allow all publicly visible types with a no-arg
        /// constructor, but this is more specific.
        /// See https://msdn.microsoft.com/en-us/library/ms731923%28v=vs.110%29.aspx
        /// for the source.
        /// </summary>
        private static readonly ISet<Type> AutoSupportedTypes =
            new HashSet<Type>(){
                typeof(string),
                typeof(DateTime),
                typeof(DateTimeOffset),
                typeof(TimeSpan),
                typeof(Guid),
                typeof(Uri),
                typeof(byte[])
            };



        /// <summary>
        /// Checks if the given type is valid as a 'DataMember' - ie. is it a type
        /// which can be serialized / deserialized by a DataContractSerializer?
        /// </summary>
        /// <param name="tp">The type to check</param>
        /// <returns>True if this method considers the type to be a valid data
        /// member; False otherwise.</returns>
        public static bool IsValidDataMember(Type tp)
        {
            // Primitive types are safe
            if (tp.IsPrimitive) return true;
            // Some other types are also safe
            if (AutoSupportedTypes.Contains(tp)) return true;
            // Enum? I'll allow it.
            if (tp.IsEnum) return true;
            // XmlSerializable? That's fine.
            if (typeof(IXmlSerializable).IsAssignableFrom(tp)) return true;
            // If it's a standard CLR class and it's marked as serializable,
            // that's allowed. (The "System." check is mainly for "Color" which
            // is in a different module to most of the CLR types that we use)
            if (Attribute.IsDefined(tp, typeof(SerializableAttribute)) && (
                tp.Module.ScopeName == "CommonLanguageRuntimeLibrary" ||
                tp.Module.Name.StartsWith("System.")))
                return true;
            // If the type has a [DataContract] attribute, it's been considered
            if (Attribute.IsDefined(tp, typeof(DataContractAttribute)))
                return true;
            // If the generic type is an ICollection then that's okay
            // (technically, it supports IEnumerable, but we have too many classes
            // which implement IEnumerable of something but aren't simple collection
            // classes)
            if (tp.IsGenericType)
            {
                Func<Type, bool> checkColl = (
                    i => i.IsGenericType &&
                         i.GetGenericTypeDefinition() == typeof(ICollection<>)
                );
                if (checkColl(tp) || tp.GetInterfaces().Any(checkColl))
                    return true;
            }
            // Non-generic collections are supported too
            if (tp == typeof(ICollection) ||
                tp.GetInterfaces().Any(i => i == typeof(ICollection)))
                return true;

            // If all of the above fails, it's not valid.
            return false;

        }


        // See http://stackoverflow.com/a/974035/430967 for the following source

        /// <summary>
        /// Uses a <see cref="DataContractSerializer"/> to serialise the object into
        /// memory, then deserialise it again and return the result.  This is useful
        /// in tests to validate that your object is serialisable, and that it
        /// serialises correctly.
        /// </summary>
        public static T DoDataContractRoundTrip<T>(T obj)
        {
            return DoDataContractRoundTrip(obj, null);
        }

        /// <summary>
        /// Uses a <see cref="DataContractSerializer"/> to serialise the object into
        /// memory, then deserialise it again and return the result.  This is useful
        /// in tests to validate that your object is serialisable, and that it
        /// serialises correctly.
        /// </summary>
        /// <remarks>The MaxDepth of the XML Reader is set to 128, which matches
        /// the MaxDepth set against the WCF connection.</remarks>
        public static T DoDataContractRoundTrip<T>(
       T obj, IEnumerable<Type> knownTypes)
        {
            var ser = new DataContractSerializer(obj.GetType(), knownTypes);
            using (var ms = new MemoryStream())
            {
                ser.WriteObject(ms, obj);
                ms.Position = 0;

                using (var xr = XmlDictionaryReader.CreateTextReader(ms, 
                    new XmlDictionaryReaderQuotas { MaxDepth = 128 }))
                {
                    obj = (T)ser.ReadObject(xr);
                }

                return obj;
            }
        }

        /// <summary>
        /// Uses a <see cref="NetDataContractSerializer"/> to serialise the object into
        /// memory, then deserialise it again and return the result.  This is useful
        /// in tests to validate that your object is serialisable, and that it
        /// serialises correctly.
        /// </summary>
        /// <remarks>The MaxDepth of the XML Reader is set to 128, which matches
        /// the MaxDepth set against the WCF connection.</remarks>
        public static T DoNetDataContractRoundTrip<T>(T obj)
        {
            var ser = new NetDataContractSerializer();

            using (var ms = new MemoryStream())
            {
                ser.WriteObject(ms, obj);
                ms.Position = 0;

                using (var xr = XmlDictionaryReader.CreateTextReader(ms,
                    new XmlDictionaryReaderQuotas { MaxDepth = 128 }))
                {
                    obj = (T)ser.ReadObject(xr);
                }

                return obj;
            }
        }

        /// <summary>
        /// Uses a <see cref="BinaryFormatter"/> to serialise the object into
        /// memory, then deserialise it again and return the result.  This is useful
        /// in tests to validate that your object is serialisable, and that it
        /// serialises correctly.
        /// </summary>
        /// <remarks>This is the serialisation used in .NET Remoting</remarks>
        public static T DoBinarySerializationRoundTrip<T>(T obj)
        {
            using (var ms = new MemoryStream())
            {
                BinaryFormatter serialiser = new BinaryFormatter();
                serialiser.Serialize(ms, obj);
                ms.Seek(0, SeekOrigin.Begin);

                return (T)serialiser.Deserialize(ms);
            }

        }


        /// <summary>
        /// Reads the given message - this is here for debugging purposes only.
        /// </summary>
        /// <param name="message">The message to read. When a message is read, it is
        /// effectively invalidated - this method replaces the incoming message with
        /// a new clone of the original which is readable again by the calling
        /// context.</param>
        public static string ReadMessage(ref Message message)
        {
            var msgBuffer = message.CreateBufferedCopy(Int32.MaxValue);
            var msg = msgBuffer.CreateMessage();
            var sb = new StringBuilder();

            using (var xw = XmlWriter.Create(sb))
            {
                msg.WriteMessage(xw);
                xw.Close();
            }

            string msgText = sb.ToString();
            Debug.Print("Got Message:\n{0}", msgText);
            message = msgBuffer.CreateMessage();

            Debugger.Break();

            return msgText;

        }

    }
}
