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
            string showgroup = "showgroup";
            string showtag = "showtag";
            if (type=="2")
            {
                showgroup = "viewgroup";
                showtag = "viewtag";
            }
            
            var x =
            from i in itemData
            join l in labels
            on i.labelId equals l.labelId
            join g in groups
            on l.labelGroupId equals g.labelGroupId
            where g.type == type
            group new { l, g } by i.id into q
            select q;

            return x.ToDictionary(
                p => p.Key,
                p=> new Dictionary<string, List<string>>()
                {
                    {showgroup, p.Select(s=>s.g.labelGroupId.ToString()).ToList() },
                    {showtag, p.Select(s=>s.l.labelId.ToString()).ToList() }
                }

            );
        }
        public static Dictionary<string, Dictionary<string, List<string>>> EDAccountLines(this IEnumerable<AccountLine> AccountLines)
        {
            if (AccountLines.Select(p=>p.id).FirstOrDefault()==null)
            {
                return new Dictionary<string, Dictionary<string, List<string>>>();
            }

            return AccountLines
                .Select(p => p.id)
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
                .Select(p => p.id)
                .GroupBy(p => p)
                .ToDictionary(
                    p => p.Key,
                    p => new Dictionary<string, List<string>>()
                    {
                        { "tax", TaxLines
                                    .Where(a => a.id == p.Key)
                                    .OrderBy(a => a.guiorder)
                                    .ThenBy(a => a.idx)
                                    .Select(a => a.lineId.ToString())
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
        public static Dictionary<string, ObjectApi> GetAccountLineObjects(IEnumerable<AccountLine> AccountLines)
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
        public static Dictionary<string, ObjectApi> GetTaxLineObjects(IEnumerable<TaxLine> TaxLines)
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
