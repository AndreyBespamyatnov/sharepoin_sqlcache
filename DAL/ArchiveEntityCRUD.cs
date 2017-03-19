using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using DapperExtensions;
using Microsoft.SharePoint;
using Navicon.SP.Components.SqlCache.Models;

namespace Navicon.SP.Components.SqlCache.DAL
{
    public class ArchiveEntityCRUD : BaseCRUD
    {
        private const string SqlDatetimeFormat = "yyyy-MM-dd HH:mm:ss";

        public ArchiveEntityCRUD(SPSite spSite)
            : base(spSite) {}

        public ArchiveEntityCRUD() {}

        public StatusValuePair<ArchiveEntities> Create( string url,
                                                        string editUrl,
                                                        int createById,
                                                        string listID = null,
                                                        string itemID = null,
                                                        DateTime? created = null,
                                                        bool isArchiveElement = false,
                                                        string documentType = null,
                                                        string description = null,
                                                        string status = null,
                                                        string barode = null
                                                       )
        {
            const string sql = "EXECUTE [dbo].[P_ArchiveEntity_Create] @Url, @EditUrl, @Created, @CreatedBy, @IsArchiveElement, @DocumentType, @Description, @Status, @ListID, @ItemID";

            StatusValuePair<ArchiveEntities> procedureSingle = StoredProcedureSingle<ArchiveEntities>(sql,
                new
                {
                    Url = url,
                    EditUrl = editUrl,
                    Created = created ?? DateTime.Now,
                    CreatedBy = createById,
                    IsArchiveElement = isArchiveElement,
                    DocumentType = documentType,
                    Description = description,
                    Status = status,
                    ListID = listID,
                    ItemID = itemID
                });

            if (!procedureSingle.HasValue || procedureSingle.Value == null)
            {
                return procedureSingle;
            }


            string barcodestring;

            if (barode != null)
            {
                barcodestring = barode;
            }
            else
            {
                barcodestring = BarcodeFabric.Create(procedureSingle.Value.Id);
            }

            ArchiveEntities archiveEntities = procedureSingle.Value;
            archiveEntities.Barcode = barcodestring;
            procedureSingle.Value.FullId = Barcodes.FormatItemID(procedureSingle.Value.ItemID);
            procedureSingle = Update(archiveEntities);

            if (!procedureSingle.HasValue)
            {
                Delete(archiveEntities);
            }

            return procedureSingle;
        }

        public StatusValuePair<ArchiveEntities> GetLastRecord()
        {
            const string sql = "EXECUTE [dbo].[P_ArchiveEntity_GetLastRecord]";
            StatusValuePair<ArchiveEntities> lastRecord = StoredProcedureSingle<ArchiveEntities>(sql);
            return lastRecord;
        }

        public StatusValuePair<List<ArchiveEntities>> GetRelatedArchiveEntities(long archiveEntityId)
        {
            const string sql = "EXECUTE [dbo].[P_ArchiveEntity_GetRelatedByArchiveEntityId] @ArchiveEntityId";
            StatusValuePair<List<ArchiveEntities>> manyToManyEntity = StoredProcedureList<ArchiveEntities>(sql,
                new
                {
                    ArchiveEntityId = archiveEntityId
                });

            return manyToManyEntity;
        }

        public StatusValuePair<List<ArchiveEntities>> GetRelatedDocumentEntities(long documentEntityId)
        {
            const string sql = "EXECUTE [dbo].[P_ArchiveEntity_GetRelatedByDocumentEntityId] @DocumentEntityId";
            StatusValuePair<List<ArchiveEntities>> manyToManyEntity = StoredProcedureList<ArchiveEntities>(sql,
                new
                {
                    DocumentEntityId = documentEntityId
                });

            return manyToManyEntity;
        }

