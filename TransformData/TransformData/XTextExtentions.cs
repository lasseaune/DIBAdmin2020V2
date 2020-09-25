using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using TransformData.Global;

namespace TransformData
{
    public static class XTextExtentions
    {

        public class XTextObject
        {
            public string text { get; set; }
            public List<XTextRange> Ranges { get; set; }
        }
        public class XTextLink
        {
            public int start { get; set; }
            public int length { get; set; }
            public string tag1 { get; set; }
            public string tag2 { get; set; }
            public string regexpName { get; set; }
            public string idArea = "external";
            public string prefix = "";
            public string groupname1 { get; set; }
            public string groupname2 { get; set; }
            public XTextLink(int start, int length)
            {
                this.start = start;
                this.length = length;
            }
        }
        
        public class XTextRange
        {
            public XText t { get; set; }
            public int start { get; set; }
            public int end { get; set; }
            public int pos { get; set; }
            public string searchText { get; set; }
            public List<XTextLink> links = new List<XTextLink>();
            public XTextRange(XText t, int start, int end, string searchText)
            {
                this.t = t;
                this.start = start;
                this.end = end;
                this.searchText = searchText;
                this.pos = start + searchText.Length;
            }
        }
        
        public static bool Between(this int num, int lower, int upper, bool inclusive = false)
        {
            return inclusive ? lower <= num && num <= upper : lower < num && num < upper;
        }

        public static void ReplaceXTextPeriodeByMatch(this IEnumerable<XTextRange> xtextrange, Match m)
        {
            foreach (XTextRange r in xtextrange)
            {
                string text = r.t.ToString();
                string resTextLeft = "";
                string resTextRight = "";
                if (m.Index - r.start > 0)
                    resTextLeft = text.Substring(0, m.Index - r.start);
                if ((m.Index - r.start) + m.Length < (r.end - r.start))
                    resTextRight = text.Substring((m.Index - r.start) + 1, text.Length - ((m.Index - r.start) + 1));


                IEnumerable<XNode> first = r.t.Ancestors("p")
                                        .First().DescendantNodes()
                                        .TakeWhile(p => (p.NodeType != r.t.NodeType ? true : p != r.t))
                                        .Select(p => p)
                                        .Union(
                                            r.t.Ancestors("p")
                                            .First().DescendantNodes()
                                            .Where(p => (p.NodeType != r.t.NodeType ? false : p == r.t))
                                            .Select(p => new XText(p.ToString().Substring(0, m.Index - r.start)))
                                        );
                                    

                                    

                XElement p1 = new XElement("punktum", first);

                IEnumerable<XNode> second = r.t.Ancestors("p")
                                        .First()
                                        .DescendantNodes()
                                        .SkipWhile(p => (p.NodeType != r.t.NodeType ? true : p != r.t))
                                        .TakeWhile(p => (p.NodeType != r.t.NodeType ? false : p == r.t))
                                        .Select(p => new XText(p.ToString().Substring((m.Index - r.start) + 1, text.Length - ((m.Index - r.start) + 1))))
                                        .Union(
                                            r.t.Ancestors("p")
                                            .First().DescendantNodes()
                                            .SkipWhile(p => (p.NodeType != r.t.NodeType ? true : p != r.t))
                                            .SkipWhile(p => (p.NodeType != r.t.NodeType ? false : p == r.t))
                                            .Select(p => p)
                                        );


                XElement p2 = new XElement("p", second);
                                        
                
                //r.t.ReplaceWith(new XText(resTextLeft), new XElement("punktum"), new XText(resTextRight));

            }
        }
        public static void RemoveXTextByMatch(this IEnumerable<XTextRange> xtextrange, Match m)
        {
            foreach (XTextRange r in xtextrange)
            {
                string text = r.t.ToString();
                string resText = "";
                if (m.Index - r.start > 0)
                    resText = text.Substring(0, m.Index - r.start);
                if ((m.Index - r.start) + m.Length < (r.end - r.start) )
                    resText = text.Substring(m.Index - r.start,  (m.Index - r.start)+ m.Length);
                r.t.ReplaceWith(new XText(resText));
                
            }
        }

        public static void AddPrefixToTitles(this IEnumerable<XElement> titles, string pretext)
        {
            foreach (XElement t in titles)
            {
                if (t.Name.LocalName== "title")
                {
                    XText textNode = t.DescendantNodes().OfType<XText>().FirstOrDefault();
                    if (textNode != null)
                        textNode.ReplaceWith(new XText(pretext + "." + textNode.ToString()));
                }
            }
        }

        public static string GetNumValue(this string s)
        {
            string m = Regex.Match(s, @"^(?<num>(([A])?\d+))(\s+)?(L|R)?(\.)(\s|$)").Groups["num"].Value;
            return m;
        }


        private static XTextObject GetXTextRanges(this IEnumerable<XText> xtexts)
        {
            List<XTextRange> ranges = new List<XTextRange>();
            XTextRange lastrange = null;
            string rangeText = "";
            foreach (XText t in xtexts)
            {
                lastrange = t.GetTextRange(lastrange);
                rangeText = rangeText + lastrange.searchText;
                ranges.Add(lastrange);
            }
            return new XTextObject
            {
                text = rangeText,
                Ranges = ranges
            };
        }
        public static XTextObject GetElementTextRange(this XElement e)
        {
            return e.DescendantNodes()
                    .OfType<XText>()
                    .GetXTextRanges();
        }

