using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using static DIBAdminAPI.Models.Result;
using DIBAdminAPI.Helpers.Extentions;
using System.IO;
using DIBAdminAPI.Services;
using System.Security.Cryptography;
using System.Runtime.CompilerServices;

namespace DIBAdminAPI.Data.Entities
{ 
    public class DibLinkSelect
    {
        public Guid resourceId { get; set; }
        public string segmentId { get; set; }
        public string regexName { get; set; }
    }
    public class DIBLinkData
    {
        public string tag1 { get; set; }
        public string name { get; set; }
        public Guid resourceId { get; set; }
        public string tag1Id { get; set; }
        public string tag2 { get; set; }
        public string tag2Id { get; set; }
        public string tag2SegmentId { get; set; }
        public string tag2Idx { get; set; }
        public string totag2 { get; set; }
        public string totag2Id { get; set; }
        public string totag2SegmentId { get; set; }
        public string totag2Idx { get; set; }
        public string tag3 { get; set; }
        public string tag3Id { get; set; }
        public string tag3Idx { get; set; }
        public string totag3 { get; set; }
        public string totag3Id { get; set; }
        public string totag3Idx { get; set; }
    }
    public class ElementMark
    {
        public bool mark { get; set; }
        public ElementMark(XElement e)
        {
            if (e.Name.LocalName != "span")
            {
                mark = true;
                e.AddAnnotation(this);
            }
            else
            {
                XElement ledd = e.Ancestors().Where(p => (string)p.Attributes("class").FirstOrDefault() == "lovdata-ledd").FirstOrDefault();
                if (ledd != null ? ledd.Annotations<ElementMark>().FirstOrDefault() == null : false)
                {
                    mark = true;
                    e.AddAnnotation(this);
                }
            }
        }
    }
    public class SegmentSelect
    {
        public bool select { get; set; }
        public bool marks { get; set; }
        public SegmentSelect(SelectedElement e)
        {
            select = true;
            e.element.AddAnnotation(this);
            if (e.mark.Elements("mark").Where(p => ((string)p.Attributes("id").FirstOrDefault() ?? "") != "").Count() > 0)
            {
                marks = true;
                List<string> ids = e.mark.Elements("mark").Where(p => ((string)p.Attributes("id").FirstOrDefault() ?? "") != "").Select(p => (string)p.Attributes("id").FirstOrDefault()).ToList();
                e.element.Descendants().Where(p => ids.Contains(((string)p.Attributes("id").FirstOrDefault() ?? ""))).ToList().ForEach(p => new ElementMark(p));
            }
        }
    }
    public class SelectedElement
    {
        public XElement element { get; set; }
        public XElement mark { get; set; }
    }
    public static class DocumentContainerExtentions
    {
        public static void AddOtherProps(this JsonElement d, string name, List<string> e)
        {
            if (d.otherprops==null)
            {
                d.otherprops = new Dictionary<string, dynamic>();
                d.otherprops.Add(name, e);
            }
            else
            {
                d.otherprops.Add(name, e);
            }
        }

