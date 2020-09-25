using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Xml.Xsl;
using System.IO;

namespace Dib.Transform
{
    
    class transformDocument
    {
        
        public static void TransformXmlWithXsl(string doc
                    , string xslFilePath
                    , XsltArgumentList args
                    , string outputPath)
        {



            StringBuilder output = new StringBuilder();
            XmlReader rawData = XmlReader.Create(doc);
            

            XmlWriterSettings writerSettings = new XmlWriterSettings();
            writerSettings.OmitXmlDeclaration = false;

            XsltSettings settings = new XsltSettings(false, true);
            settings.EnableDocumentFunction = true;

            using (XmlWriter transformedData = XmlWriter.Create(output, writerSettings))
            {
                XslCompiledTransform transform = new XslCompiledTransform();
                transform.Load(xslFilePath, settings, new XmlUrlResolver());
                transform.Transform(rawData, args, transformedData);

                FileStream fs = new FileStream(outputPath, FileMode.Create, FileAccess.Write);
                StreamWriter s = new StreamWriter(fs, Encoding.UTF8);
                s.BaseStream.Seek(0, SeekOrigin.End);
                s.Write(output.ToString());
                s.Close();

            } 

        }

        public static void TransformXmlWithXsl(string doc
                    , string xslFilePath
                    , string outputPath)
        {



            StringBuilder output = new StringBuilder();
            XmlReader rawData = XmlReader.Create(doc);


            XmlWriterSettings writerSettings = new XmlWriterSettings();
            writerSettings.OmitXmlDeclaration = true;

            XsltSettings settings = new XsltSettings(false, true);
            settings.EnableDocumentFunction = false;

            using (XmlWriter transformedData = XmlWriter.Create(output, writerSettings))
            {
                XslCompiledTransform transform = new XslCompiledTransform();
                transform.Load(xslFilePath, settings, new XmlUrlResolver());
                transform.Transform(rawData, transformedData);

                FileStream fs = new FileStream(outputPath, FileMode.Create, FileAccess.Write);
                StreamWriter s = new StreamWriter(fs, Encoding.UTF8);
                s.BaseStream.Seek(0, SeekOrigin.End);
                s.Write(output.ToString());
                s.Close();

            }

        }

        public static void TransformXmlWithXsl(string outPath, XmlDocument xdoc, string filename, string xsltPath)
        {
            XslCompiledTransform xmlTransform = new XslCompiledTransform();
            MemoryStream XmlOutput = new MemoryStream();
            xmlTransform.Load(xsltPath);

            xmlTransform.Transform(xdoc.CreateNavigator(), new XsltArgumentList(), XmlOutput);


            XmlOutput.Seek(0, SeekOrigin.Begin);
            string retVal = Encoding.UTF8.GetString(XmlOutput.ToArray());


            string modFileName = outPath + "\\" + filename;
            if (File.Exists(modFileName)) File.Delete(modFileName);

            StreamWriter SW;
            SW = File.CreateText(modFileName);
            SW.Write(retVal.Trim());
            SW.Close();


        }


        public static string TransformXmlWithXslToString(XmlDocument xdoc, string xsltPath, XsltArgumentList args)
        {
            XslCompiledTransform xmlTransform = new XslCompiledTransform();
            MemoryStream XmlOutput = new MemoryStream();
            xmlTransform.Load(xsltPath);

            xmlTransform.Transform(xdoc.CreateNavigator(), args, XmlOutput);


            XmlOutput.Seek(0, SeekOrigin.Begin);
            string retVal = Encoding.UTF8.GetString(XmlOutput.ToArray());

            return retVal;


        }

        public static string TransformXmlWithXslToString(XDocument xdoc, string xsltPath, XsltArgumentList args)
        {
            XslCompiledTransform xmlTransform = new XslCompiledTransform();
            MemoryStream XmlOutput = new MemoryStream();

            XsltSettings settings = new XsltSettings(false, true);
            settings.EnableDocumentFunction = true;
            xmlTransform.Load(xsltPath, settings, new XmlUrlResolver());
            
            xmlTransform.Transform(xdoc.CreateNavigator(), args, XmlOutput);


            XmlOutput.Seek(0, SeekOrigin.Begin);
            string retVal = Encoding.UTF8.GetString(XmlOutput.ToArray());

            return retVal;


        }

        public static string TransformXmlWithXslToString(XmlReader reader, string xsltPath, XsltArgumentList args)
        {
            XslCompiledTransform xmlTransform = new XslCompiledTransform();
            MemoryStream XmlOutput = new MemoryStream();
            xmlTransform.Load(xsltPath);

            xmlTransform.Transform(reader, args, XmlOutput);


            XmlOutput.Seek(0, SeekOrigin.Begin);
            string retVal = Encoding.UTF8.GetString(XmlOutput.ToArray());

            return retVal;


        }

    }

}
