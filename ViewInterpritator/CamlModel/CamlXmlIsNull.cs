namespace Navicon.SP.Components.SqlCache.ViewInterpritator.CamlModel
{
    using System.Xml.Serialization;

    public class CamlXmlIsNull : LogicalOperatorsBase, IFieldRefValueBase
    {
        [XmlIgnore]
        public override OperatorType OperatorName
        {
            get { return OperatorType.IsNull; }
        }

        [XmlElement]
        public CamlXmlFieldRef FieldRef { get; set; }

        [XmlElement]
        public CamlXmlValue Value { get; set; }

        [XmlIgnore]
        public string SqlOperator
        {
            get { return "IS NULL"; }
        }
    }
}