        public static IEnumerable<XNode> GetMarkedItem(this XNode n)
        {
            List<XNode> result = new List<XNode>();
            if (n.NodeType != XmlNodeType.Element)
            {
                result.Add(n);
                return result;
            }
            XElement e = (XElement)n;
            if (e.Name.LocalName == "li" && e.Parent.Name.LocalName == "ol")
            {
                int iValue = e.NodesBeforeSelf().OfType<XElement>().Where(p => p.Name.LocalName == "li").Count() + 1;
                string value = (string)e.Attributes("value").FirstOrDefault();
                if (value == null)
                {
                    e.Add(new XAttribute("value", iValue.ToString()));
                }
                else
                {
                    if (!Regex.IsMatch(value.Trim(), @"^\d+$"))
                    {
                        e.Attributes("value").FirstOrDefault().SetValue(iValue.ToString());
                    }
                }
            }
            ElementMark em = e.Annotations<ElementMark>().FirstOrDefault();
            if (em != null)
            {
                result.Add(e);
            }
            else
            {

                result.Add(new XElement(e.Name.LocalName, e.Attributes(), e.GetMarkedItems()));


            }
            return result;
        }
        public static IEnumerable<XNode> GetSelectedItem(this XNode n)
        {
            List<XNode> result = new List<XNode>();
            if (n.NodeType != XmlNodeType.Element)
            {
                result.Add(n);
                return result;
            }

            XElement e = (XElement)n;
            SegmentSelect se = e.Annotations<SegmentSelect>().FirstOrDefault();
            if (se != null)
            {
                ElementMark em = e.DescendantsAndSelf().Where(p => p.Annotations<ElementMark>().FirstOrDefault() != null).Select(p => p.Annotations<ElementMark>().FirstOrDefault()).FirstOrDefault();
                if (em == null)
                {
                    result.Add(e);
                }
                else
                {
                    XElement hx = e.Elements().Where(p => Regex.IsMatch(p.Name.LocalName, @"h\d")).FirstOrDefault();
                    string header = "";
                    string id = null;
                    if (hx != null)
                    {
                        header =
                        e.Name.LocalName == "section"
                        ? hx.DescendantNodes().OfType<XText>().Where(s => s.Parent.Ancestors().Where(a => !("a".Contains(a.Name.LocalName))).Count() > 0).Select(s => s.Value).StringConcatenate(" ")
                        : "";

                        header = Regex.Replace(header, @"\s+", " ").Trim();
                        id = (string)e.Attributes("id").FirstOrDefault();
                        hx.Remove();

                        hx = new XElement(
                                e.Name.LocalName,
                                e.Attributes(),
                                new XElement("p",
                                    new XElement("strong",
                                        new XElement("a",
                                            new XAttribute("class", "ifrsdeflink"),
                                            new XAttribute("data-segment", id),
                                            new XAttribute("data-id", id),
                                            new XText(header)
                                        )
                                    )
                                )
                        );
                        result.Add(new XElement(e.Name.LocalName, e.Attributes(), hx, e.GetMarkedItems()));
                    }
                    else
                    {
                        result.Add(new XElement(e.Name.LocalName, e.Attributes(), e.GetMarkedItems()));
                    }
                }

            }
            else
            {
                result.Add(new XElement(e.Name.LocalName, e.Attributes(), e.GetSelectedItems()));
            }
            return result;
        }
        public static IEnumerable<XNode> GetMarkedItems(this XNode n)
        {
            List<XNode> result = new List<XNode>();
            List<XNode> done = new List<XNode>();
            if (n.NodeType != XmlNodeType.Element)
            {
                result.Add(n);
                return result;
            }
            XElement e = (XElement)n;
            string name = e.Name.LocalName;
            if (name == "ol")
            {

            }
            XNode last = e.FirstNode;
            while (last != null)
            {
                List<XNode> before =
                    e
                    .Nodes()
                    .SkipWhile(p => done.Contains(p))
                    .TakeWhile(p =>
                            (
                                p.NodeType == XmlNodeType.Element
                                ? ((XElement)p).DescendantsAndSelf().Where(d => d.Annotations<ElementMark>().FirstOrDefault() != null).Count() == 0
                                : true
                            )
                        )
                        .ToList();
                done.AddRange(before);

                List<XNode> selected = e
                    .Nodes()
                    .SkipWhile(p => done.Contains(p))
                    .TakeWhile(p =>
                        (
                            p.NodeType == XmlNodeType.Element
                            ? ((XElement)p).DescendantsAndSelf().Where(d => d.Annotations<ElementMark>().FirstOrDefault() != null).Count() != 0
                            : true
                        )
                    )
                    .ToList();
                done.AddRange(selected);

                last = e.Nodes().SkipWhile(p => done.Contains(p)).Take(1).FirstOrDefault();

                if (selected.Count() > 0)
                {
                    if (before.Count() > 0 && (e.Name.LocalName == "section" && (string)e.Attributes("class").FirstOrDefault() == "lovdata-ledd"))
                    {
                        result.AddRange(before);
                    }
                    else if (before.Count() > 0)
                    {
                        result.Add(new XElement("div", "       - - - - "));
                    }

                    if (name == "ol")
                    {
                        result.Add(new XElement(e.Name.LocalName, e.Attributes(), selected.SelectMany(p => p.GetMarkedItem())));
                    }
                    else
                    {
                        result.AddRange(selected.SelectMany(p => p.GetMarkedItem()));
                    }
                }

                if (last == null && selected.Count() == 0 && before.Count() > 0)
                {
                    result.Add(new XElement("div", "       - - - - "));
                }
            }
            return result;
        }
        public static IEnumerable<XNode> GetSelectedItems(this XElement e)
        {
            List<XNode> result = new List<XNode>();
            List<XNode> done = new List<XNode>();
            string name = e.Name.LocalName;
            XElement hx = e.Elements().Where(p => Regex.IsMatch(p.Name.LocalName, @"h\d")).FirstOrDefault();
            string header = "";
            string id = null;
            if (hx != null)
            {
                header =
                name == "section"
                ? hx.DescendantNodes().OfType<XText>().Where(s => s.Parent.Ancestors().Where(n => !("a".Contains(n.Name.LocalName))).Count() > 0).Select(s => s.Value).StringConcatenate(" ")
                : "";

                header = Regex.Replace(header, @"\s+", " ").Trim();
                id = (string)e.Attributes("id").FirstOrDefault();
                hx.Remove();
            }

            XNode last = e.FirstNode;
            while (last != null)
            {
                List<XNode> before =
                    e
                    .Nodes()
                    .SkipWhile(p => done.Contains(p))
                    .TakeWhile(p =>
                        (
                            p.NodeType == XmlNodeType.Element
                            ? (
                                ((XElement)p).DescendantsAndSelf().Where(a => a.Annotations<SegmentSelect>().FirstOrDefault() != null).Count() == 0
                                && ((XElement)p).DescendantsAndSelf().Where(a => a.Annotations<ElementMark>().FirstOrDefault() != null).Count() == 0
                            )
                            : true
                        )
                    ).ToList();
                done.AddRange(before);

                List<XNode> selected = e
                    .Nodes()
                    .SkipWhile(p => done.Contains(p))
                    .TakeWhile(p =>
                        (
                            p.NodeType == XmlNodeType.Element
                            ? (
                                ((XElement)p).DescendantsAndSelf().Where(a => a.Annotations<SegmentSelect>().FirstOrDefault() != null).Count() != 0
                                || ((XElement)p).DescendantsAndSelf().Where(a => a.Annotations<ElementMark>().FirstOrDefault() != null).Count() != 0
                            )
                            : true
                        )
                    ).ToList();
                done.AddRange(selected);

                if (selected.Count() > 0)
                {
                    if (before.Count() > 0 && header != "" && id != null)
                    {
                        result.Add(
                            new XElement("section",
                                new XElement("a",
                                    new XAttribute("class", "ifrsdeflink"),
                                    new XAttribute("data-segment", id),
                                    new XAttribute("data-id", id),
                                    new XText(header)
                                ),
                                new XElement("div", "    - - - - - - "),
                                selected.SelectMany(p => p.GetSelectedItem())
                            )
                        );
                    }
                    else
                    {
                        result.AddRange(selected.SelectMany(p => p.GetSelectedItem()));
                    }
                }

                last = e
                    .Nodes()
                    .SkipWhile(p => done.Contains(p))
                    .Take(1)
                    .FirstOrDefault();
            }
            return result;
        }
        public static XElement GetDocumentParts(this XElement document, XElement items)
        {
            if (items == null || document == null) return null;
            (
                from d in document.DescendantsAndSelf().Where(p => (string)p.Attributes("id").FirstOrDefault() != null)
                join i in items.DescendantsAndSelf().Where(p => "item;select".Split(';').Contains(p.Name.LocalName))
                on
                    ((string)d.Attributes("id").FirstOrDefault() ?? "-1").Trim().ToLower()
                    equals
                    ((string)i.Attributes("id").FirstOrDefault() ?? "-0").Trim().ToLower()
                select new SelectedElement { element = d, mark = i }
            ).ToList()
            .ForEach(p => new SegmentSelect(p));

            document.Descendants("p").Where(p => (string)p.Attributes("class").FirstOrDefault() == "lovfotnote").ToList().ForEach(p => p.Remove());

            XElement result = null;
            if (items.Name.LocalName == "items")
            {
                List<XElement> sections =
                    document
                    .Descendants()
                    .Where(p => p.Annotations<SegmentSelect>().FirstOrDefault() != null)
                    .Select(p => p.AncestorsAndSelf("section").Where(a => a.Elements().Where(s => Regex.IsMatch(s.Name.LocalName, @"h\d")).FirstOrDefault() != null).FirstOrDefault())
                    .Select(p =>
                        new XElement(p.Name.LocalName,
                            p.Attributes(),
                            p.Elements()
                        )
                    )
                    .ToList();
                if (sections.Count() == 0) return document;

                result = new XElement(document.Name.LocalName,
                    document.Attributes(),
                    sections
                );
            }
            else
            {

                result = new XElement(document.Name.LocalName,
                    document.Attributes(),
                    document.GetSelectedItems()
                );
            }
            if (result == null)
            {
                result = document;
            }
            return result;
        }
    }
    
