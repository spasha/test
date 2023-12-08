using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VA.FSC.Services.Common.Enums
{
    public enum ClaimStatusRoutingFlags
    {
        /// <summary>
        /// Create Json and Enqueue
        /// </summary>
        JsonOnly = 1,
        /// <summary>
        /// Produce Legacy Extract (Flat File) Only
        /// </summary>
        LegacyExtractOnly = 2,
        /// <summary>
        /// Create Json Enqueue and Produce Legacy Extract
        /// </summary>
        JsonAndLegacyExtract = 3
    }
}
