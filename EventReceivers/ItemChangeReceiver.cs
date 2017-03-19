namespace Navicon.SP.Components.SqlCache.EventReceivers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Microsoft.SharePoint;

    using Navicon.SP.Common;
    using Navicon.SP.Components.SqlCache.SpSync;
    using Navicon.SP.Components.SqlCache.ViewInterpritator;
    using Navicon.SP.SqlCache.Common.Box;
    using System.Xml.Serialization;
    using Navicon.SP.Common.Utility;

    public sealed class ItemChangeReceiver : SPItemEventReceiver, IDisposable
    {
        private ISpSyncProvider _syncProvider;

        public override void ItemAdded(SPItemEventProperties properties)
        {
            base.ItemAdded(properties);
            properties.ListItem[Constants.BarCodeFieldName] = properties.ListItemId;
            properties.ListItem[Constants.ItemUniqueIDFieldName] = properties.ListItem.UniqueId;
            using (new DisabledItemEventsScope())
            {
                properties.ListItem.Update();
            }
            List<SyncType> syncTypes = SettingsProvider.Instance(properties.Web).CacheType(properties.List);
            this.Synchronize(properties, syncTypes, SyncActionType.Add);
        }

        /// <summary>
        ///     На самом деле это синхронный метод. Аналог ItemUpdating
        /// </summary>
        /// <param name="properties"></param>
        public override void ItemUpdated(SPItemEventProperties properties)
        {
            base.ItemUpdated(properties);
            List<SyncType> syncTypes = SettingsProvider.Instance(properties.Web).CacheType(properties.List);
            using (DisabledItemEventsScope scope = new DisabledItemEventsScope())
            {
                if (properties.ListItem.Fields.ContainsField(Fields.IsUpdated))
                {
                    bool needSetIsUpdated = false;
                    object isUpdatedObj = properties.ListItem.Properties[Fields.IsUpdated];
                    if (isUpdatedObj != null)
                    {
                        bool isUpdated;
                        if (bool.TryParse(isUpdatedObj.ToString(), out isUpdated))
                        {
                            if (!isUpdated)
                            {
                                needSetIsUpdated = true;
                            }
                        }
                    }
                    else
                    {
                        needSetIsUpdated = true;
                    }

                    if (needSetIsUpdated)
                    {
                        properties.ListItem[Fields.IsUpdated] = true;
                        properties.ListItem.Update();
                    }

                    //Если у элемента не проставлено значение в поле ItemUniqueID, то проставляем его.
                    //Такая ситуация может произойти со старыми элементами.
                    if (properties.ListItem[Constants.ItemUniqueIDFieldName] == null ||
                        string.IsNullOrEmpty(Convert.ToString(properties.ListItem[Constants.ItemUniqueIDFieldName])))
                    {
                        properties.ListItem[Constants.ItemUniqueIDFieldName] = properties.ListItem.UniqueId;
                        properties.ListItem.Update();
                    }
                }
            }
            this.Synchronize(properties, syncTypes, SyncActionType.Update);
        }

        public override void ItemDeleting(SPItemEventProperties properties)
        {
            DeleteChildrenItems(properties);

            this.ItemDeleted(properties);
            List<SyncType> syncTypes = SettingsProvider.Instance(properties.Web).CacheType(properties.List);
            this.Synchronize(properties, syncTypes, SyncActionType.Delete);
        }

        private void DeleteChildrenItems(SPItemEventProperties properties)
        {
            SPList parentList = properties.List;
            ICollection<ListRelation> childrenLists = GetChildrenLists(parentList).ToList();

            int parentItemId = properties.ListItemId;

            foreach (var childList in childrenLists)
            {
                string fieldName = childList.LookupFieldName;
                var query = new SPQuery()
                {
                    Query = string.Format("<Where><Eq><FieldRef Name='{0}' LookupId='TRUE' /><Value Type='Lookup'>{1}</Value></Eq></Where>", fieldName, parentItemId)
                };

                childList.ChildListUrl = childList.ChildListUrl.FirstOrDefault() == '/' ? childList.ChildListUrl : "/" + childList.ChildListUrl;
                SPList spList = properties.Web.GetList(childList.ChildListUrl);
                SPListItemCollection childrenListItems = spList.GetItems(query);
                var childrenItems = childrenListItems.Cast<SPListItem>().ToList();
                for (int i = 0; i < childrenItems.Count(); i++)
                {
                    childrenItems[i].Delete();
                }
            }
        }

        private IEnumerable<ListRelation> GetChildrenLists(SPList parentList)
        {
            IEnumerable<ListRelation> childrenLists = new List<ListRelation>();

            WebSettings settings = WebSettings.GetWebSettings(parentList.ParentWeb); // TODO make sure the right web is chosen
            string setting = settings[ListRelation.SettingKey];

            if (!string.IsNullOrWhiteSpace(setting))
            {
                childrenLists = UtilityObject.Deserialize<List<ListRelation>>(setting);
                childrenLists = childrenLists.Where(s => parentList.DefaultViewUrl.Contains(s.ParentListUrl));
            }

            return childrenLists;
        }

        [Serializable]
        public class ListRelation
        {
            [XmlIgnore]
            public static readonly string SettingKey = "ItemsChainDeletion";

            [XmlAttribute("WhenItemIn")]
            public string ParentListUrl { get; set; }

            [XmlAttribute("ThenDeleteIn")]
            public string ChildListUrl { get; set; }

            [XmlAttribute("ByField")]
            public string LookupFieldName { get; set; }
        }

        private void Synchronize(SPItemEventProperties properties, IReadOnlyCollection<SyncType> syncTypes, SyncActionType syncActionType)
        {
            switch (syncTypes.Count)
            {
                case 0:
                    return;
                case 1:
                    this._syncProvider = new SpSyncProvider(syncTypes.FirstOrDefault());
                    if (this._syncProvider.Synchronize(properties.ListItem, syncActionType))
                    {
                        base.ItemAdded(properties);
                    }
                    return;
                default:
                    bool result = false;
                    foreach (SyncType syncType in syncTypes)
                    {
                        this._syncProvider = new SpSyncProvider(syncType);
                        result = this._syncProvider.Synchronize(properties.ListItem, syncActionType);
                    }
                    if (result)
                    {
                        base.ItemAdded(properties);
                    }
                    break;
            }
        }

        #region Implementation of IDisposable
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (this._syncProvider != null)
            {
                this._syncProvider.Dispose();
            }
        }
        #endregion
    }
}