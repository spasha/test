using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VA.FSC.Services.Common.DTO
{
    public class ClaimStatusesInErrorDto
    {
        public string StationId { get; set; }
        public string Claim { get; set; }
        public string BatchNumber { get; set; }
        public int? ProcessId { get; set; }
    }
}
