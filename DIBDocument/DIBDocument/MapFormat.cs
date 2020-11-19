using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace DIBDocument
{
    public static class MapFormat
    {
        public static string SegmentId(this XElement map, string id)
        {
            XAttribute attribute = map
                            .DescendantsAndSelf()
                            .Attributes().Where(p => "key;id".Split(';').Contains(p.Name.LocalName) && p.Value.Trim().ToLower() == id)
                            .FirstOrDefault();
            if (attribute == null) return null;
            string sement_id = (string)attribute
                                .Parent
                                .AncestorsAndSelf()
                                .Where(p => (string)p.Attributes("segment").FirstOrDefault() == "true")
                                .Attributes().Where(p => "key;id".Split(';').Contains(p.Name.LocalName))
                                .FirstOrDefault();
            return sement_id;
        }
        private static IEnumerable<XElement> MapChildren(this XElement Map)
        {
            List<XElement> result = new List<XElement>();
            XElement root = Map.DescendantsAndSelf().Where(p => "nitems;sitems;index;navigation-ext".Split(';').Contains(p.Name.LocalName)).FirstOrDefault();
            if (root != null)
                Map = root;
            if (Map.Name.LocalName != "index")
            {
                if (!Map.Name.LocalName.EndsWith("item") && Map.Elements().Where(p => p.Attributes()
                                    .Where(s =>
                                        "key;id".Split(';').Contains(s.Name.LocalName)
                                    || "title;text".Split(';').Contains(s.Name.LocalName)
                                    ).Count() == 2).Count() == 1)
                {
                    Map = Map.Elements().Where(p => p.Attributes()
                                    .Where(s =>
                                        "key;id".Split(';').Contains(s.Name.LocalName)
                                    || "title;text".Split(';').Contains(s.Name.LocalName)
                                    ).Count() == 2)
                                    .FirstOrDefault();

                    if ((string)Map.Attributes("segment").FirstOrDefault() == "true")
                    {
                        result.Add(new XElement("item",
                                new XAttribute("id", (string)Map.Attributes().Where(s => "key;id".Split(';').Contains(s.Name.LocalName)).FirstOrDefault() ?? "none"),
                                new XAttribute("title", (string)Map.Attributes().Where(s => "title;text".Split(';').Contains(s.Name.LocalName)).FirstOrDefault() ?? "No title"),
                                ((string)Map.Attributes("segment").FirstOrDefault() ?? "") == "true" ? new XAttribute("s", "1") : null
                            )
                        );
                    }

                }
            }


            result.AddRange(
                Map
                .Elements()
                .Where(p => p.Attributes()
                            .Where(s => 
                                "key;id".Split(';').Contains(s.Name.LocalName)
                            || "title;text".Split(';').Contains(s.Name.LocalName)
                            ).Count() == 2
                )
                .Select(p =>
                    new XElement("item",
                        new XAttribute("id", (string)p.Attributes().Where(s => "key;id".Split(';').Contains(s.Name.LocalName)).FirstOrDefault() ?? "none"),
                        new XAttribute("title", (string)p.Attributes().Where(s => "title;text".Split(';').Contains(s.Name.LocalName)).FirstOrDefault() ?? "No title"),
                        ((string)p.Attributes("segment").FirstOrDefault() ?? "") == "true" ? new XAttribute("s", "1") : null,
                        ((string)p.Attributes("segment").FirstOrDefault() ?? "") == "true" ? null : p.MapChildren()
                    )
                )
            );
            return result;
        }
        public static XElement IndexItem(this XElement p, bool map = false, bool vsegment=false)
        {
            if (((string)p.Attributes().Where(s => "title;text".Split(';').Contains(s.Name.LocalName)).FirstOrDefault()??"") == "") return null; ;

            return new XElement("item",
                        new XAttribute("id", (string)p.Attributes().Where(s => "key;id".Split(';').Contains(s.Name.LocalName)).FirstOrDefault() ?? "none"),
                        new XAttribute("title", (string)p.Attributes().Where(s => "title;text".Split(';').Contains(s.Name.LocalName)).FirstOrDefault() ?? "No title"),
                        (((string)p.Attributes("segment").FirstOrDefault() ?? "") == "true" || vsegment) ? new XAttribute("s", "1") : null,
                        map ? p.MapChildren(): null
                    );
        }
        public static IEnumerable<XNode> IndexItemSegment(this IEnumerable<XNode> e)
        {
            return e.OfType<XElement>().Where(p=>(string)p.Attributes("segment").FirstOrDefault()=="true").Select(p => p.IndexItem());
        }

        public static XElement IndexItemPartent(this XElement e)
        {
            if (e == null) return null; 
            string id = e.AttributesGetValue("key;id");
            string title = e.AttributesGetValue("title;text");
            string segment = e.AttributesGetValue("segment");
            if (id != "" && title != "" && segment != "")
            {
                XElement parent = e.Parent.IndexItemPartent();
            }
            else
            {
                return null;
            }
            return null;
        }
        public static XElement IndexData(this XElement Map, string Id, string SegmentId)
        {

            XElement firstsegment = Map.DescendantsAndSelf().Where(p=>(string)p.Attributes("segment").FirstOrDefault() == "true").FirstOrDefault();
            XAttribute segment = null;
            SegmentId = SegmentId == null ? "" : SegmentId;
            if (SegmentId != "" && firstsegment == null)
            {
                segment = Map
                    .DescendantsAndSelf()
                    .Attributes().Where(p => "key;id".Split(';').Contains(p.Name.LocalName) && p.Value.Trim().ToLower() == SegmentId.ToLower())
                    .FirstOrDefault();
                if (segment != null)
                {
                    XElement sindex = new XElement("index",
                        new XAttribute("msg", "success"),
                        new XAttribute("segment_id", segment.Value),
                        segment.Parent.NodesBeforeSelf().OfType<XElement>().Select(p => p.IndexItem(false ,true)),
                        segment.Parent.IndexItem(true),
                        segment.Parent.NodesAfterSelf().OfType<XElement>().Select(p => p.IndexItem(false, true))
                    );

                    return sindex;

                }

                return new XElement("index",
                        new XAttribute("msg", "empty")
                );

            }

            if ((SegmentId != "" || firstsegment != null) && Id == "")
            {
                if  (SegmentId != "")
                {
                    segment = Map
                    .DescendantsAndSelf()
                    .Attributes().Where(p => "key;id".Split(';').Contains(p.Name.LocalName) && p.Value.Trim().ToLower() == SegmentId.ToLower())
                    .FirstOrDefault();
                }

                if (segment == null && firstsegment != null)
                {
                    segment = firstsegment.Attributes().Where(p => "key;id".Split(';').Contains(p.Name.LocalName)).FirstOrDefault();
                    XElement sindex = new XElement("index",
                        new XAttribute("msg", "success"),
                        new XAttribute("segment_id", (string)segment
                                                    .Parent
                                                    .AncestorsAndSelf()
                                                    .Where(p => (string)p.Attributes("segment").FirstOrDefault() == "true")
                                                    .Attributes().Where(p => "key;id".Split(';').Contains(p.Name.LocalName))
                                                    .FirstOrDefault()),
                        segment.Parent.Nodes().OfType<XElement>().IndexItemSegment(),
                        segment.Parent.NodesAfterSelf().OfType<XElement>().IndexItemSegment()
                    );
                    return sindex;
                }

                if (segment != null)
                {
                    if ((string)segment.Parent.Attributes("segment").FirstOrDefault() == "true")
                    {
                        XElement top = segment.Parent;
                        
                        string top_title = (string)top.Attributes().Where(s => "title;text".Split(';').Contains(s.Name.LocalName)).FirstOrDefault();
                        if ((top_title == null ? "_top" : top_title)=="_top")
                        {
                            return new XElement("index",
                                new XAttribute("msg", "success top"),
                                new XAttribute("segment_id", (string)segment.Parent.AncestorsAndSelf().Where(p => (string)p.Attributes("segment").FirstOrDefault() == "true").Attributes().Where(p => "key;id".Split(';').Contains(p.Name.LocalName)).FirstOrDefault()),
                                top.Elements()
                                    .Where(p=>((string)p.Attributes().Where(s => "title;text".Split(';').Contains(s.Name.LocalName)).FirstOrDefault() ?? "")!="")
                                    .Select(p=> p.IndexItem())
                            );
                        }
                             

                        List<XElement> anc = top
                                        .AncestorsAndSelf()
                                        .TakeWhile(p => 
                                               p.Attributes().Where(s => "title;text".Split(';').Contains(s.Name.LocalName)).Count()!=0 
                                            && ((string)p.Attributes().Where(s=>"id;key".Split(';').Contains(s.Name.LocalName)).FirstOrDefault()??"_top")!="_top"  
                                            && (string)p.Attributes("segment").FirstOrDefault() == "true").Reverse().ToList();
                        if (anc.Count()==0)
                        {
                            return  new XElement("index",
                                new XAttribute("msg", "success no parents"),
                                new XAttribute("segment_id", (string)top.AncestorsAndSelf().Where(p => (string)p.Attributes("segment").FirstOrDefault() == "true").Attributes().Where(p => "key;id".Split(';').Contains(p.Name.LocalName)).FirstOrDefault()),
                                top.NodesBeforeSelf().OfType<XElement>().Select(p => p.IndexItem()),
                                top.IndexItem(true),
                                top.NodesAfterSelf().OfType<XElement>().Select(p => p.IndexItem())
                            );
                        }

                        XElement index = new XElement("index",
                            new XAttribute("msg", "success with anc"),
                            new XAttribute("segment_id", (string)top.AncestorsAndSelf().Where(p => (string)p.Attributes("segment").FirstOrDefault() == "true").Attributes().Where(p => "key;id".Split(';').Contains(p.Name.LocalName)).FirstOrDefault())
                        );

                        XElement current = index;
                        XElement item = null;
                        for (int i = 0; i < anc.Count(); i++)
                        {
                            XElement a = anc.ElementAt(i);
                            if (a == top)
                            {
                                item = a.IndexItem(true);
                                current.Add(a.NodesBeforeSelf().OfType<XElement>().Select(p => p.IndexItem()));
                                current.Add(item);
                                current.Add(a.NodesAfterSelf().OfType<XElement>().Select(p => p.IndexItem()));
                                current = item;
                            }
                            else
                            {
                                item = a.IndexItem();
                                if (i == 0)
                                {
                                    current.Add(a.NodesBeforeSelf().OfType<XElement>().Select(p=>p.IndexItem()));
                                    current.Add(item);
                                    current.Add(a.NodesAfterSelf().OfType<XElement>().Select(p => p.IndexItem()));
                                    current = item;
                                }
                                else
                                {
                                    current.Add(item);
                                    current = item;
                                }
                                
                            }
                        }

                        

                        return index;
                    }
                    else
                    {
                        return new XElement("index", new XAttribute("msg", "fant ikke segment"), segment.Parent);
                    }

                    
                }

                return new XElement("index", new XAttribute("error", "NoSegment"));
            }
            

            XAttribute attribute = null;
            attribute = Map
                .DescendantsAndSelf()
                .Attributes().Where(p => "key;id".Split(';').Contains(p.Name.LocalName) && p.Value.Trim().ToLower() == Id.ToLower())
                .FirstOrDefault();


            string segment_id = "";
            string id = "";
            XElement parents = null;
            if (attribute != null)
            {
                segment_id = (string)attribute
                                    .Parent
                                    .AncestorsAndSelf()
                                    .Where(p => (string)p.Attributes("segment").FirstOrDefault() == "true")
                                    .Attributes()
                                    .Where(p => "key;id".Split(';').Contains(p.Name.LocalName))
                                    .FirstOrDefault() ?? "";
                id = (string)attribute
                                    .Parent
                                    .AncestorsAndSelf()
                                    .Where(p => p.Attributes().Where(s => "text;title".Split(';').Contains(s.Name.LocalName)).FirstOrDefault() != null)
                                    .Attributes().Where(p => "key;id".Split(';').Contains(p.Name.LocalName))
                                    .FirstOrDefault() ?? "";

                parents = new XElement("parents", 
                                    attribute
                                    .Parent
                                    .Ancestors()
                                    .Reverse()
                                    .Where(p => 
                                            p.Attributes().Where(s => "text;title".Split(';').Contains(s.Name.LocalName)).FirstOrDefault() != null
                                            && (string)p.Attributes().Where(s => "key;id".Split(';').Contains(s.Name.LocalName)).FirstOrDefault() != "_top"
                                    )
                                    .Select(p => new XElement("parent",
                                        new XAttribute("id", (string)p.Attributes().Where(s => "key;id".Split(';').Contains(s.Name.LocalName)).FirstOrDefault()),
                                        new XAttribute("name", (string)p.Attributes().Where(s => "text;title".Split(';').Contains(s.Name.LocalName)).FirstOrDefault())
                                        )
                                    )
                );
                                    

            }

            
            return new XElement("index",
                new XAttribute("msg", "normal med id"),
                segment_id != "" ? new XAttribute("segment_id", segment_id): null,
                id != "" && id != segment_id ? new XAttribute("id", id):null ,
                parents,
                Map.MapChildren()
            );
        }
    }
}
