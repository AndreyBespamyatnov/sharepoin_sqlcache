namespace Navicon.SP.Components.SqlCache.SpSync
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Xml;

    using Microsoft.SharePoint;

    using Navicon.SP.Common;
    using Navicon.SP.Common.Extensions;
    using Navicon.SP.Components.BuilderForm.Interfaces.Controllers;
    using Navicon.SP.Components.GridDataEdit.XML;
    using Navicon.SP.Components.SqlCache.DAL;
    using Navicon.SP.Components.SqlCache.Models;
    using Navicon.SP.Components.SqlCache.ViewInterpritator;

    public sealed class SpSyncProvider : ISpSyncProvider
    {
        private readonly SyncType _syncType;

        public SpSyncProvider(SyncType syncType)
        {
            this._syncType = syncType;
            this.Adapter = new SpSyncAdapter();
        }

        private ISpSyncAdapter Adapter { get; set; }

        public void Dispose()
        {
            this.Adapter = null;
        }

        public bool CreateDataBaseTable(SPSite site, string tableName, SPFieldCollection fields)
        {
            string dataBaseTableName = this.Adapter.BuildTableName(tableName);
            CacheTableStructXml cacheTableStructXml = new CacheTableStructXml {TableName = dataBaseTableName};

            foreach (SPField field in fields)
            {
                if (field.Hidden && field.Id != SPBuiltInFieldId.ID &&
                    this.Adapter.IncludedFields.All(f => f != field.InternalName))
                {
                    continue;
                }

                string columnType = this.Adapter.GetDataBaseType(field.Type);
                Columns column = new Columns
                {
                    ColumnName =
                        field.Id == SPBuiltInFieldId.ID
                            ? Constants.ColumnSpPrefix + field.InternalName
                            : field.InternalName,
                    ColumnType = columnType
                };
                cacheTableStructXml.Columns.Add(column);
            }

            ArchiveCacheCRUD syncCrud = new ArchiveCacheCRUD(site);
            ErrorCode result = syncCrud.CreateOrUpdateTable(cacheTableStructXml);
            return result == ErrorCode.NoError;
        }

        public bool UpdateDataTableColumns(SPList spList, string fieldXml, bool isDeleted)
        {
            string tableNameSetting = SettingsProvider.Instance(spList.ParentWeb).TableName(spList);
            string tableName = this.Adapter.BuildTableName(tableNameSetting);
            if (string.IsNullOrWhiteSpace(tableName))
            {
                //Logger.WriteError("tableName is null, feature activaiting?", "UpdateDataTableColumns(SPList spList, string fieldXml, bool isDeleted)");
                return true;
            }

            CacheTableStructXml cacheTableStructXml = new CacheTableStructXml {TableName = tableName};

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(fieldXml);
            const string fXmlTypeName = "Type";
            const string xStaticNameName = "StaticName";

            SPFieldType spFieldType;
            XmlElement root = doc.DocumentElement;
            Enum.TryParse(root.Attributes[fXmlTypeName].Value, true, out spFieldType);
            string internalName = root.Attributes[xStaticNameName].Value;

            string columnType = this.Adapter.GetDataBaseType(spFieldType);
            Columns column = new Columns
            {
                ColumnName = internalName,
                ColumnType = columnType,
                ColumnIsDeleted = isDeleted
            };
            cacheTableStructXml.Columns.Add(column);

            ArchiveCacheCRUD syncCrud = new ArchiveCacheCRUD(spList.ParentWeb.Site);
            ErrorCode result = syncCrud.CreateOrUpdateTable(cacheTableStructXml);
            return result == ErrorCode.NoError;
        }

        public bool Synchronize(SPListItem spListItem, SyncActionType syncActionType)
        {
            try
            {
                ArchiveCacheCRUD syncCrud = new ArchiveCacheCRUD(spListItem.ParentList.ParentWeb.Site);
                switch (this._syncType)
                {
                    case SyncType.SpList:
                        this.Adapter.TableStruct.TableName =
                            this.Adapter.BuildTableName(spListItem.ParentList.RootFolder.ServerRelativeUrl);
                        break;
                    case SyncType.SpContentType:
                        string tableNameSetting =
                            SettingsProvider.Instance(spListItem.ParentList.ParentWeb).TableName(spListItem.ParentList);
                        this.Adapter.TableStruct.TableName = this.Adapter.BuildTableName(tableNameSetting);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                switch (syncActionType)
                {
                    case SyncActionType.Add:
                    case SyncActionType.Update:
                        List<Columns> columns = spListItem
                            .ParentList.Fields.Cast<SPField>()
                            .Where(f => !f.Hidden || this.Adapter.IncludedFields.Contains(f.InternalName))
                            .Select(field => this.Adapter.AsColumn(spListItem, field))
                            .Where(c => c != null)
                            .ToList();
                        string editUrl = spListItem.ParentList.DefaultEditFormUrl + "?ID=" + spListItem.ID + "&RootFolder=/" + (spListItem.Folder == null ? string.Empty : spListItem.Folder.Url);
                        columns.Add(new Columns {ColumnName = "EditUrl", ColumnValue = editUrl});
                        columns.Add(new Columns {ColumnName = "ItemUrl", ColumnValue = spListItem.Url});
                        Columns keyFieldColumn = columns.FirstOrDefault(c => c.ColumnName == this.Adapter.KeyFieldName);
                        if (columns.Any(x => x.ColumnName == "Versions"))
                        {
                            columns = columns.Where(x => x.ColumnName != "Versions").ToList();
                        }
                        if (keyFieldColumn == null)
                        {
                            columns.Add(new Columns {ColumnName = this.Adapter.KeyFieldName, ColumnValue = spListItem.ID.ToString()});
                        }
                        else if(keyFieldColumn.ColumnValue == string.Empty)
                        {
                            keyFieldColumn.ColumnValue = spListItem.ID.ToString();
                        }
                        this.Adapter.TableStruct.Columns = columns;
                        break;
                    case SyncActionType.Delete:
                        SPField spField = spListItem
                            .ParentList.Fields.Cast<SPField>()
                            .FirstOrDefault(f => f.InternalName == this.Adapter.KeyFieldName);
                        Columns keyColumn = this.Adapter.AsColumn(spListItem, spField);
                        this.Adapter.TableStruct.Columns = new List<Columns> {keyColumn};
                        this.Adapter.TableStruct.RowIsDeleted = true;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("syncActionType");
                }

                ErrorCode result = syncCrud.CreateOrUpdateValues(this.Adapter.TableStruct);
                ErrorCode permissionResult = this.WritePermissions(spListItem, syncCrud);

                return result == ErrorCode.NoError && permissionResult == ErrorCode.NoError;
            }
            catch (Exception ex)
            {
                Logger.WriteError(ex.Message, "SpSyncProvider.Synchronize()");
                return false;
            }
        }

        public AccessType GetAccessRights(SPList spList, string keyValue, int userId, List<int> userGroups = null)
        {
            if (!SettingsProvider.Instance(spList.ParentWeb).IsCacheDb(spList))
            {
                return AccessType.Write;
            }

            string tableNameSetting = SettingsProvider.Instance(spList.ParentWeb).TableName(spList);
            string tableName = this.Adapter.BuildTableName(tableNameSetting);
            if (string.IsNullOrWhiteSpace(tableName))
            {
                return AccessType.NotAccessible;
            }

            ArchiveCacheCRUD syncCrud = new ArchiveCacheCRUD(spList.ParentWeb.Site);
            AccessType accessRights = syncCrud.GetAccessRights(userId, userGroups ?? new List<int>(), tableName,
                keyValue);
            return accessRights;
        }

        public DataTable GetItems(SPList spList,
                                  IEnumerable<ColumnProperties> columnList,
                                  string sqlFilter,
                                  int userId = -1,
                                  List<int> userGroups = null,
                                  long itemLimit = 0)
        {
            DataTable tableData = new DataTable();
            try
            {
                // а эта колонка нужна для работы клиентского контеста и рибона
                {
                    string columnType = this.Adapter.GetDataBaseType(SPFieldType.Text);
                    Type baseType = this.Adapter.GetTypeByDataBaseType(columnType);
                    Columns column = new Columns
                    {
                        ColumnName = Constants.ColumnSpPrefix + "ID",
                        ColumnType = columnType
                    };
                    this.Adapter.TableStruct.Columns.Add(column);
                    tableData.Columns.Add(Constants.ColumnSpPrefix + "ID", baseType);
                }

                foreach (
                    ColumnProperties columnProperties in
                        columnList.Where(c => c.FieldName != Constants.ColumnSpPrefix + "ID" && c.FieldName != "ID"))
                {
                    SPFieldType fieldType;
                    Enum.TryParse(columnProperties.Type, true, out fieldType);
                    
                    string columnType = this.Adapter.GetDataBaseType(fieldType);
                    bool decimalType = false;
                    
                    if (fieldType == SPFieldType.Number)
                    {
                        SPFieldNumber filed = spList.Fields.GetField(columnProperties.FieldName) as SPFieldNumber;
                        if (filed != null && 
                            filed.DisplayFormat != SPNumberFormatTypes.Automatic &&
                            filed.DisplayFormat != SPNumberFormatTypes.NoDecimal)
                        {
                            decimalType = true;
                        }
                    }

                    Type baseType = this.Adapter.GetTypeByDataBaseType(columnType, decimalType);
                    Columns column = new Columns
                    {
                        ColumnName = columnProperties.FieldName,
                        ColumnType = columnType
                    };

                    if (this.Adapter.TableStruct.Columns.Any(c => c.ColumnName == column.ColumnName))
                    {
                        continue;
                    }

                    this.Adapter.TableStruct.Columns.Add(column);
                    tableData.Columns.Add(columnProperties.FieldName, baseType);
                }

                string tableNameSetting;
                switch (this._syncType)
                {
                    case SyncType.SpList:
                        tableNameSetting = this.Adapter.BuildTableName(spList.RootFolder.ServerRelativeUrl);
                        break;
                    case SyncType.SpContentType:
                        tableNameSetting = SettingsProvider.Instance(spList.ParentWeb).TableName(spList);
                        this.Adapter.TableStruct.TableName = this.Adapter.BuildTableName(tableNameSetting);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                string tableName = this.Adapter.BuildTableName(tableNameSetting);
                this.Adapter.TableStruct.TableName = tableName;

                ArchiveCacheCRUD syncCrud = new ArchiveCacheCRUD(spList.ParentWeb.Site);
                IEnumerable<dynamic> cacheTableData = syncCrud.GetCacheTable(userGroups ?? new List<int>(), userId, this.Adapter.TableStruct,
                    sqlFilter, itemLimit, spList.ParentWeb);
                if (cacheTableData == null)
                {
                    return tableData;
                }

                foreach (IDictionary<string, object> item in cacheTableData)
                {
                    DataRow dataRow = tableData.NewRow();
                    foreach (DataColumn column in tableData.Columns)
                    {
                        string columnName = column.ColumnName == "Id"
                            ? column.ColumnName.ToUpperInvariant()
                            : column.ColumnName;
                        object val = item[columnName];
                        if (val != null)
                        {
                            dataRow[column.ColumnName] = val;
                        }
                    }
                    tableData.Rows.Add(dataRow);
                }
            }
            catch (Exception ex)
            {
                Logger.WriteError(ex.Message, ex);
            }

            return tableData;
        }

        private ErrorCode WritePermissions(SPListItem spListItem, ArchiveCacheCRUD syncCrud)
        {
            Columns keyColumn = this.Adapter.TableStruct.Columns.FirstOrDefault(c => c.ColumnName == this.Adapter.KeyFieldName);
            if (keyColumn == null)
            {
                return ErrorCode.UnknownError;
            }

            string keyFieldValue = keyColumn.ColumnValue;

            string readPermissionsValue = spListItem.TryGetFieldValueAsText(Constants.ReadPermisionFieldName);
            string writePermissionsValue = spListItem.TryGetFieldValueAsText(Constants.WritePermisionFieldName);

            const string separator = " ";
            string[] separatorArray = { separator, ((char)0xA0).ToString() };
            string[] readPermissions = readPermissionsValue.Split(separatorArray, StringSplitOptions.RemoveEmptyEntries);
            string[] writePermissions = writePermissionsValue.Split(separatorArray, StringSplitOptions.RemoveEmptyEntries);

            List<Permissions> permissions = new List<Permissions>();
            int userId = 0;
            permissions.AddRange(from userIdString in readPermissions
                                 where int.TryParse(userIdString, out userId)
                                 select new Permissions
                                 {
                                     Barcode = keyFieldValue, 
                                     ItemUniqueID = Convert.ToString(spListItem.UniqueId),
                                     UserGroupId = userId
                                 });

            permissions.AddRange(from userIdString in writePermissions
                                 where int.TryParse(userIdString, out userId)
                                 select new Permissions
                                 {
                                     Barcode = keyFieldValue,
                                     ItemUniqueID = Convert.ToString(spListItem.UniqueId),
                                     UserGroupId = userId,
                                     CanWrite = true
                                 });

            ErrorCode deletePermissionsResult = syncCrud.DeleteExistingPermissions(spListItem);
            ErrorCode permissionResult = syncCrud.CreateOrUpdatePermissions(permissions);
            return permissionResult;
        }
    }
}