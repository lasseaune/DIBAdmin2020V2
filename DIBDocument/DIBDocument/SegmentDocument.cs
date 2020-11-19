using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Linq;
using System.Text.RegularExpressions;

namespace DIBDocument
{
    public class SearchItem
    {
        public string text { get; set; }
        public int type { get; set; }
        public SearchItem(XElement e)
        {
            switch (e.Name.LocalName)
            {
                case "section":
                    {
                        text = Regex.Replace(e.Elements().Where(p => Regex.IsMatch(p.Name.LocalName, @"h\d")).DescendantNodes().OfType<XText>().Where(p => p.Ancestors("a").FirstOrDefault() == null).Select(p => p.Value.ToString()).StringConcatenate(" "), @"\s+", " ").Trim();
                        type = 1;
                    }
                    break;
                default:
                    {
                        text = Regex.Replace(e.DescendantNodes().OfType<XText>().Where(p => p.Ancestors("a").FirstOrDefault() == null).Select(p => p.Value.ToString()).StringConcatenate(" "), @"\s+", " ").Trim();
                        type = 0;
                    }
                    break;
            }
        }
    }

    public class ElementData
    {
        public bool TopElement = false;
        public bool IsSegment = false;
        public bool Current = false;
        public bool Generated = false;
        public bool HasContent = false;
        public string segmentName { get; set; }
        public string segment_id { get; set; }
        public XElement segment { get; set; }
        public XElement element { get; set; }
        public XElement item { get; set; }
        public XElement map { get; set; }
        public XElement index { get; set; }
        public ElementData(ElementData parent, XElement e)
        {
            Generated = true;
            if (parent.Current == true)
            {
                element = new XElement(e.Name.LocalName, e.Attributes().Where(p => p.Name.LocalName != "segment"), e.Nodes().Select(p => ElementNode(p)));
                map = new XElement("map", element.Map());
            }
        }
        public IEnumerable<XNode> ElementNode(XNode n)
        {
            List<XNode> nodelist = new List<XNode>();
            if (n.NodeType == XmlNodeType.Text)
                nodelist.Add(n);
            else if (n.NodeType == XmlNodeType.Element)
            {
                XElement e = (XElement)n;
                if ((string)e.Attributes("segment").FirstOrDefault() == "true")
                {
                    nodelist.Add(new XElement(e.Name.LocalName, e.Attributes(), e.Elements().Where(p => Regex.IsMatch(p.Name.LocalName, @"h\d"))));
                }
                else
                {
                    nodelist.Add(new XElement(e.Name.LocalName, e.Attributes(), e.Nodes().Select(p => ElementNode(p))));
                }
            }
            return nodelist;

        }
        public ElementData(XElement e)
        {
            IsSegment = (string)e.Attributes("segment").FirstOrDefault() == "true" ? true : false;
            HasContent = e.Elements().Where(p => !(Regex.IsMatch(p.Name.LocalName, @"h\d") || (string)p.Attributes("segment").FirstOrDefault() == "true")).Count() == 0 ? false : true;
            TopElement = e.AncestorsAndSelf().LastOrDefault() == e;
            Current = true;

            if (TopElement && IsSegment && HasContent)
            {
                segmentName = e.GetHeaderText();
                segment = new XElement("document",
                    new XElement(e.Name.LocalName,
                    e.Attributes(),
                    e.Elements().TakeWhile(p => ((string)p.Attributes("segment").FirstOrDefault() ?? "false") == "true")
                    )
                );
                segment = new XElement("segment",
                    new XAttribute("searchname", segmentName),
                    new XAttribute("segment_id", (string)e.Attributes("id").FirstOrDefault()),
                    segment
                );

            }
            else if (!TopElement && IsSegment && HasContent && Current)
            {
                segmentName = e.GetHeaderText();
                segment = new XElement("document",
                    new XElement(e.Name.LocalName,
                    e.Attributes(),
                    e.Elements().TakeWhile(p => ((string)p.Attributes("segment").FirstOrDefault() ?? "false") != "true")
                    )
                );
                segment = new XElement("segment",
                    new XAttribute("searchname", segmentName),
                    new XAttribute("segment_id", (string)e.Attributes("id").FirstOrDefault()),
                    segment
                );
            }
            else if (IsSegment && !HasContent && Current)
            {
                XElement first = e.Elements().Where(p => ((string)p.Attributes("segment").FirstOrDefault() ?? "false") == "true").FirstOrDefault();
                segmentName = first.GetHeaderText();
                first.Attributes("segment").Remove();
                ElementData ed = new ElementData(this, first);
                segment = new XElement("document",
                    new XElement(e.Name.LocalName,
                    e.Attributes(),
                    e.Elements().TakeWhile(p => ((string)p.Attributes("segment").FirstOrDefault() ?? "false") != "true"),
                    ed.element

                    )
                );

                segment = new XElement("segment",
                    new XAttribute("searchname", segmentName),
                    new XAttribute("segment_id", (string)e.Attributes("id").FirstOrDefault()),
                    segment
                );
            }
        }
    }
    public static class SegmentDocument
    {
        public static string GetHeaderText(this XElement e)
        {
            if (e == null) return "";
            string text = e.Elements()
                    .Where(p => Regex.IsMatch(p.Name.LocalName, @"h\d+"))
                    .DescendantNodes()
                    .OfType<XText>()
                    .Where(s => s.Ancestors().Where(t => "a;sup".Split(';').Contains(t.Name.LocalName)).Count() == 0)
                    .Select(s => s.Value)
                    .StringConcatenate();
            return text;
        }
        public static XElement GetFormat7_Segment(this XElement d, int level)
        {
            XElement doc = d;
            if (level == 1)
            {
                doc.Elements("section").Where(p => p.Elements().Where(e => Regex.IsMatch(e.Name.LocalName, @"h\d")).Count() != 0).ToList().ForEach(p => p.Add(new XAttribute("segment", "true")));
            }
            else if (level == 2)
            {
                doc.Elements("section").Where(p => p.Elements().Where(e => Regex.IsMatch(e.Name.LocalName, @"h\d")).Count() != 0).ToList().ForEach(p => p.Add(new XAttribute("segment", "true")));
                doc.Elements("section").Elements("section")
                    .Where(p => p.Elements().Where(e => Regex.IsMatch(e.Name.LocalName, @"h\d")).Count() != 0 && (string)p.Parent.Attributes("segment").FirstOrDefault() == "true")
                    .Elements()
                    .ToList().ForEach(p => p.Add(new XAttribute("segment", "true")));
            }



            doc.Descendants().Attributes(XNamespace.Xml + "space").ToList().Remove();
            XElement map = new XElement("index", doc.MapSection());

            XElement segments = new XElement("segments",
               doc
               .DescendantsAndSelf()
               .Where(p => (p.Annotation<ElementData>() == null ? null : p.Annotation<ElementData>().segment) != null)
               .Select(p => p.Annotation<ElementData>().segment)
            );

            segments.Elements("segment").ToList().ForEach(p => p.AddSearch());
            segments.Elements("segment").ToList().ForEach(p => p.AddIndex(map));
            segments.Add(map);
            return segments;

        }
        public static IEnumerable<XElement> MapIndex(this XElement i)
        {
            if (i == null) return null;
            return i.Elements().Where(p => p.Attributes("title").FirstOrDefault() != null).Select(p => new XElement("item", p.Attributes(), p.MapIndex()));
        }

