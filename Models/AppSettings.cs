using System;

namespace SignalRAcsFunctionApp.Models
{
    public class AppSettings
    {
        public string ServiceCname { get; set; }
        public string AadAppId { get; set; }
        public string AadAppSecret { get; set; }
        public Uri PlaceCallEndpointUrl { get; set; }
    }
}