    public class DocumentElementdata
    {
        public Dictionary<string,Dictionary<string,List<string>>> elementdata { get; set; }
        public Dictionary<string, ObjectApi> objects = new Dictionary<string, ObjectApi>();
    }
    public class LinkData
    {
        public string id { get; set; }
        public Guid destResourceid { get; set; }
        public string regexName { get; set; }
        public string title { get; set; }
        public string tag1 { get; set; }
        public string tag2 { get; set; }
        public string destName { get; set; }
        public string desttype { get; set; }
        public LinkData() { }
        
    }
    public class LinkInfo
    {
        public string Id { get; set; }
        public string dId { get; set; }
        public string rName { get; set; }
        public string rText { get; set; }
        public string rTag1 { get; set; }
        public string rTag2 { get; set; }
        public string dName { get; set; }
        public string dType { get; set; }
        public LinkInfo(LinkData idLink)
        {
            if (idLink == null) return;
            Id = idLink.id;
            dId = idLink.destResourceid.ToString();
            rName = idLink.regexName;
            rText = idLink.title;
            rTag1 = idLink.tag1;
            rTag2 = idLink.tag2;
            dName = idLink.destName;
            dType = idLink.desttype.ToString();
        }
        public LinkInfo(XElement idLink)
        {
            if (idLink == null) return;
            Id = (string)idLink.Attributes("id").FirstOrDefault();
            dId = (string)idLink.Attributes("rid").FirstOrDefault();
            rName = (string)idLink.Attributes("rname").FirstOrDefault();
            rText = (string)idLink.Attributes("text").FirstOrDefault();
            rTag1 = (string)idLink.Attributes("tag1").FirstOrDefault();
            rTag2 = (string)idLink.Attributes("tag2").FirstOrDefault();
            dName = (string)idLink.Attributes("dname").FirstOrDefault();
            dType = (string)idLink.Attributes("dname").FirstOrDefault();
        }
    }

