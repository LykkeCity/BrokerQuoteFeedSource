using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;

namespace QuoteFeed.Core.Tests.Stub
{
    /// <summary>
    /// Stub for the ILog interface
    /// </summary>
    public class LoggerStub : ILog
    {
        private List<LogRecord> log = new List<LogRecord>();

        public IEnumerable<LogRecord> Log { get { return this.log; } }

        public Task WriteErrorAsync(string component, string process, string context, Exception exception, DateTime? dateTime = default(DateTime?))
        {
            return this.AddRecord(Severity.Error, component, process, context, "", exception, dateTime);
        }

        public Task WriteFatalErrorAsync(string component, string process, string context, Exception exception, DateTime? dateTime = default(DateTime?))
        {
            return this.AddRecord(Severity.Fatal, component, process, context, "", exception, dateTime);
        }

        public Task WriteInfoAsync(string component, string process, string context, string info, DateTime? dateTime = default(DateTime?))
        {
            return this.AddRecord(Severity.Info, component, process, context, info, null, dateTime);
        }

        public Task WriteWarningAsync(string component, string process, string context, string info, DateTime? dateTime = default(DateTime?))
        {
            return this.AddRecord(Severity.Warning, component, process, context, info, null, dateTime);
        }

        private Task AddRecord(Severity severity, string component, string process, string context, string info, Exception exception, DateTime? dateTime)
        {
            this.log.Add(new LogRecord()
            {
                Severity = severity,
                Componet = component,
                Process = process,
                Context = context,
                Info = info,
                Exception = exception,
                DateTime = dateTime
            });
            return Task.FromResult(0);
        }

        public enum Severity
        {
            None = 0,
            Info = 1,
            Warning = 2,
            Error = 3,
            Fatal = 4
        }

        public class LogRecord
        {
            public Severity Severity { get; set; }
            public string Componet { get; set; }
            public string Process { get; set; }
            public string Context { get; set; }
            public string Info { get; set; }
            public Exception Exception { get; set; }
            public DateTime? DateTime { get; set; }
        }
    }
}
