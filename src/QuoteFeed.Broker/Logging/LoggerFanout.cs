using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;

namespace QuoteFeed.Broker
{
    internal class LoggerFanout : ILog
    {
        private Dictionary<string, ILog> logs = new Dictionary<string, ILog>();

        public LoggerFanout()
        {
        }

        /// <remarks>Not thread-safe</remarks>
        public LoggerFanout AddLogger(string name, ILog log)
        {
            logs[name] = log;
            return this;
        }

        public async Task WriteErrorAsync(string component, string process, string context, Exception exception, DateTime? dateTime = default(DateTime?))
        {
            foreach (var logName in this.logs.Keys)
            {
                await this.logs[logName].WriteErrorAsync(component, process, context, exception, dateTime);
            }
        }

        public async Task WriteFatalErrorAsync(string component, string process, string context, Exception exception, DateTime? dateTime = default(DateTime?))
        {
            foreach (var logName in this.logs.Keys)
            {
                await this.logs[logName].WriteFatalErrorAsync(component, process, context, exception, dateTime);
            }
        }

        public async Task WriteInfoAsync(string component, string process, string context, string info, DateTime? dateTime = default(DateTime?))
        {
            foreach (var logName in this.logs.Keys)
            {
                await this.logs[logName].WriteInfoAsync(component, process, context, info, dateTime);
            }
        }

        public async Task WriteWarningAsync(string component, string process, string context, string info, DateTime? dateTime = default(DateTime?))
        {
            foreach (var logName in this.logs.Keys)
            {
                await this.logs[logName].WriteWarningAsync(component, process, context, info, dateTime);
            }
        }
    }
}
