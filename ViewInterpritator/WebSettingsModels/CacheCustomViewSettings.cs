namespace Navicon.SP.Components.SqlCache.ViewInterpritator.WebSettingsModels
{
    using System.Collections.Generic;
    using System.Xml.Serialization;

    using Navicon.SP.Common.XmlProvider;

    [XmlRoot]
    public class CacheCustomViewSettings : XmlModelBase<CacheCustomViewSettings>
    {
        [XmlElement]
        public List<CustomViewSettings> ViewSettings { get; set; }
    }
}