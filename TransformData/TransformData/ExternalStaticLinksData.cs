using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Xml;
using System.Xml.Linq;
using DIB.RegExp.ExternalStaticLinks;
using System.Text.RegularExpressions;
using DIB.Data;

namespace DIB.RegExp.ExternalStaticLinks
{
    public class ExternalStaticLinksData
    {
        public XElement _parent = null;
        public XElement _child = null;
        public XElement LoadGlobalXML(string regexpName, int SERVER)
        {
            XElement returnValue = null;
            
            try
            {
                if (SERVER == 0)
                {
                    commxml cx = new commxml();
                    string sourceRegexp = cx.Execute("diba0706.dbo._Abbrevation_xml_Get '" + regexpName + "'");
                    XDocument d = XDocument.Parse(sourceRegexp);
                    returnValue = d.Root.Descendants("root").First();
                }
                else if (SERVER == 1)
                {
                    using (SqlConnection sqlConnRead = new SqlConnection("context connection=true"))
                    {
                        SqlCommand StoredProcedureCommand = new SqlCommand("Abbrevation_xml_Get", sqlConnRead);
                        StoredProcedureCommand.CommandType = CommandType.StoredProcedure;
                        SqlParameter myParm1 = StoredProcedureCommand.Parameters.Add("@regexpName", SqlDbType.VarChar, 38);
                        myParm1.Value = regexpName;
                        sqlConnRead.Open();
                        XmlReader r = StoredProcedureCommand.ExecuteXmlReader();
                        XDocument d = XDocument.Load(r);
                        r.Close();
                        XElement newE = d.Root.Descendants("root").First();
                        returnValue = newE;
                    }
                    
                }
            }
            catch
            {
                
            }
            return returnValue;
        }

        public List<ExternalStaticLinksEx1.rQuery> LoadTokenRegExp(Dictionary<string, string> regexps)
        {
            List<ExternalStaticLinksEx1.rQuery> returnValue = new List<ExternalStaticLinksEx1.rQuery>();
            foreach (KeyValuePair<string, string> k in regexps)
            {
                ExternalStaticLinksEx1.rQuery q = new ExternalStaticLinksEx1.rQuery();
                q.name = k.Key;
                q.query = new Regex(k.Value);
                returnValue.Add(q);

            }
            return returnValue;
        }

        public List<ExternalStaticLinksEx1.rQuery> LoadTokenRegExp(int SERVER)
        {
            List<ExternalStaticLinksEx1.rQuery> returnValue = new List<ExternalStaticLinksEx1.rQuery>();
            try
            {
                if (SERVER == 0)
                {
                    commxml cx = new commxml();
                    string sourceRegexp = cx.Execute("diba0706.dbo._Abbrevation_Regexp_Get");
                    XDocument d = XDocument.Parse(sourceRegexp);
                    XElement root = d.Descendants("root").First();
                    int n = root.Elements("row").Count();
                    for (int i = 0; i < n; i++)
                    {
                        XElement row = root.Elements("row").ElementAt(i);
                        ExternalStaticLinksEx1.rQuery q = new ExternalStaticLinksEx1.rQuery();
                        q.name = row.Attribute("name").Value.ToString();
                        q.query = new Regex(row.Attribute("value").Value.ToString());
                        returnValue.Add(q);
                    }

                }
                else if (SERVER == 1)
                {

                    using (SqlConnection sqlConnRead = new SqlConnection("context connection=true"))
                    {
                        string query = "Abbrevation_Regexp_Get";
                        SqlCommand myCommand = new SqlCommand(query, sqlConnRead);
                        SqlDataAdapter adap = new SqlDataAdapter(myCommand);
                        DataTable regexps = new DataTable();
                        adap.Fill(regexps);
                        int n = regexps.Rows.Count;
                        for (int i = 0; i < n; i++)
                        {
                            DataRow r = regexps.Rows[i];
                            ExternalStaticLinksEx1.rQuery q = new ExternalStaticLinksEx1.rQuery();
                            q.name = r[0].ToString();
                            q.query = new Regex(r[1].ToString());
                            returnValue.Add(q);
                        }
                    }
                }
            }
            catch
            {
                returnValue = new List<ExternalStaticLinksEx1.rQuery>();
            }

            return returnValue;
        }

