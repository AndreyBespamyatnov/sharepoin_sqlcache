using System.Collections.Generic;

namespace Navicon.SP.Components.SqlCache.ViewInterpritator.CamlModel
{
    using System.Xml.Serialization;

    public class CamlXmlAnd : LogicalOperatorsBase
    {
        private ILogicalOperators _one;
        private ILogicalOperators _two;

        [XmlIgnore]
        public override ILogicalOperators LogicalOperatorOne
        {
            get
            {
                if (this._one != null)
                {
                    return this._one;
                }
                if (this.And != null)
                {
                    this._one = this.And;
                }
                if (this.Or != null)
                {
                    this._one = this.Or;
                }
                if (this.BeginsWith != null && this.BeginsWith.Count > 0)
                {
                    this._one = this.BeginsWith[0];
                }
                if (this.Contains != null && this.Contains.Count > 0)
                {
                    this._one = this.Contains[0];
                }
                if (this.DateRangesOverlap != null && this.DateRangesOverlap.Count > 0)
                {
                    this._one = this.DateRangesOverlap[0];
                }
                if (this.Eq != null && this.Eq.Count > 0)
                {
                    this._one = this.Eq[0];
                }
                if (this.Geq != null && this.Geq.Count > 0)
                {
                    this._one = this.Geq[0];
                }
                if (this.Gt != null && this.Gt.Count > 0)
                {
                    this._one = this.Gt[0];
                }
                if (this.In != null && this.In.Count > 0)
                {
                    this._one = this.In[0];
                }
                if (this.Includes != null && this.Includes.Count > 0)
                {
                    this._one = this.Includes[0];
                }
                if (this.IsNotNull != null && this.IsNotNull.Count > 0)
                {
                    this._one = this.IsNotNull[0];
                }
                if (this.IsNull != null && this.IsNull.Count > 0)
                {
                    this._one = this.IsNull[0];
                }
                if (this.Leq != null && this.Leq.Count > 0)
                {
                    this._one = this.Leq[0];
                }
                if (this.Lt != null && this.Lt.Count > 0)
                {
                    this._one = this.Lt[0];
                }
                if (this.Membership != null && this.Membership.Count > 0)
                {
                    this._one = this.Membership[0];
                }
                if (this.Neq != null && this.Neq.Count > 0)
                {
                    this._one = this.Neq[0];
                }

                return this._one;
            }
        }

