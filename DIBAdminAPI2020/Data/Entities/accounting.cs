using DIBAdminAPI.Helpers.Extentions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Text.RegularExpressions;

namespace DIBAdminAPI.Data.Entities
{
    public static class OtherPropsOperations
    {
        public static void SetOtherProps(this KeyValuePair<string, JsonElement> ae, Dictionary<string, JsonElement> elements, string propsName, string refName)
        {
            if (ae.Value.name == "section")
            {
                KeyValuePair<string, JsonElement> target = (
                    from c in ae.Value.children
                    join e in elements
                    on c.id equals e.Key
                    where Regex.IsMatch(e.Value.name.Trim().ToLower(), @"^h\d")
                    select e
                ).FirstOrDefault();
                if (target.Key != null)
                {
                    if (target.Value.otherprops == null)
                        target.Value.otherprops = new Dictionary<string, string>();
                    target.Value.otherprops.Add(refName, ae.Key) ;
                    if (ae.Value.otherprops == null)
                        ae.Value.otherprops = new Dictionary<string, string>();
                    ae.Value.otherprops.Add(propsName, "true");
                }
                else
                {
                    if (ae.Value.otherprops == null)
                        ae.Value.otherprops = new Dictionary<string, string>();
                    ae.Value.otherprops.Add(propsName, "true");
                    ae.Value.otherprops.Add(refName, ae.Key);

                }
            }
            else
            {
                if (ae.Value.otherprops == null)
                    ae.Value.otherprops = new Dictionary<string, string>();
                ae.Value.otherprops.Add(propsName, "true");
                ae.Value.otherprops.Add(refName, ae.Key);
            }
        }
    }
    public static class AccointingOperations
    {
        public static void SetAccounting(this KeyValuePair<string, JsonElement> ae, Dictionary<string, JsonElement> elements)
        {
            if (ae.Value.name == "section")
            {
                KeyValuePair<string, JsonElement> target = (
                    from c in ae.Value.children
                    join e in elements
                    on c.id equals e.Key
                    where Regex.IsMatch(e.Value.name.Trim().ToLower(), @"^h\d")
                    select e
                ).FirstOrDefault();
                if (target.Key != null)
                {
                    target.Value.otherprops = new Dictionary<string, string>() { { "accref", ae.Key } };
                    ae.Value.otherprops = new Dictionary<string, string>() { { "accounting", "true" } };
                }
                else
                {
                    ae.Value.otherprops = new Dictionary<string, string>() { { "accounting", "true" }, { "accref", ae.Key } };
                }
            }
            else
                ae.Value.otherprops = new Dictionary<string, string>() { { "accounting", "true" }, { "accref", ae.Key } };


        }
    }
        
    public class Taxline
    {
        public int taxid { get; set; }
        public bool value { get; set; }

    }
    public class AcclineJson
    {
        public int accid { get; set; }
        public string code { get; set; }
        public int DebetCredit { get; set; }
        public List<Taxline> tax { get; set; }
    }
    public class AccountingJson
    {
        public string resourceid { get; set; }
        public string segmentid { get; set; }
        public string id { get; set; }
        public string action { get; set; }
        public string item { get; set; }
        public List<AcclineJson> line { get; set; }

        

    }
    public class AccountingType
    {
        public int AccId { get; set; }
        public string Name { get; set; }
        public int Type { get; set; }
        public Guid ResourceId { get; set; }
        public int GuiOrder { get; set; }
    }
    public class AccountingCode
    {
        public int Type { get; set; }
        public string Code { get; set; }
        public string SegmentId { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public int GuiOrder { get; set; }
    }
    public class AccountingTax
    {
        public int TaxId { get; set; }
        public string Name { get; set; }
        public Guid ResourceId { get; set; }
        public string Id { get; set; }
        public int AccId { get; set; }
    }
    //public class MvaObjects: Object
    //{
    //    public int AccId { get; set; }
    //    public string Name { get; set; }
    //    public int Type { get; set; }
    //    public List<MvaObject> lines { get; set; }
    //}
    //public class MvaObject
    //{
    //    public string Code { get; set; }
    //    public string Name { get; set; }
    //    public Guid ResourceId { get; set; }
    //    public string SegmentId { get; set; }
    //    public string Id { get; set; }
    //    public MvaObject(XElement l , AccountingCode code, Guid resoruceId)
    //    {
    //        Code = code.Code;
    //        Name = code.Name;
    //        ResourceId = resoruceId;
    //        SegmentId = code.SegmentId;
    //        Id = code.Id;

