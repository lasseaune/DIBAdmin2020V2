using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DIBAdminAPI.Models
{
    public class DibConfig
    {
        public string logging { get; set; }
        public string logpath { get; set; }
        public string sqlserver { get; set; }
        public string database { get; set; }
        public string dbuser { get; set; }
        public string dbuserpwd { get; set; }
        public string pollintervall { get; set; }
        public string updateschedule { get; set; }
        public string serverpollintervall { get; set; }
        public string serverupdateschedule { get; set; }
        public string serveruserincache { get; set; }
        public string idserver { get; set; }

        public DibConfig() { }
        public DibConfig(string Logging,
                            string Logpath,
                            string SqlServer,
                            string Database,
                            string DBuser,
                            string DBuserpwd,
                            string DBpollintervall,
                            string DBupdateschedule,
                            string Serverpollintervall,
                            string Serverupdateschedule,
                            string Serveruserincache,
                            string IDServer)
        {
            logging = Logging;
            logpath = Logpath;
            sqlserver = SqlServer;
            database = Database;
            dbuser = DBuser;
            dbuserpwd = DBuserpwd;
            pollintervall = DBpollintervall;
            updateschedule = DBupdateschedule;
            serverpollintervall = Serverpollintervall;
            serverupdateschedule = Serverupdateschedule;
            serveruserincache = Serveruserincache;
            idserver = IDServer;
        }
    }
}
