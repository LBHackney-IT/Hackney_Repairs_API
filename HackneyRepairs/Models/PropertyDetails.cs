using System;
using HackneyRepairs.PropertyService;

namespace HackneyRepairs.Models
{
    public class PropertyDetails : PropertySummary
    {
        public bool Maintainable { get; set; }
        public int LevelCode { get; set; }
        public string Description { get; set; }
        public string TenureCode { get; set; }
        public string PropertyTypeCode { get; set; }
        public string TenancyAgreementReference { get; set; }
    }
}
