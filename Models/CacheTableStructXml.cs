namespace Navicon.SP.Components.SqlCache.Models
{
    using System.Collections.Generic;
    using System.Xml.Serialization;

    using Navicon.SP.Common.XmlProvider;

    public class CacheTableStructXml : XmlModelBase<CacheTableStructXml>
    {
        public CacheTableStructXml()
        {
            this.Columns = new List<Columns>();
        }

        [XmlElement]
        public string TableName { get; set; }

        [XmlElement]
        public bool RowIsDeleted { get; set; }

        [XmlElement]
        public List<Columns> Columns { get; set; }
    }

    [XmlType]
    public class Columns
    {
        [XmlAttribute]
        public string ColumnName { get; set; }

        [XmlAttribute]
        public string ColumnType { get; set; }

        [XmlAttribute]
        public string ColumnValue { get; set; }

        [XmlAttribute]
        public bool ColumnIsDeleted { get; set; }
    }
}