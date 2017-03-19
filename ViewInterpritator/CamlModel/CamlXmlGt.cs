namespace Navicon.SP.Components.SqlCache.ViewInterpritator.CamlModel
{
    using System.Xml.Serialization;

    public class CamlXmlGt : LogicalOperatorsBase, IFieldRefValueBase
    {
        [XmlIgnore]
        public override OperatorType OperatorName
        {
            get { return OperatorType.Gt; }
        }

        [XmlElement]
        public CamlXmlFieldRef FieldRef { get; set; }

        [XmlElement]
        public CamlXmlValue Value { get; set; }

        [XmlIgnore]
        public string SqlOperator
        {
            get { return ">"; }
        }
    }
}