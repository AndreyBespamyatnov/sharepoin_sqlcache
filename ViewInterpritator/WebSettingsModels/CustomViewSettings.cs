namespace Navicon.SP.Components.SqlCache.ViewInterpritator.WebSettingsModels
{
    using System.Collections.Generic;
    using System.Xml.Serialization;

    using Navicon.SP.Components.SqlCache.SpSync;

    public class CustomViewSettings
    {
        public CustomViewSettings()
        {
            this.CustomViews = new List<CustomViews>();
            this.CacheType = new List<SyncType>();
        }

        [XmlAttribute]
        public string ListUrl { get; set; }

        [XmlAttribute]
        public bool IsCacheDb { get; set; }
        
        [XmlAttribute]
        public bool ForceCreating { get; set; }
        [XmlAttribute]
        public bool IsOverrideView { get; set; }

        [XmlElement]
        public List<SyncType> CacheType { get; set; }

        [XmlAttribute]
        public string TableName { get; set; }

        [XmlElement]
        public List<CustomViews> CustomViews { get; set; }
    }
}