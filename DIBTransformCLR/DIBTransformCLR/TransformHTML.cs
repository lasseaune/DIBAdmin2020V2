using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace DIBTransformCLR
{
    public static class TransformHTML
    {
     
        private class LinkInfo
        {
            string id { get; set; }
            string old_id { get; set; }
        }
        
        public static IEnumerable<XElement> ConvertXMLtoHTML5(this IEnumerable<XElement> documents, XElement dibLinks)
        {
            List<XElement> result = new List<XElement>();
            
            foreach (XElement document in documents)
            {
                (
                from a in document.Descendants("dibparameter")
                join i in dibLinks.Elements("i")
                on ((string)a.Attributes("refid").FirstOrDefault()??"").Trim().ToLower() equals ((string)i.Attributes("old_id").FirstOrDefault()??"").Trim().ToLower()
                select new { a, i }
                ).ToList().ForEach(p => p.a.ReplaceWith(
                        new XElement("a", 
                            p.a.Attributes("id"),
                            new XAttribute("data-type", "edited"),
                            new XAttribute("date-id", (string)p.i.Attributes("id").FirstOrDefault()),
                            new XText((string)p.a.Attributes("replaceText").FirstOrDefault())
                        )
                    )
                );
                (
                from a in document.Descendants("diblink")
                join i in dibLinks.Elements("i")
                on ((string)a.Attributes("refid").FirstOrDefault()).Trim().ToLower() equals ((string)i.Attributes("old_id").FirstOrDefault()).Trim().ToLower()
                select new { a, i }
                ).ToList().ForEach(p => p.a.ReplaceWith(
                        new XElement("a",
                            p.a.Attributes("id"),
                            new XAttribute("data-type", "auto"),
                            new XAttribute("date-id", (string)p.i.Attributes("id").FirstOrDefault()),
                            p.a.Nodes()
                        )
                    )
                );

                (
                from a in document.Descendants("a").Where(p=>((string)p.Attributes("class").FirstOrDefault()??"").ToLower()=="diblink")
                join i in dibLinks.Elements("i")
                on ((string)a.Attributes("date-refid").FirstOrDefault()).Trim().ToLower() equals ((string)i.Attributes("old_id").FirstOrDefault()).Trim().ToLower()
                select new { a, i }
                ).ToList().ForEach(p => p.a.ReplaceWith(
                        new XElement("a",
                            p.a.Attributes("id"),
                            new XAttribute("data-type", "auto"),
                            new XAttribute("data-id", (string)p.i.Attributes("id").FirstOrDefault()),
                            p.a.Nodes()
                        )
                    )
                );

                (
                from a in document.Descendants("a").Where(p => ((string)p.Attributes("class").FirstOrDefault() ?? "").ToLower().Split(' ').Contains("iref"))
                select a
                ).ToList().ForEach(p => p.ReplaceWith(
                        new XElement("a",
                            p.Attributes("id"),
                            p.Attributes("class"),
                            ((string)p.Attributes("data-segment").FirstOrDefault()??"") == "" ? null : new XAttribute("data-segmentId", ((string)p.Attributes("data-segment").FirstOrDefault() ?? "").Trim().ToLower()) ,
                            p.Attributes("data-id"),
                            p.Nodes()
                        )
                    )
                );

                (
                from a in document.Descendants("a").Where(p =>
                    (((string)p.Attributes("href").FirstOrDefault() ?? "").ToLower() == "http://dibadmin.dib.no/"
                    || ((string)p.Attributes("href").FirstOrDefault() ?? "").ToLower() == "https://dibadmin.dib.no/"
                    || ((string)p.Attributes("href").FirstOrDefault() ?? "").ToLower() == "dibimages/uploads/")
                    && ((string)p.Attributes("title").FirstOrDefault() ?? "").ToLower() != ""
                )
                select a
                ).ToList().ForEach(p => p.ReplaceWith(
                         new XElement("a",
                             new XAttribute("class", "fotnote"),
                             p.Attributes("id"),
                             new XAttribute("title", (string)p.Attributes("title").FirstOrDefault()),
                             p.Nodes()
                         )
                     )
                );

                (
                from a in document.Descendants("a").Where(p => 
                    (((string)p.Attributes("href").FirstOrDefault() ?? "").ToLower()== "http://dibadmin.dib.no/"
                    || ((string)p.Attributes("href").FirstOrDefault() ?? "").ToLower() == "https://dibadmin.dib.no/"
                    || ((string)p.Attributes("href").FirstOrDefault() ?? "").ToLower() == "dibimages/uploads/")
                    && ((string)p.Attributes("title").FirstOrDefault() ?? "").ToLower() == ""
                )
                select a
                ).ToList().ForEach(p => p.ReplaceWith(
                         new XElement("a",
                             new XAttribute("class", "fotnote"),
                             p.Attributes("id"),
                             new XAttribute("title", "Mangler fotnote"),
                             p.Nodes()
                         )
                     )
                );

                document.Descendants().Where(p => ((string)p.Attributes("id").FirstOrDefault() ?? "") == "").ToList().ForEach(p => p.SetAttributeValueEx("id", Guid.NewGuid().ToString()));

                result.Add(new XElement(document.Name.LocalName, document.Attributes(), document.Transform()));
            }

            return result;
        }
        private static IEnumerable<XNode> Transform(this XNode n)
        {
            List<XNode> result = new List<XNode>();
            try
            {
                if (n.NodeType == XmlNodeType.Text) result.Add(n);
                if (n.NodeType == XmlNodeType.Element)
                {
                    XElement e = ((XElement)n);
                    switch (e.Name.LocalName)
                    {
                        case "x-alternative":
                            //x-alternative title="Redusere pålydende" n="1" id="x45"
                            result.Add(new XElement("section",
                                new XAttribute("id", Guid.NewGuid().ToString()),
                                new XAttribute("class", "dib-x-alternative"),
                                new XAttribute("data-var-value", (string)e.Attributes("n").FirstOrDefault()),
                                new XAttribute("data-var-id", (string)e.Parent.Attributes("id").FirstOrDefault()),
                                new XAttribute("data-var-name", (string)e.Parent.Attributes("title").FirstOrDefault()),
                                e.Nodes().SelectMany(p => p.Transform())
                                )
                            );
                            break;
                        case "x-alternatives":
                            //x-alternatives id="1_Alternative"
                            result.Add(new XElement("section",
                                new XAttribute("id", Guid.NewGuid().ToString()),
                                new XAttribute("class", "dib-x-alternatives"),
                                new XAttribute("data-var-id", (string)e.Attributes("id").FirstOrDefault()),
                                e.Nodes().SelectMany(p => p.Transform())
                                )
                            );
                            break;
                        case "x-letterhead":
                            result.Add(new XElement("section",
                                new XAttribute("id", Guid.NewGuid().ToString()),
                                new XAttribute("class", "dib-x-letterhead"),
                                e.Nodes().SelectMany(p => p.Transform())
                                )
                            );
                            break;
                        case "x-list":
                            //header="Antall øvrige styremedlemmer i Hjelpeselskapet:" varname="Antall-S---S-øvrige-S-styremedlemmer-S-i-S-Hjelpeselskapet" defaultcounter="0" oftype="cell-line"
                            result.Add(new XElement("section",
                                new XAttribute("id", Guid.NewGuid().ToString()),
                                new XAttribute("class", "dib-x-list"),
                                new XAttribute("data-var-oftype", (string)e.Attributes("oftype").FirstOrDefault()),
                                new XAttribute("data-var-id", (string)e.Attributes("varname").FirstOrDefault()),
                                new XAttribute("data-var-header", (string)e.Attributes("header").FirstOrDefault()),
                                new XAttribute("data-default-counter", (string)e.Attributes("defaultcounter").FirstOrDefault()),
                                e.Nodes().SelectMany(p => p.Transform())
                                )
                            );
                            break;
                        case "x-optional":
                            //keyword="IAS 23" id="IAS - S - 23_FreeElement"
                            result.Add(new XElement("section",
                                new XAttribute("id", Guid.NewGuid().ToString()),
                                new XAttribute("class", "dib-x-optional"),
                                new XAttribute("data-var-id", (string)e.Attributes("id").FirstOrDefault()),
                                new XAttribute("data-var-keyword", (string)e.Attributes("keyword").FirstOrDefault()),
                                e.Nodes().SelectMany(p => p.Transform())
                                )
                            );
                            break;
                        case "x-var":
                            if (Regex.IsMatch(((string)e.Attributes("id").FirstOrDefault()??""),@"\*[nN]\*"))
                            {
                                result.Add(new XElement("span",
                                    new XAttribute("id", Guid.NewGuid().ToString()),
                                    new XAttribute("class", "dib-var-n"),
                                    new XAttribute("data-var-id", (string)e.Attributes("id").FirstOrDefault()),
                                    new XAttribute("data-var-area", (string)e.Attributes("class").FirstOrDefault()),
                                    new XAttribute("data-var-text", e.Value),
                                    e.Nodes().SelectMany(p => p.Transform())
                                    )
                                );
                            }
                            else
                            {
                                result.Add(new XElement("span",
                                    new XAttribute("id", Guid.NewGuid().ToString()),
                                    new XAttribute("class", "dib-var"),
                                    new XAttribute("data-var-id", (string)e.Attributes("id").FirstOrDefault()),
                                    new XAttribute("data-var-area", (string)e.Attributes("class").FirstOrDefault()),
                                    new XAttribute("data-var-text", e.Value),
                                    e.Nodes().SelectMany(p => p.Transform())
                                    )
                                );
                            }
                            
                            break;
                        case "uth":
                            result.Add(new XElement("u",
                                    e.Attributes("id"),
                                    e.Nodes().SelectMany(p => p.Transform())
                                )
                            );
                            break;
                        case "Pkt":
                            result.Add(new XElement("blockquote",
                                    e.Attributes("id"),
                                    e.Nodes().SelectMany(p => p.Transform())
                                )
                            );
                            break;
                        case "ot":
                            result.Add(new XElement("ol",
                                    e.Attributes("id"),
                                    e.Nodes().SelectMany(p => p.Transform())
                                )
                            );
                            break;
                        case "ny-anf":
                            result.Add(new XElement("em",
                                    e.Attributes("id"),
                                    e.Nodes().SelectMany(p => p.Transform())
                                )
                            );
                            break;
                        case "small":
                        case "references":
                        case "font":
                        case "address":
                        case "externalparts":
                        case "Note":
                            result.AddRange(e.Nodes().SelectMany(p => p.Transform())); break;
                        case "blokk":
                            result.Add(new XElement("section",
                                    e.Attributes("id"),
                                    new XAttribute("class","dib-blokk"),
                                    e.Nodes().SelectMany(p => p.Transform())
                                )
                            );
                            break;
                        case "ledd":
                            result.Add(new XElement("section",
                                    e.Attributes("id"),
                                    e.Nodes().SelectMany(p => p.Transform())
                                )
                            );
                            break;
                        case "mtit":
                            result.Add(new XElement("p",
                                    e.Attributes("id"),
                                    new XElement("strong",
                                        new XAttribute("id", Guid.NewGuid().ToString()),
                                        e.Nodes().SelectMany(p => p.Transform())
                                    )
                                )
                            );
                            break;
                        case "format":
                        case "class":
                        case "timerelation":
                            break;
                        case "docpart":
                        case "document": result.AddRange(e.Document()); break;
                        case "figcapiton":
                            result.Add(new XElement("figcaption",
                                    e.Attributes("id"),
                                    e.Nodes().SelectMany(p => p.Transform())
                                )
                            );
                            break;
                        case "fig":
                            result.Add(new XElement("figure",
                                    e.Attributes("id"),
                                    e.Nodes().SelectMany(p => p.Transform())
                                )
                            );
                            break;
                        case "h1":
                        case "h2":
                        case "h3":
                        case "h4":
                        case "h5":
                        case "h6":
                        case "h7":
                        case "h8":
                        case "h9":
                        case "h10":
                        case "h11":
                        case "h12":
                        case "h13":
                        case "h14":
                            result.AddRange(e.h()); break;
                        case "x-box": result.AddRange(e.xbox()); break;
                        case "a": 
                            result.AddRange(e.a()); break;
                        case "img": result.AddRange(e.img()); break;
                        case "section":
                            result.AddRange(e.Section()); break;
                        case "table": result.AddRange(e.Table()); break;
                        case "item": result.AddRange(e.item()); break;
                        
                        default: result.AddRange(e.Default()); break;
                    }
                }
                return result;
            }
            catch (Exception e)
            {
                result.Add(new XElement("p", "Transformation error:" + e.Message));
                return result;
            }
        }
        #region //ElementStyle

        
        private static IEnumerable<XNode> a(this XElement e)
        {
            string name = e.Name.LocalName;
            string className = (string)e.Attributes("class").FirstOrDefault();

            List<XNode> result = new List<XNode>();
            
            result.Add(new XElement(e.Name.LocalName,
                e.Attributes(),
                e.Nodes()
                )
            );
            return result;
        }
        
        private static IEnumerable<XNode> item(this XElement e)
        {

            List<XNode> result = new List<XNode>();
            int n = e.AncestorsAndSelf("item").Count();
            if (n > 6)
                n = 6;
            result.Add(new XElement("section",
                e.Attributes("id"),
                e.Attributes("type").FirstOrDefault() == null ? null : new XAttribute("data-type", (string)e.Attributes("type").FirstOrDefault()),
                new XAttribute("class", "check-item"),
                (
                    e.Nodes().OfType<XElement>().Where(p => p.Name.LocalName == "ititle").Count() == 1
                    ? new XElement("h" + n.ToString(),
                        e.Nodes().OfType<XElement>().Where(p => p.Name.LocalName == "ititle").Attributes("id").FirstOrDefault(),
                        new XAttribute("class", "check-title"),
                        e.Nodes().OfType<XElement>().Where(p => p.Name.LocalName == "ititle").Nodes().SelectMany(p => p.Transform()))
                    : new XElement("h" + n.ToString(),
                            new XAttribute("id", Guid.NewGuid().ToString()),
                            new XAttribute("class", "check-title"),
                            new XText("[Infopunkt]")
                      )
                ),
                new XElement("section",
                    new XAttribute("class", "check-ingress"),
                    (
                        e.Nodes().OfType<XElement>().Where(p => p.Name.LocalName == "ingress").Attributes("id").FirstOrDefault() == null
                        ? new XAttribute("id", Guid.NewGuid().ToString())
                        : e.Nodes().OfType<XElement>().Where(p => p.Name.LocalName == "ingress").Attributes("id").FirstOrDefault()
                    ),
                    e.Nodes().OfType<XElement>().Where(p => p.Name.LocalName == "ingress").Nodes().SelectMany(p => p.Transform())
                ),
                new XElement("section",
                    new XAttribute("class", "check-description"),
                    (
                        e.Nodes().OfType<XElement>().Where(p => p.Name.LocalName == "description").Attributes("id").FirstOrDefault() == null
                        ? new XAttribute("id", Guid.NewGuid().ToString())
                        : e.Nodes().OfType<XElement>().Where(p => p.Name.LocalName == "description").Attributes("id").FirstOrDefault()
                    ),
                    e.Nodes().OfType<XElement>().Where(p => p.Name.LocalName == "description").Nodes().SelectMany(p => p.Transform())
                ),
                new XElement("section",
                    new XAttribute("class", "check-law"),
                    (
                        e.Nodes().OfType<XElement>().Where(p => p.Name.LocalName == "law").Attributes("id").FirstOrDefault() == null
                        ? new XAttribute("id", Guid.NewGuid().ToString())
                        : e.Nodes().OfType<XElement>().Where(p => p.Name.LocalName == "law").Attributes("id").FirstOrDefault()
                    ),
                    e.Nodes().OfType<XElement>().Where(p => p.Name.LocalName == "law").Nodes().SelectMany(p => p.Transform())
                ),
                new XElement("section",
                    new XAttribute("id", Guid.NewGuid().ToString()),
                    new XAttribute("class", "check-children"),
                    e.Nodes().OfType<XElement>().Where(p => p.Name.LocalName == "item").SelectMany(p => p.Transform())
                    )
                )
            );

            return result;
        }
       
        private static IEnumerable<XNode> Table(this XElement e)
        {
            List<XNode> result = new List<XNode>();
            if (e.Parent.Name.LocalName + ((string)e.Parent.Attributes("class").FirstOrDefault() ?? "") != "divtable-wrapper")
            {
                result.Add(new XElement("div",
                        new XAttribute("id", Guid.NewGuid().ToString()),
                        new XAttribute("class", "table-wrapper"),
                        new XElement(e.Name.LocalName,
                        e.Attributes(),
                        e.Nodes().SelectMany(p => p.Transform())
                        )
                    )
                );
            }
            else
            {
                result.Add(new XElement(e.Name.LocalName,
                    e.Attributes(),
                    e.Nodes().SelectMany(p => p.Transform())
                    )
                );
            }
            return result;
        }
        private static IEnumerable<XNode> xbox(this XElement e)
        {
            List<XNode> result = new List<XNode>();
            result.Add(new XElement("figure",
                e.Attributes("class"),
                e.Attributes("id"),
                e.Elements("x-box-title").FirstOrDefault() == null ? null : new XElement("figcaption", e.Element("x-box-title").Attributes("id"), e.Element("x-box-title").Nodes()),
                new XElement("div",
                    new XAttribute("id", Guid.NewGuid().ToString()),
                    e.Nodes().Where(p => (p.NodeType == XmlNodeType.Element ? ((XElement)p).Name.LocalName : "") != "x-box-title")
                    )
                )
            );
            return result;
        }
        private static IEnumerable<XNode> h(this XElement e)
        {
            List<XNode> result = new List<XNode>();
            if (e == null) return result;
            if (e.Parent.Name.LocalName=="fig")
            {
                result.Add(new XElement("figcaption",
                        e.Attributes("id"),
                         e.Nodes().SelectMany(p => p.Transform())
                    )
                );
                return result;
            }
            string name = e.Name.LocalName.ToLower();
            switch (name)
            {
                case "h7":
                case "h8":
                case "h9":
                case "h10":
                case "h11":
                case "h12":
                case "h13":
                case "h14":
                    name = "h6"; break;
            }

            result.Add(new XElement(name,
                   e.Attributes(),
                   e.Nodes().SelectMany(p => p.Transform())
                   )
            );
           
            return result;
        }
        private static IEnumerable<XNode> img(this XElement e)
        {
            List<XNode> result = new List<XNode>();
            if (e == null) return result;
            string src = ((string)e.Attributes("src").FirstOrDefault() ?? "").Trim();
            if (src == "") return result;
            if (!Regex.IsMatch(src.Trim().ToLower(), @"(\/)?dibimages/uploads"))
            {
                src = src.Split('/').LastOrDefault();
                if (src == "") return result;
                src = @"dibimages/" + (string)e.Ancestors().Attributes("topic_id").LastOrDefault() + @"/" + src;
            }

            result.Add(new XElement("figure",
                    new XAttribute("id", Guid.NewGuid().ToString()),
                    new XAttribute("class", "img-no-caption"),
                    new XElement("div",
                        new XAttribute("id", Guid.NewGuid().ToString()),
                        new XElement(e.Name.LocalName.ToString(),
                            new XAttribute("src", src),
                            e.Attributes().Where(p => p.Name.LocalName != "src"),
                            e.Nodes().SelectMany(p => p.Transform())
                        )
                    )
                )
            );

            return result;
        }
        private static IEnumerable<XNode> Document(this XElement e)
        {
            return e.Nodes().SelectMany(p => p.Transform());
        }
       
        
        private static IEnumerable<XNode> Default(this XElement e)
        {
            List<XNode> result = new List<XNode>();
            if (e == null) return result;
            
            result.Add(new XElement(e.Name.LocalName.ToString(),
                    e.Attributes(),
                    e.Nodes().SelectMany(p => p.Transform())
                    )
                );

            return result;
        }
        private static IEnumerable<XNode> Section(this XElement e)
        {
            List<XNode> result = new List<XNode>();
            result.Add(new XElement("section",
                e.Attributes("id"),
                e.Attributes("class"),
                e.Attributes().Where(a => a.Name.LocalName.StartsWith("data")),
                e.Nodes().SelectMany(p => p.Transform())
                )
            );
            return result;
        }

        #endregion
    }
}
