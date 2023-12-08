using System;


namespace VA.FSC.Services.Common.DTO
{
    public class OutputFilesDto
    {
        public int OutputFileHistoryId { get; set; }
        public int ClaimAcknowledgementHeaderId { get; set; }
        public string FileType { get; set; }
        public DateTime FileCreatedDate { get; set; }
    }
}