    //    }
        
    //}
    //public class TaxObject 
    //{
    //    public int TaxId { get; set; }
    //    public string Name { get; set; }
    //    public bool Value { get; set; }
    //}
    //public class LonnObjects
    //{
    //    public int AccId { get; set; }
    //    public string Name { get; set; }
    //    public int Type { get; set; }
    //    public List<KontoObject> lines { get; set; }
    //    public List<TaxObject> tax { get; set; }
    //}

    //public class KontoObjects
    //{
    //    public int AccId { get; set; }
    //    public string Name { get; set; }
    //    public int Type { get; set; }
    //    public List<KontoObject> lines { get; set; }
    //}
    //public class KontoObject
    //{
    //    public string Code { get; set; }
    //    public string Name { get; set; }
    //    public Guid ResourceId { get; set; }
    //    public string SegmentId { get; set; }
    //    public string Id { get; set; }
    //    public string Description { get; set; }
    //    public int DebetCredit { get; set; }

    //    public KontoObject(XElement l, AccountingCode code, Guid resoruceId)
    //    {
    //        Code = code.Code;
    //        Name = code.Code;
    //        Description = code.Name;
    //        DebetCredit = Convert.ToInt32(((string)l.Attributes("debet_credit").FirstOrDefault() ?? "0"));
    //        ResourceId = resoruceId;
    //        SegmentId = code.SegmentId;
    //        Id = code.Id;
    //    }
    //}
    //public class AccLineGroup
    //{
    //    public XElement Line { get; set; }
        

    //}
    //public class AccountingObject
    //{
    //    public MvaObjects mva { get; set; }
    //    public KontoObjects konto { get; set; }
    //    public LonnObjects lonn { get; set; }
    //    public  AccountingObject(IGrouping<string, XElement> lines,
    //        IEnumerable<AccountingType> accountingType,
    //        IEnumerable<AccountingCode> accountingCodes,
    //        IEnumerable<AccountingTax> accountingTax
    //    )
    //    {

    //        (
    //            from a in accountingType
    //            join l in lines
    //            on a.AccId.ToString() equals (string)l.Attributes("accounting_id").FirstOrDefault()
    //            where a.AccId == 1
    //            orderby a.GuiOrder
    //            group new AccLineGroup { Line = l,  } by new {id = a.AccId,name=a.Name, type= a.Type, ResoruceId = a.ResourceId } into g
    //            select g
    //        ).ToList()
    //        .ForEach(p=>mva = new MvaObjects{
    //            AccId = p.Key.id,
    //            Type = p.Key.type,
    //            Name= p.Key.name,
    //            lines = (
    //                        from l in p.Where(l => l.Line.Name.LocalName == "accline").Select(l=>l.Line)
    //                        join c in accountingCodes
    //                        on new { id = (string)l.Attributes("code").FirstOrDefault(), type = (string)l.Attributes("type").FirstOrDefault() } equals new { id = c.Code , type=c.Type.ToString()}
    //                        select new MvaObject (l,c,p.Key.ResoruceId)
    //                    ).ToList()
    //        });

    //        (
    //            from a in accountingType
    //            join l in lines
    //            on a.AccId.ToString() equals (string)l.Attributes("accounting_id").FirstOrDefault()
    //            where a.AccId == 3
    //            orderby a.GuiOrder
    //            group new AccLineGroup { Line = l } by new { id = a.AccId, name = a.Name, type = a.Type, ResoruceId = a.ResourceId } into g
    //            select g
    //        ).ToList()
    //        .ForEach(p => konto = new KontoObjects
    //        {
    //            AccId = p.Key.id,
    //            Type = p.Key.type,
    //            Name = p.Key.name,
    //            lines = (
    //                        from l in p.Where(l => l.Line.Name.LocalName == "accline").Select(l => l.Line)
    //                        join c in accountingCodes
    //                        on new { id = (string)l.Attributes("code").FirstOrDefault(), type = (string)l.Attributes("type").FirstOrDefault() } equals new { id = c.Code, type = c.Type.ToString() }
    //                        select new KontoObject(l, c, p.Key.ResoruceId)
    //                    ).ToList()
    //        });

