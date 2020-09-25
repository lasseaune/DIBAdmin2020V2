using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Diagnostics;


namespace DIB.RegExp.ExternalStaticLinks
{

    public class InternalStaticLinksEx1
    {
        public int LinkCounter = 0;
        public int MatchCounter = 0;
        private int _SERVER = 0;
        private XElement _RETURNTYPE = null;
        public Regex _INTERNALREGEXP { get; set; }
        
        Regex _regit = new Regex("([^\\x00-\\x7E\\xA1-\\xFF\\x152\\x153\\x160\\x161\\x178\\x192])");
        public InternalStaticLinksEx1(Regex internalRegexp)
        {
            _INTERNALREGEXP = internalRegexp;
        }
        public string _lang = "no";


        private static string HEXREPL(Match m)
        {
            string returnValue = "";
            try
            {
                int x = Convert.ToChar(m.ToString());
                if (x > 32767)
                {
                    x = (x - 65536);
                    x = x & 0xFF;
                    char a = (char)x;
                    returnValue = a.ToString();
                }

                switch (x)
                {
                    case 160:
                    case 8201:
                        returnValue = " ";
                        break;
                    case 8208:
                    case 8209:
                    case 8210:
                    case 8211:
                    case 8212:
                    case 8213:
                        returnValue = "-";
                        break;
                    case 171:
                    case 187:
                    case 8216:
                    case 8217:
                    case 8220:
                    case 8221:
                        {
                            char a = (char)34;
                            returnValue = a.ToString();
                        }
                        break;
                    default:
                        returnValue = m.ToString();
                        break;
                }
            }
            catch (SystemException e)
            {
                returnValue = m.ToString();
            }
            return returnValue;
        }

        private class xReference
        {
            public string topic_id { get; set; }
            public string bm { get; set; }
            public string name { get; set; }
            public xReference(string topic_id, string bm, string name)
            {
                this.topic_id = topic_id;
                this.bm = bm;
                this.name = name;
            }
            public xReference(string topic_id, string name)
            {
                this.topic_id = topic_id;
                this.bm = null;
                this.name = name;
            }

        }
        private class xLink
        {
            public int start { get; set; }
            public int length { get; set; }
            public List<xReference> references = new List<xReference>();
            public xLink(int start, int length)
            {
                this.start = start;
                this.length = length;
            }
        }

        private class xRange
        {
            public string n { get; set; }
            public int start { get; set; }
            public int end { get; set; }
            public XNode node { get; set; }
            public List<xLink> links = new List<xLink>();
            public xRange(string n, int start, int end)
            {
                this.n = n;
                this.start = start;
                this.end = end;
            }
        }

        private List<xRange> GetXTextInRange(XElement e)
        {
            List<xRange> returnValue = new List<xRange>();
            int n = 0;
            int pos = 0;
            foreach (XText t in e.DescendantNodes().OfType<XText>())
            {
                xRange r = new xRange(n.ToString(), pos, pos + t.ToString().Length);
                r.node = t;
                returnValue.Add(r);
                pos = pos + t.ToString().Length;
                n++;
            }
            return returnValue;
        }

        private void ExecuteKapActions(ref ExternalStaticLinksEx1.actionObject ao, ref List<xRange> container)
        {
            try
            {
                if (ao.m.Groups["avsnitt"].Success)
                {
                    foreach (Capture c in ao.m.Groups["kapnum"].Captures)
                    {
                        ActionMarkInternal("kapnum", "KAP" + c.Value, ref ao, ref container);
                    }
                }
            }
            catch
            {
            }
        }

