namespace Navicon.SP.Components.SqlCache.ViewInterpritator.CamlModel
{
    using System.Xml.Serialization;

    public class CamlXmlMembership : LogicalOperatorsBase
    {
        [XmlAttribute]
        public string Type { get; set; }

        [XmlElement]
        public CamlXmlFieldRef FieldRef { get; set; }

        [XmlIgnore]
        public override OperatorType OperatorName
        {
            get { return OperatorType.Membership; }
        }
    }
}