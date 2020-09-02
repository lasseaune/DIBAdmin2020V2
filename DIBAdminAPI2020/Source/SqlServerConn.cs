using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DIBAdminAPI.Source
{
    public static class SqlServerConn
    {
        public static XDocument QueryDB(string sSqlQuery, Hashtable param)
        {
            XDocument xdoc;
            DataTable data = new DataTable();
            try
            {
                //"Provider=SQLOLEDB;Server=DIBDB03; Database=diba0706;User ID=dibapp;Password=lau610523;";
                using (var cn = new SqlConnection(string.Format("Server={0}; Database={1};User ID={2};Password={3};",
                                                                    Config.Instance.getDibConfig().sqlserver,
                                                                    Config.Instance.getDibConfig().database,
                                                                    Config.Instance.getDibConfig().dbuser,
                                                                    Config.Instance.getDibConfig().dbuserpwd)))
                {
                    using (SqlCommand cmd = new SqlCommand(sSqlQuery, cn))
                    {
                        cmd.CommandTimeout = 300;
                        cmd.CommandType = CommandType.StoredProcedure;

                        foreach (DictionaryEntry c in param)
                        {
                            cmd.Parameters.Add(c.Key.ToString(), SqlDbType.VarChar).Value = c.Value.ToString();
                        }

                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            da.Fill(data);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.LogLine("".GetCallerName() + string.Format("{0}", ex.Message));
                return XDocument.Parse(string.Format("<error msg=\"KontohjelpApi: [{0}]\" />", ex.Message));
            }

            if (data.Rows.Count == 0)
                xdoc = XDocument.Parse("<searchresult />");
            else
                xdoc = XDocument.Parse(data.Rows[0][0].ToString());

            Logger.Instance.LogLine("".GetCallerName() + string.Format("{0}", xdoc.ToString()));
            return xdoc;
        }

        public static XDocument LogonDB(string sSqlQuery, Hashtable param)
        {
            XDocument xdoc;
            DataTable data = new DataTable();
            try
            {
                //"Provider=SQLOLEDB;Server=DIBDB03; Database=diba0706;User ID=dibapp;Password=lau610523;";
                using (SqlConnection cn = new SqlConnection(string.Format("Server={0}; Database={1};User ID={2};Password={3};",
                                                                    Config.Instance.getDibConfig().sqlserver,
                                                                    Config.Instance.getDibConfig().database,
                                                                    Config.Instance.getDibConfig().dbuser,
                                                                    Config.Instance.getDibConfig().dbuserpwd)))
                {
                    using (SqlCommand cmd = new SqlCommand(sSqlQuery, cn))
                    {
                        cmd.CommandTimeout = 300;
                        cmd.CommandType = CommandType.StoredProcedure;

                        foreach (DictionaryEntry c in param)
                        {
                            cmd.Parameters.Add(c.Key.ToString(), SqlDbType.VarChar).Value = c.Value.ToString();
                        }

                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            da.Fill(data);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return XDocument.Parse(string.Format("<user error=\"KontohjelpApi(LogonExternal): [{0}]\" />", ex.Message));
            }

            if (data.Rows.Count == 0)
                xdoc = XDocument.Parse("<user />");
            else
                //xdoc = XDocument.Parse(data.Rows[0][0].ToString());
                xdoc = new XDocument(new XElement("user",
                                        new XAttribute("session_id", data.Rows[0][0].ToString()),
                                        new XAttribute("brukerkey", data.Rows[0][3].ToString()),
                                        new XAttribute("rolleid", data.Rows[0][4].ToString()),
                                        new XAttribute("rolle", data.Rows[0][0].ToString()),
                                        new XAttribute("rettighet", data.Rows[0][0].ToString())
                                        )
                                    );

            return xdoc;
        }

        public static bool LogDB(string session_id, string query, string topic_id, string bm, int num_konteringTema, int num_kontering, int num_satser, int num_kontoTema, int num_konto, string entry_method, string ip_address, string klient_type)
        {
            bool bretval;
            DataTable data = new DataTable();

            if (session_id.Length == 38)
            {
                try
                {
                    //"Provider=SQLOLEDB;Server=DIBDB03; Database=diba0706;User ID=dibapp;Password=lau610523;";
                    using (SqlConnection cn = new SqlConnection(string.Format("Server={0}; Database={1};User ID={2};Password={3};",
                                                                                Config.Instance.getDibConfig().sqlserver,
                                                                                Config.Instance.getDibConfig().database,
                                                                                Config.Instance.getDibConfig().dbuser,
                                                                                Config.Instance.getDibConfig().dbuserpwd)))
                    {
                        using (SqlCommand cmd = new SqlCommand("DIBkunnskapWebApi_log", cn))
                        {
                            cmd.CommandTimeout = 300;
                            cmd.CommandType = CommandType.StoredProcedure;

                            cmd.Parameters.Add("session_id", SqlDbType.VarChar).Value = session_id;
                            cmd.Parameters.Add("query", SqlDbType.VarChar).Value = query;
                            if (topic_id == null)
                                cmd.Parameters.Add("topic_id", SqlDbType.VarChar).Value = DBNull.Value;
                            else
                                cmd.Parameters.Add("topic_id", SqlDbType.VarChar).Value = topic_id;

                            if (bm.Length == 0)
                                cmd.Parameters.Add("bm", SqlDbType.VarChar).Value = DBNull.Value;
                            else
                                cmd.Parameters.Add("bm", SqlDbType.VarChar).Value = bm;

                            if (num_kontering == -1)
                                cmd.Parameters.Add("num_konteringTema", SqlDbType.Int).Value = DBNull.Value;
                            else
                                cmd.Parameters.Add("num_konteringTema", SqlDbType.Int).Value = num_konteringTema;

                            if (num_kontering == -1)
                                cmd.Parameters.Add("num_kontering", SqlDbType.Int).Value = DBNull.Value;
                            else
                                cmd.Parameters.Add("num_kontering", SqlDbType.Int).Value = num_kontering;

                            if (num_satser == -1)
                                cmd.Parameters.Add("num_satser", SqlDbType.Int).Value = DBNull.Value;
                            else
                                cmd.Parameters.Add("num_satser", SqlDbType.Int).Value = num_satser;

                            if (num_konto == -1)
                                cmd.Parameters.Add("num_kontoTema", SqlDbType.Int).Value = DBNull.Value;
                            else
                                cmd.Parameters.Add("num_kontoTema", SqlDbType.Int).Value = num_kontoTema;

                            if (num_konto == -1)
                                cmd.Parameters.Add("num_konto", SqlDbType.Int).Value = DBNull.Value;
                            else
                                cmd.Parameters.Add("num_konto", SqlDbType.Int).Value = num_konto;

                            if (entry_method.Length == 0)
                                cmd.Parameters.Add("entry_method", SqlDbType.VarChar).Value = DBNull.Value;
                            else
                                cmd.Parameters.Add("entry_method", SqlDbType.VarChar).Value = entry_method;

                            if (ip_address.Length == 0)
                                cmd.Parameters.Add("ip_address", SqlDbType.VarChar).Value = DBNull.Value;
                            else
                                cmd.Parameters.Add("ip_address", SqlDbType.VarChar).Value = ip_address;

                            if (klient_type.Length == 0)
                                cmd.Parameters.Add("klient_type", SqlDbType.VarChar).Value = DBNull.Value;
                            else
                                cmd.Parameters.Add("klient_type", SqlDbType.VarChar).Value = klient_type;

                            using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                            {
                                da.Fill(data);
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    return false;
                }

                if (data.Rows.Count == 0)
                    bretval = false;
                else
                    bretval = (bool)data.Rows[0][0];
            }
            else
            {
                bretval = false;
            }

            return bretval;
        }
    }
}
