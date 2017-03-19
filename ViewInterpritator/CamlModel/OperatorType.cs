namespace Navicon.SP.Components.SqlCache.ViewInterpritator.CamlModel
{
    public enum OperatorType
    {
        Invalid = 0,
        Where,
        And,
        Or,
        Membership,
        BeginsWith,
        Contains,
        DateRangesOverlap,
        Eq,
        Geq,
        Gt,
        In,
        Includes,
        IsNotNull,
        IsNull,
        Leq,
        Lt,
        Neq,
        NotIncludes
    }
}