        private void ActionMarkInternal(string groupName, string bookmark, ref ExternalStaticLinksEx1.actionObject ao, ref List<xRange> container)
        {
            try
            {
                int start = 0;
                int end = 0;
                if (ao.type == 0)
                {
                    start = ao.offset + ao.m.Groups[groupName.Split('|').First()].Index;
                    end = ao.offset + ao.m.Groups[groupName.Split('|').Last()].Index + ao.m.Groups[groupName.Split('|').Last()].Length;
                }
                else if (ao.type == 1)
                {
                    start = ao.offset + ao.c.Index;
                    end = ao.offset + ao.c.Index + ao.c.Length;
                }

                if (container.Count() != 0)
                {
                    List<xRange> rl = container.Where(p => p.start <= start
                            && p.end >= end).ToList();
                    xRange range = null;
                    if (rl.Count() == 1)
                    {
                        range = rl.ElementAt(0);
                    }
                    else if (rl.Count() > 1)
                    {
                        string rangeId = rl.Min(p => Convert.ToInt32(p.n).ToString()
                                + "-" + rl.Min(q => Convert.ToInt32(q.n).ToString()));
                        if (container.Where(p => p.n == rangeId).Count() != 0)
                        {
                            range = container.Where(p => p.n == rangeId).First();
                        }
                        else
                        {
                            range = new xRange(rangeId, start, end);
                            container.Add(range);
                        }

                    }
                    if (range != null)
                    {
                        xLink link = new xLink(start, end - start);
                        link.references.Add(new xReference("internal", bookmark, ""));
                        range.links.Add(link);
                    }

                }
                ao.start = end;
            }
            catch (SystemException e)
            {
            }
        }
        private List<xRange> GetRanges(XNode n, ref string textNode)
        {
            List<xRange> rN = null;
            if (n.NodeType == XmlNodeType.Element)
            {
                textNode = ((XElement)n).DescendantNodes().OfType<XText>().Select(p => (string)p.ToString()).StringConcatenate();
                textNode = _regit.Replace(textNode, new MatchEvaluator(HEXREPL), -1);
                rN = GetXTextInRange((XElement)n);
            }
            else if (n.NodeType == XmlNodeType.Text)
            {
                textNode = n.ToString();
                rN = new List<xRange>();
                xRange nRange = new xRange("0", 0, textNode.Length);
                nRange.node = n;
                rN.Add(nRange);
            }
            return rN;
        }

        private XElement MarkRanges(List<xRange> rN)
        {
            XElement returnValue = null;
            List<xRange> Ranges = rN
                            .Where(p => p.links.Count() != 0 && p.n.IndexOf("-") == -1)
                            .OrderBy(p => p.start).ToList();

            List<xRange> splitRanges = rN
                .Where(p => p.links.Count() != 0 && p.n.IndexOf("-") != -1)
                .OrderBy(p => p.start).ToList();

            for (int iR = 0; iR < Ranges.Count(); iR++)
            {
                xRange range = Ranges.ElementAt(iR);
                bool found = false;
                for (int iSR = 0; iSR < splitRanges.Count(); iSR++)
                {
                    xRange splitRange = splitRanges.ElementAt(iSR);
                    string nRange = splitRange.n;
                    if (nRange.Split('-').Where(p => p == range.n).Count() == 0)
                    {
                        found = true;
                    }
                }
                if (!found)
                {
                    XNode nF = rN.Where(p => p.n == range.n).First().node;
                    string nFText = nF.ToString();
                    int nFStart = 0;
                    int rangeStart = range.start;
                    XElement nContainer = new XElement("ncontainer");

                    foreach (xLink link in range.links.OrderBy(p => p.start))
                    {
                        int linkStart = link.start;
                        int linkLength = link.length;
                        if ((linkStart - rangeStart) < nFText.Length)
                        {
                            nContainer.Add(new XText(nFText.Substring(nFStart, (linkStart - rangeStart) - nFStart)));
                        }
                        string linkText = nFText.Substring(linkStart - rangeStart, linkLength);
                        Debug.Print("link: " + linkText);
                        CreateLinksInternal(nContainer, link, linkText);
                        //nContainer.Add(link.Nodes());
                        nFStart = (linkStart - rangeStart) + linkLength;

                    }
                    if (nContainer.HasElements)
                    {
                        if (nFStart < nFText.Length)
                            nContainer.Add(new XText(nFText.Substring(nFStart, nFText.Length - nFStart)));

                        if (((XElement)nF.Parent).Attributes(XNamespace.Xml + "space").Count() == 0)
                        {
                            XAttribute a = new XAttribute(XNamespace.Xml + "space", "preserve");
                            ((XElement)nF.Parent).Add(a);
                        }

                        nF.ReplaceWith(nContainer.Nodes());
                        returnValue = new XElement("x", nContainer.Nodes());
                    }

                }
            }
            return returnValue;
        }