        public bool Exists(string barcode, bool isArchiveElement = true)
        {
            if (string.IsNullOrWhiteSpace(barcode) || barcode == default(int).ToString(CultureInfo.InvariantCulture))
            {
                return false;
            }

            PredicateGroup searchPredicates = new PredicateGroup
            {
                Operator = GroupOperator.And,
                Predicates = new List<IPredicate>(1)
            };
            searchPredicates.Predicates.Add(Predicates.Field<ArchiveEntities>(f => f.Barcode, Operator.Eq, barcode));
            searchPredicates.Predicates.Add(Predicates.Field<ArchiveEntities>(f => f.IsArchiveElement, Operator.Eq, isArchiveElement));

            StatusValuePair<bool?> exists = Exists<ArchiveEntities>(searchPredicates);
            return exists.HasValue && exists.Value.HasValue && exists.Value.Value;
        }

        public StatusValuePair<ArchiveEntities> GetByBarcode(string barcode)
        {
            if (string.IsNullOrWhiteSpace(barcode) || barcode == default(int).ToString(CultureInfo.InvariantCulture))
            {
                return new StatusValuePair<ArchiveEntities>(null, ErrorCode.UnknownError);
            }

            IFieldPredicate p = Predicates.Field<ArchiveEntities>(f => f.Barcode, Operator.Eq, barcode);
            StatusValuePair<ArchiveEntities> result = GetSingle<ArchiveEntities>(p);
            return result;
        }

        public List<ArchiveEntities> GetByBarcodes(List<string> barcodes)
        {
            if (barcodes == null || barcodes.Count == 0 || barcodes.All(string.IsNullOrWhiteSpace))
            {
                return new List<ArchiveEntities>();
            }

            PredicateGroup searchPredicates = new PredicateGroup
            {
                Operator = GroupOperator.Or,
                Predicates = new List<IPredicate>(0)
            };
            foreach (string barcode in barcodes)
            {
                searchPredicates.Predicates.Add(Predicates.Field<ArchiveEntities>(f => f.Barcode, Operator.Eq, barcode));
            }

            StatusValuePair<List<ArchiveEntities>> resultPair = GetList<ArchiveEntities>(searchPredicates);

            if (resultPair.HasValue && resultPair.ErrorCode == ErrorCode.NoError)
            {
                return resultPair.Value;
            }

            return new List<ArchiveEntities>();
        }

        public StatusValuePair<List<ArchiveEntities>> GetListArchiveElements()
        {
            IFieldPredicate p = Predicates.Field<ArchiveEntities>(f => f.IsArchiveElement, Operator.Eq, true);
            StatusValuePair<List<ArchiveEntities>> result = GetList<ArchiveEntities>(p);
            return result;
        }

        public StatusValuePair<ArchiveEntities> UpdateByBarcode(ArchiveEntities archiveItem)
        {
            if (string.IsNullOrWhiteSpace(archiveItem.Barcode) || archiveItem.Barcode == default(int).ToString(CultureInfo.InvariantCulture))
            {
                return new StatusValuePair<ArchiveEntities>(null, ErrorCode.UnknownError);
            }

            IFieldPredicate p = Predicates.Field<ArchiveEntities>(f => f.Barcode, Operator.Eq, archiveItem.Barcode);
            StatusValuePair<ArchiveEntities> result = GetSingle<ArchiveEntities>(p);

            if (!result.HasValue)
            {
                return new ArchiveEntities();
            }

            result.Value.SE = archiveItem.SE;
            result.Value.LP = archiveItem.LP;
            result.Value.RegNumber = archiveItem.RegNumber;
            result.Value.SumPlusVAT = archiveItem.SumPlusVAT;
            result.Value.BPStatus = archiveItem.BPStatus;
            result.Value.Contractor = archiveItem.Contractor;
            result.Value.Currency = archiveItem.Currency;
            result.Value.DocumentDate = archiveItem.DocumentDate;
            result.Value.DocumentKind = archiveItem.DocumentKind;
            result.Value.DocumentNumber = archiveItem.DocumentNumber;
            result.Value.DocumentType = archiveItem.DocumentType;
            result.Value.Description = archiveItem.Description;
            result.Value.Status = archiveItem.Status;
            result.Value.FullId = archiveItem.FullId;
            result.Value.IsUpdated = true;
            if (!string.IsNullOrWhiteSpace(archiveItem.Url))
            {
                result.Value.Url = archiveItem.Url;
            }

            StatusValuePair<ArchiveEntities> updated = Update(result.Value);
            return updated;
        }

