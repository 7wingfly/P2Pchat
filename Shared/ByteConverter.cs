// Written by Benjamin Watkins 2015
// watkins.ben@gmail.com

using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace Shared
{
    public static class ByteConverter
    {
        public static byte[] ToByteArray(this IP2PBase clientInfo)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream Stream = new MemoryStream();

            formatter.Serialize(Stream, clientInfo);
            return Stream.ToArray();
        }

        public static IP2PBase ToP2PBase(this byte[] bytes)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream Stream = new MemoryStream();

            Stream.Write(bytes, 0, bytes.Length);
            Stream.Seek(0, SeekOrigin.Begin);

            IP2PBase clientInfo = (IP2PBase)formatter.Deserialize(Stream);

            return clientInfo;
        }
    }
}
