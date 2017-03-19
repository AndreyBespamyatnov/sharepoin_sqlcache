namespace Navicon.SP.Components.SqlCache.ViewInterpritator.CamlModel
{
    using System.Collections.Generic;
    using System.Xml.Serialization;

    public class CamlXmlOrderBy
    {
        [XmlAttribute]
        public string Override { get; set; }

        [XmlAttribute]
        public string UseIndexForOrderBy { get; set; }

        [XmlElement]
        public List<CamlXmlFieldRef> FieldRef { get; set; }
    }
}