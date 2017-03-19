using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.SharePoint;
using Navicon.SP.Common;
using Navicon.SP.Common.Extensions;
using DisabledItemEventsScope = Navicon.SP.Common.DisabledItemEventsScope;
using System.Text.RegularExpressions;
using Navicon.SP.Components.SqlCache.DAL;
using Navicon.SP.Components.SqlCache.Models;

namespace Navicon.SP.Components.SqlCache
{
    public class ArchiveProvider
    {
        private readonly ArchiveEntityCRUD _archiveEntityCrud;
        private readonly ManyToManyEntityCRUD _manyToManyEntityCrud;
        public readonly SPList SpList;

        private SPSite _spSite = null;
        public ArchiveProvider()
        {
            _spSite = SPContext.Current.Site;
            SpList = GetListFromSettings();
            _archiveEntityCrud = new ArchiveEntityCRUD();
            _manyToManyEntityCrud = new ManyToManyEntityCRUD();
        }

        public ArchiveProvider(SPSite spSite)
        {
            _spSite = spSite;
            SpList = GetListFromSettings(spSite);
            _archiveEntityCrud = new ArchiveEntityCRUD(spSite);
            _manyToManyEntityCrud = new ManyToManyEntityCRUD(spSite);
        }

        /// <summary>
        ///     TODO GET FROM REAL WebSettings
        /// </summary>
        /// <returns></returns>
        private SPList GetListFromSettings(SPSite spSite = null)
        {
            return null;
            //SPList spList = spSite != null ? ListArchiveDocs.GetInstance(spSite.RootWeb) : ListArchiveDocs.GetInstance();
            //return spList;
        }

        public string CreateDocumentEntity(string itemUrl, string editUrl, int userId = 0)
        {
            if (String.IsNullOrWhiteSpace(itemUrl))
            {
                return null;
            }

            if (userId == 0)
            {
                userId = SPContext.Current.Web.CurrentUser.ID;
            }
            SPFolder spFolder = SpList.ParentWeb.GetFolder(itemUrl);
            DateTime created = DateTime.Now;


            long barcodelong = 0;
            if (spFolder.Item[Constants.BarCodeFieldName] != null)
            {
                var barcodeValue = spFolder.Item[Constants.BarCodeFieldName].ToString();
                BarcodeFabric.TryParse(barcodeValue, out barcodelong);
            }

            var barval = barcodelong > 0 ? barcodelong.ToString() : null;

            StatusValuePair<ArchiveEntities> archiveElement = _archiveEntityCrud.Create(itemUrl, editUrl, userId, spFolder.Item.ParentList.ID.ToString(), spFolder.Item.ID.ToString("D"), created, barode: barval);

            if (!archiveElement.HasValue)
            {
                return null;
            }

            string barcode = archiveElement.Value.Barcode;

            // записываем права для автора

            List<Permissions> permissions = new List<Permissions>(1);
            string author = spFolder.Item.TryGetFieldValue(SPBuiltInFieldId.Author, string.Empty);
            if (!string.IsNullOrWhiteSpace(author))
            {
                SPFieldUserValue authorUserValue = new SPFieldUserValue(SpList.ParentWeb, author);
                Permissions authorPermission = new Permissions
                {
                    Barcode = barcode,
                    CanWrite = true,
                    UserGroupId = authorUserValue.User.ID
                };
                permissions.Add(authorPermission);
            }

            CreateOrUpdatePermissions(permissions);

            return barcode;
        }

        public string UpdateArchiveElement(ArchiveEntities archiveItem,  IEnumerable<Permissions> permissions)
        {
            bool existElement = _archiveEntityCrud.Exists(archiveItem.Barcode);
            if (!existElement)
            {
                return null;
            }

            StatusValuePair<ArchiveEntities> archiveElement = _archiveEntityCrud.UpdateByBarcode(archiveItem);
            string result = archiveElement.HasValue ? archiveElement.Value.Barcode : null;

            if (string.IsNullOrWhiteSpace(result))
            {
                return result;
            }

            bool permissionsResult = CreateOrUpdatePermissions(permissions);
            if (!permissionsResult)
            {
                Logger.ShowErrorOnPage("Не удалось создать уникальные права для этого элемента.",
                    "ArchiveElement.CreateOrUpdateDbRecord(SPItemEventProperties properties)");
            }

            return result;
        }

