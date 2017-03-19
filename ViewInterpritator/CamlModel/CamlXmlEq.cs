namespace Navicon.SP.Components.SqlCache.ViewInterpritator.CamlModel
{
    using System.Xml.Serialization;

    public class CamlXmlEq : LogicalOperatorsBase, IFieldRefValueBase
    {
        [XmlIgnore]
        public override OperatorType OperatorName
        {
            get { return OperatorType.Eq; }
        }

        [XmlElement]
        public CamlXmlFieldRef FieldRef { get; set; }

        [XmlElement]
        public CamlXmlValue Value { get; set; }

        [XmlIgnore]
        public string SqlOperator
        {
            get { return "="; }
        }
    }
}