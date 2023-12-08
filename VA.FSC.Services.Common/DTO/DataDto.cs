using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VA.FSC.Services.Common.DTO
{
    public class DataDto
    {
        public Fhir277DataDto Fhir277DataDto { get; set; }
        public List<StatusInformationDto> StatusInformationList { get; set; }
        public bool QueryIsValid { get; set; }
    }
}
