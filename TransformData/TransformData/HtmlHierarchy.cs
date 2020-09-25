using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using DIB.RegExp.ExternalStaticLinks;
using TransformData.Global;
using System.Diagnostics;

namespace TransformData
{
    public class HtmlHierarchy
    {
        private string _SectionName;

        public HtmlHierarchy()
        {
            _SectionName = "section";
        }
        public HtmlHierarchy(string SectionName)
        {
            _SectionName = SectionName;
        }
        
        private class Heading
        {
            public int HeadingLevel { get; set; }
            public string idx { get; set; }
            public string title { get; set; }
            public string id { get; set; }
        }



        public XElement GetIndexFromSectionDocument(XElement root)
        {

            XElement content = new XElement("content");
            foreach (XElement e in root.DescendantsAndSelf("document").Elements("section"))
            {
                XElement item = new XElement("item",
                                    new XAttribute("id", e.Attribute("id").Value),
                                    new XAttribute("text", e.Element("title").Value),
                                    new XAttribute("pid", e.Attribute("id").Value),
                                    new XAttribute("nav_view", e.Attribute("nav_view") == null ? "0" : (e.Attribute("nav_view").Value == "" ? "0" :e.Attribute("nav_view").Value))                                    
                                    );

                GetIndexItems(e, item);
                content.Add(item);

            }
            return content;
        }


        private void GetIndexItems(XElement el, XElement items)
        {

            foreach (XElement e in el.Elements("section"))
            {
                XElement item = new XElement("item",
                                    new XAttribute("id", e.Attribute("id").Value),
                                    new XAttribute("text", e.Element("title").Value),
                                    new XAttribute("pid", items.Attribute("id").Value),
                                    new XAttribute("nav_view", e.Attribute("nav_view") == null ? "0" : (e.Attribute("nav_view").Value == "" ? "0" :e.Attribute("nav_view").Value))                                    
                                    );

                GetIndexItems(e, item);
                items.Add(item);

            }
        }

        public XElement GetDIBXMLContent(XElement document)
        {
            XElement content = new XElement("nitems");
            try
            {
                
                foreach (XElement e in document.Elements("level"))
                {
                    if (e.Attribute("id") == null)
                    {
                        XAttribute id = new XAttribute("id", Guid.NewGuid().GetHashCode().ToString());
                        e.Add(id);
                    }
                    else
                    {
                        if (e.Attribute("id").Value == "")
                        {
                            e.Attribute("id").Value = Guid.NewGuid().GetHashCode().ToString();
                        }
                    }
                    if (e.Attribute("pid") == null)
                    {
                        e.Add(new XAttribute("pid", e.AncestorsAndSelf("level").Last().Attribute("id").Value));
                    }
                    else
                    {
                        e.Attribute("pid").Value = e.AncestorsAndSelf("level").Last().Attribute("id").Value;
                    }
                    XElement item = new XElement("nitem"
                                        , new XAttribute("title"
                                            , e.Element("title") == null
                                                ? e.DescendantNodes().OfType<XText>().Select(o => (string)o.ToString()).StringConcatenate().Trim().Substring(0, 20) + "..."
                                                : e.Element("title").DescendantNodes().OfType<XText>().Select(o => (string)o.ToString()).StringConcatenate().Trim())
                                         , new XAttribute("key", e.Attribute("id").Value)
                                         , new XAttribute("pkey", e.Attribute("pid").Value) //Lagt til lau 28/3-12
                                         , new XAttribute("idx", e.Attribute("idx") == null ? "0" : e.Attribute("idx").Value)
                                         , new XAttribute("nav_view", e.Attribute("nav_view") == null ? "0" : (e.Attribute("nav_view").Value == "" ? "0" :e.Attribute("nav_view").Value))
                                         );

                    GetDibXmlIndexItems(e, item);
                    content.Add(item);

                }
               
            }
            catch(SystemException e)
            {
            }
            return content;
        }

