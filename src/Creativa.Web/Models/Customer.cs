using System.Runtime.Serialization;

namespace Creativa.Web.Models
{
    [DataContract]
    public class Customer
    {
        [DataMember]
        public string CustomerID { get; set; } = string.Empty;

        [DataMember]
        public string CompanyName { get; set; } = string.Empty;

        [DataMember]
        public string? ContactName { get; set; }

        [DataMember]
        public string? Country { get; set; }

        [DataMember]
        public string? Phone { get; set; }

        [DataMember]
        public string? Fax { get; set; }
    }
}
