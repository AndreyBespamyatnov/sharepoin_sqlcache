namespace Navicon.SP.Components.SqlCache.ViewInterpritator.CamlModel
{
    using System.Xml.Serialization;

    using Navicon.SP.Common.XmlProvider;

    [XmlRoot("View")]
    public class CamlXmlView : XmlModelBase<CamlXmlView>
    {
        [XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute]
        public string DisplayName { get; set; }

        [XmlAttribute]
        public string Url { get; set; }

        [XmlAttribute]
        public string DefaultView { get; set; }

        [XmlElement]
        public CamlXmlQuery Query { get; set; }

        [XmlElement]
        public CamlXmlFieldRefs ViewFields { get; set; }

        [XmlElement]
        public CamlXmlRowLimit RowLimit { get; set; }

        [XmlElement]
        public CamlXmlAggregations Aggregations { get; set; }
    }
}