        public List<ExternalStaticLinksEx1.rQuery> LoadTokenRegExp(int SERVER, XElement querys)
        {
            List<ExternalStaticLinksEx1.rQuery> returnValue = new List<ExternalStaticLinksEx1.rQuery>();
            try
            {
                if (SERVER == 0)
                {
                    commxml cx = new commxml();
                    foreach (XElement q in querys.Elements("query"))
                    {
                        string name = q.Attribute("name").Value;
                        string sourceRegexp = cx.Execute("diba0706.dbo._Abbrevation_Regexp_Get_Query '" + name + "'");
                        XDocument d = XDocument.Parse(sourceRegexp);
                        XElement root = d.Descendants("root").First();
                        int n = root.Elements("row").Count();
                        for (int i = 0; i < n; i++)
                        {
                            XElement row = root.Elements("row").ElementAt(i);
                            ExternalStaticLinksEx1.rQuery rQ = new ExternalStaticLinksEx1.rQuery();
                            rQ.name = row.Attribute("name").Value.ToString();
                            rQ.query = new Regex(row.Attribute("value").Value.ToString());
                            returnValue.Add(rQ);
                        }
                    }

                }
                else if (SERVER == 1)
                {

                    foreach (XElement q in querys.Elements("query"))
                    {
                        string name = q.Attribute("name").Value;
                        using (SqlConnection sqlConnRead = new SqlConnection("context connection=true"))
                        {
                            SqlCommand StoredProcedureCommand = new SqlCommand("Abbrevation_Regexp_Get_Query", sqlConnRead);
                            StoredProcedureCommand.CommandType = CommandType.StoredProcedure;
                            SqlParameter myParm1 = StoredProcedureCommand.Parameters.Add("@name", SqlDbType.VarChar, 38);
                            myParm1.Value = name;
                            sqlConnRead.Open();
                            SqlDataAdapter adap = new SqlDataAdapter(StoredProcedureCommand);
                            DataTable regexps = new DataTable();
                            adap.Fill(regexps);
                            int n = regexps.Rows.Count;
                            for (int i = 0; i < n; i++)
                            {
                                DataRow r = regexps.Rows[i];
                                ExternalStaticLinksEx1.rQuery rQ = new ExternalStaticLinksEx1.rQuery();
                                rQ.name = r[0].ToString();
                                rQ.query = new Regex(r[1].ToString());
                                returnValue.Add(rQ);
                            }
                        }
                    }
                }
            }
            catch
            {
                returnValue = new List<ExternalStaticLinksEx1.rQuery>();
            }

            return returnValue;
        }

        public List<ExternalStaticLinksEx1.iObject> GetChildEx(ExternalStaticLinksEx1.actionObject ao, ExternalStaticLinksEx1.iObject io, string tag2, int SERVER)
        {
            List<ExternalStaticLinksEx1.iObject> returnValue = new List<ExternalStaticLinksEx1.iObject>();
            try
            {
                if (SERVER == 0)
                {
                    if (_child == null)
                    {
                        commxml cx = new commxml();
                        string sourceRegexp = cx.Execute("diba0706.dbo._Abbrevation_Child_Get");
                        XDocument d = XDocument.Parse(sourceRegexp);
                        _child = d.Descendants("childs").First();
                    }
                    string topic_id = io.id;
                    int n = 0;
                    foreach (XElement r in _child.Elements("child").Where(p => p.Attribute("topic_id").Value == topic_id
                                        && p.Attribute("tag2").Value == tag2))
                    {
                        ExternalStaticLinksEx1.iObject newIo = new ExternalStaticLinksEx1.iObject();
                        newIo.tag = tag2;
                        newIo.id = newIo.name = r.Attribute("id").Value.ToString();
                        returnValue.Add(newIo);

                        n++;
                    }
                    if (n == 0)
                    {
                        ExternalStaticLinksEx1.iObject newIo = new ExternalStaticLinksEx1.iObject();
                        newIo.tag = tag2;
                        newIo.text = ao.mTop.Value;
                        returnValue.Add(newIo);
                    }


                }
                else if (SERVER == 1)
                {
                    using (SqlConnection sqlConnRead = new SqlConnection("context connection=true"))
                    {
                        SqlCommand StoredProcedureCommand = new SqlCommand("Abbrevation_Identify_Child_Ex", sqlConnRead);
                        StoredProcedureCommand.CommandType = CommandType.StoredProcedure;
                        SqlParameter myParm1 = StoredProcedureCommand.Parameters.Add("@topic_id", SqlDbType.VarChar, 50);
                        myParm1.Value = io.id;
                        SqlParameter myParm2 = StoredProcedureCommand.Parameters.Add("@tag2", SqlDbType.VarChar, 50);
                        myParm2.Value = tag2;

                        sqlConnRead.Open();
                        SqlDataAdapter adap = new SqlDataAdapter(StoredProcedureCommand);
                        DataTable childs = new DataTable();
                        adap.Fill(childs);
                        int n = childs.Rows.Count;

                        if (n != 0)
                        {
                            for (int i = 0; i < n; i++)
                            {
                                DataRow r = childs.Rows[i];
                                ExternalStaticLinksEx1.iObject newIo = new ExternalStaticLinksEx1.iObject();
                                newIo.tag = tag2;
                                newIo.id = newIo.name = r[0].ToString();
                                returnValue.Add(newIo);
                            }
                        }
                        else
                        {
                            ExternalStaticLinksEx1.iObject newIo = new ExternalStaticLinksEx1.iObject();
                            newIo.tag = tag2;
                            newIo.text = ao.mTop.Value;
                            returnValue.Add(newIo);
                        }
                    }
                }
            }
            catch (SystemException e)
            {
                throw new Exception(string.Format("GetChildEx Error: {0}", e.Message.ToString()));
            }
            return returnValue;
        }

