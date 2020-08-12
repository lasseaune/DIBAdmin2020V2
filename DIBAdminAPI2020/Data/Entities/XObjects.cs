using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DIBAdminAPI.Data.Entities
{
    public class XVarResult
    {
        public string status { get; set; }
        public string id { get; set; }
        public string message { get; set; }
        public string rowvalue { get; set; }
        public DateTime date { get; set; }

    }
    public class XVarOption
    {
        public string label { get; set; }
        public string value { get; set; }

        public XVarOption(XElement option)
        {
            label = (string)option.Attributes("label").FirstOrDefault();
            value = (string)option.Attributes("value").FirstOrDefault();
        }
    }
    public class XVarRow
    {
        public int n { get; set; }
        public XCountable row { get; set; }

        public XVarRow(int no, XCountable c)
        {
            n = no;
            row = c;
        }
    }
    public class XObjRow
    {
        public int n { get; set; }
        public IEnumerable<XVariable> row { get; set; }

        public XObjRow(int nr, List<XCountableVar> xcvar)
        {
            n = nr;
            row = xcvar.Select(p => new XVariable(nr, p));
        }
    }

    public class XCountableVar
    {
        public XCountable id { get; set; }
        public XElement nvar { get; set; }
        public XCountableVar() { }
    }
    public class XCountable
    {
        public string id { get; set; }
        public string varid { get; set; }
        public int itemNo { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public string value { get; set; }

        public XCountable() { }
    }
    public class XObjConstructor
    {
        public int max { get; set; }
        public int counter { get; set; }
        public IEnumerable<XObj> variables { get; set; }
        public IEnumerable<XObjRow> rows { get; set; }
        public IEnumerable<XVariable> rowconstructor { get; set; }
        public XObjConstructor(List<XObjCountable> countable)
        {
            if ((countable == null ? 0 : countable.Count()) == 0) return;
            List<string> sn = countable.SelectMany(p => p.numbers.Split(';').Where(s => s != "")).GroupBy(p => p).Select(p => p.Key).ToList();
            List<int> n = new List<int>();
            if (sn.Count() != 0)
                n = sn.Select(p => Convert.ToInt32(p)).ToList();
            counter = n.Count();
            max = n.Count() == 0 ? 0 : n.Max();
            variables = countable
                .Where(p => (p.variable == null ? "" : (string)p.variable.Attributes("counter").FirstOrDefault()) != "true")
                .Select(p => new XObj(p.xvar));
            Regex rx = new Regex(@"\*N\*");

            rowconstructor = countable
                    .Where(p => (p.variable == null ? "" : (string)p.variable.Attributes("counter").FirstOrDefault()) == "true")
                        .Select(p =>
                            new XVariable
                            {
                                id = p.variable.Elements("id").Select(v => v.Value).FirstOrDefault(),
                                name = rx.Replace(p.variable.Elements("name").Select(s => s.Value).FirstOrDefault(), "").Trim(),
                                type = p.variable.Elements("type").Select(s => s.Value).FirstOrDefault(),
                            });

            if (countable.Where(p => p.variable != null).SelectMany(p => p.variable.Elements("variable")).Count() != 0)
            {
                List<XObjRow> temp = new List<XObjRow>();
                foreach (int i in n.OrderBy(p => p))
                {
                    List<XCountable> id = countable.Where(p => (p.variable == null ? "" : (string)p.variable.Attributes("counter").FirstOrDefault()) == "true")
                        .Select(p =>
                            new {
                                id = p.variable.Elements("id").Select(v => v.Value).FirstOrDefault(),
                                name = p.variable.Elements("name").Select(s => s.Value).FirstOrDefault(),
                                type = p.variable.Elements("type").Select(s => s.Value).FirstOrDefault(),
                            })
                        .Select(p =>
                            new XCountable
                            {
                                id = p.id,
                                varid = rx.Replace(p.id, i.ToString()),
                                itemNo = i,
                                name = rx.Replace(p.name, "").Trim()
                            }).ToList();
                    List<XCountableVar> nv =
                    (
                        from e in id
                        join c in countable.Where(p => (p.variable == null ? "" : (string)p.variable.Attributes("counter").FirstOrDefault()) == "true").Select(p => p.variable.Elements("variable"))
                        on e.varid equals c.Elements("id").Select(p => p.Value).FirstOrDefault() into subvar
                        from subv in subvar.DefaultIfEmpty()
                        select new XCountableVar { id = e, nvar = subv == null ? null : subv.FirstOrDefault() }
                    ).ToList();

                    XObjRow row = new XObjRow(i, nv);
                    temp.Add(row);
                }
                rows = temp;
            }

        }
    }
    public class XObjCountable
    {
        public string numbers { get; set; }
        public XElement xvar { get; set; }
        public XElement variable { get; set; }
        public XObjCountable(XElement XVar, XElement Variable)
        {
            xvar = XVar;
            variable = Variable;
            numbers = variable == null ? "" : variable.Elements("numbers").Select(p => p.Value).FirstOrDefault() ?? "";
        }
    }

    public class XObj
    {
        public string name { get; set; }
        public string id { get; set; }
        public string type { get; set; }
        public string title { get; set; }
        public string keyword { get; set; }
        public bool trigger { get; set; }
        public IEnumerable<XObj> children { get; set; }
        public XObjConstructor loop { get; set; }

        public XObj(XElement xobj)
        {
            name = xobj.Name.LocalName;
            id = (string)xobj.Attributes("id").FirstOrDefault();
            type = (string)xobj.Attributes("type").FirstOrDefault();
            title = (string)xobj.Attributes("title").FirstOrDefault();
            keyword = (string)xobj.Attributes("keyword").FirstOrDefault();
            trigger = (string)xobj.Attributes("trigger").FirstOrDefault() == "true" ? true : false;
        }
        public XObj(XElement xobj, XElement variables)
        {
            name = xobj.Name.LocalName;
            id = (string)xobj.Attributes("id").FirstOrDefault();
            type = (string)xobj.Attributes("type").FirstOrDefault();
            title = (string)xobj.Attributes("title").FirstOrDefault();
            keyword = (string)xobj.Attributes("keyword").FirstOrDefault();
            trigger = (string)xobj.Attributes("trigger").FirstOrDefault() == "true" ? true : false;
            children = xobj.Elements().Where(p => !(p.Name.LocalName == "x-var" && (string)p.Attributes("counter").FirstOrDefault() == "true")).Select(p => new XObj(p, variables));
            if (name == "x-list")
            {
                List<XObjCountable> loopList =
                (
                    from xvar in xobj.Elements("x-var")
                    join variable in variables.Elements("variable").Where(p => (string)p.Attributes("counter").FirstOrDefault() == "true")
                    on (string)xvar.Attributes("id").FirstOrDefault() equals variable.Elements("id").Select(p => p.Value).FirstOrDefault() into subvars
                    from subvar in subvars.DefaultIfEmpty()
                    select new XObjCountable(xvar, subvar == null ? null : subvar)
                ).ToList();
                if (loopList.Count() != 0)
                {
                    loop = new XObjConstructor(loopList);
                }

            }
        }
    }


    public class XVariable
    {
        public string varid { get; set; }
        public int itemno { get; set; }
        //public int nr { get; set; }
        public string id { get; set; }
        public string type { get; set; }
        public string name { get; set; }
        public string standard { get; set; }
        public string comment { get; set; }
        public string value { get; set; }
        public IEnumerable<XVariable> children { get; set; }
        public IEnumerable<XVarRow> rows { get; set; }
        public IEnumerable<XVarOption> option { get; set; }
        public XVariable() { }
        public XVariable(int Nr, XCountableVar nv)
        {
            //nr = Nr;
            if (nv.nvar == null)
            {
                id = nv.id.id;
                varid = nv.id.varid;
                itemno = nv.id.itemNo;
                name = nv.id.name;
                type = nv.id.type;
            }
            else
            {
                id = nv.id.id;
                varid = nv.id.varid;
                itemno = nv.id.itemNo;
                name = nv.id.name;
                type = nv.id.type;
                value = nv.nvar.Elements("value").Select(p => p.Value).FirstOrDefault();
            }
        }
        public XVariable(XElement xvar)
        {
            id = xvar.Elements("id").Select(p => p.Value).FirstOrDefault();
            name = xvar.Elements("name").Select(p => p.Value).FirstOrDefault();
            type = xvar.Elements("type").Select(p => p.Value).FirstOrDefault();
            standard = xvar.Elements("standard").Select(p => p.Value).FirstOrDefault();
            comment = xvar.Elements("comment").Select(p => p.Value).FirstOrDefault();
            value = xvar.Elements("value").Select(p => p.Value).FirstOrDefault();
            XElement opt = xvar.Elements("options").FirstOrDefault();
            if (opt != null)
            {
                option = opt.Elements("option").Select(p => new XVarOption(p));
            }

            string numbers = (string)xvar.Elements("numbers").FirstOrDefault();
            if (numbers != null)
            {
                Regex rx = new Regex(@"\*N\*");
                rows = from e in numbers.Split(';').Select(p => Convert.ToInt32(p)).OrderBy(p => p)
                       join v in xvar.Elements("variable").Elements("nr")
                       on e.ToString() equals v.Value
                       select new XVarRow(e,
                           new XCountable
                           {
                               id = v.Parent.Parent.Elements("id").Select(p => p.Value).FirstOrDefault(),
                               varid = v.Parent.Elements("id").Select(p => p.Value).FirstOrDefault(),
                               itemNo = e,
                               name = rx.Replace(v.Parent.Parent.Elements("name").Select(p => p.Value).FirstOrDefault(), "").Trim(),
                               type = v.Parent.Parent.Elements("type").Select(p => p.Value).FirstOrDefault(),
                               value = v.Parent.Elements("value").Select(p => p.Value).FirstOrDefault()
                           }
                       );

            }

        }
    }
}
