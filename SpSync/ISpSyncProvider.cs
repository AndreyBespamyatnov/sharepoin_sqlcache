namespace Navicon.SP.Components.SqlCache.SpSync
{
    using System;
    using System.Collections.Generic;
    using System.Data;

    using Microsoft.SharePoint;

    using Navicon.SP.Components.BuilderForm.Interfaces.Controllers;
    using Navicon.SP.Components.GridDataEdit.XML;

    internal interface ISpSyncProvider : IDisposable
    {
        bool CreateDataBaseTable(SPSite site, string tableName, SPFieldCollection fields);
        bool UpdateDataTableColumns(SPList spList, string fieldXml, bool isDeleted);
        bool Synchronize(SPListItem spListItem, SyncActionType syncActionType);
        AccessType GetAccessRights(SPList spList, string keyValue, int userId, List<int> userGroups = null);
        DataTable GetItems(SPList spList, IEnumerable<ColumnProperties> columnList, string sqlFilter, int userId = -1, List<int> userGroups = null, long itemLimit = 0);
    }
}