    public class CheckLabelJsons
    {
        public List<string> viewroot { get; set; }
        public List<string> showroot { get; set; }
        public Dictionary<string, ObjectApi> objects = new Dictionary<string, ObjectApi>();
        public CheckLabelJsons(IEnumerable<ChecklistLabelGroup> gr, IEnumerable<ChecklistLabel> lb, IEnumerable<ChecklistItemData> iD)
        {
            List<IGrouping<ChecklistLabelGroup, ChecklistLabel>> gl = new List<IGrouping<ChecklistLabelGroup, ChecklistLabel>>();  
            if (iD.Select(p => p.id).FirstOrDefault() != null)
            {
                gl.AddRange(
                    from i in iD
                    join l in lb
                    on i.labelId equals l.labelId
                    join g in gr
                    on l.labelGroupId equals g.labelGroupId
                    group l by g into c
                    select c
                );

            }
            List<IGrouping<ChecklistLabelGroup, ChecklistLabel>> gla = new List<IGrouping<ChecklistLabelGroup, ChecklistLabel>>();
            if (gr.Select(p => p.labelGroupId).FirstOrDefault() != null)
            {
                gla.AddRange(
                         from l in lb
                         join g in gr
                         on l.labelGroupId equals g.labelGroupId
                         group l by g into c
                         select c
                );
                viewroot = gla.Where(p => p.Key.type == "1").OrderBy(p => p.Key.name).Select(p => p.Key.labelGroupId.ToString()).ToList();
                showroot = gla.Where(p => p.Key.type == "2").OrderBy(p => p.Key.name).Select(p => p.Key.labelGroupId.ToString()).ToList();

                objects.AddRange(
                    (
                        from ga in gla
                        join gs in gl
                        on ga.Key.labelGroupId equals gs.Key.labelGroupId into g
                        from subG in g.DefaultIfEmpty()
                        select new {a = ga, b = (subG == null ? false : true)}
                    ).ToDictionary(
                        p => p.a.Key.labelGroupId.ToString(),
                        p => new ObjectApi
                        {
                            id = p.a.Key.labelGroupId,
                            type = "clgrouplabel",
                            selected = p.b,
                            transactionId = p.a.Key.transactionId,
                            data = new
                            {
                                id = p.a.Key.labelGroupId,
                                name = p.a.Key.name,
                                type = p.a.Key.type,
                                selected=p.b
                            },
                            children = p.a.Select(l => l.labelId.ToString()).ToList()
                        }
                    )
                );
                objects.AddRange(
                    (from l in gla.SelectMany(p => p.Select(l => l))
                     join i in iD.GroupBy(p => p.labelId).Select(p => p.Key)
                     on l.labelId equals i into g
                     from subG in g.DefaultIfEmpty()
                     select new {a = l, b = (subG == null ? false :true)}
                    )
                    .ToDictionary(
                        p => p.a.labelId.ToString(),
                        p => new ObjectApi
                        {
                            id = p.a.labelId,
                            type = "cllabel",
                            selected = p.b,
                            transactionId = p.a.transactionId,
                            data = new
                            {
                                id = p.a.labelId,
                                name = p.a.name,
                                groupId = p.a.labelGroupId,
                                selected = p.b
                            }

                        }
                    )
                );
            }


        }
    }
    public class DocumentContainer
    {
        public int eCount { get; set; }
        public bool Edited = false;
        public string topicId { get; set; }
        public string selectId { get; set; }
        public string segmentId { get; set; }
        public string id { get; set; }
        public string resourceTypeId { get; set; }
        public bool access { get; set; }
        public bool companylookup { get; set; }
        public string name { get; set; }
        public List<JsonChild> root { get; set; }
        public Dictionary<string, JsonElement> elements { get; set; }
        public List<JsonChild> tocroot { get; set; }
        public Dictionary<string, JsonToc> toc { get; set; }
        //public Dictionary<string, AccountingElementApi> elementdata { get; set; }
        public Dictionary<string, Dictionary<string, List<string>>> elementdata = new Dictionary<string, Dictionary<string, List<string>>>();
        public Dictionary<string, ObjectApi> objects = new Dictionary<string, ObjectApi>();
        public IEnumerable<XVariable> variables { get; set; }
        public IEnumerable<XObj> xobjects { get; set; }
        public List<string> viewroot { get; set; }
        public List<string> showroot { get; set; }
        private IEnumerable<ChecklistItemData> ItemData { get; set; }
        private IEnumerable<ChecklistLabelGroup> LabelGroups { get; set; }
        private IEnumerable<ChecklistLabel> Labels { get; set; }
        private IEnumerable<AccountLine> AccountLines { get; set; }
        private IEnumerable<TaxLine> TaxLines { get; set; }
        
