namespace Navicon.SP.Components.SqlCache.SpSync
{
    using System.Xml.Serialization;

    [XmlType]
    public enum SyncType
    {
        [XmlEnum("0")]
        SpList = 0,

        [XmlEnum("1")]
        SpContentType = 1
    }
}