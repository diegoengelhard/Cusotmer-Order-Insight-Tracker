namespace Creativa.Web.Models;

public class Order
{
    public int OrderID { get; set; }
    public string? CustomerID { get; set; }
    public DateTime? OrderDate { get; set; }
    public DateTime? ShippedDate { get; set; }
}