        private void GetDibXmlIndexItems(XElement el, XElement items)
        {
            try
            {
                foreach (XElement e in el.Elements("level"))
                {
                    e.SetSectionID();
                     
                    if (e.Attribute("pid") == null)
                    {

                        e.Add(new XAttribute("pid", e.AncestorsAndSelf("level").Last().Attribute("id").Value));
                    }
                    else
                    {
                        e.Attribute("pid").Value = e.AncestorsAndSelf("level").Last().Attribute("id").Value;
                    }
                    
                    XElement item = new XElement("nitem"
                                        , new XAttribute("title"
                                            , e.Element("title") == null
                                                ? e.DescendantNodes().OfType<XText>().Select(o => (string)o.ToString()).StringConcatenate().Trim().Substring(0, 20) + "..."
                                                : e.Element("title").DescendantNodes().OfType<XText>().Select(o => (string)o.ToString()).StringConcatenate().Trim())
                                        , new XAttribute("key", e.Attribute("id").Value)
                                        //, new XAttribute("pkey", e.Ancestors("level").Last().Attribute("id").Value)
                                        , new XAttribute("pkey", e.Attribute("pid").Value) //Endret Lagt til lau 28/3-12
                                        , new XAttribute("idx", e.Attribute("idx") == null ? "0" : e.Attribute("idx").Value)
                                        , new XAttribute("nav_view", e.Attribute("nav_view") == null ? "0" : (e.Attribute("nav_view").Value == "" ? "0" : e.Attribute("nav_view").Value))
                                        );

                    GetDibXmlIndexItems(e, item);
                    items.Add(item);

                }
            }
            catch
            {
            }
        }


        public XElement CreateDibXMLDocument(XElement root, XElement footnotes, string FrontPageName, string FootnoteName)
        {
            try
            {
                int counter = 1;
                SetElementId(root, ref counter);
                XElement toc = GetHtmlToc(root);
                XElement newS = new XElement(root.Name.LocalName, root.Attributes());
                if (toc != null)
                {

                    newS.Add(root.Elements()
                        .Where(p =>
                            Convert.ToInt32(p.Attribute("idx").Value) <
                                Convert.ToInt32(toc.Elements(_SectionName).First().Attribute("idx").Value)));
                    newS.Add(toc.Nodes());
                    int n = newS.Descendants(_SectionName).Count();

                    for (int i = 0; i < n; i++)
                    {
                        XElement el = newS.Descendants(_SectionName).ElementAt(i);

                        XElement next = null;
                        if (i + 1 < n)
                            next = toc.Descendants(_SectionName).ElementAt(i + 1);

                        if (next != null)
                        {
                            el.Element("title").AddAfterSelf(new XElement("text", root.Elements().Where(p =>
                                Convert.ToInt32(p.Attribute("idx").Value) > Convert.ToInt32(el.Attribute("idx").Value)
                                &&
                                Convert.ToInt32(p.Attribute("idx").Value) < Convert.ToInt32(next.Attribute("idx").Value)
                                )));
                        }
                        else
                        {
                            el.Element("title").AddAfterSelf(new XElement("text", root.Elements().Where(p =>
                                Convert.ToInt32(p.Attribute("idx").Value) > Convert.ToInt32(el.Attribute("idx").Value)
                            )));

                        }
                    }
                }

                //Endret 28/3 - Lasse Aune
                if (newS.Descendants(_SectionName).Count() != 0)
                {
                    XElement forside = new XElement(_SectionName
                        , newS.Nodes().Where(p => (p.NodeType == XmlNodeType.Element ? ((XElement)p).Name.LocalName : "") != _SectionName));
                    newS.Nodes().Where(p => (p.NodeType == XmlNodeType.Element ? ((XElement)p).Name.LocalName : "") != _SectionName).Remove();
                    if (forside.HasElements)
                    {
                        XElement newForside = new XElement(_SectionName
                            , new XElement("title", FrontPageName)
                            , new XElement("text", forside.Nodes())
                            );
                        newS.Nodes().Where(p => (p.NodeType == XmlNodeType.Element ? ((XElement)p).Name.LocalName : "") != _SectionName).Remove();
                        newS.AddFirst(newForside);
                    }
                }

                XElement newD = new XElement("document"
                            , newS.Nodes()
                );


                //legg til fotnoter for hver toppseksjon
                if (footnotes.Elements(_SectionName).Count() != 0
                    && newD.Descendants("a")
                        .Where(p => (p.Attribute("class") == null ? "" : p.Attribute("class").Value) == "xref"
                            && (p.Attribute("data-bm") == null ? false : p.Attribute("data-bm").Value.StartsWith("fn")) == true).Count() != 0)
                {

                    foreach (XElement s in newD.Elements(_SectionName))
                    {
                        XElement sectionFootnotes = null;
                        if (s.Descendants("a").Where(p => (p.Attribute("class") == null ? "" : p.Attribute("class").Value) == "xref"
                            && (p.Attribute("data-bm") == null ? false : p.Attribute("data-bm").Value.StartsWith("fn")) == true).Count() != 0)
                        {
                            int fn = s.Descendants("a").Where(p => (p.Attribute("class") == null ? "" : p.Attribute("class").Value) == "xref"
                                && (p.Attribute("data-bm") == null ? false : p.Attribute("data-bm").Value.StartsWith("fn")) == true).Count();
                            for (int i = 0; i < fn; i++)
                            {
                                XElement el = s.Descendants("a").Where(p => (p.Attribute("class") == null ? "" : p.Attribute("class").Value) == "xref"
                                    && (p.Attribute("data-bm") == null ? false : p.Attribute("data-bm").Value.StartsWith("fn")) == true).ElementAt(i);

                                XElement currFn = footnotes.Elements(_SectionName).Where(p => (p.Attribute("id") == null ? "" : p.Attribute("id").Value) == el.Attribute("data-bm").Value).FirstOrDefault();
                                if (currFn != null)
                                {
                                    if (sectionFootnotes == null)
                                    {
                                        sectionFootnotes = new XElement(_SectionName
                                                , new XElement("title", FootnoteName)
                                                );
                                    }
                                    sectionFootnotes.Add(currFn);
                                }
                            }
                        }
                        if (sectionFootnotes != null)
                        {
                            s.Add(sectionFootnotes);
                        }
                    }
                }
                return newD;
            }
            catch
            {
                return null;
            }
            
        }

