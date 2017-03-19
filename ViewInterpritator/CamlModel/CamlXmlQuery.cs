namespace Navicon.SP.Components.SqlCache.ViewInterpritator.CamlModel
{
    using System.Xml.Serialization;

    public class CamlXmlQuery
    {
        [XmlElement]
        public CamlXmlWhere Where { get; set; }

        [XmlElement]
        public CamlXmlGroupBy GroupBy { get; set; }

        [XmlElement]
        public CamlXmlOrderBy OrderBy { get; set; }
    }
}