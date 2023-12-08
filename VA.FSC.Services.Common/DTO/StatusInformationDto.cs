using System;

namespace VA.FSC.Services.Common.DTO
{
    public class StatusInformationDto
    {
        public string ClaimCategoryCode1 { get; set; }
        public string ClaimStatusCode1 { get; set; }
        public string ErrorField { get; set; }
        public string ErrorMessage { get; set; }
        public string FreeFormMessageText { get; set; }
        public int? ErrorMessageNumber { get; set; }
        public int? LevelId { get; set; }
        public string StatusActionCode1 { get; set; }
        public DateTime? StatusEffectiveDate { get; set; }
    }
}
