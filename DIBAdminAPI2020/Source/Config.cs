using DIBAdminAPI.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DIBAdminAPI.Source
{
    public sealed class Config
    {
        public DibConfig _dibConfig = null;

        private static volatile Config instance;
        private static object syncRoot = new Object();

        private string _baseUrl = string.Empty;

        public static Config Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new Config();
                    }
                }

                return instance;
            }
        }

        public Config()
        {
            var config = new ConfigurationBuilder()
                                .SetBasePath(Directory.GetCurrentDirectory())
                                .AddJsonFile("appsettings.json").Build();

            _dibConfig = new DibConfig(config["Dib:logging"],
                                        config["Dib:logpath"],
                                        config["Dib:sqlserver"],
                                        config["Dib:database"],
                                        config["Dib:dbuser"],
                                        config["Dib:dbuserpwd"],
                                        config["Dib:pollintervall"],
                                        config["Dib:updateschedule"],
                                        config["Dib:serverpollintervall"],
                                        config["Dib:serverupdateschedule"],
                                        config["Dib:serveruserincache"],
                                        config["Dib:idserver"]);
        }

        public void Init()
        {
        }

        public void Init(IOptions<DibConfig> DibConfig)
        {
            if (_dibConfig != null)
            {
                lock (syncRoot)
                {
                    _dibConfig = null;
                    _dibConfig = DibConfig.Value;
                }
            }
        }

        public DibConfig getDibConfig()
        {
            return _dibConfig == null ? new DibConfig() : _dibConfig;
        }
        //public string getDibLogging()
        //{
        //    return _dibConfig != null ? _dibConfig.logging : string.Empty;
        //}
        //public string getDibLogpath()
        //{
        //    return _dibConfig != null ? _dibConfig.logpath : string.Empty;
        //}

        //public string getSqlserver()
        //{
        //    return _dibConfig != null ? _dibConfig.sqlserver : string.Empty;
        //}
        //public string getDatabase()
        //{
        //    return _dibConfig != null ? _dibConfig.database : string.Empty;
        //}
        //public string getDBuser()
        //{
        //    return _dibConfig != null ? _dibConfig.dbuser : string.Empty;
        //}
        //public string getDBuserpwd()
        //{
        //    return _dibConfig != null ? _dibConfig.dbuserpwd : string.Empty;
        //}
    }
}
