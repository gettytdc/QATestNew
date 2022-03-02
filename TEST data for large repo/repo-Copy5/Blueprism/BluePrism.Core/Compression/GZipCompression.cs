using BluePrism.Core.Properties;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;

namespace BluePrism.Core.Compression
{
    public class GZipCompression
    {

        public static byte[] Compress(string input)
        {
            var raw = Encoding.Unicode.GetBytes(input);
            using (var memory = new MemoryStream())
            {
                using (var gzip = new GZipStream(memory, CompressionMode.Compress))
                {
                    gzip.Write(raw, 0, raw.Length);
                    gzip.Close();
                }
                return memory.ToArray();
            }
        }

        public static string Decompress(byte[] gzip)
        {
            const int size = 4096;
            using (var stream = new GZipStream(new MemoryStream(gzip), CompressionMode.Decompress))
            {
                var buffer = new byte[4097];
                using (var memory = new MemoryStream())
                {
                    var count = 0;
                    do
                    {
                        count = stream.Read(buffer, 0, size);
                        if (count > 0)
                        {
                            memory.Write(buffer, 0, count);
                        }
                    }
                    while (count > 0);
                    return Encoding.Unicode.GetString(memory.ToArray());
                }
            }
        }

        public static byte[] SerializeAndCompress<T>(T data)
        {
            var serializer = new DataContractSerializer(typeof(T));
            using (var stream = new MemoryStream())
            {
                using (var compress = new GZipStream(stream, CompressionMode.Compress))
                {
                    serializer.WriteObject(compress, data);
                }
                return stream.ToArray();
            }
        }

        public static T InflateAndDeserialize<T>(byte[] serializedData)
        {
            if (serializedData == null || serializedData.Length == 0)
            {
                throw new System.ArgumentException(Resource.GZipCompression_CannotDeserializeEmptyString);
            }

            var serializer = new DataContractSerializer(typeof(T));
            using (var stream = new MemoryStream(serializedData))
            {
                using (var decompress = new GZipStream(stream, CompressionMode.Decompress))
                {
                    using (var xmlReader = XmlReader.Create(decompress))
                    {
                        return (T)serializer.ReadObject(xmlReader, true);
                    }
                }
            }
        }
    }
}