        public bool Exists(string barcode)
        {
            bool existElement = _archiveEntityCrud.Exists(barcode);
            return existElement;
        }

        public bool ExistsAnyType(string barcode)
        {
            bool existElement = _archiveEntityCrud.Exists(barcode);
            bool existElement2 = _archiveEntityCrud.Exists(barcode, false);
            return existElement || existElement2;
        }

        public ArchiveEntities CreateArchiveElement(string documentBarcode, string documentType = null, string description = null, string status = null)
        {
            int userId = SPContext.Current.Web.CurrentUser.ID;
            DateTime created = DateTime.Now;

            // создаём новую запись в БД, генерируем название папки из Id новой записи
            StatusValuePair<ArchiveEntities> archiveElement = _archiveEntityCrud.Create(string.Empty, string.Empty, userId, null, null, created, true, documentType, description, status);
            if (!archiveElement.HasValue)
            {
                return null;
            }

            // создаём папку с именем = id новой записи
            SPContentType contentType = SpList.ParentWeb.ContentTypes.Cast<SPContentType>().FirstOrDefault(c => c.Name == Navicon.SP.SqlCache.Common.Box.WebRoot.ContentTypes.DocumentElement);
            if (contentType == null)
            {
                return null;
            }

            Dictionary<string, object> fieldValues = new Dictionary<string, object>
            {
                {"ContentTypeId", contentType.Id},
                {Constants.BarCodeFieldName, archiveElement.Value.Barcode}
            };

            foreach (SPField field in contentType.Fields)
            {
                if (!string.IsNullOrWhiteSpace(field.DefaultValue))
                {
                    fieldValues.Add(field.InternalName, field.DefaultValue);
                }
            }

            SPFolder folder = null;
            SPUserToken currentUserToken = SPContext.Current.Web.CurrentUser.UserToken;
            folder = SpList.CreateSubFolderIfNotExists(SPContext.Current.Web.CurrentUser.ID + "_" + ((DateTime.Now.Ticks - 621355968000000000) / 10000 - 1412440895000), fieldValues: fieldValues, userToken: currentUserToken);
            if (folder == null)
            {
                Logger.ShowErrorOnPage("Не удалось создать элемент архива.",
                    "Navicon.SP.Components.Archive.ArchiveProvider.CreateArchiveElement()");
                return null;
            }

            // обновляем url и данные списка и элемента
            string editUrl = folder.Item.ParentList.DefaultEditFormUrl + "?ID=" + folder.Item.ID + "&RootFolder=/" + (folder.Item.Folder == null ? string.Empty : folder.Item.Folder.Url);
            archiveElement = _archiveEntityCrud.UpdateFolderUrlBarcode(archiveElement.Value.Barcode, folder, editUrl);

            // записываем уровни доступа
            List<Permissions> permissions = PermissionsHelper.GetPermissions(archiveElement.Value.Barcode, folder.Item);
            CreateOrUpdatePermissions(permissions);

            StatusValuePair<ArchiveEntities> docEntity = _archiveEntityCrud.GetByBarcode(documentBarcode);
            if (docEntity.HasValue)
            {
                _manyToManyEntityCrud.Create(docEntity.Value.Id, archiveElement.Value.Id);
                return archiveElement;
            }

            return null;
        }

        public ArchiveEntities CreateArchiveElement(SPFolder folder, string documentType = null, string description = null, string status = null)
        {
            if (folder == null || folder.Item == null || folder.Item[SPBuiltInFieldId.Author] == null)
            {
                return null;
            }

            SPFieldUserValue author = new SPFieldUserValue(folder.Item.Web, folder.Item[SPBuiltInFieldId.Author].ToString());
            int userId = author.User.ID;
            DateTime created = DateTime.Now;

            // создаём новую запись в БД, генерируем название папки из Id новой записи
            string editUrl = folder.Item.ParentList.DefaultEditFormUrl + "?ID=" + folder.Item.ID + "&RootFolder=/" + (folder.Item.Folder == null ? string.Empty : folder.Item.Folder.Url);

            long barcode = 0;
            if(folder.Item[Constants.BarCodeFieldName] != null)
            {
                var barcodeValue = folder.Item[Constants.BarCodeFieldName].ToString();
                BarcodeFabric.TryParse(barcodeValue, out barcode);
            }

            StatusValuePair<ArchiveEntities> archiveElement = _archiveEntityCrud.Create(folder.Url, editUrl, userId, folder.Item.ParentList.ID.ToString(), folder.Item.ID.ToString("D"), created, true, documentType, description, status, barcode > 0 ? barcode.ToString() : null);
            if (!archiveElement.HasValue)
            {
                return null;
            }

            using (DisabledItemEventsScope scope = new DisabledItemEventsScope())
            {
                folder.Item[Constants.BarCodeFieldName] = archiveElement.Value.Barcode;
                folder.Item.Update();
            }

            List<Permissions> permissions = PermissionsHelper.GetPermissions(archiveElement.Value.Barcode, folder.Item);
            CreateOrUpdatePermissions(permissions);
            return archiveElement;
        }