        public List<ExternalStaticLinksEx1.iObject> GetParentEx(ExternalStaticLinksEx1.actionObject ao, string tag1, string language, int SERVER)
        {
            List<ExternalStaticLinksEx1.iObject> returnValue = new List<ExternalStaticLinksEx1.iObject>();
            try
            {
                if (SERVER == 0)
                {
                    if (_parent == null)
                    {
                        commxml cx = new commxml();
                        string sourceRegexp = cx.Execute("diba0706.dbo._Abbrevation_Parent_Get");
                        XDocument d = XDocument.Parse(sourceRegexp);
                        _parent = d.Descendants("parents").First();
                    }
                    int n = 0;
                    foreach (XElement r in _parent.Elements("parent").Where(p => p.Attribute("tag1").Value == tag1
                                        && p.Attribute("lang").Value == language))
                    {
                        ExternalStaticLinksEx1.iObject newIo = new ExternalStaticLinksEx1.iObject();
                        newIo.tag = tag1;
                        newIo.lang = language;
                        newIo.id = r.Attribute("topic_id").Value.ToString();
                        newIo.name = r.Attribute("name").Value.ToString();
                        returnValue.Add(newIo);

                        n++;
                    }
                    if (n == 0)
                    {
                        ExternalStaticLinksEx1.iObject newIo = new ExternalStaticLinksEx1.iObject();
                        newIo.tag = tag1;
                        newIo.lang = language;
                        newIo.text = ao.mTop.Value;
                        returnValue.Add(newIo);
                    }


                }
                else if (SERVER == 1)
                {
                    using (SqlConnection sqlConnRead = new SqlConnection("context connection=true"))
                    {
                        SqlCommand StoredProcedureCommand = new SqlCommand("Abbrevation_Identify_Parent_Ex", sqlConnRead);
                        StoredProcedureCommand.CommandType = CommandType.StoredProcedure;
                        SqlParameter myParm1 = StoredProcedureCommand.Parameters.Add("@tag1", SqlDbType.VarChar, 50);
                        myParm1.Value = tag1;
                        SqlParameter myParm2 = StoredProcedureCommand.Parameters.Add("@lang", SqlDbType.VarChar, 2);
                        myParm2.Value = language;
                        sqlConnRead.Open();

                        SqlDataAdapter adap = new SqlDataAdapter(StoredProcedureCommand);
                        DataTable parents = new DataTable();
                        adap.Fill(parents);
                        int n = parents.Rows.Count;
                        

                        if (n != 0)
                        {

                            for (int i = 0; i < n; i++)
                            {
                                DataRow r = parents.Rows[i];
                                //q.name = r[0].ToString();
                                ExternalStaticLinksEx1.iObject newIo = new ExternalStaticLinksEx1.iObject();
                                newIo.tag = tag1;
                                newIo.lang = language;
                                newIo.id = r[0].ToString();
                                newIo.name = r[1].ToString();
                                returnValue.Add(newIo);
                            }
                        }
                        else
                        {
                            ExternalStaticLinksEx1.iObject newIo = new ExternalStaticLinksEx1.iObject();
                            newIo.tag = tag1;
                            newIo.lang = language;
                            newIo.text = ao.mTop.Value;
                            returnValue.Add(newIo);
                        }
                    }
                }
            }
            catch (SystemException e)
            {
                throw new Exception(string.Format("GetParentEx Error: {0}", e.Message.ToString()));
            }
            return returnValue;

        }

