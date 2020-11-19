using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace DIBDocument
{
    public static class DIBLinkFormat
    {
        public static XElement IDLinksGet(this XElement idlinks, string topic_id)
        {
            string version = (string)idlinks.AncestorsAndSelf().Attributes("version").FirstOrDefault();
            XElement topics = null;
            if (version == null)
            #region //versjon=""
            {
                topics = new XElement("topics",
                        idlinks
                        .Descendants()
                        .Where(p => p.Name.LocalName == "idlink" || p.Name.LocalName == "link")
                        .GroupBy(p => (string)p.Attributes("topic_id").FirstOrDefault() ?? "")
                        .Where(p => p.Key != "")
                        .Select(p => new XElement("topic",
                            new XAttribute("topic_id", p.Key == "_self" ? topic_id.ToString() : p.Key),
                                p.Select(o => o).Where(o => o.Attribute("segment_id") != null).Count() == 0
                                ?
                                (
                                    p.Select(o => o).Where(o => o.Attribute("segment_id") == null)
                                    .GroupBy(o => (string)o.Attributes("bm").FirstOrDefault() ?? "")
                                    .Where(o => o.Key != "")
                                    .Select(o => new XElement("select",
                                        new XAttribute("select_id", o.Key)
                                        )
                                    )
                                )
                                :
                                (
                                    p.Select(o => o).Where(o => o.Attribute("segment_id") != null)
                                    .GroupBy(o => (string)o.Attributes("segment_id").FirstOrDefault())
                                    .Where(o => o.Key != "")
                                    .Select(o => new XElement("segment",
                                            new XAttribute("segment_id", o.Key),
                                            o.Select(r => r).Where(r => r.Attribute("bm") != null)
                                            .GroupBy(r => (string)r.Attributes("bm").FirstOrDefault() ?? "")
                                            .Where(r => r.Key != "")
                                            .Select(r => new XElement("select",
                                                new XAttribute("select_id", r.Key)
                                                )
                                            )
                                        )
                                    )
                                )


                            )
                        )
                );
            }
            #endregion
            else if (version == "1.0")
            #region //version = "1.0"
            {
                topics = new XElement("topics",
                    idlinks
                    .Descendants("topic")
                    .GroupBy(topic => (string)topic.Attributes("topic_id").FirstOrDefault())
                    .Select(topic =>
                        new XElement("topic",
                            new XAttribute("topic_id", topic.Key == "_self" ? topic_id.ToString() : topic.Key),
                            topic
                                .Where(s => s.Elements("segment").Attributes("segment_id").Count() != 0)
                                .SelectMany(s => s.Elements("segment"))
                                  .GroupBy(segment => (string)segment.Attributes("segment_id").FirstOrDefault() ?? "")
                                  .Select(segment => new XElement("segment",
                                            segment.Key == "" ? null : new XAttribute("segment_id", segment.Key),
                                            segment.SelectMany(r => r.Elements("select"))
                                                .Where(select => (string)select.Attributes("select_id").FirstOrDefault() != "_self")
                                                .GroupBy(select => (string)select.Attributes("select_id").FirstOrDefault())
                                                .Select(select => new XElement("select",
                                                    new XAttribute("select_id", select.Key),
                                                        select.SelectMany(m => m.Elements("mark"))
                                                            .GroupBy(mark => (string)mark.Attributes("mark_id").FirstOrDefault())
                                                            .Select(mark => new XElement("mark",
                                                                new XAttribute("mark_id", mark.Key)))))))
                            .Union(
                                topic
                                    .Where(s => s.Elements("segment").Attributes("segment_id").Count() == 0)
                                    .SelectMany(r => r.Descendants("select"))
                                    .Where(select => (string)select.Attributes("select_id").FirstOrDefault() != "_self")
                                    .GroupBy(select => (string)select.Attributes("select_id").FirstOrDefault())
                                    .Select(select => new XElement("select",
                                        new XAttribute("select_id", select.Key),
                                            select.SelectMany(m => m.Elements("mark"))
                                                .GroupBy(mark => (string)mark.Attributes("mark_id").FirstOrDefault())
                                                .Select(mark => new XElement("mark",
                                                    new XAttribute("mark_id", mark.Key))))))
                        )
                    )
                );
            }
            #endregion
            return topics;
        }
    }
}