        public StatusValuePair<ArchiveEntities> UpdateFolderUrlBarcode(string barcode, SPFolder folder, string editUrl)
        {
            if (string.IsNullOrWhiteSpace(barcode) || barcode == default(int).ToString(CultureInfo.InvariantCulture))
            {
                return new StatusValuePair<ArchiveEntities>(null, ErrorCode.UnknownError);
            }

            IFieldPredicate p = Predicates.Field<ArchiveEntities>(f => f.Barcode, Operator.Eq, barcode);
            StatusValuePair<ArchiveEntities> result = GetSingle<ArchiveEntities>(p);

            if (!result.HasValue)
            {
                return new ArchiveEntities();
            }

            result.Value.Url = folder.Url;
            result.Value.EditUrl = editUrl;
            result.Value.ListID = folder.Item.ParentList.ID.ToString();
            result.Value.ItemID = folder.Item.ID.ToString("D");

            StatusValuePair<ArchiveEntities> updated = Update(result.Value);
            return updated;
        }

        public StatusValuePair<List<ArchiveEntities>> Search(SearchParams searchParams)
        {
            List<string> barcodes = searchParams.Barcodes.ToList();
            bool? isArchiveElement = searchParams.IsArchiveElement;
            List<int> userGroupIds = searchParams.UserGroupIds;
            List<SearchParams.SpFieldsStructHidden> fieldsStruct = searchParams.FieldsStruct;

            // когда всё пусто
            if ((!searchParams.Barcodes.Any() || searchParams.Barcodes.All(string.IsNullOrWhiteSpace)) &&
                !searchParams.IsArchiveElement.HasValue &&
                searchParams.FieldsStruct.Count == 0)
            {
                return new StatusValuePair<List<ArchiveEntities>>(null, ErrorCode.UnknownError);
            }

            StringBuilder sqlQuery = new StringBuilder();
            sqlQuery.AppendFormat(
                "SELECT A.* FROM ArchiveEntities AS A left join ArchiveDocsContentType as AD on A.{0}=AD.{0} WHERE ",
                Constants.BarCodeFieldName);

            // Permissions
            string permissionsFilter = string.Empty;
            if (userGroupIds.Count > 1)
            {
                foreach (int uId in userGroupIds)
                {
                    if (permissionsFilter.EndsWith(")"))
                    {
                        permissionsFilter += " OR ";
                    }

                    permissionsFilter += "([UserGroupId] = " + uId + ")";
                }
            }
            else
            {
                permissionsFilter = "[UserGroupId] = " + userGroupIds.FirstOrDefault();
            }
            sqlQuery.AppendFormat(" A.[{0}] IN (SELECT [{0}] FROM [{1}] WHERE ({2})) ",
                Constants.BarCodeFieldName,
                Constants.PermissionsDataTableName,
                permissionsFilter);
            // End Permissions

            // IsArchiveElement And IsUpdated
            if (isArchiveElement.HasValue)
            {
                sqlQuery.AppendFormat(" AND (A.[IsArchiveElement] = {0}) ", isArchiveElement.Value ? 1 : 0);
            }
            sqlQuery.Append(" AND (A.[IsUpdated] = 1 OR AD.[IsUpdated] = 1) ");
            // End IsArchiveElement And IsUpdated

            // BarCodes
            string barcodeFilter = string.Empty;
            if (barcodes.Count() > 1)
            {
                foreach (string barcode in barcodes)
                {
                    if (barcodeFilter.EndsWith(")"))
                    {
                        barcodeFilter += " OR ";
                    }

                    barcodeFilter += "(A.[" + Constants.BarCodeFieldName + "] = '" + barcode + "')";
                }
            }
            else if (barcodes.Any())
            {
                barcodeFilter = "A.[" + Constants.BarCodeFieldName + "] = '" + barcodes.FirstOrDefault() + "'";
            }
            // End BarCodes

            // Field Filters
            string fieldsFilter = string.Empty;
            if (!barcodes.Any())
            {
                if (fieldsStruct.Count() > 1)
                {
                    foreach (SearchParams.SpFieldsStructHidden fieldFilter in fieldsStruct)
                    {
                        string buildFieldsFilter = BuildFieldsFilter(fieldFilter);
                        if (string.IsNullOrWhiteSpace(buildFieldsFilter))
                        {
                            continue;
                        }

                        if (fieldsFilter.EndsWith(")"))
                        {
                            fieldsFilter += " AND ";
                        }

                        fieldsFilter += buildFieldsFilter;
                    }
                }
                else if (fieldsStruct.Any())
                {
                    SearchParams.SpFieldsStructHidden spFieldsStructHidden = fieldsStruct.First();
                    if (!string.IsNullOrWhiteSpace(spFieldsStructHidden.InternalName) &&
                        !string.IsNullOrWhiteSpace(spFieldsStructHidden.Value))
                    {
                        fieldsFilter = BuildFieldsFilter(spFieldsStructHidden);
                    }
                }
            }
            // End Field Filters

            if (!string.IsNullOrWhiteSpace(barcodeFilter) && !string.IsNullOrWhiteSpace(fieldsFilter))
            {
                sqlQuery.AppendFormat(" AND ({0} OR {1}) ", barcodeFilter, fieldsFilter);
            }
            else if (!string.IsNullOrWhiteSpace(barcodeFilter))
            {
                sqlQuery.AppendFormat(" AND ({0}) ", barcodeFilter);
            }
            else if (!string.IsNullOrWhiteSpace(fieldsFilter))
            {
                sqlQuery.AppendFormat(" AND ({0})", fieldsFilter);
            }

            string sql = sqlQuery.ToString();

            StatusValuePair<List<ArchiveEntities>> entitieses = StoredProcedureList<ArchiveEntities>(sql);
            //entitieses.Value  = entitieses.Value.Select(archiveItem=>archiveItem);
            List<ArchiveEntities> global = entitieses.Value.Select(archiveEntity =>
                {
                    archiveEntity.Barcode = Barcodes.FormatBarcode(archiveEntity.Barcode);
                    return archiveEntity;
                }).ToList();

            if (entitieses.HasValue && entitieses.ErrorCode == ErrorCode.NoError)
            {
                return entitieses.Value;
            }

            return new StatusValuePair<List<ArchiveEntities>>(null, ErrorCode.UnknownError);
        }