        public XElement GetReferanceStringTextpart(XNode n)
        {
            XElement returnValue = new XElement("x", n);

            try
            {
                if (!(n.NodeType == XmlNodeType.Element || n.NodeType == XmlNodeType.Text)) return returnValue;

                string textNode = "";
                List<xRange> rN = GetRanges(n, ref textNode);

                if ((rN == null ? 0 : rN.Count()) == 0) return returnValue;

                MatchCollection mc = _INTERNALREGEXP.Matches(textNode);
                int start = 0;
                int count = mc.Count;
                if (count != 0)
                {
                    for (int i = 0; i < count; i++)
                    {
                        Match m = mc[i];
                        Debug.Print("Match: " + m.Value);
                        MatchCounter++;
                        ExternalStaticLinksEx1.actionObject ao = new ExternalStaticLinksEx1.actionObject(textNode, m, start);
                        ExecuteKapActions(ref ao, ref rN);
                        start = ao.start;

                    }
                    if (rN
                        .Where(p => p.links.Count() != 0).Count() != 0)
                    {
                        XElement markedRanges = MarkRanges(rN);
                        if (markedRanges != null)
                            returnValue = markedRanges;
                    }
                }
            }
            catch (SystemException e)
            {
                throw new Exception(string.Format("GetReferanceString Error: {0}", e.Message.ToString()));
            }
            return returnValue;
        }

        private XElement GetReferanceStringInternal(XNode n)
        {
            XElement returnValue = new XElement("x", n);

            try
            {
                if (!(n.NodeType == XmlNodeType.Element || n.NodeType == XmlNodeType.Text)) return returnValue;

                string textNode = "";
                List<xRange> rN = GetRanges(n, ref textNode);

                if ((rN == null ? 0 : rN.Count()) == 0) return returnValue;

                MatchCollection mc = _INTERNALREGEXP.Matches(textNode);
                int start = 0;
                int count = mc.Count;
                if (count != 0)
                {
                    for (int i = 0; i < count; i++)
                    {
                        Match m = mc[i];
                        Debug.Print("Match: " + m.Value);
                        MatchCounter++;
                        ExternalStaticLinksEx1.actionObject ao = new ExternalStaticLinksEx1.actionObject(textNode, m, start);
                        ExecuteKapActions(ref ao, ref rN);
                        start = ao.start;

                    }
                    if (rN
                        .Where(p => p.links.Count() != 0).Count() != 0)
                    {
                        XElement markedRanges = MarkRanges(rN);
                        if (markedRanges != null)
                            returnValue = markedRanges;
                    }
                }
            }
            catch (SystemException e)
            {
                throw new Exception(string.Format("GetReferanceString Error: {0}", e.Message.ToString()));
            }
            return returnValue;
        }

        private void CreateLinksInternal(XElement container, xLink link, string text)
        {
                int n = link.references.Count();
                if (n == 1)
                {
                    xReference r = link.references.ElementAt(0);
                    XElement a = new XElement("xref"
                        , new XAttribute("rid", r.bm)
                        , new XAttribute("type", "xref")
                        , text);

                    container.Add(a);
                    LinkCounter++;
                }

        }

