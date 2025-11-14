using System.Runtime.Serialization;

namespace Creativa.Web.Models
{
    [DataContract]
    public class Order
    {
        [DataMember]
        public int OrderID { get; set; }

        [DataMember]
        public string? CustomerID { get; set; }

        [DataMember]
        public DateTime? OrderDate { get; set; }

        [DataMember]
        public DateTime? ShippedDate { get; set; }
    }
}
