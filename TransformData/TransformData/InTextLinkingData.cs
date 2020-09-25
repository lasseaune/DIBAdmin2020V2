using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using DIB.Data;
using System.Text.RegularExpressions;

namespace DIB.InTextLinking
{
    public class InTextLinkingData
    {
        private bool _Local = false;
        public InTextLinkingData(bool Local)
        {
            if (Local) _Local = true;
        }

        public string RegexpExpressionGet(string name)
        {
            string returnValue = string.Empty;
            try
            {
                if (_Local)
                {
                    commxml cx = new commxml();
                    string sourceRegexp = cx.Execute("TopicMap.dbo.AbbrevationRegexpGet '" + name + "'");
                    XDocument d = XDocument.Parse(sourceRegexp);
                    XElement result = d.Descendants("result").FirstOrDefault();
                    if (result != null) returnValue = result.Value.ToString();

                }
                else
                {
                    using (SqlConnection sqlConnRead = new SqlConnection("context connection=true"))
                    {
                        SqlCommand StoredProcedureCommand = new SqlCommand("dbo.AbbrevationRegexpGet", sqlConnRead);
                        StoredProcedureCommand.CommandType = CommandType.StoredProcedure;
                        SqlParameter myParm1 = StoredProcedureCommand.Parameters.Add("@name", SqlDbType.VarChar, 38);
                        myParm1.Value = name;
                        sqlConnRead.Open();
                        XmlReader r = StoredProcedureCommand.ExecuteXmlReader();
                        XDocument d = XDocument.Load(r);
                        r.Close();
                        XElement result = d.Descendants("result").FirstOrDefault();
                        if (result != null) returnValue = result.Value.ToString();
                    }
                }
            }
            catch
            {
            }

            return returnValue;
        }

        public bool Global_Regexp_Update(string name, string regexp, string user)
        {
            bool returnValue = false;
            try
            {
                if (_Local)
                {
                    commxml cx = new commxml();
                    string sourceRegexp = cx.Execute("topicmap.dbo.Global_Parameters_Regexp_Update '" + name + "','" + regexp + "','" + user + "'," + (regexp.Trim() != "" ? "null" : "'Utrykk mangler'"));
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
                else
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

        public XElement LoadGlobalXML(string regexpName)
        {
            XElement returnValue = null;

            try
            {
                if (_Local)
                {
                    commxml cx = new commxml();
                    string sourceRegexp = cx.Execute("topicmap.dbo.Abbrevation_xml_Get '" + regexpName + "'");
                    XDocument d = XDocument.Parse(sourceRegexp);
                    returnValue = d.Root.Descendants("root").First();
                }
                else
                {
                    using (SqlConnection sqlConnRead = new SqlConnection("context connection=true"))
                    {
                        SqlCommand StoredProcedureCommand = new SqlCommand("topicmap.dbo.Abbrevation_xml_Get", sqlConnRead);
                        StoredProcedureCommand.CommandType = CommandType.StoredProcedure;
                        SqlParameter myParm1 = StoredProcedureCommand.Parameters.Add("@regexpName", SqlDbType.VarChar, 38);
                        myParm1.Value = regexpName;
                        sqlConnRead.Open();
                        XmlReader r = StoredProcedureCommand.ExecuteXmlReader();
                        XDocument d = XDocument.Load(r);
                        r.Close();
                        XElement newE = d.Root;
                        returnValue = newE;
                    }

                }
            }
            catch
            {

            }
            return returnValue;
        }

        public XElement IdentifyTags(string base64Parameter, string language)
        {
            XElement returnValue = null;
            if (_Local)
            {
                commxml cx = new commxml();
                string sourceRegexp = cx.Execute("TopicMap.dbo.Abbreviation_Identify_Tags '" + base64Parameter + "', '" + language + "'");
                XDocument d = XDocument.Parse(sourceRegexp);
                returnValue = d.Descendants("result").FirstOrDefault();
            }
            else
            {
                using (SqlConnection sqlConnRead = new SqlConnection("context connection=true"))
                {
                    SqlCommand StoredProcedureCommand = new SqlCommand("Abbreviation_Identify_Tags", sqlConnRead);
                    StoredProcedureCommand.CommandType = CommandType.StoredProcedure;
                    SqlParameter myParm1 = StoredProcedureCommand.Parameters.Add("@base64", SqlDbType.VarChar);
                    myParm1.Value = base64Parameter;
                    SqlParameter myParm2 = StoredProcedureCommand.Parameters.Add("@lang", SqlDbType.VarChar, 2);
                    myParm2.Value = language;
                    sqlConnRead.Open();
                    XmlReader r = StoredProcedureCommand.ExecuteXmlReader();
                    XDocument d = XDocument.Load(r);
                    r.Close();
                    returnValue = d.Descendants("result").FirstOrDefault();
                }
            }
            return returnValue;
        }
        
        public string GetExternalExpression(string name)
        {
            switch (name)
            {
                case "idsources":
                    InTextLinkingData _data = new InTextLinkingData(true);
                    InTextLinkingAbbrevations _abbrev = new InTextLinkingAbbrevations();
                    //return _abbrev.GetAbbrevations(_data.GetAbbrevations());
                    return _abbrev.Tag1();
                default:
                    return "";
                    
            }
        }
        public List<string> GetAbbrevations()
        {
            List<string> returnValue = null;
            string abbrevRegexp = string.Empty;
            try
            {
                if (_Local)
                {
                    commxml cx = new commxml();
                    string sourceRegexp = cx.Execute("TopicMap.dbo.AbbrevationsGet");
                    XDocument d = XDocument.Parse(sourceRegexp);
                    XElement result = d.Descendants("result").FirstOrDefault();
                    if (result != null)
                    {
                        returnValue = result.Elements("data").Select(p => p.Value).ToList();
                    }
                }
                else if (!_Local)
                {
                    using (SqlConnection sqlConnRead = new SqlConnection("context connection=true"))
                    {
                        SqlCommand StoredProcedureCommand = new SqlCommand("AbbrevationsGet", sqlConnRead);
                        StoredProcedureCommand.CommandType = CommandType.StoredProcedure;
                        sqlConnRead.Open();
                        XmlReader r = StoredProcedureCommand.ExecuteXmlReader();
                        XDocument d = XDocument.Load(r);
                        r.Close();
                        XElement result = d.Descendants("result").FirstOrDefault();
                        if (result != null)
                        {
                            returnValue = result.Elements("data").Select(p => p.Value).ToList();
                        }
                    }
                }
            }
            catch (SystemException e)
            {
                throw new Exception(string.Format("GetAbbrevations: {0}", e.Message.ToString()));
            }
            return returnValue;
        }

    }
}
