using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Text.RegularExpressions;

namespace DIBDocument
{
    public class DGVariableEval
    {
        private string name { get; set; }
        private string id { get; set; }
        public bool collection { get; set; }
        private Regex GetN = new Regex(@"\*N\*", RegexOptions.IgnoreCase);
        private Regex NumberFromName { get; set; }
        private Regex NumberFromId { get; set; }
        public DGVariableEval(string Name, string Id)
        {
            name = Name;
            id = Id;
            collection = Regex.IsMatch(name, @"\*N\*", RegexOptions.IgnoreCase);
            if (collection)
            {
                NumberFromName = GetRegexNr(name);
                NumberFromId = GetRegexNr(id);
            }
        }
        private Regex GetRegexNr(string s)
        {
            string r = ReplaceNToGroup(s);
            return new Regex(r, RegexOptions.IgnoreCase);
        }
        private string ReplaceNToGroup(string s)
        {
            return GetN.Replace(s, @"(?<nr>(\d+))");
        }
        public string GetNumberInName(string name)
        {
            if (!collection) return null;
            string result = NumberFromName.Match(name).Groups["nr"].Value;
            if (Regex.IsMatch(result, @"^\d+$"))
                return result;
            return null;
        }
        public string GetNumberInId(string id)
        {
            if (!collection) return null;
            string result = NumberFromName.Match(id).Groups["nr"].Value;
            if (Regex.IsMatch(result, @"^\d+$"))
                return result;
            return null;
        }

        public string SetNumberInId(string nr)
        {
            if (!collection) return null;
            return GetN.Replace(id, nr);
        }
    }
    public static class DIBVariables
    {
        public static void VariableUpdate(this XElement variable, XElement triggervariable)
        {
            string name = variable.Elements("name").Select(p => p.Value).FirstOrDefault();
            string id = variable.Elements("id").Select(p => p.Value).FirstOrDefault();
            DGVariableEval dgv = new DGVariableEval(name, id);
            if (dgv.collection)
            {
                if (variable.Elements("variable").Count() == 0 && triggervariable.Elements("variable").Count() != 0)
                {
                    foreach (XElement v in triggervariable.Elements("variable"))
                    {
                        string vname = (string)v.Attributes("name").FirstOrDefault();
                        string nr = dgv.GetNumberInName(vname);
                        XElement newvar = new XElement("variable",
                            new XElement("id", dgv.SetNumberInId(nr)),
                            new XElement("value", v.Value)
                            );
                        variable.Add(newvar);
                    }
                }
                else if (variable.Elements("variable").Count() != 0 && triggervariable.Elements("variable").Count() != 0)
                {
                    List<XElement> ex_vars = variable.Elements("variable").ToList();
                    variable.Elements("variable").Remove();
                    string nr = "";
                    foreach (XElement v in triggervariable.Elements("variable"))
                    {
                        string vname = (string)v.Attributes("name").FirstOrDefault();
                        nr = dgv.GetNumberInName(vname);
                        if (nr != null)
                        {
                            XElement ex_var = ex_vars.Where(p => dgv.GetNumberInId((p.Elements("id").Select(s => s.Value).FirstOrDefault() ?? "")) == nr).FirstOrDefault();
                            if (ex_var != null)
                            {
                                variable.Add(new XElement(ex_var));
                                ex_vars.Where(p => dgv.GetNumberInId((p.Elements("id").Select(s => s.Value).FirstOrDefault() ?? "")) == nr).Remove();
                            }
                            else
                            {
                                XElement newvar = new XElement("variable",
                                    new XElement("id", dgv.SetNumberInId(nr)),
                                    new XElement("value", v.Value)
                                    );
                                variable.Add(newvar);
                            }
                        }
                    }
                    if (ex_vars.Count() != 0)
                    {
                        variable.Add(ex_vars.Select(p => new XElement(p)));
                    }
                }
            }
            else
            {
                XElement value = variable.Elements("value").FirstOrDefault();

                if (triggervariable.Value != "")
                {
                    if (value == null)
                        variable.Add(new XElement("value", triggervariable.Value));
                    else
                    {
                        if (value.Value.Trim() == "")
                            value.SetValue(triggervariable.Value);
                    }
                }
            }

        }
        public static XElement InsertProffData(this XElement variables, XElement triggervariables)
        {
            (
                from v in variables.Elements("variable").Where(p => p.Elements("name").FirstOrDefault() != null)
                join tv in triggervariables.Elements("variable")
                on
                    (v.Elements("name").Select(p => p.Value).FirstOrDefault() ?? "-1").Trim().ToLower()
                    equals
                    ((string)tv.Attributes("name").FirstOrDefault() ?? "-0").Trim().ToLower()
                select new { var = v, trig = tv }
            ).ToList()
            .ForEach(p => p.var.VariableUpdate(p.trig));
            return variables;
        }

        public static XElement GetProffData(this XElement companydata, XElement triggervariables)
        {
            string ElementNameBase = @"[a-zA-Z]+((\.[a-zA-Z]+)+)?";
            string Prefix = @"((?<prefix>((\,\s|\s))))?";
            string Where = @"((?<where>(\s+hvor\s+(?<pname>(" + ElementNameBase + @"))((\s+)?\=(\s+)?)(?<pval>([a-zA-Z\|]+)))))?";
            string ElementName = @"(?<ename>(" + ElementNameBase + @"))" + Where;
            string ElementSet = @"(?<elementset>(" + Prefix + ElementName + @"))";
            Regex r = new Regex(@"(" + ElementSet + ")+");

            triggervariables.DescendantsAndSelf().Attributes("Error").Remove();
            foreach (XElement t in triggervariables.Elements("variable"))
            {
                string v = (string)t.Attributes("elements").FirstOrDefault();
                string name = (string)t.Attributes("name").FirstOrDefault();
                if (t.Attributes("id").FirstOrDefault() == null)
                    t.Add(new XAttribute("id", name.CreateVariableId()));
                else
                {
                    t.Attribute("id").SetValue(name.CreateVariableId());
                }
                bool collection = Regex.IsMatch(name, @"\*N\*", RegexOptions.IgnoreCase);
                Match m = r.Match(v);
                if (m.Success)
                {
                    ElementSets es = new ElementSets(m, companydata);
                    if (es.Error != null)
                    {
                        t.Add(new XAttribute("qResult", es.Error));
                    }
                    else
                    {
                        if ((es.elementsetvalue == null ? 0 : es.elementsetvalue.Count()) != 0)
                        {
                            if (collection)
                            {
                                int i = 1;
                                foreach (string e in es.elementsetvalue)
                                {
                                    string subname = Regex.Replace(name, @"\*N\*", i.ToString(), RegexOptions.IgnoreCase);
                                    XElement var = new XElement("variable",
                                        new XAttribute("n", i.ToString()),
                                        new XAttribute("name", subname),
                                        new XAttribute("id", subname.CreateVariableId()),
                                        new XText(e)
                                    );
                                    t.Add(var);
                                    i++;
                                }
                            }
                            else
                            {
                                t.SetValue(es.elementsetvalue.FirstOrDefault());
                            }
                        }
                    }


                }
            }
            triggervariables.DescendantsAndSelf().Attributes("elements").Remove();
            return triggervariables;
        }
    }
}
