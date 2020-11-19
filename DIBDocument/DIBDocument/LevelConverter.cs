using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace DIBDocument
{
    public static class LevelConverter
    {
        public static string GetTitleFromDescriprion(this XElement d)
        {
            string title = d.DescendantNodes().OfType<XText>().Select(t => t.ToString()).StringConcatenate();
            if (title.Length > 50)
            {
                title = title.Substring(0, 50) + "...";
            }
            return title;
        }
        public static IEnumerable<XElement> Index5(this XElement e)
        {
            if (e == null) return null;
            List<XElement> result = new List<XElement>();
            result.AddRange(
                e.Elements()
                    .Where(p => p.Name.LocalName == "level" || (p.Name.LocalName == "item" && (p.Elements("description").Count() == 0 ? "" : p.Elements("description").DescendantNodes().OfType<XText>().Select(t => t.ToString()).StringConcatenate()) != ""))
                    .Select(p => new XElement("item",
                       p.Attributes("id"),
                       p.Name.LocalName == "level"
                       ? new XAttribute("title", p.Elements("title").Select(t => t.Value).FirstOrDefault())
                       : new XAttribute("title", p.Elements("description").FirstOrDefault().GetTitleFromDescriprion()),
                       p.Index5()
                    )
                )
            );
            return result;
        }
        public static IEnumerable<XElement> Index6_7(this XElement e)
        {
            if (e == null) return null;
            List<XElement> result = new List<XElement>();
            result.AddRange(
                e
                .Elements("level")
                .Select(p => new XElement("item",
                    p.Attributes("id"),
                    p.Attributes("segment"),
                    p.Attributes("data-opitional"),
                    p.Attributes("data-default"),
                    p.Attributes("data-autocount"),
                    p.Attributes("data-type"),
                    //new XAttribute("title", p.Elements("title").Select(t => t.Value).FirstOrDefault()),
                    new XAttribute("title", p.Elements("title").Nodes().GetNodesWithXVar()),
                    p.Elements("text").FirstOrDefault().IndexXBox(),
                    p.Index6_7()
                    )
                )
            );
            return result;
        }
        public static string GetTitleFromClass(this XAttribute a)
        {
            if (a == null) return "Eksempel";
            switch (a.Value)
            {
                case "am": return "A-melding";
                case "nb": return "NB!";
                case "xsample": return "Eksempel";
            }
            return "Ekesempel";
        }
        public static IEnumerable<XElement> IndexXBox(this XElement e)
        {
            if (e == null) return null;
            List<XElement> result = new List<XElement>();
            result.AddRange(
                e
                .Elements("x-box")
                .Select(p => new XElement("item",
                    p.Attributes("id"),
                    p.Attributes("title").FirstOrDefault() == null ? new XAttribute("title", p.Attributes("class").FirstOrDefault().GetTitleFromClass()) : new XAttribute("title", (string)p.Attributes("title").FirstOrDefault())
                    //p.Index6_7()
                    )
                )
            );
            return result;
        }
        public static XElement Convert5X(this XElement convert)
        {
            if (convert.Name.LocalName == "convert5")
            {
                XElement document = convert.Elements("document").FirstOrDefault();
                XElement index = convert.Elements().Where(p => "index;navigation".Split(';').Contains(p.Name.LocalName)).FirstOrDefault();

                if (document != null && index != null)
                {
                    (
                        from l in document.Descendants("level")
                        join i in index.Descendants().Where(p => p.Name.LocalName.EndsWith("item"))
                        on (string)l.Attributes("id").FirstOrDefault() equals (string)i.Attributes().Where(p => "key;id".Split(';').Contains(p.Name.LocalName)).FirstOrDefault()
                        select new
                        {
                            level = l,
                            item = i
                        }
                    )
                    .ToList()
                    .ForEach(p => p.level.AddFirst(new XElement("title", (string)p.item.Attributes().Where(s => "title;text".Contains(s.Name.LocalName)).FirstOrDefault())));

                    document.Descendants().Where(p => "description;hjemler;tidspunkt;ansvar;references".Split(';').Contains(p.Name.LocalName) && p.Nodes().Count() == 0).ToList().ForEach(p => p.Remove());
                    document.Descendants("topicinfo").Attributes("pid").Where(p => p.Value.Replace("undefined", "") == "").ToList().ForEach(p => p.Remove());
                    document.Descendants("level").Where(p => p.Elements("title").FirstOrDefault() == null && p.Descendants("topicinfo").FirstOrDefault() == null).ToList().ForEach(p => p.Remove());
                    document.Descendants("item").Elements("description").ToList().ForEach(p => p.ReplaceWith(new XElement("ititle", p.DescendantNodes().OfType<XText>().Select(s => s.ToString()).StringConcatenate())));

                    int n = 0;

                    document.Elements("level").ToList().ForEach(p => p.Add(new XAttribute("label_id", (n++).ToString())));

                    XElement labels = new XElement("labels",
                        document
                        .Elements("level")
                        .Select(p => new XElement("label",
                            new XAttribute("id", (string)p.Attributes("label_id").FirstOrDefault()),
                            new XAttribute("name", p.Elements("title").Select(s=>s.Value).FirstOrDefault()??"none"),
                            new XAttribute("colortext", "#ffffff"),
                            new XAttribute("colorbackground", "#888888"),
                            new XAttribute("group_id", "_pakke")
                            )
                        )
                    );

                    XElement itemlabels = new XElement("itemlabels",
                        document
                        .Elements("level")
                        .Select(p => new XElement("itemlabel",
                            new XAttribute("item_id", (string)p.Attributes("id").FirstOrDefault()),
                            new XElement("label",
                                new XAttribute("id", (string)p.Attributes("label_id").FirstOrDefault()) 
                                )
                            )
                        )
                    );


                    XElement labelgroups = new XElement("labelgroups",
                        new XElement("labelgroup",
                            new XAttribute("id", "_pakke"),
                            new XAttribute("name", "Filter"),
                            new XAttribute("type", "1"),
                            new XAttribute("default", (string)document.Elements("level").Attributes("label_id").FirstOrDefault())
                        )
                    );

                    XElement docpart = new XElement("document",
                            document
                            .Elements("level")
                            .Select(p=>new XElement("item",
                                p.Attributes("id"),
                                new XElement("ititle", p.Elements("title").Select(s=>s.Value).FirstOrDefault()??"none"),
                                p.Descendants("item")
                                .Where(s=>s.Elements("references").Elements("topicinfo").Where(r=>(string)r.Attributes("id").FirstOrDefault()!="").Count() != 0)
                                .Select(s=>new XElement("item",
                                        s.Attributes("id"),
                                        s.Elements("ititle"),
                                        new XElement("references",
                                             s.Elements("references")
                                             .Elements("topicinfo")
                                             .Where(r => (string)r.Attributes("id").FirstOrDefault() != "")
                                             .Select(r=>new XElement("topicinfo",
                                                r.Attributes("id"),
                                                ((string)r.Attributes("bm").FirstOrDefault()??"")=="" ? null : r.Attributes("bm")
                                                ) 
                                             )
                                        )
                                    )
                                )
                            )
                        )
                    );

                    XElement topics = new XElement("topics",
                        document
                        .Descendants("topicinfo")
                        .GroupBy(p => new
                        {
                            topic_id = (string)p.Attributes("id").FirstOrDefault(),
                            id = ((string)p.Attributes("pid").FirstOrDefault() ?? "").Replace("undefined", "").Trim()
                        })
                        .GroupBy(p => p.Key.topic_id)
                        .Select(p =>
                            new XElement("topic",
                                new XAttribute("topic_id", p.Key),
                                p.Where(s => s.Key.id.Trim() != "").Select(s => s.Key.id).Count() == 0
                                    ? null
                                    : (
                                        new XElement("bookmarks",
                                            p.Where(s => s.Key.id.Trim() != "")
                                            .Select(s => new XElement("bookmark", new XAttribute("id", s.Key.id)))
                                        )
                                      )
                                )
                            )
                    );

                    return new XElement("converted5",
                        labelgroups,
                        labels,
                        itemlabels,
                        docpart,
                        topics
                        );
                }
            }
            return null;
        }
        public static XElement Convert5(this XElement convert)
        {
            if (convert.Name.LocalName == "convert5")
            {
                XElement document = convert.Elements("document").FirstOrDefault();
                XElement index = convert.Elements().Where(p => "index;navigation".Split(';').Contains(p.Name.LocalName)).FirstOrDefault();
                if (document != null && index != null)
                {
                    (
                        from l in document.Descendants("level")
                        join i in index.Descendants().Where(p => p.Name.LocalName.EndsWith("item"))
                        on (string)l.Attributes("id").FirstOrDefault() equals (string)i.Attributes().Where(p => "key;id".Split(';').Contains(p.Name.LocalName)).FirstOrDefault()
                        select new
                        {
                            level = l,
                            item = i
                        }
                    )
                    .ToList()
                    .ForEach(p => p.level.AddFirst(new XElement("title", (string)p.item.Attributes().Where(s => "title;text".Contains(s.Name.LocalName)).FirstOrDefault())));
                    document.Descendants().Where(p => "description;hjemler;tidspunkt;ansvar;references".Split(';').Contains(p.Name.LocalName) && p.Nodes().Count() == 0).ToList().ForEach(p => p.Remove());
                    document.Descendants("topicinfo").Attributes("pid").Where(p => p.Value.Replace("undefined", "") == "").ToList().ForEach(p => p.Remove());

                    document.Descendants("level").Where(p => p.Elements("title").FirstOrDefault() == null && p.Descendants("topicinfo").FirstOrDefault() == null).ToList().ForEach(p => p.Remove());

                    XElement topics = new XElement("topics",
                        document
                        .Descendants("topicinfo")
                        .GroupBy(p => new
                        {
                            topic_id = (string)p.Attributes("id").FirstOrDefault(),
                            id = ((string)p.Attributes("pid").FirstOrDefault() ?? "").Replace("undefined", "").Trim()
                        })
                        .GroupBy(p => p.Key.topic_id)
                        .Select(p => 
                            new XElement("topic",
                                new XAttribute("topic_id", p.Key),
                                p.Where(s => s.Key.id.Trim() != "").Select(s => s.Key.id).Count() == 0
                                    ? null
                                    : (
                                        new XElement("bookmarks",
                                            p.Where(s => s.Key.id.Trim() != "")
                                            .Select(s => new XElement("bookmark", new XAttribute("id", s.Key.id)))
                                        )
                                      )
                                )
                            )
                    );

                    index = new XElement("index", document.Index5());
                    return new XElement("converted",
                        index,
                        document,
                        topics
                        );
                }
            }
            return null;
        }
    }
}
