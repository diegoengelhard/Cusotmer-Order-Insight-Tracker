namespace Creativa.Web.Models
{
    public class WebTrackerEntry
    {
        public int Id { get; set; }
        public string UrlRequest { get; set; } = string.Empty;
        public string SourceIp { get; set; } = string.Empty;
        public DateTime TimeOfAction { get; set; }
    }
}