        private void GetNodes(XElement element, XElement copy, ref string text, ref int start)
        {
            try
            {
                int m = element.Nodes().Count();
                for (int i = 0; i < m; i++)
                {
                    XNode n = element.Nodes().ElementAt(i);
                    if (n.NodeType == XmlNodeType.Element)
                    {
                        XElement sElement = (XElement)n;
                        XElement elCopy = new XElement(sElement.Name.ToString(), sElement.Attributes());
                        copy.Add(elCopy);
                        GetNodes(sElement, elCopy, ref text, ref start);
                    }
                    else if (n.NodeType == XmlNodeType.Text)
                    {
                        text = text + n.ToString();
                        copy.Add(new XElement("text",
                            new XAttribute("start", start),
                            new XAttribute("end", (start + n.ToString().Length).ToString())));
                        start = start + n.ToString().Length;
                    }
                }

            }
            catch (SystemException err)
            {
            }
        }


        private void GetReferancesInternal(XElement e)
        {
            try
            {
                int m = e.Nodes().Count();

                //foreach (XNode n in e.Nodes())
                for (int i = 0; i < m; i++)
                {
                    XNode n = e.Nodes().ElementAt(i);
                    if (n.NodeType == XmlNodeType.Element)
                    {
                        
                        XElement test = (XElement)n;
                        if (!test.Name.ToString().EndsWith("ref")
                            && !((test.Name.ToString() == "title") && (test.Attributes("type").Count() != 0 ? test.Attribute("type").Value : "") != "enkeltsaker-tittel")
                            && !(test.Name.ToString() == "a" 
                                && (test.Attributes("type").Count() != 0 ?
                                    test.Attribute("type").Value : "") == "xref")
                            && !(test.Name.ToString() == "a"
                                && test.Attributes("href").Count() != 0))
                        {
                            if (test.DescendantNodes().OfType<XText>().Count() == 1)
                            {
                                
                                XNode n1 = test.DescendantNodes().OfType<XText>().ElementAt(0);
                                GetReferanceStringInternal(n1);
                        
                            }
                            else if (test.Name.ToString() == "p")
                            {
                                GetReferanceStringInternal(n);
                            }
                            else if (test.Name.ToString() == "a"
                                && (test.Attributes("type").Count() != 0 ?
                                    test.Attribute("type").Value : "") != "xref")
                            {
                                GetReferanceStringInternal(n);
                            }
                            else
                                GetReferancesInternal((XElement)n);
                        }
                        else
                        {
                        }
                    }
                    else if (n.NodeType == XmlNodeType.Text)
                    {

                        XElement test = GetReferanceStringInternal(n);
                        if (test.Nodes().Count() != 1)
                        {
                            m = m + test.Nodes().Count() - 1;
                            i = i + test.Nodes().Count() - 1;
                        }
                    }
                }
            }
            catch (SystemException err)
            {
                throw new Exception(string.Format("GetReferances Error: {0}", err.Message.ToString()));
            }
        }




        //Startprosedyren - normalt
        public XElement GetInternalStaticLinks(XElement document)
        {
            try
            {
                document = GetDocumentPartsInternal(document);
            }
            catch (SystemException e)
            {
                throw new Exception(string.Format("GetInternalStaticLinks Error: {0}", e.Message.ToString()));
            }
            return document;
        }


        private XElement GetDocumentPartsInternal(XElement document)
        {
            try
            {
                GetReferancesInternal(document);
                //if (_INTERNALREGEXP != null)
                //{
                //    //foreach (XElement e in document.Descendants("document"))
                //    if (document.DescendantsAndSelf("section").Count() != 0)
                //    {
                //        int n = document.DescendantsAndSelf("section").Count();
                //        //foreach (XElement e in document.DescendantsAndSelf("level"))
                //        for (int i = 0; i < n; i++)
                //        {
                //            XElement e = document.DescendantsAndSelf("section").ElementAt(i);
                //            GetReferancesInternal(e);
                //        }
                //    }
                //}
            }
            catch (SystemException e)
            {
                throw new Exception(string.Format("GetDocumentPartsInternal Error: {0}", e.Message.ToString()));
            }
            return document;

        }

    }
}