        private static string BuildFieldsFilter(SearchParams.SpFieldsStructHidden spFieldsStructHidden)
        {
            if (string.IsNullOrWhiteSpace(spFieldsStructHidden.Value))
            {
                return string.Empty;
            }

            string fieldsFilter;

            switch (spFieldsStructHidden.FieldType)
            {
                case SPFieldType.DateTime:
                    string[] dateTimes = spFieldsStructHidden.Value.Split(new[] {'#'}, StringSplitOptions.None);

                    DateTime? dateTimeFrom = null;
                    DateTime? dateTimeTo = null;

                    CultureInfo ruRU = new CultureInfo("ru-RU");

                    if (!string.IsNullOrWhiteSpace(dateTimes[0]))
                    {
                        DateTime tFrom;
                        if (DateTime.TryParseExact(dateTimes[0], "dd.MM.yyyy H:mm", ruRU, DateTimeStyles.None, out tFrom))
                        {
                            dateTimeFrom = tFrom;
                        }
                        else if (DateTime.TryParseExact(dateTimes[0], "dd.MM.yyyy", ruRU, DateTimeStyles.None, out tFrom))
                        {
                            dateTimeFrom = new DateTime(tFrom.Year, tFrom.Month, tFrom.Day, 0, 0, 1);
                        }
                    }
                    if (!string.IsNullOrWhiteSpace(dateTimes[1]))
                    {
                        DateTime tTo;
                        if (DateTime.TryParseExact(dateTimes[1], "dd.MM.yyyy H:mm", ruRU, DateTimeStyles.None, out tTo))
                        {
                            dateTimeTo = tTo;
                        }
                        else if (DateTime.TryParseExact(dateTimes[1], "dd.MM.yyyy", ruRU, DateTimeStyles.None, out tTo))
                        {
                            dateTimeTo = new DateTime(tTo.Year, tTo.Month, tTo.Day, 23, 59, 59);
                        }
                    }

                    if (!dateTimeFrom.HasValue && !dateTimeTo.HasValue)
                    {
                        return string.Empty;
                    }

                    if (dateTimeFrom.HasValue && dateTimeTo.HasValue)
                    {
                        fieldsFilter = string.Format("(AD.[{0}] >= '{1}' AND AD.[{0}] <= '{2}')",
                            spFieldsStructHidden.InternalName, dateTimeFrom.Value.ToString(SqlDatetimeFormat), dateTimeTo.Value.ToString(SqlDatetimeFormat));
                    }
                    else if (dateTimeFrom.HasValue)
                    {
                        fieldsFilter = string.Format("(AD.[{0}] >= '{1}')", spFieldsStructHidden.InternalName, dateTimeFrom.Value.ToString(SqlDatetimeFormat));
                    }
                    else
                    {
                        fieldsFilter = string.Format("(AD.[{0}] <= '{1}')", spFieldsStructHidden.InternalName, dateTimeTo.Value.ToString(SqlDatetimeFormat));
                    }
                    break;
                default:
                    fieldsFilter = "(AD.[" + spFieldsStructHidden.InternalName + "] like '%" + spFieldsStructHidden.Value + "%')";
                    break;
            }

            return fieldsFilter;
        }