        public IEnumerable<ChecklistLabel> GetLabels()
        {
            return Labels;
        }
        public IEnumerable<ChecklistLabelGroup> GetLabelGroups()
        {
            return LabelGroups;
        }
        public IEnumerable<ChecklistItemData> GetItemData()
        {
            return ItemData;
        }

        public void RemoveItemData(string id,  List<string> labelIds)
        {
            IEnumerable<ChecklistItemData> temp = ItemData.Where(p => !(p.id.Trim().ToLower() == id.Trim().ToLower() && labelIds.Contains(p.labelId.ToString())));
            ItemData = temp;
        }

        public DocumentContainer(DocumentContainer dc)
        {

            Edited = dc.Edited;
            topicId = dc.topicId;
            selectId = dc.selectId;
            segmentId = dc.segmentId;
            id = dc.id;
            resourceTypeId = dc.resourceTypeId;
            access = dc.access;
            companylookup = dc.companylookup;
            name = dc.name;
            root = dc.root;
            elements = dc.elements;
            tocroot = dc.tocroot;
            toc = dc.toc;
            elementdata = dc.elementdata;
            objects = dc.objects;
            variables = dc.variables;
            xobjects = dc.xobjects;
            viewroot = dc.viewroot;
            showroot = dc.showroot;
            ItemData = dc.ItemData;
            LabelGroups = dc.LabelGroups;
            Labels = dc.Labels;
            AccountLines = dc.AccountLines;
            TaxLines = dc.TaxLines;
        }
        public DocumentContainer(ResourceHTML5Element r)
        {
            ItemData = r.itemData;
            Labels = r.Labels;
            LabelGroups = r.LabelGroups;
            AccountLines = r.AccountLines;
            TaxLines = r.TaxLines;

            elementdata.AddRange(GetElementdata());

            objects.AddRange(AccountLines.GetAccountLineObjects());
            objects.AddRange(TaxLines.GetTaxLineObjects());

            CheckLabelJsons cll = new CheckLabelJsons(LabelGroups, Labels, ItemData);
            viewroot = cll.viewroot;
            showroot = cll.showroot;
            objects.AddRange(cll.objects);
        }

