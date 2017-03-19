namespace Navicon.SP.Components.SqlCache.ViewInterpritator.CamlModel
{
    using System;
    using System.Xml.Serialization;

    public abstract class LogicalOperatorsBase : ILogicalOperators
    {
        [XmlIgnore]
        public virtual ILogicalOperators LogicalOperatorOne
        {
            get { return null; }
        }

        [XmlIgnore]
        public virtual ILogicalOperators LogicalOperatorTwo
        {
            get { return null; }
        }

        [XmlIgnore]
        public virtual OperatorType OperatorName
        {
            get { return OperatorType.Invalid; }
        }

        public new Type GetType()
        {
            Type type = null;

            switch (this.OperatorName)
            {
                case OperatorType.Where:
                    type = typeof (CamlXmlWhere);
                    break;
                case OperatorType.And:
                    type = typeof (CamlXmlAnd);
                    break;
                case OperatorType.Or:
                    type = typeof (CamlXmlOr);
                    break;
                case OperatorType.Membership:
                    type = typeof (CamlXmlMembership);
                    break;
                case OperatorType.BeginsWith:
                    type = typeof (CamlXmlBeginsWith);
                    break;
                case OperatorType.Contains:
                    type = typeof (CamlXmlContains);
                    break;
                case OperatorType.DateRangesOverlap:
                    type = typeof (CamlXmlDateRangesOverlap);
                    break;
                case OperatorType.Eq:
                    type = typeof (CamlXmlEq);
                    break;
                case OperatorType.Geq:
                    type = typeof (CamlXmlGeq);
                    break;
                case OperatorType.Gt:
                    type = typeof (CamlXmlGt);
                    break;
                case OperatorType.In:
                    type = typeof (CamlXmlIn);
                    break;
                case OperatorType.Includes:
                    type = typeof (CamlXmlIncludes);
                    break;
                case OperatorType.IsNotNull:
                    type = typeof (CamlXmlIsNotNull);
                    break;
                case OperatorType.IsNull:
                    type = typeof (CamlXmlIsNull);
                    break;
                case OperatorType.Leq:
                    type = typeof (CamlXmlLeq);
                    break;
                case OperatorType.Lt:
                    type = typeof (CamlXmlLt);
                    break;
                case OperatorType.Neq:
                    type = typeof (CamlXmlNeq);
                    break;
                case OperatorType.NotIncludes:
                    type = typeof (CamlXmlNotIncludes);
                    break;
            }

            return type;
        }
    }
}