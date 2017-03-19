namespace Navicon.SP.Components.SqlCache.SpSync
{
    using System;
    using System.Collections.Generic;

    using Microsoft.SharePoint;

    using Navicon.SP.Components.SqlCache.Models;

    internal interface ISpSyncAdapter
    {
        string KeyFieldName { get; }
        CacheTableStructXml TableStruct { get; }
        IList<string> IncludedFields { get; }
        Type GetTypeByDataBaseType(string dataBaseType, bool decimalType = false);
        string GetDataBaseType(SPFieldType fieldType);
        string BuildTableName(string listServerRelativeUrl);
        Columns AsColumn(SPListItem spListItem, SPField field);
    }
}