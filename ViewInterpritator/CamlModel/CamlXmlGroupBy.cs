namespace Navicon.SP.Components.SqlCache.ViewInterpritator.CamlModel
{
    using System.Collections.Generic;
    using System.Xml.Serialization;

    public class CamlXmlGroupBy
    {
        [XmlAttribute]
        public string Collapse { get; set; }

        [XmlAttribute]
        public string GroupLimit { get; set; }

        [XmlElement]
        public List<CamlXmlFieldRef> FieldRef { get; set; }
    }
}