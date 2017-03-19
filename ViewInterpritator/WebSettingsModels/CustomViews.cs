namespace Navicon.SP.Components.SqlCache.ViewInterpritator.WebSettingsModels
{
    using System;
    using System.Xml.Serialization;

    public class CustomViews
    {
        [XmlAttribute]
        public Guid ViewId { get; set; }

        [XmlAttribute]
        public string ViewUrl { get; set; }

        [XmlAttribute]
        public string SchemaXml { get; set; }

        [XmlAttribute]
        public bool IsChanget { get; set; }

        [XmlAttribute]
        public string PersonalViewUrl { get; set; }
    }
}