

namespace VA.FSC.Services.Common.DTO
{
    public class DailyClaimCountColumnTotalsDto
    {
        public int? ColumnTotalReceivedFromChcCount { get; set; }
        public int? ColumnTotalFscErrorCount { get; set; }
        public int? ColumnTotalSentToRmqCount { get; set; }
        public int? ColumnTotalTasErrorCount { get; set; }
        public int? ColumnTotalReceivedInVistaCount { get; set; }
    }
}
