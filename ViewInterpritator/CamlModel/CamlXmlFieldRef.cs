namespace Navicon.SP.Components.SqlCache.ViewInterpritator.CamlModel
{
    using System.Xml.Serialization;

    public class CamlXmlFieldRef
    {
        [XmlAttribute]
        public string Alias { get; set; }

        [XmlAttribute]
        public string Ascending { get; set; }

        [XmlAttribute]
        public string CreateURL { get; set; }

        [XmlAttribute]
        public string DisplayName { get; set; }

        [XmlAttribute]
        public string Explicit { get; set; }

        [XmlAttribute]
        public string Format { get; set; }

        [XmlAttribute]
        public string Id { get; set; }

        [XmlAttribute]
        public string Key { get; set; }

        [XmlAttribute]
        public string List { get; set; }

        [XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute]
        public string RefType { get; set; }

        [XmlAttribute]
        public string ShowField { get; set; }

        [XmlAttribute]
        public string TextOnly { get; set; }

        [XmlAttribute]
        public string Type { get; set; }
    }
}