        public static void AddIndex(this XElement s, XElement map)
        {

            XElement mapitem = map.Descendants().Where(p => (string)p.Attributes("id").FirstOrDefault() == (string)s.Attributes("segment_id").FirstOrDefault()).FirstOrDefault();
            if (mapitem == null) return;
            XElement index = new XElement("index");
            XElement current = index;
            XElement last = null;
            foreach (XElement p in mapitem.AncestorsAndSelf().Where(p => p.Attributes("title").FirstOrDefault() != null).Reverse())
            {
                current.Add(p.NodesBeforeSelf().OfType<XElement>().Where(i => i.Attributes("title").FirstOrDefault() != null).Select(i => new XElement("item", i.Attributes())));
                XElement item = new XElement("item", p.Attributes());
                current.Add(item);
                current.Add(p.NodesAfterSelf().OfType<XElement>().Where(i => i.Attributes("title").FirstOrDefault() != null).Select(i => new XElement("item", i.Attributes())));
                current = item;
                last = p;
            }
            current.Add(last.MapIndex());
            s.Add(index);
        }
        public static void GetSearchItems(this XElement e)
        {
            switch (e.Name.LocalName)
            {
                case "h1":
                case "h2":
                case "h3":
                case "h4":
                case "h5":
                case "h6":
                case "h7":
                    break;
                case "p":
                case "td":
                case "li":
                    {
                        e.AddAnnotation(new SearchItem(e));
                    }
                    break;

                case "section":
                    if (e.Elements().Where(p => Regex.IsMatch(p.Name.LocalName, @"h\d")).FirstOrDefault() != null)
                    {
                        e.AddAnnotation(new SearchItem(e));
                        e.Elements().ToList().ForEach(p => p.GetSearchItems()); break;

                    }
                    else
                    {
                        e.Elements().ToList().ForEach(p => p.GetSearchItems()); break;
                    }

                default:
                    e.Elements().ToList().ForEach(p => p.GetSearchItems()); break;
            }

        }
        public static void AddSearch(this XElement s)
        {
            string segment_id = (string)s.Attributes("segment_id").FirstOrDefault();
            string searchName = (string)s.Attributes("searchname").FirstOrDefault();
            if (searchName == null) return;
            if (searchName != null)
            {
                s.Elements().ToList().ForEach(p => p.GetSearchItems());
                XElement searchitems = new XElement("searchitems",
                    new XAttribute("parent_id", segment_id),
                    new XAttribute("searchname", searchName),
                    s.Descendants()
                        .Where(p => (string)p.Attributes("id").FirstOrDefault() != null && (p.Annotation<SearchItem>() == null ? null : p.Annotation<SearchItem>().text) != null)
                        .Select(p => new XElement("search",
                                   new XAttribute("object_id", (string)p.Attributes("id").FirstOrDefault()),
                                   new XAttribute("text_type", p.Annotation<SearchItem>().type.ToString()),
                                   new XAttribute("parent_id", segment_id),
                                   new XText(p.Annotation<SearchItem>().text)
                               )
                    )
                );
                s.Add(searchitems);
            }
        }