    //        (
    //            from a in accountingType
    //            join l in lines
    //            on a.AccId.ToString() equals (string)l.Attributes("accounting_id").FirstOrDefault()
    //            where a.AccId == 2
    //            orderby a.GuiOrder
    //            group new AccLineGroup { Line = l } by new { id = a.AccId, name = a.Name, type = a.Type, ResoruceId = a.ResourceId } into g
    //            select g
    //        ).ToList()
    //        .ForEach(p => lonn = new LonnObjects
    //        {
    //            AccId = p.Key.id,
    //            Type = p.Key.type,
    //            Name = p.Key.name,
    //            lines = (
    //                        from l in p.Where(l => l.Line.Name.LocalName == "accline").Select(l => l.Line)
    //                        join c in accountingCodes
    //                        on new { id = (string)l.Attributes("code").FirstOrDefault(), type = (string)l.Attributes("type").FirstOrDefault() } equals new { id = c.Code, type = c.Type.ToString() }
    //                        select new KontoObject(l, c, p.Key.ResoruceId)
    //                    ).ToList(),
    //            tax = p.Where(l => l.Line.Name.LocalName == "taxline").Count()==0
    //                  ? null
    //                  :  (from t in accountingTax
    //                   join l in p.Where(l => l.Line.Name.LocalName == "taxline").Select(l => l.Line).DefaultIfEmpty()
    //                   on t.Id.ToString() equals (string)l.Attributes("tax_id").FirstOrDefault() into g
    //                   from e in g.DefaultIfEmpty()
    //                   select new TaxObject {
    //                           TaxId = t.TaxId,
    //                           Name = t.Name,
    //                           Value = e==null ? false : true
    //                       }
    //                   )
    //                   .ToList()
    //        });
    //    }
    //}
    public class TaxLine
    {
        public Guid lineid { get; set; }
        public Guid resorceid { get; set; }
        public string id { get; set; }
        public int accid { get; set; }
        public int taxid { get; set; }
    }

    
    public class AccountLine
    {
        public Guid lineid { get; set; }
        public string id { get; set; }
        public int accid { get; set; }
        public string name { get; set; }
        public string code { get; set; }
        public int? debetcredit = null;
    }

    public class AccountingSection
    {
        public int accid { get; set; }
        public string name { get; set; }
        public Guid resourceid { get; set; }
        public int type { get; set; }
        public List<string> accountline { get; set; }
        public List<string> taxline { get; set; }

        public AccountingSection(
            string parentId,
            AccountingType a,
            IEnumerable<XElement> lines,
            IEnumerable<AccountingCode> accountingCodes,
            IEnumerable<AccountingTax> accountingTax)
        {
            accid = a.AccId;
            name = a.Name;
            resourceid = a.ResourceId;
            type = a.Type;
            

            accountline = (
                    from l in lines.Where(p => p.Name.LocalName == "accline")
                    join ac in accountingCodes
                    on new
                    {
                        code = (string)l.Attributes("code").FirstOrDefault(),
                        type = (string)l.Attributes("type").FirstOrDefault()
                    } equals new
                    {
                        code = ac.Code,
                        type = ac.Type.ToString()
                    }
                    orderby ac.GuiOrder
                    select (string)l.Attributes("lineid").FirstOrDefault()
                    ).ToList();
            taxline = (
                    from l in lines.Where(p => p.Name.LocalName == "taxline")
                    join t in accountingTax
                    on (string)l.Attributes("tax_id").FirstOrDefault() equals t.TaxId.ToString()
                    orderby t.Id
                    select (string)l.Attributes("lineid").FirstOrDefault()
                    ).ToList();
        }
    }
    
