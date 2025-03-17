using System.IO;
using System.Xml.Serialization;

namespace WaveMaker.Helper
{
    public static class XmlHelper
    {
        public static void WriteToXmlFile<T>(T data, string fileName)
        {
            string xmlString = XmlHelper.SerializeToString(data);
            System.IO.File.WriteAllText(fileName, xmlString);
        }

        public static T LoadFromXmlFile<T>(string fileName)
        {
            string xmlString = System.IO.File.ReadAllText(fileName);
            return XmlHelper.LoadFromXmlString<T>(xmlString);
        }

        public static T LoadFromXmlString<T>(string xmlString)
        {
            XmlSerializer deserializer = new XmlSerializer(typeof(T));
            StringReader reader = new StringReader(xmlString);
            object obj = deserializer.Deserialize(reader);
            T data = (T)obj;
            reader.Close();
            return data;
        }

        public static string SerializeToString<T>(T data)
        {
            XmlSerializer ser = new XmlSerializer(typeof(T));
            StringWriter stringWriter = new StringWriter();
            ser.Serialize(stringWriter, data);
            stringWriter.Close();
            return stringWriter.ToString();
        }
    }
}
