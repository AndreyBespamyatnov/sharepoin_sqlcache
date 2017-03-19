namespace Navicon.SP.Components.SqlCache.ViewInterpritator
{
    using System.Collections.Generic;
    using System.Web.UI;

    public class PersonalViewsStructs : List<PersonalViewsStruct>, IHierarchicalEnumerable
    {
        public IHierarchyData GetHierarchyData(object enumeratedItem)
        {
            // Believe your eyes! Nothing much TODO here really
            return enumeratedItem as IHierarchyData;
        }
    }
}