namespace Navicon.SP.Components.SqlCache.ViewInterpritator.CamlModel
{
    using System;

    public interface ILogicalOperators
    {
        ILogicalOperators LogicalOperatorOne { get; }
        ILogicalOperators LogicalOperatorTwo { get; }
        OperatorType OperatorName { get; }
        Type GetType();
    }
}