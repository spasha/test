using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VA.FSC.Services.Common.Enums
{
    public enum ApplicationNames
    {
        /// <summary>
        /// FHIR Processor
        /// </summary>
        FHIRProcessor = 1,

        /// <summary>
        /// FHIR Listener
        /// </summary>
        FHIRListener = 2,

        /// <summary>
        /// RMQ Data Pump
        /// </summary>
        FHIR837DataPump = 3,

        /// <summary>
        /// Database Data Pump (277.ClaimAcknowledgementHeader)
        /// </summary>
        FHIR277DataPump = 4
    }
}
