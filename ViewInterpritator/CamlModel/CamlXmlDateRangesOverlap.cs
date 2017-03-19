namespace Navicon.SP.Components.SqlCache.ViewInterpritator.CamlModel
{
    using System.Collections.Generic;
    using System.Xml.Serialization;

    public class CamlXmlDateRangesOverlap : LogicalOperatorsBase
    {
        [XmlElement]
        public List<CamlXmlFieldRef> FieldRef { get; set; }

        [XmlElement]
        public CamlXmlValue Value { get; set; }

        [XmlIgnore]
        public override OperatorType OperatorName
        {
            get { return OperatorType.DateRangesOverlap; }
        }
    }
}