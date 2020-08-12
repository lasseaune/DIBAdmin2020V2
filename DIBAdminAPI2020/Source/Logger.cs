using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DIBAdminAPI.Source
{
    public sealed class Logger
    {
        private static volatile Logger instance;
        private static object syncRoot = new Object();

        private Logger()
        {

        }

        public static Logger Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new Logger();
                    }
                }

                return instance;
            }
        }

        public void LogLine(string Message)
        {
            if (Config.Instance.getDibConfig().logging.ToLower() == "trace")
                baseLogger<TraceLogger>.Instance.LoggLine(Message);
            else if (Config.Instance.getDibConfig().logging.ToLower() == "file")
                baseLogger<FileLogger>.Instance.LoggLine(Message);
        }
    }



    public class baseLogger<T> where T : baseLogger<T>, new()
    {
        private static volatile T instance;
        private static object syncRoot = new Object();
        protected string _ProjectName { get; set; }

        protected baseLogger()
        {
            init("".GetProgramName());
            LoggHeader();
        }

        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new T();
                    }
                }

                return instance;
            }
        }

        protected virtual void LoggHeader()
        {
            try
            {
                Debug.WriteLine(getLogHeader());
            }
            catch (Exception)
            { }
        }

        public virtual void LoggLine(string Message)
        {
            try
            {
                Debug.WriteLine(formatMessage(Message));
            }
            catch (Exception)
            { }
        }

        protected virtual void init(string projectName)
        {
            _ProjectName = projectName;
        }

        protected virtual string getLogHeader()
        {
            return string.Format("{0}{2} startet {1}{0}----------------------------------------{0}{0}", Environment.NewLine, DateTime.Now.ToString("F"), _ProjectName);
        }

        protected virtual string formatMessage(string message)
        {
            return string.Format("{0}    {1}{2}", DateTime.Now.ToString("G"), message, Environment.NewLine);
        }
    }

    public class TraceLogger : baseLogger<TraceLogger>
    {
        public override void LoggLine(string Message)
        {
            try
            {
                Trace.WriteLine(formatMessage(Message));
            }
            catch (Exception)
            { }
        }

        protected override void LoggHeader()
        {
            try
            {
                Trace.WriteLine(getLogHeader());
            }
            catch (Exception)
            {
            }
        }
    }

    public class FileLogger : baseLogger<FileLogger>
    {
        private string _tmppath = string.Empty;
        private string _loggpath = string.Empty;
        protected override void init(string projectName)
        {
            base.init(projectName);
            string logpath = getLoggPath();
            if (logpath.Trim().Length > 0)
                if (!Directory.Exists(logpath))
                    Directory.CreateDirectory(logpath);
        }

        public override void LoggLine(string Message)
        {
            if (getLoggFile().Trim() != string.Empty)
            {
                try
                {
                    lock (this)
                    {
                        if (getLoggFile().Trim() != string.Empty)
                        {
                            File.AppendAllText(getLoggFile(), formatMessage(Message));
                        }
                    }
                }
                catch (Exception)
                { }
            }
        }

        protected override void LoggHeader()
        {
            if (getLoggFile().Trim() != string.Empty)
            {
                try
                {
                    lock (this)
                    {
                        if (getLoggFile().Trim() != string.Empty)
                        {
                            File.AppendAllText(getLoggFile(), getLogHeader());
                        }
                    }
                }
                catch (Exception)
                {
                }
            }
        }

        private string getLoggFile()
        {
            string path = Config.Instance.getDibConfig().logpath;
            return path.Trim().Length > 0 ? path : string.Empty;
        }

        private string getLoggPath()
        {
            string path = Config.Instance.getDibConfig().logpath;
            if (path.Trim().Length > 0)
            {
                path = path.Substring(0, path.LastIndexOf(@"\"));
            }
            else
                path = string.Empty;

            return path;
        }
    }
}