    public class AccountingObjects
    {
       
        //public Dictionary<string, dynamic> accountings { get; set; }
        public Dictionary<string, Dictionary<string, AccountingSection>> accounts { get; set; }
        public Dictionary<string, TaxLine> taxelement { get; set; }
        public Dictionary<string, AccountLine> accountelement { get; set; }
        public List<string> aobjects { get; set; }
        public AccountingObjects(
            XElement AccRoot,
            IEnumerable<AccountingType> accountingType,
            IEnumerable<AccountingCode> accountingCodes,
            IEnumerable<AccountingTax> accountingTax
        )
        {
            if (AccRoot == null ? true: AccRoot.Elements().Count() == 0)
            {
                accounts = null;
                taxelement = null;
                accountelement = null;
                return;
            }

            aobjects = AccRoot
                .Descendants()
                .Where(p => "accline;taxline".Split(';').Contains(p.Name.LocalName))
                .GroupBy(p => (string)p.Attributes("id").FirstOrDefault())
                .Select(p => p.Key)
                .ToList();


            accountelement = (
                    from a in AccRoot.Descendants("accline")
                    join at in accountingType
                    on (string)a.Attributes("accounting_id").FirstOrDefault() equals at.AccId.ToString()
                    join ac in accountingCodes 
                    on new {
                        type = at.Type.ToString(),
                        code = (string)a.Attributes("code").FirstOrDefault()
                    } equals new {
                        type = ac.Type.ToString(),
                        code = ac.Code
                    }
                    select new { a, at,  ac }
                )
               .ToDictionary(
                   p => (string)p.a.Attributes("lineid").FirstOrDefault(),
                   p => new AccountLine
                   {
                       lineid = (Guid)p.a.Attributes("lineid").FirstOrDefault(),
                       id = p.ac.Id,
                       accid = p.at.AccId,
                       name = p.ac.Name,
                       code = p.ac.Code,
                       debetcredit = (p.at.AccId == 1).SetValueToNull(Convert.ToInt32((string)p.a.Attributes("debet_credit").FirstOrDefault()??"0"))
                   }
               );

            taxelement = (
                    from l in AccRoot.Descendants("taxline")
                    join t in accountingTax
                    on (string)l.Attributes("tax_id").FirstOrDefault() equals t.TaxId.ToString()
                    join at in accountingType
                    on (string)l.Attributes("accounting_id").FirstOrDefault() equals at.AccId.ToString()
                    select new {l, t, at}
                )
                .ToDictionary(
                    p => (string)p.l.Attributes("lineid").FirstOrDefault(),
                    p => new TaxLine
                    {
                        lineid = (Guid)p.l.Attributes("lineid").FirstOrDefault(),
                        resorceid = p.t.ResourceId,
                        id = p.t.Id,
                        accid = p.at.AccId,
                        taxid = p.t.TaxId
                    }
                );

            accounts = AccRoot
                .Elements()
                .GroupBy(p => (string)p.Attributes("id").FirstOrDefault())
                .Select(p => p)
                .ToDictionary(p => p.Key.ToString(), p=>
                                        accountingType
                                        .Select(b=>b)
                                        .OrderBy(b=>b.GuiOrder)
                                        .ToDictionary(b => b.GuiOrder.ToString(), b=> new AccountingSection(
                                            p.Key, 
                                            b,
                                            from l in p
                                            where ((string)l.Attributes("accounting_id").FirstOrDefault()??"").ToLower() == b.AccId.ToString().ToLower()
                                            select l
                                            ,
                                            accountingCodes, accountingTax))
                );


         
        }
    }
    public class AccountingElementApi
    {
        public List<string> accounting { get; set; }
        public List<string> tax { get; set; }
    }
    public class AccountingObjectsAPI
    {

        //public Dictionary<string, dynamic> accountings { get; set; }
        public Dictionary<string, ObjectApi> objects { get; set; }
        public Dictionary<string, AccountingElementApi> objectsList { get; set; }
        public List<string> aobjects { get; set; }

