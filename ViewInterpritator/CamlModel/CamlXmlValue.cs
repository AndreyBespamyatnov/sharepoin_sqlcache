namespace Navicon.SP.Components.SqlCache.ViewInterpritator.CamlModel
{
    using System.Xml.Serialization;

    public class CamlXmlValue
    {
        [XmlAttribute]
        public string Type { get; set; }

        [XmlAttribute]
        public string IncludeTimeValue { get; set; }

        [XmlElement]
        public CamlXmlListProperty ListProperty { get; set; }

        [XmlElement]
        public CamlXmlMonth Month { get; set; }

        [XmlElement]
        public CamlXmlNow Now { get; set; }

        [XmlElement]
        public CamlXmlToday Today { get; set; }

        [XmlElement]
        public CamlXmlUserID UserID { get; set; }

        [XmlText]
        public string Value { get; set; }
    }
}