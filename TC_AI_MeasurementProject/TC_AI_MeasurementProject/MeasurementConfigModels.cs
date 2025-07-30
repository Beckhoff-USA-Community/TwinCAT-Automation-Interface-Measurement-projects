
using Newtonsoft.Json;
using System.Collections.Generic;

namespace TC_AI_MeasurementProject
{
    public class MeasurementProjectConfig
    {
        [JsonProperty("measurementProject")]
        public MeasurementProject MeasurementProject { get; set; }
    }

    public class MeasurementProject
    {
        [JsonProperty("scopeProject")]
        public ScopeProject ScopeProject { get; set; }
    }

    public class ScopeProject
    {
        [JsonProperty("dataPool")]
        public DataPool DataPool { get; set; }

        [JsonProperty("charts")]
        public List<Chart> Charts { get; set; }
    }

    public class DataPool
    {
        [JsonProperty("adsAcquisitions")]
        public List<AdsAcquisition> AdsAcquisitions { get; set; }
    }

    public class AdsAcquisition
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("dataType")]
        public string DataType { get; set; }

        [JsonProperty("symbolName")]
        public string SymbolName { get; set; }

        [JsonProperty("targetPort")]
        public int TargetPort { get; set; }

        [JsonProperty("targetSystem")]
        public string TargetSystem { get; set; }
    }

    public class Chart
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("axisGroups")]
        public List<AxisGroup> AxisGroups { get; set; }
    }

    public class AxisGroup
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("channels")]
        public List<Channel> Channels { get; set; }
    }

    public class Channel
    {
        [JsonProperty("acquisition")]
        public string Acquisition { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
