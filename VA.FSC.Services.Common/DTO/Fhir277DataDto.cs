using System;

namespace VA.FSC.Services.Common.DTO
{
    public class Fhir277DataDto
    {
        public int ClaimAcknowledgmentHeaderId { get; set; }
        //public string ClaimCategoryCode1 { get; set; }
        public DateTime? ClaimLevelServiceDateEnd { get; set; }
        public DateTime? ClaimLevelServiceDateStart { get; set; }
        //public string ClaimStatusCode1 { get; set; }
        public string ClaimType { get; set; }
        public string ClearinghouseTraceNumber { get; set; }
        public string DmiKey { get; set; }
        //public string ErrorField { get; set; }
        //public string ErrorMessage { get; set; }
        //public int? ErrorMessageNumber { get; set; }
        public DateTime? FirstServiceDate { get; set; }

        //public string FreeFormMessageText { get; set; }
        public string GroupSenderId { get; set; }
        public DateTime? HeaderDate { get; set; }
        public string InformationReceiverBatchNumber { get; set; }
        public decimal? InformationReceiverTotalAcceptedAmount { get; set; }
        public int? InformationReceiverTotalAcceptedQuantity { get; set; }
        public decimal? InformationReceiverTotalRejectedAmount { get; set; }
        public int? InformationReceiverTotalRejectedQuantity { get; set; }
        public string InformationSourceLastName { get; set; }
        public string InformationSourcePayorId { get; set; }
        public DateTime? InformationSourceProcessDate { get; set; }
        public DateTime? LastServiceDate { get; set; }
        //public int? LevelId { get; set; }
        public string MD03 { get; set; }
        public string MD04 { get; set; }
        public string MRAStatus { get; set; }
        public string PatientControlNumber { get; set; }
        public string PatientFirstName { get; set; }
        public int? PatientNameId { get; set; }
        public string PatientHealthId { get; set; }
        public string PatientLastName { get; set; }
        public string PatientMemberId { get; set; }
        public string PatientMiddleName { get; set; }
        public string PayorClaimControlNumber { get; set; }
        public string PayerGenerated { get; set; }
        public string PayerLastName { get; set; }
        public DateTime? PayerStatusDate { get; set; }
        public string SourceDataFormat { get; set; }
        public string SourceLastName { get; set; }
        public string SplitClaimIndicator { get; set; }
        //public string StatusActionCode1 { get; set; }
        //public DateTime? StatusEffectiveDate { get; set; }
        public int? SubscriberNameId { get; set; }
        public string SubscriberFirstName { get; set; }
        public string SubscriberLastName { get; set; }
        public string SubscriberMiddleName { get; set; }
        public string SubscriberMemberSubscriberId { get; set; }

    }
}
