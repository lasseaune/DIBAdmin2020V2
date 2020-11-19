using DIBAdminAPI.Helpers.Extentions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using DIBAdminAPI.Models;
using System.Security.Cryptography;

namespace DIBAdminAPI.Data.Entities
{
    public static class ElementData
    {
        public static Dictionary<string, Dictionary<string, List<string>>> EDRelated(this IEnumerable<TopicSubElement> related)
        {
            if (related == null) return new Dictionary<string, Dictionary<string, List<string>>>();

            Dictionary<string, Dictionary<string, List<string>>> y =
                related
                .Where(p=>(p.itemId==null?"" : p.itemId.Trim())!="")
                .GroupBy(p => p.itemId)
                .ToDictionary(
                    p => p.Key.ToLower(),
                    p => new Dictionary<string, List<string>>()
                    {
                        {"related", p.Select(p=>p.id.ToString()).ToList() }
                    }

            );

            return y;
            
        }
        public static Dictionary<string, Dictionary<string, List<string>>> EDChecklistShow(this IEnumerable<ChecklistItemData> itemData, IEnumerable<ChecklistLabel> labels, IEnumerable<ChecklistLabelGroup> groups, string type)
        {
            if (itemData.Select(p=>p.id).FirstOrDefault()==null)
            {
                return new Dictionary<string, Dictionary<string, List<string>>>();
            }
            if (!"1;2".Split(';').Contains(type))
            {
                return new Dictionary<string, Dictionary<string, List<string>>>();
            }
            string showgroup = "viewgroup";
            string showtag = "viewtag";
            if (type=="2")
            {
                showgroup = "showgroup";
                showtag = "showtag"; ;
            }
            
            var x =
            from i in itemData
            join l in labels
            on i.labelId.ToString().ToLower() equals l.labelId.ToString().ToLower()
            join g in groups
            on l.labelGroupId.ToString().ToLower() equals g.labelGroupId.ToString().ToLower()
            where g.type == type
            group new { lable=l.labelId.ToString().ToLower(), gr=g.labelGroupId.ToString().ToLower() } by i.id.ToLower() into q
            select q;

            var y =
                x.ToDictionary(
                p => p.Key.ToLower(),
                p=> new Dictionary<string, List<string>>()
                {
                    {showgroup, p.GroupBy(s=>s.gr.ToString().ToLower()).Select(p=>p.Key).ToList() },
                    {showtag, p.GroupBy(s=>s.lable.ToString().ToLower()).Select(p=>p.Key).ToList() }
                }

            );
            
            return y;
        }
        public static Dictionary<string, Dictionary<string, List<string>>> EDAccountLines(this IEnumerable<AccountLine> AccountLines)
        {
            if (AccountLines.Select(p=>p.id).FirstOrDefault()==null)
            {
                return new Dictionary<string, Dictionary<string, List<string>>>();
            }

            return AccountLines
                .GroupBy(p => p.id.ToLower())
                .ToDictionary(
                    p => p.Key.ToLower(),
                    p => new Dictionary<string, List<string>>()
                    {
                        { "accounting", p.Select(a=>a)
                                    //.Where(a => a.id == p.Key)
                                    .OrderBy(a => a.guiorder)
                                    .ThenBy(a => a.idx)
                                    .Select(a => a.lineId.ToString().ToLower())
                                    .ToList()
                        }
                    }
                );
        }
        public static Dictionary<string, Dictionary<string, List<string>>> EDTaxLines(this IEnumerable<TaxLine> TaxLines)
        {
            if (TaxLines.Select(p => p.id).FirstOrDefault() == null)
            {
                return new Dictionary<string, Dictionary<string, List<string>>>();
            }
            return TaxLines
                .GroupBy(p => p.id.ToLower())
                .ToDictionary(
                    p => p.Key.ToLower(),
                    p => new Dictionary<string, List<string>>()
                    {
                        { "tax", p.Select(a=>a)
                                    .OrderBy(a => a.guiorder)
                                    .ThenBy(a => a.idx)
                                    .Select(a => a.lineId.ToString().ToLower())
                                    .ToList()
                        }
                    }
                );
        }

