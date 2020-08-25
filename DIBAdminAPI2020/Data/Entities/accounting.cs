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
                .Select(p => p.Id)
                .Union(
                    TaxLines
                    .Select(p => p.Id)
                )
                .GroupBy(p => p)
                .ToDictionary(
                    p => p.Key,
                    p => new AccountingElementApi
                    {
                        accounting = AccountLines
                                    .Where(a => a.Id == p.Key)
                                    .OrderBy(a => a.guiorder)
                                    .ThenBy(a => a.idx)
                                    .Select(a => a.lineId.ToString())
                                    .ToList(),
                        tax = TaxLines
                                    .Where(a => a.Id == p.Key)
                                    .OrderBy(a => a.idx)
                                    .Select(a => a.lineid.ToString())
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

                        accId = p.accId,
                        name = p.name,
                        code = p.code,
                        debetcredit = p.debetcredit,
                        refResourceId = p.refResourceId.ToString(),
                        refId = p.refId,
                        type = p.type
                    }
                }
                );
        }
        public static Dictionary<string, ObjectApi> GetTaxLineObjects(IEnumerable<TaxLine> TaxLines)
        {
            return TaxLines
                .ToDictionary(
                p => p.lineid.ToString(),
                p => new ObjectApi
                {
                    type = "taxline",
                    id = p.lineid.ToString(),
                    transactionId = p.transactionId,
                    data = new TaxLineAPI
                    {
                        accId =  p.accId,
                        taxId = p.taxId,
                        name =p.name,
                        refResourceId= p.refResourceId.ToString(),
                        refId = p.Id,
                    }
                }
            );
        }
    }
    public class AccountingType
    {
        public int accId { get; set; }
        public string name { get; set; }
        public int type { get; set; }
        public Guid resourceId { get; set; }
        public int guiOrder { get; set; }
    }
    public class AccountingCode
    {
        public int type { get; set; }
        public string code { get; set; }
        public string segmentId { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public int guiOrder { get; set; }
    }
    public class AccountingTax
    {
        public int taxId { get; set; }
        public string name { get; set; }
        public Guid resourceId { get; set; }
        public string id { get; set; }
        public int accId { get; set; }
    }
    public class TaxLineAPI
    {
        public int accId { get; set; }
        public int taxId { get; set; }
        public string name { get; set; }
        public string refResourceId { get; set; }
        public string refId { get; set; }
    }
    public class TaxLine
    {
        public Guid lineid { get; set; }
        public Guid resorceId { get; set; }
        public string Id { get; set; }
        public int accId { get; set; }
        public int taxId { get; set; }
        public string name { get; set; }
        public int guiorder { get; set; }
        public int idx { get; set; }
        public Guid refResourceId { get; set; }
        public string refId { get; set; }
        public string transactionId { get; set; }
    }
    public class AccountLineAPI
    {
        public int accId { get; set; } 
        public string name { get; set; }
        public string code { get; set; }
        public bool debetcredit { get; set; }
        public string refResourceId { get; set; }
        public string refId { get; set; }
        public int type { get; set; }
    }
        
    public class AccountLine
    {
        public Guid lineId { get; set; }
        public Guid resourceId { get; set; }
        public string Id { get; set; }
        public int accId { get; set; }
        public int type { get; set; }
        public string code { get; set; }
        public string name { get; set; }
        public bool debetcredit { get; set; }
        public int guiorder { get; set; }
        public int idx { get; set; }
        public Guid refResourceId { get; set; }
        public string refId { get; set; }
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
