using System.Xml.Serialization;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Basisklasse für alle Xml-Erweiterungs-Klassen

namespace XmlBase
{

    public class XmlFilePath
    {
        public XmlFilePath(string path)
        {
            Path = path;
        }

        public string Path { get; set; }
    }

    public abstract class XmlBase : IXml
    {

        /// <summary>
        /// Gets or sets the file.
        /// </summary>
        /// <value>
        /// The file.
        /// </value>
        public string File { get; set; }

        /// <summary>
        /// Loads the specified file into a Xml object.
        /// </summary>
        /// <typeparam name="T">Desired return type</typeparam>
        /// <param name="xmlFilePath">The XML file path.</param>
        /// <returns></returns>
        // Typ 'XmlFilePath' wird verwendet weil 'String' nicht geht, 'Load(string)' wird für das Übergeben des Xml-Strings verwendet (nicht Datei sondern Xml-Inhalt)
        public static T Load<T>(XmlFilePath xmlFilePath) where T : IXml
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));

            T obj;
            using (TextReader reader = new StreamReader(xmlFilePath.Path))
            {
                obj = (T)serializer.Deserialize(reader);
            }
            
            // Geladene Datei sichern
            obj.File = xmlFilePath.Path;
            
            // Xml-Objekt zurückgeben
            return obj;
        }

        /// <summary>
        /// Loads the Xml document specified be a stream into a Xml object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stream">The stream.</param>
        /// <returns></returns>
        public static T Load<T>(Stream stream) where T : IXml
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            T obj = (T)serializer.Deserialize(stream);

            // Geladene Datei ist unbekannt
            obj.File = "<unknown>";

            // Xml-Objekt zurückgeben
            return obj;
        }

        /// <summary>
        /// Loads the Xml document specified be a Xml string into a Xml object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stream">The stream.</param>
        /// <returns></returns>
        public static T Load<T>(string xmlString) where T : IXml
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));

            T obj;
            using (TextReader reader = new StringReader(xmlString))
            {
                obj = (T)serializer.Deserialize(reader);
            }

            // Geladene Datei ist unbekannt
            obj.File = "<unknown>";

            // Xml-Objekt zurückgeben
            return obj;
        }

        /// <summary>
        /// Saves the Xml object to file.
        /// </summary>
        public void Save()
        {
            this.SaveFile(File);
        }

        /// <summary>
        /// Saves the Xml object to file.
        /// </summary>
        /// <param name="file">The file.</param>
        public void Save(string file)
        {
            SaveFile(file);
        }

        /// <summary>
        /// Saves the Xml object to file including the specified prefix lines.
        /// </summary>
        /// <param name="prefixLines">The prefix lines.</param>
        public void Save(List<string> prefixLines)
        {
            SaveFile(File, prefixLines);
        }

        /// <summary>
        /// Saves the Xml object to file including the specified prefix lines.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="prefixLines">The prefix lines.</param>
        public void Save(string file, List<string> prefixLines)
        {
            SaveFile(file, prefixLines);
        }

        /// <summary>
        /// Saves the Xml object to file (internal usage).
        /// </summary>
        /// <param name="file">The file.</param>
        private void SaveFile(string file)
        {
            XmlSerializer serializer = new XmlSerializer(this.GetType());
            using (TextWriter writer = new StreamWriter(file))
            {
                serializer.Serialize(writer, this);
            }
        }

        /// <summary>
        /// Saves the Xml object to file (internal usage).
        /// </summary>
        /// <param name="file">The file.</param>
        private void SaveFile(string file, List<string> prefixLines)
        {
            // TODO: Funktion sollte man sich nochmal anschauen, ob das nicht auch einfacher geht - und wahrscheinlich auch schneller

            XmlSerializer serializer = new XmlSerializer(this.GetType());
            MemoryStream memory = new MemoryStream();
            serializer.Serialize(memory, this);

            // Stream in Einzelzeilen umwandeln
            var str = StreamToString(memory);
            var strs = str.Split(new string[] { "\r\n" }, System.StringSplitOptions.None);

            var writePrefixLinesFinished = false;
            using (StreamWriter sw = new StreamWriter(file, false, System.Text.Encoding.UTF8))
            {
                // Alle Zeilen durchgehen und in Datei schreiben
                foreach (var line in strs)
                {
                    sw.WriteLine(line);

                    if (!writePrefixLinesFinished)
                    {
                        // Nach der 1. Zeile ("<?xml version="1.0" encoding="utf-8"?>") die Prefix-Zeilen ausgeben
                        // Alle Prefix-Zeilen durchgehen
                        foreach (var prefixLine in prefixLines)
                        {
                            sw.WriteLine(prefixLine);
                        }
                        writePrefixLinesFinished = true;
                    }
                }

                sw.Flush();
                sw.Close();
                sw.Dispose();
            }

        }

        /// <summary>
        /// Gets the XML data as a XML string.
        /// </summary>
        /// <returns></returns>
        public string GetAsXmlString()
        {
            XmlSerializer serializer = new XmlSerializer(this.GetType());
            MemoryStream memory = new MemoryStream();
            serializer.Serialize(memory, this);

            // Stream in Einzelzeilen umwandeln
            var str = StreamToString(memory);

            return str;
        }

        /// <summary>
        /// Converts a stream to a string.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <returns></returns>
        private static string StreamToString(Stream stream)
        {
            stream.Position = 0;
            using (StreamReader reader = new StreamReader(stream, System.Text.Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }

    }

}