        public void CreateMapOfArchiveElements(IEnumerable<string> archiveElementBarcodes, string documentBarcode)
        {
            foreach (string archiveElementBarcode in archiveElementBarcodes)
            {
                StatusValuePair<ArchiveEntities> archiveElement = _archiveEntityCrud.GetByBarcode(archiveElementBarcode);
                if (!archiveElement.HasValue)
                {
                    continue;
                }

                StatusValuePair<ArchiveEntities> docEntity = _archiveEntityCrud.GetByBarcode(documentBarcode);
                if (!docEntity.HasValue)
                {
                    continue;
                }

                _manyToManyEntityCrud.Create(docEntity.Value.Id, archiveElement.Value.Id);
            }
        }

        public bool CreateOrUpdatePermissions(IEnumerable<Permissions> permissions)
        {
            ErrorCode errorCode = _archiveEntityCrud.CreatePermissions(permissions);
            return errorCode == ErrorCode.NoError;
        }

        public List<ArchiveDataSource> GetByDocumentEntityBarcode(string barcode)
        {
            List<ArchiveDataSource> dataSources = new List<ArchiveDataSource>();

            barcode = Regex.Replace(barcode, @"[^\d]", "");

            StatusValuePair<ArchiveEntities> documentEntity = _archiveEntityCrud.GetByBarcode(barcode);
            if (!documentEntity.HasValue)
            {
                return dataSources;
            }

            StatusValuePair<List<ArchiveEntities>> related = _archiveEntityCrud.GetRelatedDocumentEntities(documentEntity.Value.Id);
            if (!related.HasValue)
            {
                return dataSources;
            }

            dataSources.AddRange(related.Value.Select(ToArchiveDataSource));
            return dataSources;
        }

        public ArchiveEntities GetByBarcode(string barcode)
        {
            barcode = Regex.Replace(barcode, @"[^\d]", "");

            StatusValuePair<ArchiveEntities> documentEntity = _archiveEntityCrud.GetByBarcode(barcode);
            if (!documentEntity.HasValue)
            {
                return null;
            }

            var archiveEntity = documentEntity.Value;
            return archiveEntity;
        }

        public List<ArchiveDataSource> GetByArchiveEntityBarcode(string barcode)
        {
            List<ArchiveDataSource> dataSources = new List<ArchiveDataSource>();

            barcode = Regex.Replace(barcode, @"[^\d]", "");
            
            StatusValuePair<ArchiveEntities> documentEntity = _archiveEntityCrud.GetByBarcode(barcode);
            if (!documentEntity.HasValue)
            {
                return dataSources;
            }

            StatusValuePair<List<ArchiveEntities>> related = _archiveEntityCrud.GetRelatedArchiveEntities(documentEntity.Value.Id);
            if (!related.HasValue)
            {
                return dataSources;
            }

            dataSources.AddRange(related.Value.Select(ToArchiveDataSource));
            return dataSources;
        }

        public List<ArchiveDataSource> GetByArchiveEntityBarcodes(List<string> barcodes)
        {
            List<ArchiveDataSource> dataSources = new List<ArchiveDataSource>();
            foreach (string barcode in barcodes)
            {
                dataSources.AddRange(GetByArchiveEntityBarcode(barcode));
            }
            return dataSources;
        }

        public List<ArchiveDataSource> SearchArchiveElements(SearchParams searchParams)
        {
            // Overide
            searchParams.IsArchiveElement = true;
            List<ArchiveDataSource> result = Search(searchParams);
            return result;
        }