        public static Dictionary<string, Dictionary<string, List<string>>> GetElementData(IEnumerable<AccountLine> AccountLines, IEnumerable<TaxLine> TaxLines)
        {
            return AccountLines
                .Select(p => p.id)
                .Union(
                    TaxLines
                    .Select(p => p.id)
                )
                .GroupBy(p => p)
                .ToDictionary(
                    p => p.Key,
                    p => new Dictionary<string, List<string>>()
                    {
                        { "accounting", AccountLines
                                    .Where(a => a.id == p.Key)
                                    .OrderBy(a => a.guiorder)
                                    .ThenBy(a => a.idx)
                                    .Select(a => a.lineId.ToString())
                                    .ToList() },
                        { "tax",TaxLines
                                    .Where(a => a.id == p.Key)
                                    .OrderBy(a => a.idx)
                                    .Select(a => a.lineId.ToString())
                                    .ToList() }

                    }
                ); ;

        }
        public static Dictionary<string, AccountingElementApi> GetElementDataX(IEnumerable<AccountLine> AccountLines, IEnumerable<TaxLine> TaxLines)
        {
            return AccountLines
                .Select(p => p.id)
                .Union(
                    TaxLines
                    .Select(p => p.id)
                )
                .GroupBy(p => p)
                .ToDictionary(
                    p => p.Key,
                    p => new AccountingElementApi
                    {
                        accounting = AccountLines
                                    .Where(a => a.id == p.Key)
                                    .OrderBy(a => a.guiorder)
                                    .ThenBy(a => a.idx)
                                    .Select(a => a.lineId.ToString())
                                    .ToList(),
                        tax = TaxLines
                                    .Where(a => a.id == p.Key)
                                    .OrderBy(a => a.idx)
                                    .Select(a => a.lineId.ToString())
                                    .ToList()

                    }
                ); ;
        }
        public static Dictionary<string, ObjectApi> GetAccountLineObjects(this IEnumerable<AccountLine> AccountLines)
        {
            return AccountLines
                .ToDictionary(
                p => p.lineId.ToString(),
                p => new ObjectApi
                {
                    type = "accline",
                    id = p.lineId.ToString(),
                    transactionId = p.transactionId,
                    //data = new Dictionary<string, string>
                    data = new AccountLineAPI
                    {
                        id = p.lineId.ToString(),
                        codeId = p.codeId,
                        name = p.name,
                        debetcredit = p.debetcredit,
                        accId = p.accId,
                        typeId = p.typeId,
                        dataResourceId = p.dataResourceId.ToString(),
                        dataId = p.dataId,
                        
                    }
                }
                );
        }
        public static Dictionary<string, ObjectApi> GetTaxLineObjects(this IEnumerable<TaxLine> TaxLines)
        {
            return TaxLines
                .ToDictionary(
                p => p.lineId.ToString(),
                p => new ObjectApi
                {
                    type = "taxline",
                    id = p.lineId.ToString(),
                    transactionId = p.transactionId,
                    data = new TaxLineAPI
                    {
                        id = p.id,
                        taxId = p.taxId,
                        name =p.name,
                        accId = p.accId,
                        dataResourceId = p.dataResorceId.ToString(),
                        dataId = p.dataId,
                    }
                }
            );
        }
    }
    public class AccountingType
    {
        public string id { get; set; }
        public string name { get; set; }
        public string typeId { get; set; }
    }
    public class AccountingCode
    {
        public string id { get; set; }
        public string name { get; set; }
        public string typeId { get; set; }
        
    }
    public class AccountingTax
    {
        public string id { get; set; }
        public string name { get; set; }
        public string accId { get; set; }
    }
    public class TaxLineAPI
    {
        public string id { get; set; }
        public string taxId { get; set; }
        public string name { get; set; }
        public string accId { get; set; }
        public string dataResourceId { get; set; }
        public string dataId { get; set; }
    }
    public class TaxLine
    {
        public string id { get; set; }
        public Guid lineId { get; set; }
        public string taxId { get; set; }
        public string name { get; set; }
        public string accId { get; set; }
        public int guiorder { get; set; }
        public int idx { get; set; }
        public Guid dataResorceId { get; set; }
        public string dataId { get; set; }
        public string transactionId { get; set; }
    }
    public class AccountLineAPI
    {
        public string id { get; set; }
        public string codeId { get; set; }
        public string name { get; set; }
        public bool debetcredit { get; set; }
        public string accId { get; set; }
        public string typeId { get; set; }
        public string dataResourceId { get; set; }
        public string dataId { get; set; }
        
    }
        
