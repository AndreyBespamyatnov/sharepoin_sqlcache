namespace Navicon.SP.Components.SqlCache.ViewInterpritator.CamlModel
{
    using System.Xml.Serialization;

    public class CamlXmlIn : LogicalOperatorsBase
    {
        [XmlElement]
        public CamlXmlFieldRef FieldRef { get; set; }

        [XmlElement]
        public CamlXmlValues Values { get; set; }

        [XmlIgnore]
        public override OperatorType OperatorName
        {
            get { return OperatorType.In; }
        }
    }
}