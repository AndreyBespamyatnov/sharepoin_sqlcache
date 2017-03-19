namespace Navicon.SP.Components.SqlCache.ViewInterpritator.CamlModel
{
    using System.Collections.Generic;
    using System.Xml.Serialization;

    public class CamlXmlValues
    {
        public CamlXmlValues()
        {
            this.Value = new List<CamlXmlValue>();
        }

        [XmlElement]
        public List<CamlXmlValue> Value { get; set; }
    }
}