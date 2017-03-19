namespace Navicon.SP.Components.SqlCache.SpSync
{
    using System;
    using System.Collections.Generic;

    using Microsoft.SharePoint;

    using Navicon.SP.Common;
    using Navicon.SP.Components.SqlCache.Models;
    using Navicon.SP.SqlCache.Common.Box;

    public class SpSyncAdapter : ISpSyncAdapter
    {
        private const string FolderSeparator = "/";
        private const string FolderSeparatorReplaserChar = "_";
        private const string BigIntType = "bigint";
        private const string NCharType = "nvarchar(255)";
        private const string NVarCharType = "nvarchar(max)";
        private const string FloatType = "float";
        private const string NTextType = "ntext";
        private const string DateTime2Type = "datetime2";
        private const string BitType = "bit";

        public SpSyncAdapter()
        {
            this.TableStruct = new CacheTableStructXml();
        }

        public string KeyFieldName
        {
            get { return Constants.BarCodeFieldName; }
        }

        public IList<string> IncludedFields
        {
            get
            {
                return new[]
                {
                    Fields.IsUpdated
                };
            }
        }

        public CacheTableStructXml TableStruct { get; private set; }

        public string GetDataBaseType(SPFieldType fieldType)
        {
            switch (fieldType)
            {
                case SPFieldType.Integer:
                    return BigIntType;
                case SPFieldType.Number:
                    return FloatType;
                case SPFieldType.Invalid:
                    return NVarCharType;
                case SPFieldType.Text:
                    return NVarCharType;
                case SPFieldType.Note:
                    return NTextType;
                case SPFieldType.DateTime:
                    return DateTime2Type;
                case SPFieldType.Counter:
                    return NVarCharType;
                case SPFieldType.Choice:
                    return NVarCharType;
                case SPFieldType.Lookup:
                    return NVarCharType;
                case SPFieldType.Boolean:
                    return BitType;
                case SPFieldType.Currency:
                    return NVarCharType;
                case SPFieldType.URL:
                    return NVarCharType;
                case SPFieldType.Computed:
                    return NVarCharType;
                case SPFieldType.Threading:
                    return NVarCharType;
                case SPFieldType.Guid:
                    return NVarCharType;
                case SPFieldType.MultiChoice:
                    return NVarCharType;
                case SPFieldType.GridChoice:
                    return NVarCharType;
                case SPFieldType.Calculated:
                    return NVarCharType;
                case SPFieldType.File:
                    return NVarCharType;
                case SPFieldType.Attachments:
                    return NVarCharType;
                case SPFieldType.User:
                    return NVarCharType;
                case SPFieldType.Recurrence:
                    return NVarCharType;
                case SPFieldType.CrossProjectLink:
                    return NVarCharType;
                case SPFieldType.ModStat:
                    return NVarCharType;
                case SPFieldType.Error:
                    return NCharType;
                case SPFieldType.ContentTypeId:
                    return NVarCharType;
                case SPFieldType.PageSeparator:
                    return NVarCharType;
                case SPFieldType.ThreadIndex:
                    return NVarCharType;
                case SPFieldType.WorkflowStatus:
                    return NVarCharType;
                case SPFieldType.AllDayEvent:
                    return NVarCharType;
                case SPFieldType.WorkflowEventType:
                    return NVarCharType;
                case SPFieldType.Geolocation:
                    return NVarCharType;
                case SPFieldType.OutcomeChoice:
                    return NVarCharType;
                case SPFieldType.MaxItems:
                    return NVarCharType;
                default:
                    return NVarCharType;
            }
        }

        public Type GetTypeByDataBaseType(string dataBaseType, bool decimalType = false)
        {
            if (decimalType && string.Equals(dataBaseType, FloatType, StringComparison.InvariantCultureIgnoreCase))
            {
                return typeof(double);
            }
            if (string.Equals(dataBaseType, BigIntType, StringComparison.InvariantCultureIgnoreCase) ||
                string.Equals(dataBaseType, FloatType, StringComparison.InvariantCultureIgnoreCase))
            {
                return typeof (long);
            }
            if (string.Equals(dataBaseType, NCharType, StringComparison.InvariantCultureIgnoreCase) ||
                string.Equals(dataBaseType, NVarCharType, StringComparison.InvariantCultureIgnoreCase) ||
                string.Equals(dataBaseType, NTextType, StringComparison.InvariantCultureIgnoreCase))
            {
                return typeof (string);
            }
            if (string.Equals(dataBaseType, DateTime2Type, StringComparison.InvariantCultureIgnoreCase))
            {
                return typeof (DateTime);
            }
            if (string.Equals(dataBaseType, BitType, StringComparison.InvariantCultureIgnoreCase))
            {
                return typeof (bool);
            }

            return null;
        }

        public string BuildTableName(string listServerRelativeUrl)
        {
            string tableName = listServerRelativeUrl.Replace(FolderSeparator, FolderSeparatorReplaserChar);
            return tableName;
        }

        public Columns AsColumn(SPListItem spListItem, SPField field)
        {
            string columnType = this.GetDataBaseType(field.Type);
            object fValue = null;
            try
            {
                fValue = spListItem[field.InternalName];
            }
            catch {}

            string columnValue = string.Empty;

            if (field.InternalName == this.KeyFieldName)
            {
                columnValue = fValue == null ? string.Empty : fValue.ToString();
            }
            else if (field is SPFieldLookup)
            {
                columnValue = fValue == null ? string.Empty : fValue.ToString();
            }
            else if (field is SPFieldDateTime)
            {
                try
                {
                    if (fValue != null)
                    {
                        const string format = "yyyy-MM-dd HH:mm:ss:fff";
                        DateTime dateTime = (DateTime) fValue;
                        columnValue = dateTime.ToString(format);
                    }
                }
                catch (Exception ex)
                {
                    Logger.WriteError(ex.Message, ex);
                    columnValue = string.Empty;
                }
            }
            else
            {
                if (fValue != null)
                {
                    columnValue = field.GetFieldValueAsText(fValue) ?? fValue.ToString();
                }
            }

            if (columnValue.ToUpperInvariant() == "NO" || columnValue.ToUpperInvariant() == "НЕТ")
            {
                columnValue = "0";
            }
            if (columnValue.ToUpperInvariant() == "YES" || columnValue.ToUpperInvariant() == "ДА")
            {
                columnValue = "1";
            }

            if (columnType == FloatType && field.InternalName != this.KeyFieldName)
            {
                columnValue = columnValue.Trim().Replace(",", ".").Replace(((char)0xA0).ToString(), "");
            }

            Columns item = new Columns
            {
                ColumnName = field.Id == SPBuiltInFieldId.ID ? Constants.ColumnSpPrefix + field.InternalName : field.InternalName,
                ColumnType = columnType,
                ColumnValue = columnValue
            };

            return item;
        }
    }
}