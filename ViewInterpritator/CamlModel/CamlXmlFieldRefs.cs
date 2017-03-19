namespace Navicon.SP.Components.SqlCache.ViewInterpritator.CamlModel
{
    using System.Collections.Generic;
    using System.Xml.Serialization;

    public class CamlXmlFieldRefs
    {
        public CamlXmlFieldRefs()
        {
            this.FieldRef = new List<CamlXmlFieldRef>();
        }

        [XmlElement]
        public List<CamlXmlFieldRef> FieldRef { get; set; }
    }
}