        public void SetElementId(XElement el, ref int i)
        {
            try
            {
                foreach (XElement subEl in el.DescendantsAndSelf())
                {
                    i++;
                    if (subEl.Attribute("idx") != null)
                    {
                        subEl.Attribute("idx").Value = i.ToString();
                    }
                    else
                        subEl.Add(new XAttribute("idx", string.Format("{0}", i)));
                }
            }
            catch
            {
            }
        }


        private List<Heading> GetHeadingList(XElement html)
        {
            try
            {
                List<XElement> brs = html.Descendants().Where(p => Regex.IsMatch(p.Name.LocalName, @"h\d")).DescendantsAndSelf("br").ToList();
                foreach (XElement br in brs) br.ReplaceWith(new XText(" "));

                List<Heading> headingList =
                        html.Descendants()
                        .Where(p => Regex.IsMatch(p.Name.LocalName, @"h\d"))
                        .Select(p =>
                        {
                            int headingLevel;
                            Int32.TryParse(p.Name.LocalName.Substring(1), out headingLevel);
                            return new Heading
                            {
                                HeadingLevel = headingLevel,
                                idx = p.Attribute("idx").Value,
                                id = (p.Attribute("id") == null ? null : p.Attribute("id").Value),
                                title = p.GetElementText(" ").Trim()
                            };
                        }
                        )
                        .ToList();
                return headingList;
            }
            catch (SystemException err)
            {
                return null;
            }
        }

        private IEnumerable<XElement> GetChildrenHeadings(IEnumerable<Heading> headingList, Heading parent)
        {
            return
                headingList
                    .SkipWhile(h => h != parent)
                    .Skip(1)
                    .TakeWhile(h => h.HeadingLevel > parent.HeadingLevel)
                    .Where(h => h.HeadingLevel == parent.HeadingLevel + 1)
                    .Select(h =>
                        new XElement(_SectionName
                            , (h.id == null ? null :new XAttribute("id", h.id))
                            , new XAttribute("idx", h.idx)
                            , new XElement("title",h.title)
                            , GetChildrenHeadings(headingList, h)
                        )
                    );
        }

