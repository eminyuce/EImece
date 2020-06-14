using System;
using System.Xml;
using System.Xml.Serialization;

namespace EImece.Domain.Helpers
{
    public class XmlParserHelper
    {
        public static string ToXml<T>(T objectToParse) where T : class, new()
        {
            if (objectToParse == null)
                throw new Exception("Unable to parse a object which is null.", new ArgumentNullException("objectToParse"));

            var stringwriter = new System.IO.StringWriter();
            var serializer = new XmlSerializer(typeof(T));
            try
            {
                XmlSerializerNamespaces xmlnsEmpty = new XmlSerializerNamespaces(new[]
               {
                new XmlQualifiedName(string.Empty, string.Empty),
            });

                serializer.Serialize(stringwriter, objectToParse, xmlnsEmpty);
            }
            catch (Exception e)
            {
                throw new Exception(string.Format("Unable to serialize the object {0}.", objectToParse.GetType()), e);
            }

            return stringwriter.ToString();
        }

        public static T ToObject<T>(string xmlTextToParse) where T : class, new()
        {
            if (string.IsNullOrEmpty(xmlTextToParse))
            {
                return new T();
            }
            //  throw new Exception("Invalid string input. Cannot parse an empty or null string.", new ArgumentException("xmlTestToParse"));

            var stringReader = new System.IO.StringReader(xmlTextToParse);
            var serializer = new XmlSerializer(typeof(T));
            try
            {
                return serializer.Deserialize(stringReader) as T;
            }
            catch (Exception e)
            {
                throw new Exception(string.Format("Unable to convert to given string into the type {0}. See inner exception for details.", typeof(T)), e);
            }
        }
    }
}