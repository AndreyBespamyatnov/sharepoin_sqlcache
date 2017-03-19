namespace Navicon.SP.Components.SqlCache.ViewInterpritator.CamlModel
{
    using System.Xml.Serialization;

    public class CamlXmlWhere : LogicalOperatorsBase
    {
        [XmlIgnore]
        public override ILogicalOperators LogicalOperatorOne
        {
            get
            {
                if (this.And != null)
                {
                    return this.And;
                }
                if (this.Or != null)
                {
                    return this.Or;
                }
                if (this.BeginsWith != null)
                {
                    return this.BeginsWith;
                }
                if (this.Contains != null)
                {
                    return this.Contains;
                }
                if (this.DateRangesOverlap != null)
                {
                    return this.DateRangesOverlap;
                }
                if (this.Eq != null)
                {
                    return this.Eq;
                }
                if (this.Geq != null)
                {
                    return this.Geq;
                }
                if (this.Gt != null)
                {
                    return this.Gt;
                }
                if (this.In != null)
                {
                    return this.In;
                }
                if (this.Includes != null)
                {
                    return this.Includes;
                }
                if (this.IsNotNull != null)
                {
                    return this.IsNotNull;
                }
                if (this.IsNull != null)
                {
                    return this.IsNull;
                }
                if (this.Leq != null)
                {
                    return this.Leq;
                }
                if (this.Lt != null)
                {
                    return this.Lt;
                }
                if (this.Membership != null)
                {
                    return this.Membership;
                }
                if (this.Neq != null)
                {
                    return this.Neq;
                }
                return null;
            }
        }

        [XmlIgnore]
        public override OperatorType OperatorName
        {
            get { return OperatorType.Where; }
        }

        [XmlElement]
        public CamlXmlAnd And { get; set; }

        [XmlElement]
        public CamlXmlOr Or { get; set; }

        [XmlElement]
        public CamlXmlBeginsWith BeginsWith { get; set; }

        [XmlElement]
        public CamlXmlContains Contains { get; set; }

        [XmlElement]
        public CamlXmlDateRangesOverlap DateRangesOverlap { get; set; }

        [XmlElement]
        public CamlXmlEq Eq { get; set; }

        [XmlElement]
        public CamlXmlGeq Geq { get; set; }

        [XmlElement]
        public CamlXmlGt Gt { get; set; }

        [XmlElement]
        public CamlXmlIn In { get; set; }

        [XmlElement]
        public CamlXmlIncludes Includes { get; set; }

        [XmlElement]
        public CamlXmlIsNotNull IsNotNull { get; set; }

        [XmlElement]
        public CamlXmlIsNull IsNull { get; set; }

        [XmlElement]
        public CamlXmlLeq Leq { get; set; }

        [XmlElement]
        public CamlXmlLt Lt { get; set; }

        [XmlElement]
        public CamlXmlMembership Membership { get; set; }

        [XmlElement]
        public CamlXmlNeq Neq { get; set; }

        [XmlElement]
        public CamlXmlNotIncludes NotIncludes { get; set; }
    }
}