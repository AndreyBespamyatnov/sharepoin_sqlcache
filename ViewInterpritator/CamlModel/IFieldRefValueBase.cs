namespace Navicon.SP.Components.SqlCache.ViewInterpritator.CamlModel
{
    public interface IFieldRefValueBase
    {
        CamlXmlFieldRef FieldRef { get; }
        CamlXmlValue Value { get; }
        string SqlOperator { get; }
    }
}