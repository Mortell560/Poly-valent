using Newtonsoft.Json;

namespace Poly_valent
{
    public struct ConfigJson
    {
        [JsonProperty("token")]
        public string Token { get; private set; }
        [JsonProperty("prefix")]
        public string Prefix { get; private set; }
        [JsonProperty("guildtest")]
        public ulong Id { get; private set; }
        [JsonProperty("password")]
        public string Password { get; private set; }
        [JsonProperty("studentId")]
        public long studentId { get; private set; }
    }
}