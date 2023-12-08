using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VA.FSC.Services.Common.Enums
{
    public enum AnalysisEventNames
    {
        /// <summary>
        /// Listener Startup
        /// </summary>
        ListenerRmqConnectionStartupSuccess = 0,

        /// <summary>
        /// Any RMQ Connection Issue
        /// </summary>
        ListenerRmqConnectionError = 1,

        /// <summary>
        /// Processor Startup
        /// </summary>
        ProcessorRmqConnectionStartupSuccess = 2,

        /// <summary>
        /// Any RMQ Connection Issue
        /// </summary>
        ProcessorRmqConnectionError = 3
    }
}
