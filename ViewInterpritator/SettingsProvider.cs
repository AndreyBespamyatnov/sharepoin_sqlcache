namespace Navicon.SP.Components.SqlCache.ViewInterpritator
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Microsoft.SharePoint;

    using Navicon.SP.Common.Extensions;
    using Navicon.SP.Common.XmlProvider;
    using Navicon.SP.Components.SqlCache.SpSync;
    using Navicon.SP.Components.SqlCache.ViewInterpritator.WebSettingsModels;

    /// <summary>
    ///     Обёртка над классом web настроек, позволяет получить доступ к настройкам более элегантно.
    /// </summary>
    public sealed class SettingsProvider : CacheCustomViewSettingsProvider<CacheCustomViewSettings>
    {
        /// <summary>
        ///     Получает настройки для текущего SPWeb
        /// </summary>
        public static CacheCustomViewSettingsProvider<CacheCustomViewSettings> Instance()
        {
            CacheCustomViewSettingsProvider<CacheCustomViewSettings> provider = new CacheCustomViewSettingsProvider<CacheCustomViewSettings>();
            return provider;
        }

        /// <summary>
        ///     Получает настройки для заданного SPWeb
        /// </summary>
        /// <param name="web">SPWeb из которого будут читаться настройки.</param>
        public static CacheCustomViewSettingsProvider<CacheCustomViewSettings> Instance(SPWeb web)
        {
            CacheCustomViewSettingsProvider<CacheCustomViewSettings> provider = new CacheCustomViewSettingsProvider<CacheCustomViewSettings>(DefaultSettingsKey, web);
            return provider;
        }
    }

    /// <summary>
    ///     Провайдер настроек для кеша.
    /// </summary>
    public class CacheCustomViewSettingsProvider<TOut> : XmlSettingsProviderBase<CacheCustomViewSettings, TOut> where TOut : CacheCustomViewSettings, new()
    {
        protected const string DefaultSettingsKey = "CacheCustomViewSettings";

        /// <summary>
        ///     Получает настройки для текущего SPWeb
        /// </summary>
        /// <remarks>Если настройки не существуют они будут заданны автоматически.</remarks>
        internal CacheCustomViewSettingsProvider()
            : base(DefaultSettingsKey) {}

        /// <summary>
        ///     Получает настройки для заданного SPWeb
        /// </summary>
        /// <param name="key">Ключ по которому хранятся настройки</param>
        /// <param name="web">SPWeb из которого будут читаться настройки.</param>
        /// <remarks>Если настройки не существуют они будут заданны автоматически.</remarks>
        internal CacheCustomViewSettingsProvider(string key, SPWeb web)
            : base(key, web) {}

        /// <summary>
        ///     Задаёт натсройку 'IsCacheDb' для заданного листа.
        /// </summary>
        /// <param name="spList">Лист для которого выставляется значение</param>
        /// <param name="value">Значение</param>
        /// <remarks>Если настройки не существуют они будут заданны автоматически.</remarks>
        public void SetIsCacheDb(SPList spList, bool value)
        {
            if (spList == null)
            {
                return;
            }

            string listUrl = spList.Url();

            TOut settings = this.GetSettings();
            CustomViewSettings listSettings = settings.ViewSettings.FirstOrDefault(s => s.ListUrl == listUrl);
            if (listSettings == null)
            {
                settings.ViewSettings.Add(new CustomViewSettings
                {
                    ListUrl = listUrl,
                    IsCacheDb = value
                });
            }
            else
            {
                listSettings.IsCacheDb = value;
            }

            this.SetSettings(settings);
        }
        
        /// <summary>
        ///     Задаёт натсройку 'ForceCreating' для заданного листа.
        /// </summary>
        /// <param name="spList">Лист для которого выставляется значение</param>
        /// <param name="value">Значение</param>
        /// <remarks>Если настройки не существуют они будут заданны автоматически.</remarks>
        public void SetForceCreating(SPList spList, bool value)
        {
            if (spList == null)
            {
                return;
            }

            string listUrl = spList.Url();

            TOut settings = this.GetSettings();
            CustomViewSettings listSettings = settings.ViewSettings.FirstOrDefault(s => s.ListUrl == listUrl);
            if (listSettings == null)
            {
                settings.ViewSettings.Add(new CustomViewSettings
                {
                    ListUrl = listUrl,
                    ForceCreating = value
                });
            }
            else
            {
                listSettings.ForceCreating = value;
            }

            this.SetSettings(settings);
        }

        /// <summary>
        ///     Задаёт натсройку 'IsOverrideView' для заданного листа.
        /// </summary>
        /// <param name="spList">Лист для которого выставляется значение</param>
        /// <param name="value">Значение</param>
        /// <remarks>Если настройки не существуют они будут заданны автоматически.</remarks>
        public void SetIsOverrideView(SPList spList, bool value)
        {
            if (spList == null)
            {
                return;
            }

            string listUrl = spList.Url();

            TOut settings = this.GetSettings();
            CustomViewSettings listSettings = settings.ViewSettings.FirstOrDefault(s => s.ListUrl == listUrl);
            if (listSettings == null)
            {
                settings.ViewSettings.Add(new CustomViewSettings
                {
                    ListUrl = listUrl,
                    IsOverrideView = value
                });
            }
            else
            {
                listSettings.IsOverrideView = value;
            }

            this.SetSettings(settings);
        }

        /// <summary>
        ///     Возвращает значение свойства <see cref="IsCacheDb" /> для листа.
        /// </summary>
        /// <param name="spList">Лист для которого запрашиваются настройки.</param>
        /// <returns>Булевое значение свойства <see cref="IsCacheDb" /></returns>
        /// <remarks>Если настройки не существуют они будут заданны автоматически и будет присвоенно значение по умолчанию.</remarks>
        public bool IsCacheDb(SPList spList)
        {
            if (spList == null)
            {
                return false;
            }

            string listUrl = spList.Url();

            TOut settings = this.GetSettings();
            CustomViewSettings listSettings = settings.ViewSettings.FirstOrDefault(s => s.ListUrl == listUrl);
            if (listSettings == null)
            {
                return false;
            }

            return listSettings.IsCacheDb;
        }
        
        /// <summary>
        ///     Возвращает значение свойства <see cref="ForceCreating" /> для листа.
        /// </summary>
        /// <param name="spList">Лист для которого запрашиваются настройки.</param>
        /// <returns>Булевое значение свойства <see cref="ForceCreating" /></returns>
        /// <remarks>Если настройки не существуют они будут заданны автоматически и будет присвоенно значение по умолчанию.</remarks>
        public bool ForceCreating(SPList spList)
        {
            if (spList == null)
            {
                return false;
            }

            string listUrl = spList.Url();

            TOut settings = this.GetSettings();
            CustomViewSettings listSettings = settings.ViewSettings.FirstOrDefault(s => s.ListUrl == listUrl);
            if (listSettings == null)
            {
                return false;
            }

            return listSettings.ForceCreating;
        }

        /// <summary>
        ///     Возвращает значение свойства <see cref="IsOverrideView" /> для листа.
        /// </summary>
        /// <param name="spList">Лист для которого запрашиваются настройки.</param>
        /// <returns>Булевое значение свойства <see cref="IsOverrideView" /></returns>
        /// <remarks>Если настройки не существуют они будут заданны автоматически и будет присвоенно значение по умолчанию.</remarks>
        public bool IsOverrideView(SPList spList)
        {
            if (spList == null)
            {
                return false;
            }

            string listUrl = spList.Url();

            TOut settings = this.GetSettings();
            CustomViewSettings listSettings = settings.ViewSettings.FirstOrDefault(s => s.ListUrl == listUrl);
            if (listSettings == null)
            {
                return false;
            }

            return listSettings.IsOverrideView;
        }

        /// <summary>
        ///     Задаёт настройку 'IsChanget' для заданного отображения.
        /// </summary>
        /// <param name="spView">Отображение для которого выставляется значение</param>
        /// <param name="value">Значение</param>
        /// <remarks>Если настройки не существуют они будут заданны автоматически.</remarks>
        public void SetIsChanget(SPView spView, bool value)
        {
            if (spView == null)
            {
                return;
            }

            string listUrl = spView.ParentList.Url();
            TOut settings = this.GetSettings();
            CustomViewSettings listSettings = settings.ViewSettings.FirstOrDefault(s => s.ListUrl == listUrl);
            if (listSettings == null)
            {
                CustomViewSettings customViewSettings = new CustomViewSettings
                {
                    ListUrl = listUrl,
                    CustomViews = new List<CustomViews>
                    {
                        new CustomViews
                        {
                            IsChanget = value,
                            ViewUrl = spView.Url,
                            ViewId = spView.ID,
                            SchemaXml = spView.SchemaXml
                        }
                    }
                };
                settings.ViewSettings.Add(customViewSettings);
            }
            else
            {
                CustomViews customView = listSettings.CustomViews.FirstOrDefault(v => v.ViewId == spView.ID);
                if (customView == null)
                {
                    CustomViews customViews = new CustomViews
                    {
                        IsChanget = value,
                        ViewUrl = spView.Url,
                        ViewId = spView.ID,
                        SchemaXml = spView.SchemaXml
                    };
                    listSettings.CustomViews.Add(customViews);
                }
                else
                {
                    customView.IsChanget = value;
                    customView.SchemaXml = spView.SchemaXml;
                }
            }

            this.SetSettings(settings);
        }

        public void SetPersonalViewUrl(SPView spView, string value)
        {
            if (spView == null)
            {
                return;
            }

            string listUrl = spView.ParentList.Url();
            TOut settings = this.GetSettings();
            CustomViewSettings listSettings = settings.ViewSettings.FirstOrDefault(s => s.ListUrl == listUrl);
            if (listSettings == null)
            {
                CustomViewSettings customViewSettings = new CustomViewSettings
                {
                    ListUrl = listUrl,
                    CustomViews = new List<CustomViews>
                    {
                        new CustomViews
                        {
                            PersonalViewUrl = value,
                            ViewUrl = spView.Url,
                            ViewId = spView.ID,
                            IsChanget = true,
                            SchemaXml = spView.SchemaXml
                        }
                    }
                };
                settings.ViewSettings.Add(customViewSettings);
            }
            else
            {
                CustomViews customView = listSettings.CustomViews.FirstOrDefault(v => v.ViewId == spView.ID);
                if (customView == null)
                {
                    CustomViews customViews = new CustomViews
                    {
                        PersonalViewUrl = value,
                        ViewUrl = spView.Url,
                        ViewId = spView.ID,
                        IsChanget = true,
                        SchemaXml = spView.SchemaXml
                    };
                    listSettings.CustomViews.Add(customViews);
                }
                else
                {
                    customView.PersonalViewUrl = value;
                    customView.IsChanget = true;
                    customView.SchemaXml = spView.SchemaXml;
                }
            }

            this.SetSettings(settings);
        }

        /// <summary>
        ///     Возвращает значение свойства <see cref="IsChanged" /> для отображения.
        /// </summary>
        /// <param name="spView">Отображение для которого запрашиваются настройки.</param>
        /// <returns>Булевое значение свойства <see cref="IsChanged" /></returns>
        /// <remarks>Если настройки не существуют они будут заданны автоматически и будет присвоенно значение по умолчанию.</remarks>
        public bool IsChanged(SPView spView)
        {
            if (spView == null)
            {
                return false;
            }

            string listUrl = spView.ParentList.Url();
            TOut settings = this.GetSettings();
            CustomViewSettings listSettings = settings.ViewSettings.FirstOrDefault(s => s.ListUrl == listUrl);
            if (listSettings == null)
            {
                return false;
            }
            CustomViews customView = listSettings.CustomViews.FirstOrDefault(v => v.ViewId == spView.ID);
            if (customView == null)
            {
                return false;
            }

            return customView.IsChanget && string.Equals(spView.SchemaXml, customView.SchemaXml, StringComparison.InvariantCultureIgnoreCase);
        }

        public string PersonalViewUrl(SPView spView)
        {
            if (spView == null)
            {
                return string.Empty;
            }

            string listUrl = spView.ParentList.Url();
            TOut settings = this.GetSettings();
            CustomViewSettings listSettings = settings.ViewSettings.FirstOrDefault(s => s.ListUrl == listUrl);
            if (listSettings == null)
            {
                return string.Empty;
            }
            CustomViews customView = listSettings.CustomViews.FirstOrDefault(v => v.ViewId == spView.ID);
            if (customView == null)
            {
                return string.Empty;
            }

            return customView.PersonalViewUrl;
        }

        public List<SyncType> CacheType(SPList spList)
        {
            if (spList == null)
            {
                return new List<SyncType> {SyncType.SpList};
            }

            string listUrl = spList.Url();
            TOut settings = this.GetSettings();
            CustomViewSettings listSettings = settings.ViewSettings.FirstOrDefault(s => s.ListUrl == listUrl);
            if (listSettings == null)
            {
                return new List<SyncType> {SyncType.SpList};
            }
            return listSettings.CacheType;
        }

        public void SetCacheTypeDb(SPList spList, SyncType syncType)
        {
            if (spList == null)
            {
                return;
            }

            string listUrl = spList.Url();

            TOut settings = this.GetSettings();
            CustomViewSettings listSettings = settings.ViewSettings.FirstOrDefault(s => s.ListUrl == listUrl);
            if (listSettings == null)
            {
                settings.ViewSettings.Add(new CustomViewSettings
                {
                    ListUrl = listUrl,
                    CacheType = new List<SyncType> {syncType}
                });
            }
            else
            {
                if (!listSettings.CacheType.Contains(syncType))
                {
                    listSettings.CacheType.Add(syncType);
                }
            }

            this.SetSettings(settings);
        }

        public string TableName(SPList spList)
        {
            if (spList == null)
            {
                return string.Empty;
            }

            string listUrl = spList.Url();
            TOut settings = this.GetSettings();
            CustomViewSettings listSettings = settings.ViewSettings.FirstOrDefault(s => s.ListUrl == listUrl);
            if (listSettings == null)
            {
                return string.Empty;
            }
            return listSettings.TableName;
        }

        public void SetTableName(SPList spList, string tableName)
        {
            if (spList == null)
            {
                return;
            }

            string listUrl = spList.Url();

            TOut settings = this.GetSettings();
            CustomViewSettings listSettings = settings.ViewSettings.FirstOrDefault(s => s.ListUrl == listUrl);
            if (listSettings == null)
            {
                settings.ViewSettings.Add(new CustomViewSettings
                {
                    ListUrl = listUrl,
                    TableName = tableName
                });
            }
            else
            {
                listSettings.TableName = tableName;
            }

            this.SetSettings(settings);
        }

        public void Clear()
        {
            this.SetSettings(new TOut());
        }
    }
}