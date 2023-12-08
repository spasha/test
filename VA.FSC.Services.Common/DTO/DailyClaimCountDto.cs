using System;

namespace VA.FSC.Services.Common.DTO
{
    public class DailyClaimCountDto
    {
        public int DailyClaimCountID { get; set; }
        public DateTime? ReportingDate { get; set; }
        public string StationId { get; set; }
        public int? ReceivedFromChcCount { get; set; }
        public int? FscErrorCount { get; set; }
        public int? SentToRmqCount { get; set; }
        public int? TasErrorCount { get; set; }
        public int? ReceivedInVistaCount { get; set; }
    }
}