    public class AccountLine
    {
        public string id { get; set; }
        public Guid lineId { get; set; }
        public string codeId { get; set; }
        public string name { get; set; }
        public bool debetcredit { get; set; }
        public string accId { get; set; }
        public string typeId { get; set; }
        public int guiorder { get; set; }
        public int idx { get; set; }
        public Guid dataResourceId { get; set; }
        public string dataId { get; set; }
        public string transactionId { get; set; }
    }

    public class AccountingElementApi
    {
        public List<string> accounting { get; set; }
        public List<string> tax { get; set; }
    }


    public static class OtherPropsOperations
    {
        public static List<KeyValuePair<string, JsonElement>> UpdateOtherProps(this KeyValuePair<string, JsonElement> ae, KeyValuePair<string, AccountingElementApi> d, List<string> deleted, string propsName)
        {
            List<KeyValuePair<string, JsonElement>> result = new List<KeyValuePair<string, JsonElement>>();

            if ((deleted == null ? 0 : deleted.Count()) > 0)
            {
                foreach (string s in deleted)
                {
                    if (d.Value.accounting != null)
                    {
                        if (d.Value.accounting.Contains(s))
                            d.Value.accounting.Remove(s);
                    }
                    if (d.Value.tax != null)
                    {
                        if (d.Value.tax.Contains(s))
                            d.Value.tax.Remove(s);
                    }
                }
            }
            if (((d.Value.accounting ==null ? 0 : d.Value.accounting.Count()) + (d.Value.tax == null ? 0 :d.Value.tax.Count))>0)
            {
                if (ae.Value.otherprops == null)
                    ae.Value.otherprops = new Dictionary<string, dynamic>();
                if (!ae.Value.otherprops.ContainsKey(propsName))
                {
                    ae.Value.otherprops.Add(propsName,true);
                    result.Add(ae);
                }
            }
            else
            {
                if (ae.Value.otherprops!= null)
                {
                    if (ae.Value.otherprops.ContainsKey(propsName))
                    {
                        ae.Value.otherprops.Remove(propsName);
                        result.Add(ae);
                    }
                }
            }
            
            return result;
        }
        public static List<KeyValuePair<string, JsonElement>>  UpdateOtherProps(this KeyValuePair<string, JsonElement> ae,  string propsName)
        {
            List<KeyValuePair<string, JsonElement>> result = new List<KeyValuePair<string, JsonElement>>();

            if (ae.Value.otherprops == null)
                ae.Value.otherprops = new Dictionary<string, dynamic>();
            if (!ae.Value.otherprops.ContainsKey(propsName))
            {
                ae.Value.otherprops.Add(propsName, true);
                result.Add(ae);
            }
            return result;
        }
        public static void SetOtherProps(this KeyValuePair<string, JsonElement> ae, Dictionary<string, JsonElement> elements, string propsName)
        {
            
            if (ae.Value.otherprops == null)
                ae.Value.otherprops = new Dictionary<string, dynamic>();
            ae.Value.otherprops.Add(propsName, true);
        }
    }
}
