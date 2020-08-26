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
    public static class ElementData
    {
        public static Dictionary<string, AccountingElementApi> GetElementData(IEnumerable<AccountLine> AccountLines, IEnumerable<TaxLine> TaxLines)
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
                        id = p.lineId,
                        codeId = p.codeId,
                        name = p.name,
                        debetcredit = p.debetcredit,
                        accId = p.accId,
                        typeId = p.typeId,
                        dataResourceId = p.dataResourceId,
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
                        dataResourceId = p.dataResorceId,
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
        public string lineId { get; set; }
        public string taxId { get; set; }
        public string name { get; set; }
        public string accId { get; set; }
        public int guiorder { get; set; }
        public int idx { get; set; }
        public string dataResorceId { get; set; }
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
        public string lineId { get; set; }
        public string codeId { get; set; }
        public string name { get; set; }
        public bool debetcredit { get; set; }
        public string accId { get; set; }
        public string typeId { get; set; }
        public int guiorder { get; set; }
        public int idx { get; set; }
        public string dataResourceId { get; set; }
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
                    target.Value.otherprops.Add(refName, ae.Key);
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
}