        [XmlIgnore]
        public override ILogicalOperators LogicalOperatorTwo
        {
            get
            {
                if (this._two != null)
                {
                    return this._two;
                }

                if (this.And != null && this.LogicalOperatorOne != this.And)
                {
                    this._two = this.And;
                }
                if (this.Or != null && this.LogicalOperatorOne != this.Or)
                {
                    this._two = this.Or;
                }
                if (this.BeginsWith != null && this.BeginsWith.Count > 0)
                {
                    if (this.BeginsWith.Count == 2 && this.LogicalOperatorOne != this.BeginsWith[1])
                    {
                        this._two = this.BeginsWith[1];
                    }
                    else if(this.BeginsWith.Count == 1 && this.LogicalOperatorOne != this.BeginsWith[0])
                    {
                        this._two = this.BeginsWith[0];
                    }
                }
                if (this.Contains != null && this.Contains.Count > 0)
                {
                    if (this.Contains.Count == 2 && this.LogicalOperatorOne != this.Contains[1])
                    {
                        this._two = this.Contains[1];
                    }
                    else if (this.Contains.Count == 1 && this.LogicalOperatorOne != this.Contains[0])
                    {
                        this._two = this.Contains[0];
                    }
                }
                if (this.DateRangesOverlap != null && this.DateRangesOverlap.Count > 0)
                {
                    if (this.DateRangesOverlap.Count == 2 && this.LogicalOperatorOne != this.DateRangesOverlap[1])
                    {
                        this._two = this.DateRangesOverlap[1];
                    }
                    else if (this.DateRangesOverlap.Count == 1 && this.LogicalOperatorOne != this.DateRangesOverlap[0])
                    {
                        this._two = this.DateRangesOverlap[0];
                    }
                }
                if (this.Eq != null && this.Eq.Count > 0)
                {
                    if (this.Eq.Count == 2 && this.LogicalOperatorOne != this.Eq[1])
                    {
                        this._two = this.Eq[1];
                    }
                    else if (this.Eq.Count == 1 && this.LogicalOperatorOne != this.Eq[0])
                    {
                        this._two = this.Eq[0];
                    }
                }
                if (this.Geq != null && this.Geq.Count > 0)
                {
                    if (this.Geq.Count == 2 && this.LogicalOperatorOne != this.Geq[1])
                    {
                        this._two = this.Geq[1];
                    }
                    else if (this.Geq.Count == 1 && this.LogicalOperatorOne != this.Geq[0])
                    {
                        this._two = this.Geq[0];
                    }
                }
                if (this.Gt != null && this.Gt.Count > 0)
                {
                    if (this.Gt.Count == 2 && this.LogicalOperatorOne != this.Gt[1])
                    {
                        this._two = this.Gt[1];
                    }
                    else if (this.Gt.Count == 1 && this.LogicalOperatorOne != this.Gt[0])
                    {
                        this._two = this.Gt[0];
                    }
                }
                if (this.In != null && this.In.Count > 0)
                {
                    if (this.In.Count == 2 && this.LogicalOperatorOne != this.In[1])
                    {
                        this._two = this.In[1];
                    }
                    else if (this.In.Count == 1 && this.LogicalOperatorOne != this.In[0])
                    {
                        this._two = this.In[0];
                    }
                }
                if (this.Includes != null && this.Includes.Count > 0)
                {
                    if (this.Includes.Count == 2 && this.LogicalOperatorOne != this.Includes[1])
                    {
                        this._two = this.Includes[1];
                    }
                    else if (this.Includes.Count == 1 && this.LogicalOperatorOne != this.Includes[0])
                    {
                        this._two = this.Includes[0];
                    }
                }
                if (this.IsNotNull != null && this.IsNotNull.Count > 0)
                {
                    if (this.IsNotNull.Count == 2 && this.LogicalOperatorOne != this.IsNotNull[1])
                    {
                        this._two = this.IsNotNull[1];
                    }
                    else if (this.IsNotNull.Count == 1 && this.LogicalOperatorOne != this.IsNotNull[0])
                    {
                        this._two = this.IsNotNull[0];
                    }
                }
                if (this.IsNull != null && this.IsNull.Count > 0)
                {
                    if (this.IsNull.Count == 2 && this.LogicalOperatorOne != this.IsNull[1])
                    {
                        this._two = this.IsNull[1];
                    }
                    else if (this.IsNull.Count == 1 && this.LogicalOperatorOne != this.IsNull[0])
                    {
                        this._two = this.IsNull[0];
                    }
                }
                if (this.Leq != null && this.Leq.Count > 0)
                {
                    if (this.Leq.Count == 2 && this.LogicalOperatorOne != this.Leq[1])
                    {
                        this._two = this.Leq[1];
                    }
                    else if (this.Leq.Count == 1 && this.LogicalOperatorOne != this.Leq[0])
                    {
                        this._two = this.Leq[0];
                    }
                }
                if (this.Lt != null && this.Lt.Count > 0)
                {
                    if (this.Lt.Count == 2 && this.LogicalOperatorOne != this.Lt[1])
                    {
                        this._two = this.Lt[1];
                    }
                    else if (this.Lt.Count == 1 && this.LogicalOperatorOne != this.Lt[0])
                    {
                        this._two = this.Lt[0];
                    }
                }
                if (this.Membership != null && this.Membership.Count> 0)
                {
                    if (this.Membership.Count == 2 && this.LogicalOperatorOne != this.Membership[1])
                    {
                        this._two = this.Membership[1];
                    }
                    else if (this.Membership.Count == 1 && this.LogicalOperatorOne != this.Membership[0])
                    {
                        this._two = this.Membership[0];
                    }
                }
                if (this.Neq != null && this.Neq.Count > 0)
                {
                    if (this.Neq.Count == 2 && this.LogicalOperatorOne != this.Neq[1])
                    {
                        this._two = this.Neq[1];
                    }
                    else if (this.Neq.Count == 1 && this.LogicalOperatorOne != this.Neq[0])
                    {
                        this._two = this.Neq[0];
                    }
                }

                return this._two;
            }
        }

        [XmlIgnore]
        public override OperatorType OperatorName
        {
            get { return OperatorType.And; }
        }

        [XmlElement]
        public CamlXmlAnd And { get; set; }

        [XmlElement]
        public CamlXmlOr Or { get; set; }

        [XmlElement]
        public List<CamlXmlBeginsWith> BeginsWith { get; set; }

        [XmlElement]
        public List<CamlXmlContains> Contains { get; set; }

        [XmlElement]
        public List<CamlXmlDateRangesOverlap> DateRangesOverlap { get; set; }

        [XmlElement]
        public List<CamlXmlEq> Eq { get; set; }

        [XmlElement]
        public List<CamlXmlGeq> Geq { get; set; }

        [XmlElement]
        public List<CamlXmlGt> Gt { get; set; }

        [XmlElement]
        public List<CamlXmlIn> In { get; set; }

        [XmlElement]
        public List<CamlXmlIncludes> Includes { get; set; }

        [XmlElement]
        public List<CamlXmlIsNotNull> IsNotNull { get; set; }

        [XmlElement]
        public List<CamlXmlIsNull> IsNull { get; set; }

        [XmlElement]
        public List<CamlXmlLeq> Leq { get; set; }

        [XmlElement]
        public List<CamlXmlLt> Lt { get; set; }

        [XmlElement]
        public List<CamlXmlMembership> Membership { get; set; }

        [XmlElement]
        public List<CamlXmlNeq> Neq { get; set; }

        [XmlElement]
        public List<CamlXmlNotIncludes> NotIncludes { get; set; }
    }
}