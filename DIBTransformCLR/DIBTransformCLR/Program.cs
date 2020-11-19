using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace DIBTransformCLR
{
    class Program
    {
        static void Main(string[] args)
        {
            XElement root = XElement.Load(@"D:\_DIBTransformCLR\root88.xml");
            IEnumerable<XElement> documents = root.Elements("document");
            XElement diblink = new XElement(root.Elements("links").FirstOrDefault());

            IEnumerable<XElement> result = documents.ConvertXMLtoHTML5(diblink);
        }
    }
}
