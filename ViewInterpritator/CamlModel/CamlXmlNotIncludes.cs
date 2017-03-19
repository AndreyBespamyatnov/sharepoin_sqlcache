namespace Navicon.SP.Components.SqlCache.ViewInterpritator.CamlModel
{
    using System.Collections.Generic;
    using System.Xml.Serialization;

    public class CamlXmlNotIncludes : LogicalOperatorsBase
    {
        public CamlXmlNotIncludes()
        {
            this.Value = new List<CamlXmlValue>();
        }

        [XmlElement]
        public CamlXmlFieldRef FieldRef { get; set; }

        [XmlElement]
        public List<CamlXmlValue> Value { get; set; }

        [XmlIgnore]
        public override OperatorType OperatorName
        {
            get { return OperatorType.NotIncludes; }
        }
    }
}