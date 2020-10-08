using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using System.Runtime.CompilerServices;
using DIBAdminAPI.Data.Entities;
using DIBAdminAPI.Models;

namespace DIBAdminAPI.Data.Entities
{
    public static class DocumentExtentions
    {
        public static XElement GetChecklistElements(this XElement element, IEnumerable<string> labels)
        {
            List<XElement> result = new List<XElement>(); 
            int i = (
                    from l in labels
                    join e in element.Annotations<Labels>().SelectMany(p => p.labelIds)
                    on l equals e
                    select l
            ).ToList().Count();

            int n = (
                from l in labels
                join e in element.DescendantsAndSelf().SelectMany(p => p.Annotations<Labels>().SelectMany(l => l.labelIds))
                on l equals e
                select l
            ).ToList().Count();
            if (i > 0 )
            {

            }

                

            
            return null;

        }
    }
}
