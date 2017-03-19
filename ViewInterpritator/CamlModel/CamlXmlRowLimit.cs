namespace Navicon.SP.Components.SqlCache.ViewInterpritator.CamlModel
{
    using System.Xml.Serialization;

    public class CamlXmlRowLimit
    {
        [XmlAttribute]
        public string Paged { get; set; }

        [XmlText]
        public int Value { get; set; }
    }
}