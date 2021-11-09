using Newtonsoft.Json;

namespace SignalRAcsFunctionApp.Models
{
    public class User
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "acsUserId")]
        public string AcsUserId { get; set; }

        [JsonProperty(PropertyName = "meetingUrl")]
        public string MeetingUrl { get; set; }

        [JsonProperty(PropertyName = "meetingName")]
        public string MeetingName { get; set; }
    }
}