        private  int GetHeadingListMinLevel(List<Heading> headingList)
        {
            var query = from p in headingList
                        select p.HeadingLevel;
            if (query.Count() == 0) return 0;
            return query.Min();
        }
        public XElement GetHtmlToc(XElement html)
        {
            List<Heading> headingList = GetHeadingList(html);
            List<Heading> paragrafList = html.Elements()
                                .Where(p => p.Name.LocalName == "p"
                                && (p.Attribute("id") == null ? false : (p.Attribute("id").Value == "" ? false :(p.Attribute("id").Value.StartsWith("temp") ? false : true))))
                                        .Select(p =>
                                            {
                                                return new Heading
                                                {
                                                    HeadingLevel = 10,
                                                    idx = p.Attribute("idx").Value,
                                                    id = (p.Attribute("id") == null ?null : p.Attribute("id").Value),
                                                    title = p.GetElementText(" ").Trim()
                                                };
                                            }
                                        ).ToList();

            int h1 = GetHeadingListMinLevel(headingList);
            if (h1 == 0) return null;
            XElement toc = new XElement("root",
                    headingList
                        .Where(p => p.HeadingLevel == h1)
                        .Select
                        (
                            p => new XElement(_SectionName
                                , (p.id == null ? null :new XAttribute("id", p.id))
                                , new XAttribute("idx", p.idx)
                                , new XElement("title", p.title)
                                , GetChildrenHeadings(headingList, p)
                            )
                        )
                );
            
            bool lastHeading = false;
            bool restart = false;
            string currIDX = "0";
            while (toc.Descendants(_SectionName).Count() != (headingList.Count + paragrafList.Count))
            {
                restart = false;
                int n = toc.Descendants(_SectionName).Where(p=>Convert.ToInt32(p.Attribute("idx").Value) > Convert.ToInt32(currIDX)).Count();
                int currIdx=0;
                for (int i = 0; i < n; i++)
                {
                    XElement curr = toc.Descendants(_SectionName)
                        .Where(p => Convert.ToInt32(p.Attribute("idx").Value) > Convert.ToInt32(currIDX))
                        .ElementAt(i);

                    
                    currIdx = Convert.ToInt32(curr.Attribute("idx").Value);
                    XElement next = null;
                    int nextIdx = headingList.Max(p=>Convert.ToInt32(p.idx)) + 1;
                    if (n > (i + 1))
                    {
                        next = toc.Descendants(_SectionName)
                            .Where(p => Convert.ToInt32(p.Attribute("idx").Value) > Convert.ToInt32(currIDX))
                            .ElementAt(i + 1);
                        nextIdx = Convert.ToInt32(next.Attribute("idx").Value);
                    }
                    else
                        nextIdx = 1000000000;


                    if (headingList
                        .Where(p =>
                            toc.Descendants("section").Where(q => (q.Attribute("idx") == null ? "" : q.Attribute("idx").Value).Trim() == p.idx.Trim()).Count() == 0
                            && Convert.ToInt32(p.idx) > currIdx
                            && Convert.ToInt32(p.idx) < nextIdx)
                        .Count() != 0)
                    {
                        for (int j = 1; j < 10; j++)
                        {
                            List<Heading> h = headingList
                                            .Where(p => toc.Descendants("section").Where(q => q.Attribute("idx").Value == p.idx).Count() == 0
                                                && Convert.ToInt32(p.idx) > currIdx
                                                && Convert.ToInt32(p.idx) < nextIdx
                                                && p.HeadingLevel == j).ToList();
                            if (h.Count != 0)
                            {
                                foreach (Heading section in h.OrderBy(p => Convert.ToInt32(p.idx)))
                                {
                                    if (next != null ? next == (curr.Elements(_SectionName).Count() == 0 ? null : curr.Elements(_SectionName).First()) : false)
                                    {
                                        next.AddBeforeSelf(next);
                                    }
                                    else
                                    {
                                        curr.Add(
                                            new XElement(_SectionName
                                            , (section.id == null ? null : new XAttribute("id", section.id))
                                            , new XAttribute("idx", section.idx)
                                            , new XElement("title", section.title)));
                                    }
                                }
                                restart = true;
                                break;

                            }

                
                        }
                    }
                    else
                    {
                        if (paragrafList
                            .Where(p => toc
                                        .Descendants("section").Where(q =>
                                                q.Attribute("idx").Value == p.idx).Count() == 0
                                                && Convert.ToInt32(p.idx) > currIdx
                                                && Convert.ToInt32(p.idx) < nextIdx)
                                        .Count() != 0)
                        {
                            List<Heading> h = paragrafList
                                .Where(p => toc.Descendants("section").Where(q => q.Attribute("idx").Value == p.idx).Count() == 0
                                    && Convert.ToInt32(p.idx) > currIdx
                                    && Convert.ToInt32(p.idx) < nextIdx).ToList();
                            if (h.Count != 0)
                            {
                                foreach (Heading section in h.OrderBy(p => Convert.ToInt32(p.idx)))
                                {
                                    curr.Add(
                                        new XElement(_SectionName
                                        , (section.id == null ? null : new XAttribute("id", section.id))
                                        , new XAttribute("idx", section.idx)
                                        , new XElement("title", section.title)));
                                }
                                restart = true;
                                break;

                            }

                        }
                    }
                    if (restart) break;
                    if (next == null) lastHeading = true;
                }
                
                currIDX = currIdx.ToString();

                if (lastHeading) break;
            }
            return toc;
        }
    }
}
