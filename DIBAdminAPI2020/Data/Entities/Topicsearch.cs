using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DIBAdminAPI.Data.Entities
{
    public class Topicsearch
    {
        public string query { get; set; }
        public IList<string> db { get; set; }
        public IList<string> category { get; set; }
        public IList<string> type { get; set; }
        public IList<string> supplier { get; set; }
        public DateTime date { get; set; }
        public DateTime enddate { get; set; }
        public string language { get; set; }

        public XElement SearchFilter()
        {
            return new XElement("filter",
                   (query ?? "") == "" ? null : new XAttribute("query", query),
                   date==null  ? null : new XAttribute("date", date),
                   enddate == null ? null : new XAttribute("enddata", enddate),
                   language == null ? null : new XAttribute("language", language),
                   db == null ? null : db.Count() == 0 ? null : db
                   .Select(p => p)
                   .SelectMany(p => p.ToLower().Split(','))
                   .GroupBy(p => p)
                   .Select(p => new XElement("item",
                           new XAttribute("type", "1"),
                           new XAttribute("value", p.Key)
                       )
                   )
                   ,
                   category == null ? null : category.Count() == 0 ? null : category
                   .Select(p => p)
                   .SelectMany(p => p.ToLower().Split(','))
                   .GroupBy(p => p)
                   .Select(p => new XElement("item",
                           new XAttribute("type", "2"),
                           new XAttribute("value", p.Key)
                       )
                   )
                   ,
                   type == null ? null : type.Count() == 0 ? null : type
                   .Select(p => p)
                   .SelectMany(p => p.ToLower().Split(','))
                   .GroupBy(p => p)
                   .Select(p => new XElement("item",
                           new XAttribute("type", "3"),
                           new XAttribute("value", p.Key)
                       )
                   )
                   ,
                   supplier == null ? null : supplier.Count() == 0 ? null : supplier
                   .Select(p => p)
                   .SelectMany(p => p.ToLower().Split(','))
                   .GroupBy(p => p)
                   .Select(p => new XElement("item",
                           new XAttribute("type", "4"),
                           new XAttribute("value", p.Key)
                       )
                   )
               );
        }
    }

}
