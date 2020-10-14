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
        public static bool EvalLabels(this Labels l, bool Global, bool View )
        {
            if (l == null)
            {
                return false;
            }
            
            else
            {
                if (Global && !View)
                {
                    if (l.Global)
                        return true;
                    else
                        return false;
                }
                else if (Global && View)
                {
                    if (l.Global || l.View)
                        return true;
                    else
                        return false;
                }
                else if (!Global && View)
                {
                    if (l.View)
                        return true;
                    else
                        return false;
                }
            }
            return false;
        }
        public static void SetLabelAnnotation(this XElement e, int type, IGrouping<string, ChecklistItemData> iD )
        {
            Labels l = e.Annotations<Labels>().FirstOrDefault();
            if (l == null)
            {
                l = new Labels
                {
                    Global = (type == 1 ? true : false),
                    View = (type == 2 ? true : false)
                };
                e.AddAnnotation(l);
            }
            else
            {
                if (type==1)
                {
                    l.Global = true;
                }
                else if (type==2)
                {
                    l.View = true;
                }
            }
        }
        public static IEnumerable<XElement> GetChecklistElement(this XElement xElement, bool Global, bool Local)
        {
            List<XElement> result = new List<XElement>();
            if (xElement.Name.LocalName == "document")
            {
                result.Add(new XElement(
                    xElement.Name.LocalName,
                    xElement.Attributes(),
                    xElement.Elements().Select(p => p.GetChecklistElement(Global, Local))
                    )
                );
            }
            else
            {
                bool select = xElement.Annotations<Labels>().FirstOrDefault().EvalLabels(Global, Local);
                int n = xElement.Descendants().Where(p => p.Annotations<Labels>().FirstOrDefault().EvalLabels(Global, Local)).Count();
                if (select)
                {
                    result.Add(new XElement(
                        xElement.Name.LocalName,
                        xElement.Attributes(),
                        xElement.Elements()
                            .TakeWhile(p=>p.Name.LocalName + ((string)p.Attributes("class").FirstOrDefault() ?? "") != "sectioncheck-item"),
                        xElement.Elements()
                            .SkipWhile(p => p.Name.LocalName + ((string)p.Attributes("class").FirstOrDefault()??"") != "sectioncheck-item")
                            .TakeWhile(p => p.Name.LocalName + ((string)p.Attributes("class").FirstOrDefault() ?? "") == "sectioncheck-item").Select(p=>p.GetChecklistElement(Global, Local))
                        )
                    );
                }
                else if (n>0)
                {
                    result.Add(new XElement(
                        xElement.Name.LocalName,
                        xElement.Attributes(),
                        xElement.Elements()
                            .TakeWhile(p => p.Name.LocalName.StartsWith("h")),
                        xElement.Elements()
                            .SkipWhile(p => p.Name.LocalName + ((string)p.Attributes("class").FirstOrDefault() ?? "") != "sectioncheck-item")
                            .TakeWhile(p => p.Name.LocalName + ((string)p.Attributes("class").FirstOrDefault() ?? "") == "sectioncheck-item").Select(p => p.GetChecklistElement(Global, Local))
                        )
                    );

                }
            }



            return result;
        }
        public static XElement GetChecklistElements(
            this XElement xDocument,
            IEnumerable<string> labelId,
            IEnumerable<ChecklistItemData> itemdata,
            IEnumerable<ChecklistLabel> labels,
            IEnumerable<ChecklistLabelGroup> labelsGroups
        )
        {
            List<string> GLabel = (
                from lg in labelsGroups
                join l in labels
                on lg.labelGroupId equals l.labelGroupId
                join li in labelId
                on l.labelId equals li
                where lg.type == "1"
                select li
            ).ToList();

            List<string> LLabel = (
                from lg in labelsGroups
                join l in labels
                on lg.labelGroupId equals l.labelGroupId
                join li in labelId
                on l.labelId equals li
                where lg.type == "2"
                select li
            ).ToList();


            IEnumerable<IGrouping<string, ChecklistItemData>> GItem = (
               
               from iD in itemdata
               join g in GLabel
               on iD.labelId equals g
               group iD by iD.id into q 
               where q.Count() == GLabel.Count()
               select q
            );

            IEnumerable<IGrouping<string, ChecklistItemData>> LItem = (
               
                from iD in itemdata
                join g in LLabel
                on iD.labelId equals g
                group iD by iD.id into q
                select q
            );

            (
                    from de in xDocument.Descendants()
                    join iD in GItem
                    on ((string)de.Attributes("id").FirstOrDefault()??"").ToLower() equals iD.Key.ToLower()
                    select new { de, iD }
            ).ToList().ForEach(p => p.de.SetLabelAnnotation(1, p.iD));

            (
                    from de in xDocument.Descendants()
                    join iD in LItem
                    on ((string)de.Attributes("id").FirstOrDefault()??"").ToLower() equals iD.Key.ToLower()
                    select new { de, iD }
            ).ToList().ForEach(p => p.de.SetLabelAnnotation(1, p.iD));

            return xDocument.GetChecklistElement(GItem.Count()>0, LItem.Count()>0).FirstOrDefault();
                

            
           

        }
    }
}