        public void UpdateElementData(ResourceHTML5Element elementData)
        {
            ItemData = from i in ItemData
                       join n in elementData.itemData
                       on i.id equals n.id into g
                       from s in g.DefaultIfEmpty()
                       where s == null
                       select i;

            ItemData = ItemData.Concat(elementData.itemData);


            AccountLines = from a in AccountLines
                           join n in elementData.AccountLines
                           on a.lineId equals n.lineId into g
                           from s in g.DefaultIfEmpty()
                           where s == null
                           select a;

            AccountLines = AccountLines.Concat(elementData.AccountLines);

            TaxLines = from a in TaxLines
                           join n in elementData.TaxLines
                           on a.lineId equals n.lineId into g
                           from s in g.DefaultIfEmpty()
                           where s == null
                           select a;
            TaxLines = TaxLines.Concat(elementData.TaxLines);

            elementdata = GetElementdata();

            objects = new Dictionary<string, ObjectApi>();

            objects.AddRange(AccountLines.GetAccountLineObjects());
            objects.AddRange(TaxLines.GetTaxLineObjects());

            CheckLabelJsons cll = new CheckLabelJsons(LabelGroups, Labels, ItemData);
            viewroot = cll.viewroot;
            showroot = cll.showroot;
            objects.AddRange(cll.objects);
        }
        public void RemoveItemData(List<string> el, string id)
        {
            ItemData = ItemData.Where(p => !(p.id.ToLower()==id.ToLower() && el.Contains(p.labelId.ToString().ToLower()))).Select(p => p);
                //from i in ItemData
                //join e in el
                //on new
                //{
                //    Id = i.id.ToLower(),
                //    Lid = i.labelId.ToString().ToLower()
                //}
                //equals new
                //{
                //    Id = id.ToLower(),
                //    Lid = e.ToLower()
                //}
                //       into g
                //from s in g
                //where s == null
                //select i;
            elementdata = GetElementdata();
        }
        public void RemoveAccountLines(List<string> el)
        {
            AccountLines = AccountLines.Where(p => !(el.Contains(p.lineId.ToString()))).Select(p => p);
            //from a in AccountLines
            //           join e in elements
            //           on a.lineId.ToString().ToLower() equals e.ToLower() into g
            //           from s in g
            //           where s == null
            //           select a;
            elementdata = GetElementdata();
        }
        public void RemoveTaxLines(List<string> el)
        {
            TaxLines = TaxLines.Where(p => !(el.Contains(p.lineId.ToString()))).Select(p => p);
            //from a in TaxLines
            //        join e in elements
            //        on a.lineId.ToString().ToLower() equals e.ToLower() into g
            //        from s in g
            //        where s == null
            //        select a;
            elementdata = GetElementdata();
        }