        public static XElement MapItem(this XElement e)
        {
            return new XElement("item",
                e.Attributes("id"),
                (string)e.Attributes("segment").FirstOrDefault() == "true" ? new XAttribute("s", "1") : null,
                e.Name.LocalName == "section" ? new XAttribute("title", e.Elements().Where(p => Regex.IsMatch(p.Name.LocalName, @"h\d")).Select(p => p.DescendantNodes().OfType<XText>().Select(s => s.Value).StringConcatenate()).FirstOrDefault() ?? "") : null,
                e.Map()
            );
        }
        public static IEnumerable<XElement> Map(this XElement e)
        {
            return e.Elements().Select(p => p.MapItem());
        }
        public static IEnumerable<XElement> MapSection(this XElement e)
        {
            if (e.Annotation<ElementData>() == null)
            {
                ElementData ed = new ElementData(e);
                e.AddAnnotation(ed);
            }
            return e.Elements().Select(p => p.MapSectionItem());
        }
        public static XElement MapSectionItem(this XElement e)
        {
            ElementData ed = null;
            if (e.Annotation<ElementData>() == null)
            {
                ed = new ElementData(e);
                e.AddAnnotation(ed);
            }
            else
            {
                ed = e.Annotation<ElementData>();
            }
            return new XElement("item",
                e.Attributes("id"),
                ed.IsSegment && ed.Generated == false ? new XAttribute("s", "1") : null,
                e.Name.LocalName == "section" && e.Elements().Where(p => Regex.IsMatch(p.Name.LocalName, @"h\d")).Count() != 0
                ? (
                    ((string)e.Attributes("content_title").FirstOrDefault() ?? "") == ""
                    ? new XAttribute("title", e.Elements().Where(p => Regex.IsMatch(p.Name.LocalName, @"h\d")).Select(p => p.DescendantNodes().OfType<XText>().Select(s => s.Value).StringConcatenate()).FirstOrDefault() ?? "")
                    : new XAttribute("title", (string)e.Attributes("content_title").FirstOrDefault())
                  )
                : null,
                e.MapSection()
            );
        }
    }
}