        public string Abbrevations_Build(int SERVER)
        {
            string returnValue = string.Empty;
            string abbrevRegexp = string.Empty;
            try
            {
                if (SERVER == 0)
                {
                    commxml cx = new commxml();
                    string sourceRegexp = cx.Execute("diba0706.dbo._Abbrevation_Get");
                    XDocument d = XDocument.Parse(sourceRegexp);
                    XElement root = d.Descendants("root").First();
                    int n = root.Elements("row").Count();
                    for (int i = 0; i < n; i++)
                    {
                        XElement row = root.Elements("row").ElementAt(i);
                        if (row != null)
                        {
                            string exp = row.Attribute("abbrev").Value.ToString();
                            Abbrevation_Replace(ref exp);
                            abbrevRegexp += (abbrevRegexp == "" ? exp : "|" + exp);
                        }
                    }
                    if (abbrevRegexp != string.Empty)
                        abbrevRegexp = "(" + abbrevRegexp + ")";
                }
                else if (SERVER == 1)
                {

                    using (SqlConnection sqlConnRead = new SqlConnection("context connection=true"))
                    {
                        SqlCommand StoredProcedureCommand = new SqlCommand("Abbrevation_Get", sqlConnRead);
                        StoredProcedureCommand.CommandType = CommandType.StoredProcedure;
                        sqlConnRead.Open();

                        SqlDataAdapter adap = new SqlDataAdapter(StoredProcedureCommand);
                        DataTable abbrevs = new DataTable();
                        adap.Fill(abbrevs);

                        if (abbrevs.Rows.Count > 0)
                        {
                            foreach (DataRow row in abbrevs.Rows)
                            {
                                if (row != null)
                                {
                                    string exp = row[0].ToString().Trim();
                                    Abbrevation_Replace(ref exp);
                                    abbrevRegexp += (abbrevRegexp == "" ? exp : "|" + exp);
                                }
                            }
                            if (abbrevRegexp != string.Empty)
                                abbrevRegexp = "(" + abbrevRegexp + ")";
                        }
                    }
                }
                returnValue = abbrevRegexp;
            }
            catch (SystemException e)
            {
                throw new Exception(string.Format("Abbrevations_Build Error: {0}", e.Message.ToString()));
            }
            return returnValue;
        }

        private void Abbrevation_Replace(ref string exp)
        {
            try
            {
                if (Regex.IsMatch(exp, "^[a-zæøå][a-zæøå]"))
                {
                    exp = "(" + exp.Substring(0, 1).ToUpper() + "|" + exp.Substring(0, 1).ToLower() + ")" + exp.Substring(1);
                }
                exp = Regex.Replace(exp, @"\s+", @"\s+");
                exp = Regex.Replace(exp, @"\/", @"(\s+)?\/(\s+)?");
                exp = exp.Replace(@".", @"\.");
            }
            catch (SystemException e)
            {
                exp = "";
            }
        }

        public bool Global_Regexp_Update(string name, string regexp, string user, int SERVER)
        {
            bool returnValue = false;
            try
            {
                if (SERVER == 0)
                {
                    commxml cx = new commxml();
                    string sourceRegexp = cx.Execute("diba0706.dbo._Global_Parameters_Regexp_Update '" + name + "','" + regexp + "','" + user + "'" + regexp.Trim() == "" ? "" : "Utrykk mangler" + "'");
                    XDocument d = XDocument.Parse(sourceRegexp);
                    XElement root = d.Descendants("root").First();
                    if (root.Elements("row").First().Attribute("status").Value == "ok")
                    {
                        returnValue = true;
                    }
                    else
                    {
                        throw new Exception(string.Format("StoreAbbrevSourceRegexp Error: {0}", root.Elements("row").First().Attribute("status").Value.ToString()));
                    }
                    
                }
                else if (SERVER == 1)
                {
                    using (SqlConnection sqlConnRead = new SqlConnection("context connection=true"))
                    {
                        SqlCommand StoredProcedureCommand = new SqlCommand("Global_Parameters_Regexp_Update", sqlConnRead);
                        StoredProcedureCommand.CommandType = CommandType.StoredProcedure;
                        SqlParameter myParm1 = StoredProcedureCommand.Parameters.Add("@parameter_name", SqlDbType.VarChar, 50);
                        myParm1.Value = name;
                        SqlParameter myParm2 = StoredProcedureCommand.Parameters.Add("@parameter_value", SqlDbType.NVarChar, -1);
                        myParm2.Value = regexp;
                        SqlParameter myParm3 = StoredProcedureCommand.Parameters.Add("@username", SqlDbType.VarChar, 50);
                        myParm3.Value = user;
                        if (regexp.Trim() == "")
                        {
                            SqlParameter myParm4 = StoredProcedureCommand.Parameters.Add("@regexp_error", SqlDbType.NVarChar, -1);
                            myParm4.Value = "Uttrykk mangler";
                        }
                        else
                        {
                            try
                            {
                                Regex test = new Regex(regexp);
                            }
                            catch (SyntaxErrorException e)
                            {
                                SqlParameter myParm4 = StoredProcedureCommand.Parameters.Add("@regexp_error", SqlDbType.NVarChar, -1);
                                myParm4.Value = e.Message.ToString();
                            }
                        }
                        sqlConnRead.Open();
                        StoredProcedureCommand.ExecuteNonQuery();
                    }
                    returnValue = true;
                }
            }
            catch (SystemException e)
            {
                throw new Exception(string.Format("Global_Regexp_Update Error: {0}", e.Message.ToString()));
            }
            return returnValue;
        }
    }
}