        private static XTextRange GetTextRange(this XText t, XTextRange lastXTextRange)
        {
            int pos = lastXTextRange == null ? 0 : lastXTextRange.pos;
            string text = t.ToString();
            int sLength = text.Length;
            int offseth = 0;
            string TDReplace = " | ";

            if (("//td//entry//").IndexOf("//" + t.Parent.Name.LocalName + "//") != -1)
            {
                XNode x = t.Parent.DescendantNodes().OfType<XText>().FirstOrDefault();
                if (x == t)
                {
                    text = TDReplace + text;
                    offseth = TDReplace.Length;
                    pos = pos + offseth;
                }
            }
            text = text + " ";
            return new XTextRange(t, pos, pos + sLength,text);
        }

        public static void RemoveDuplicateTempID(this XElement newDocument)
        {
            //Kontroller om duplikate Id
            var grpMulti = newDocument.Descendants("level").GroupBy(p =>
                p.Attribute("id") == null ? "" : p.Attribute("id").Value).Where(p => p.Key != "");

            foreach (var mId in grpMulti)
            {
                if (newDocument.Descendants("level").Where(p => (p.Attribute("id") == null ? "" : p.Attribute("id").Value) == mId.Key.ToString()).Count() > 1)
                {
                    int mNo = 1;
                    foreach (XElement mEl in newDocument.Descendants("level").Where(p => (p.Attribute("id") == null ? "" : p.Attribute("id").Value) == mId.Key.ToString()))
                    {
                        mEl.Attribute("id").Value = mEl.Attribute("id").Value + "_" + mNo.ToString();
                        mNo++;
                    }
                }

            }
        }
        public static XElement GetElementPeriode(this XElement e)
        {
            string regexpExpr = @"[a-zæøå0-9](?<punktum>(\.))(\s+[A-ZÆØÅ]|(\s+)?$)";
            bool found = true;
            XElement leddContainer = new XElement(e.Name.LocalName, e.Attributes());
            bool bPunktumFound = false;
            while (found)
            {
                XTextExtentions.XTextObject xto = e.GetElementTextRange();
                Match m = Regex.Match(xto.text, regexpExpr);
                found = false;
                if (m.Groups["punktum"].Success)
                {
                    bPunktumFound = true;
                    found = true;
                    List<XTextRange> t = xto
                        .Ranges
                        .Where(p =>
                            m.Groups["punktum"].Index.Between(p.start, p.end, true)
                            || (m.Groups["punktum"].Index + m.Groups["punktum"].Length).Between(p.start, p.end)
                            )
                        .Select(q => q)
                        .ToList();
                    if (t.Count() == 1)
                    {
                        IEnumerable<XNode> first = t.First().t.Ancestors("p")
                                            .First().DescendantNodes()
                                            .TakeWhile(p => (p.NodeType != t.First().t.NodeType ? true : p != t.First().t))
                                            .Select(p => p)
                                            .Union(
                                                t.First().t.Ancestors("p")
                                                .First().DescendantNodes()
                                                .Where(p => (p.NodeType != t.First().t.NodeType ? false : p == t.First().t))
                                                .Select(p => new XText(p.ToString().Substring(0, (m.Groups["punktum"].Index - t.First().start) + 1)))
                                            );




                        XElement p1 = new XElement("lovpunktum", new XElement("text", new XElement("span", first)));
                        leddContainer.Add(p1);

                        IEnumerable<XNode> second = t.First().t.Ancestors("p")
                                                .First()
                                                .DescendantNodes()
                                                .SkipWhile(p => (p.NodeType != t.First().t.NodeType ? true : p != t.First().t))
                                                .TakeWhile(p => (p.NodeType != t.First().t.NodeType ? false : p == t.First().t))
                                                .Select(p => new XText(p.ToString().Substring((m.Groups["punktum"].Index - t.First().start) + 1, t.First().t.ToString().Length - ((m.Groups["punktum"].Index - t.First().start) + 1))))
                                                .Union(
                                                    t.First().t.Ancestors("p")
                                                    .First().DescendantNodes()
                                                    .SkipWhile(p => (p.NodeType != t.First().t.NodeType ? true : p != t.First().t))
                                                    .SkipWhile(p => (p.NodeType != t.First().t.NodeType ? false : p == t.First().t))
                                                    .Select(p => p)
                                                );


                        e = new XElement("p", second);
                    }
                
                }
            }

            if (!bPunktumFound) leddContainer.Add(e.Nodes());
            
            return leddContainer;

        }
        public static void RemoveReferances(this XElement e)
        {
            string regexpExpr = @"(?<ref>(\(Ref(\:)?.+))$";
            List<XElement> levelTitles = e.Descendants("title").Where(p => (p.Parent.Name.LocalName == "level"
                                                                        || p.Parent.Name.LocalName == "section")
                                                                        && Regex.IsMatch(p.GetElementText(" ").Trim(), regexpExpr)
                                                                        ).ToList();
            foreach (XElement title in levelTitles)
            {
                string strTitle = Regex.Replace(title.GetElementText(" ").Trim(), @"\s+", " ").Trim();
                string strRef = Regex.Match(strTitle, regexpExpr).Groups["ref"].Value;
                XTextExtentions.XTextObject xto = title.GetElementTextRange();
                MatchCollection mc = Regex.Matches(xto.text, regexpExpr);
                foreach (Match m in mc)
                {
                    xto
                    .Ranges
                    .Where(p =>
                        m.Index.Between(p.start, p.end, true)
                        || (m.Index + m.Length).Between(p.start, p.end)
                        )
                    .Select(q => q)
                    .RemoveXTextByMatch(m);
                }


                if (title.Parent.Elements("text").Count() == 0)
                {
                    title.AddAfterSelf(new XElement("text",
                        new XElement("p",
                            new XElement("strong", strRef)
                            )
                        ));
                }
                else
                {
                    title.Parent.Elements("text").First().AddFirst(new XElement("text",
                        new XElement("p",
                            new XElement("strong", strRef)
                            )
                        ));
                }
            }
        }

    }
}
