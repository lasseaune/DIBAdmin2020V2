using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace DIB.BuildHeaderHierachy
{
    class AnalyseXML
    {
        public XElement _DOC = null;
        public AnalyseXML(XElement document)
        {
            _DOC = document;
        }

        public XElement AnalyseResult(ref int idx)
        {
            XElement result = null;
            XElement section = _DOC;
            result = AnalyseBySection(section, ref idx);
            return result;
        }

        private XElement AnalyseBySection(XElement section, ref int idx)
        {
            XElement result = null;
            foreach (XElement next in section.Elements())
            {
                switch (next.Name.LocalName)
                {
                    case "h1":
                    case "h2":
                    case "h3":
                    case "h4":
                    case "h5":
                    case "h6":
                    case "h7":
                    case "h8":
                    case "h9":
                    case "div":
                        if (next.Attribute("inspected")==null)
                            result = next;
                        break;
                    case "document":
                    case "section":
                    case "blokk":
                        result = AnalyseBySection(next, ref idx);
                        break;
                }
                if (result != null) break;
            }
            return result;
        }
    }
}