        public ErrorCode Delete(ArchiveEntities entity)
        {
            StatusValuePair<ArchiveEntities> result = Delete<ArchiveEntities>(entity);
            return result.ErrorCode;
        }

        public ErrorCode CreatePermissions(IEnumerable<Permissions> permissions)
        {
            ErrorCode errorCode = ErrorCode.NoError;
            foreach (Permissions permission in permissions)
            {
                PredicateGroup searchPredicates = new PredicateGroup
                {
                    Operator = GroupOperator.And,
                    Predicates = new List<IPredicate>(1)
                };
                searchPredicates.Predicates.Add(Predicates.Field<Permissions>(f => f.Barcode, Operator.Eq, permission.Barcode));
                searchPredicates.Predicates.Add(Predicates.Field<Permissions>(f => f.UserGroupId, Operator.Eq, permission.UserGroupId));

                StatusValuePair<bool?> exists = Exists<Permissions>(searchPredicates);
                if (exists.HasValue && exists.Value.HasValue && !exists.Value.Value)
                {
                    StatusValuePair<Permissions> toCreateResult = Create(permission);
                    errorCode = toCreateResult.ErrorCode;
                }
                else
                {
                    StatusValuePair<Permissions> existingPermissions = GetSingle<Permissions>(searchPredicates);
                    permission.Id = existingPermissions.Value.Id;
                    StatusValuePair<Permissions> toUpdateResult = Update(permission);
                    errorCode = toUpdateResult.ErrorCode;
                }
            }
            return errorCode;
        }
    }
}