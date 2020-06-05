namespace Steam.WebApi.AuthenticateTicket
{
    using System;
    using System.Collections.Generic;

    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public partial class AuthenticateTicket
    {
        [JsonProperty("response")]
        public Response Response { get; set; }
    }

    public partial class Response
    {
        [JsonProperty("params", NullValueHandling = NullValueHandling.Ignore)]
        public Params Params { get; set; }

        [JsonProperty("error", NullValueHandling = NullValueHandling.Ignore)]
        public Error Error { get; set; }
    }

    public partial class Params
    {
        [JsonProperty("result")]
        public string Result { get; set; }

        [JsonProperty("steamid")]
        public string Steamid { get; set; }

        [JsonProperty("ownersteamid")]
        public string Ownersteamid { get; set; }

        [JsonProperty("vacbanned")]
        public bool? Vacbanned { get; set; }

        [JsonProperty("publisherbanned")]
        public bool? Publisherbanned { get; set; }
    }

    public partial class Error
    {
        [JsonProperty("errorcode")]
        public long? Errorcode { get; set; }

        [JsonProperty("errordesc")]
        public string Errordesc { get; set; }
    }

    public partial class AuthenticateTicket
    {
        public static AuthenticateTicket FromJson(string json) => JsonConvert.DeserializeObject<AuthenticateTicket>(json, Steam.WebApi.AuthenticateTicket.Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this AuthenticateTicket self) => JsonConvert.SerializeObject(self, Steam.WebApi.AuthenticateTicket.Converter.Settings);
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }
}