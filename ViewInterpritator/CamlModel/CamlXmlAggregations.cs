namespace Navicon.SP.Components.SqlCache.ViewInterpritator.CamlModel
{
    using System.Collections.Generic;
    using System.Xml.Serialization;

    public class CamlXmlAggregations
    {
        public CamlXmlAggregations()
        {
            this.FieldRef = new List<CamlXmlFieldRef>();
        }

        [XmlAttribute]
        public string Value { get; set; }

        [XmlElement]
        public List<CamlXmlFieldRef> FieldRef { get; set; }
    }
}