        public List<ArchiveDataSource> Search(SearchParams searchParams)
        {
            List<ArchiveDataSource> dataSources = new List<ArchiveDataSource>();
            StatusValuePair<List<ArchiveEntities>> archiveElements = _archiveEntityCrud.Search(searchParams);
            if (!archiveElements.HasValue)
            {
                return dataSources;
            }

            dataSources.AddRange(archiveElements.Value.Select(ToArchiveDataSource));
            return dataSources;
        }

        public int Delete(string archiveBarcode, string documentEntityBarcode)
        {
            // TODO move to stored procedure
            StatusValuePair<ArchiveEntities> archiveElement = _archiveEntityCrud.GetByBarcode(archiveBarcode);
            StatusValuePair<ArchiveEntities> documentEntity = _archiveEntityCrud.GetByBarcode(documentEntityBarcode);

            if (!archiveElement.HasValue || !documentEntity.HasValue)
            {
                return 1;
            }

            StatusValuePair<ManyToManyEntities> result = _manyToManyEntityCrud.Delete(documentEntity.Value.Id, archiveElement.Value.Id);
            return (int) result.ErrorCode;
        }

        public long GetLastIdOfArchive()
        {
            StatusValuePair<ArchiveEntities> result = _archiveEntityCrud.GetLastRecord();
            if (!result.HasValue || result.Value == null)
            {
                return 0;
            }

            long id = result.Value.Id;
            return id;
        }

        public void DeleteUnregisterDataBaseCache()
        {
            StatusValuePair<List<ArchiveEntities>> allRecords = _archiveEntityCrud.GetListArchiveElements();
            if (!allRecords.HasValue || allRecords.Value == null)
            {
                return;
            }

            foreach (ArchiveEntities archiveEntity in allRecords.Value)
            {
                SPFolder folder = SpList.ParentWeb.GetFolder(archiveEntity.Url);
                if (folder != null && folder.Item != null)
                {
                    continue;
                }

                _manyToManyEntityCrud.Delete(archiveEntity.Id);
                _archiveEntityCrud.Delete(archiveEntity);
            }
        }

        public bool Delete(string barcode)
        {
            StatusValuePair<ArchiveEntities> entity = _archiveEntityCrud.GetByBarcode(barcode);
            if (!entity.HasValue || entity.Value == null)
            {
                return false;
            }

            StatusValuePair<ManyToManyEntities> resultDeleteRelationship = _manyToManyEntityCrud.Delete(entity.Value.Id);
            ErrorCode resultDeleteArchive = _archiveEntityCrud.Delete(entity);

            if (resultDeleteRelationship.ErrorCode == ErrorCode.NoError && resultDeleteArchive == ErrorCode.NoError)
            {
                return true;
            }

            return false;
        }

        private ArchiveDataSource ToArchiveDataSource(ArchiveEntities archiveElement)
        {
            SPUser user = _spSite.RootWeb.SiteUsers.GetByID(archiveElement.CreatedBy);
            ArchiveDataSource item = new ArchiveDataSource
            {
                Id = Convert.ToInt32(archiveElement.Id),
                BarCode = archiveElement.Barcode,
                SE = archiveElement.SE,
                LP = archiveElement.LP,
                DocumentDate = archiveElement.DocumentDate,
                RegNumber = archiveElement.RegNumber,
                Currency = archiveElement.Currency,
                BPStatus = archiveElement.BPStatus,
                Contractor = archiveElement.Contractor,
                DocumentNumber = archiveElement.DocumentNumber,
                SumPlusVAT = archiveElement.SumPlusVAT,
                DocumentType = archiveElement.DocumentType,
                Created = archiveElement.Created,
                CreatedBy = user.Name,
                Status = archiveElement.Status,
                Description = archiveElement.Description,
                Url = archiveElement.Url,
                EditUrl = archiveElement.EditUrl,
                ListID = archiveElement.ListID,
                ItemID = archiveElement.ItemID,
                FullId = archiveElement.FullId,
            };
            return item;
        }

        public bool DeleteArchiveElementFields(string barCode)
        {
            throw new NotImplementedException();
        }

        public bool UpdateArchiveElementFieldValues(List<Columns> columns)
        {
            throw new NotImplementedException();
        }
    }
}