        public AccountingObjectsAPI(
            XElement AccRoot,
            IEnumerable<AccountingType> accountingType,
            IEnumerable<AccountingCode> accountingCodes,
            IEnumerable<AccountingTax> accountingTax
        )
        {
            if (AccRoot == null ? true : AccRoot.Elements().Count() == 0)
            {
                return;
            }

            aobjects = AccRoot
                .Descendants()
                .Where(p => "accline;taxline".Split(';').Contains(p.Name.LocalName))
                .GroupBy(p => (string)p.Attributes("id").FirstOrDefault())
                .Select(p => p.Key)
                .ToList();
            objects = new Dictionary<string, ObjectApi>();
            objects.AddRange(
                (
                     from a in AccRoot.Descendants("accline")
                     join at in accountingType
                     on (string)a.Attributes("accounting_id").FirstOrDefault() equals at.AccId.ToString()
                     join ac in accountingCodes
                     on new
                     {
                         type = at.Type.ToString(),
                         code = (string)a.Attributes("code").FirstOrDefault()
                     } equals new
                     {
                         type = ac.Type.ToString(),
                         code = ac.Code
                     }
                     select new { a, at, ac }
                 )
                .ToDictionary(
                    p => (string)p.a.Attributes("lineid").FirstOrDefault(),
                    p => new ObjectApi
                    {
                        type = "accline",
                        id = (string)p.a.Attributes("lineid").FirstOrDefault(),
                        data = new Dictionary<string, string>
                        {
                           //{ "id", (string)p.a.Attributes("lineid").FirstOrDefault() },
                           { "refId", p.ac.Id },
                           { "accId" , p.at.AccId.ToString() },
                           { "name", p.ac.Name },
                           { "code", p.ac.Code },
                           { "debetcredit",(p.at.AccId == 1 ? "" : ((string)p.a.Attributes("debet_credit").FirstOrDefault() ?? "0"))}
                        }
                   }
                )
            );

            objects.AddRange(
                (
                    from l in AccRoot.Descendants("taxline")
                    join t in accountingTax
                    on (string)l.Attributes("tax_id").FirstOrDefault() equals t.TaxId.ToString()
                    join at in accountingType
                    on (string)l.Attributes("accounting_id").FirstOrDefault() equals at.AccId.ToString()
                    select new { l, t, at }
                )
                .ToDictionary(
                    p => (string)p.l.Attributes("lineid").FirstOrDefault(),
                    p => new ObjectApi
                    {
                        type = "taxLine",
                        id = (string)p.l.Attributes("lineid").FirstOrDefault(),
                        data = new Dictionary<string, string>
                        {
                            { "refResourceId" , p.t.ResourceId.ToString() },
                            { "refId", p.t.Id },
                            { "accId" , p.at.AccId.ToString() },
                            { "taxid" , p.t.TaxId.ToString() }
                        }
                    }
                )
            );

            objectsList = AccRoot
                .Elements()
                .GroupBy(p => (string)p.Attributes("id").FirstOrDefault())
                .Select(p => p)
                .ToDictionary(p =>
                    p.Key.ToString(),
                    p => new AccountingElementApi
                    {
                        accounting = (
                                from at in accountingType
                                join l in p.Where(a => a.Name.LocalName == "accline")
                                on at.AccId.ToString() equals (string)l.Attributes("accounting_id").FirstOrDefault()
                                join ac in accountingCodes
                                on new
                                {
                                    code = (string)l.Attributes("code").FirstOrDefault(),
                                    type = (string)l.Attributes("type").FirstOrDefault()
                                } equals new
                                {
                                    code = ac.Code,
                                    type = ac.Type.ToString()
                                }
                                orderby at.GuiOrder, ac.GuiOrder
                                select (string)l.Attributes("lineid").FirstOrDefault()
                            ).ToList(),
                        tax = (
                            from l in p.Where(a => a.Name.LocalName == "taxline")
                            join t in accountingTax
                            on (string)l.Attributes("tax_id").FirstOrDefault() equals t.TaxId.ToString()
                            orderby t.Id
                            select (string)l.Attributes("lineid").FirstOrDefault()
                            ).ToList()
                    }
                );



        }
    }
}
