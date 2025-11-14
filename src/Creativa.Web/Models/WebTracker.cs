namespace Creativa.Web.Models;

public class WebTracker
{
    public int Id { get; set; }
    public string URLRequest { get; set; } = string.Empty;
    public string SourceIp { get; set; } = string.Empty;
    public DateTime TimeOfAction { get; set; }
}