        public Dictionary<string, List<string>> ConcatDictionary(IEnumerable<Dictionary<string, List<string>>> l)
        {
            Dictionary<string, List<string>> result = new Dictionary<string, List<string>>();
            foreach (Dictionary<string, List<string>> d  in l )
            {
                result.AddRange(d);
            }
            return result;
        }
        public Dictionary<string, Dictionary<string,List<string>>> GetElementdata()
        {
            return
                AccountLines.EDAccountLines()
                .Union(
                    TaxLines.EDTaxLines()
                )
                .Union(
                    ItemData.EDChecklistShow(Labels, LabelGroups, "1")
                )
                .Union(
                    ItemData.EDChecklistShow(Labels, LabelGroups, "2")
                )
                .GroupBy(p => p.Key.ToLower())
                .ToDictionary(
                    p => p.Key.ToLower(),
                    p => ConcatDictionary(p.Select(s => s).Select(s => s.Value))
                );
        }
        public DocumentContainer(ResourceHTML5 r)
        {
            name = r.Name;
            topicId = r.topicId.ToLower();
            id = r.id.ToLower();
            selectId = r.selectId??"";
            segmentId = r.segmentId??"";
            resourceTypeId = r.ResourceTypeId.ToString();
            access = r.Accsess == 1 ? true : false;
            XElement map = r.ResourceMap;
            

            if (r.Document != null)
            {
                r.Document.Descendants().Where(p => (string)p.Attributes("id").FirstOrDefault() == null && p.Nodes().Count() == 0).ToList().ForEach(p => p.Remove());
                r.Document.Descendants().Where(p => (string)p.Attributes("id").FirstOrDefault() == null && p.Nodes().Count() != 0).ToList().ForEach(p => p.ReplaceWith(p.Nodes()));

                string lookup = r.Document.Descendants("x-settings").Elements("companylookup").Select(p => p.Value).FirstOrDefault();
                companylookup = false;
                
                if (lookup == "off")
                    companylookup = false;
                else
                    companylookup = true;

                if (r.Document.Attributes("id").FirstOrDefault() == null)
                {
                    r.Document.Add(new XAttribute("id", r.resourceId));
                }
                else
                {
                    r.Document.Attributes("id").FirstOrDefault().SetValue(r.resourceId);
                }


                XElement docparthtml = r.Document.ConvertXMLtoHTML5(r.Links);
                
                TocJson tocJson = new TocJson(map, docparthtml, r.id, r.segmentId);
                tocroot = tocJson.tocroot;
                toc = tocJson.toc;

                string document_id = "document;" + r.id + ";" + r.segmentId;
                root = new List<JsonChild>
                {
                    new JsonChild { id = document_id }
                };
                elements = new Dictionary<string, JsonElement>
                {
                    {
                        document_id,
                        new JsonElement
                        {
                            
                            name = "div",
                            attributes = new Dictionary<string, string>() { 
                                { "class", "doccontainer" },
                                { "id", document_id },
                            },
                            children = docparthtml.Elements().Select(p => new JsonChild { id = (string)p.Attributes("id").FirstOrDefault() }).ToList()
                        }
                    }
                };

                elements.AddRange(
                    docparthtml
                    .Descendants()
                    .Select(p => p)
                    .ToDictionary(p => (string)p.Attributes("id").FirstOrDefault(), p => new JsonElement(p))
                );

                ItemData = r.itemData;
                Labels = r.Labels;
                LabelGroups = r.LabelGroups;
                AccountLines = r.AccountLines;
                TaxLines = r.TaxLines;

                elementdata.AddRange(GetElementdata());
                objects.AddRange(AccountLines.GetAccountLineObjects());
                objects.AddRange(TaxLines.GetTaxLineObjects());

                CheckLabelJsons cll = new CheckLabelJsons(LabelGroups, Labels, ItemData);
                viewroot = cll.viewroot;
                showroot = cll.showroot;
                objects.AddRange(cll.objects);
               


                eCount = elements.Count();

                if (r.Tags.Count()>0)
                {
                    (
                        from e in elements
                        join t in r.Tags
                        on e.Key.ToLower() equals t.ToLower()
                        select e
                    ).ToList()
                    .ForEach(p => p.SetOtherProps(elements, "tags"));
                }
                if (r.Related.Count()>0)
                {
                    (
                        from e in elements
                        join t in r.Related
                        on e.Key.ToLower() equals t.ToLower()
                        select e
                    ).ToList()
                    .ForEach(p => p.SetOtherProps(elements, "related"));
                }
            }
        }
    }
    public class SearchWords
    {
        public string word { get; set; }
        public List<string> extensions { get; set; }
        public SearchWords(XElement w)
        {
            word = w.Nodes().OfType<XText>().Select(s => s.Value).StringConcatenate();
            extensions = w.Elements("ws").Select(p => p.Value).ToList();

        }
    }
    public class DocumentSearch
    {
        public Dictionary<string, DocumentIntemScore> searchresult { get; set; }
        //public List<string> mark { get; set; }
        public List<SearchWords> mark { get; set; }
        public DocumentSearch(XElement result)
        {
            mark = result.Elements("sw")
                .Elements("w")
                .Select(p => new SearchWords(p))
                .ToList();

            searchresult = result
                .Elements("item")
                 .Select(p => new KeyValuePair<string, DocumentIntemScore>(
                        (string)p.Attributes("id").FirstOrDefault(),
                        new DocumentIntemScore(p)
                    ))
                    .ToDictionary(p => p.Key, p => p.Value);
        }
    }
    public class DocumentIntemScore
    {
        public string id { get; set; }
        public int hitcount { get; set; }
        public int score { get; set; }
        public int max { get; set; }
        //public Dictionary<string, DocumentIntemScore> hits { get; set; }
        public List<DocumentIntemScore> hits { get; set; }
        public DocumentIntemScore(XElement item)
        {
            id = (string)item.Attributes("id").FirstOrDefault();
            hitcount = Convert.ToInt32((string)item.Attributes("n").FirstOrDefault() ?? "0"); //  item.NodesBeforeSelf().OfType<XElement>().Count();
            max = Convert.ToInt32((string)item.Attributes("max").FirstOrDefault() ?? "0");
            score = Convert.ToInt32((string)item.Attributes("score").FirstOrDefault() ?? "0");
            if (item.Elements("item").Count() > 0)
            {
                hits = item
                    .Elements("item")
                     .Select(p => new DocumentIntemScore(p))
                     .ToList();
            }
        }
    }
    public class DocumentTOC
    {
        public string title { get; set; }
        public string id { get; set; }
        public bool segment { get; set; }
        public string segmentId { get; set; }
        public int hitcount { get; set; }
        public int score { get; set; }
        public int max { get; set; }
        public IEnumerable<DocumentTOC> children { get; set; }
        //public Dictionary<string, DocumentIntemScore> hits { get; set; }
        public List<DocumentIntemScore> hits { get; set; }
        public DocumentTOC(XElement item, string segment_id)
        {
            DocumentIntemScore ds = item.Annotation<DocumentIntemScore>();
            title = (string)item.Attributes("title").FirstOrDefault();
            if (title == null) title = "Ingen tittel";
            //if (item.Attributes("n").FirstOrDefault() != null)
            //    hitcount = Convert.ToInt32((string)item.Attributes("n").FirstOrDefault()); //  item.NodesBeforeSelf().OfType<XElement>().Count();
            //if (item.Attributes("max").FirstOrDefault() != null)
            //    max = Convert.ToInt32((string)item.Attributes("max").FirstOrDefault());
            //if (item.Attributes("score").FirstOrDefault() != null)
            //    score = Convert.ToInt32((string)item.Attributes("score").FirstOrDefault());
            if (ds != null)
            {
                hitcount = ds.hitcount; //  item.NodesBeforeSelf().OfType<XElement>().Count();
                max = ds.max;
                score = ds.score;
                hits = ds.hits;
            }

            id = (string)item.Attributes("id").FirstOrDefault() ?? "-1";
            segmentId = segment_id;

            if ((string)item.Attributes("s").FirstOrDefault() == "1" || segment_id == id)
            {
                segment = true;
            }
            if (item.Elements("item").Count() != 0)
            {
                children = item.Elements("item").Select(p => new DocumentTOC(p, segment_id));
            }
        }
        public DocumentTOC(XElement item)
        {
            DocumentIntemScore ds = item.Annotation<DocumentIntemScore>();
            title = (string)item.Attributes("title").FirstOrDefault();
            if (title == null) title = "Ingen tittel";
            if (ds != null)
            {
                hitcount = ds.hitcount; //  item.NodesBeforeSelf().OfType<XElement>().Count();
                max = ds.max;
                score = ds.score;
                hits = ds.hits;
            }
            //if (item.Attributes("n").FirstOrDefault() != null) 
            //    hitcount = Convert.ToInt32((string)item.Attributes("n").FirstOrDefault()); //  item.NodesBeforeSelf().OfType<XElement>().Count();
            //if (item.Attributes("max").FirstOrDefault() != null)
            //    max = Convert.ToInt32((string)item.Attributes("max").FirstOrDefault());
            //if (item.Attributes("score").FirstOrDefault() != null)
            //    score = Convert.ToInt32((string)item.Attributes("score").FirstOrDefault());

            if ((string)item.Attributes("s").FirstOrDefault() == "1")
            {
                segment = true;
            }
            id = (string)item.Attributes("id").FirstOrDefault() ?? "-1";
            if (item.Elements("item").Count() != 0)
            {
                children = item.Elements("item").Select(p => new DocumentTOC(p));
            }
        }
    }
    public class ParentsItem
    {
        public string name { get; set; }
        public string id { get; set; }
        public string segmentId { get; set; }
    }

}
