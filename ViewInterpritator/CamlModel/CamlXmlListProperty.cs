namespace Navicon.SP.Components.SqlCache.ViewInterpritator.CamlModel
{
    using System.Xml.Serialization;

    public class CamlXmlListProperty
    {
        [XmlAttribute]
        public string AutoHyperLink { get; set; }

        [XmlAttribute]
        public string AutoHyperLinkNoEncoding { get; set; }

        [XmlAttribute]
        public string AutoNewLine { get; set; }

        [XmlAttribute]
        public string Default { get; set; }

        [XmlAttribute]
        public string ExpandXML { get; set; }

        [XmlAttribute]
        public string HTMLEncode { get; set; }

        [XmlAttribute]
        public string Select { get; set; }

        [XmlAttribute]
        public string StripWS { get; set; }

        [XmlAttribute]
        public string URLEncode { get; set; }

        [XmlAttribute]
        public string URLEncodeAsURL { get; set; }
    }
}