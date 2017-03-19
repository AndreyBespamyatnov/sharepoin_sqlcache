using System;
using DapperExtensions.Mapper;

namespace Navicon.SP.Components.SqlCache.Models
{
    public class ArchiveEntities
    {
        public long Id { get; set; }
        public string Barcode { get; set; }

        public string LP { get; set; }
        public string SE { get; set; }
        public string Url { get; set; }

        public string Contractor { get; set; }
        public string SumPlusVAT { get; set; }

        public string DocumentKind { get; set; }
        public string EditUrl { get; set; }
        public DateTime Created { get; set; }
        public DateTime? DocumentDate { get; set; }
        public int CreatedBy { get; set; }
        public bool IsArchiveElement { get; set; }
        public string DocumentNumber { get; set; }
        public string RegNumber { get; set; }

        public string Currency { get; set; }
        public string BPStatus { get; set; }

        public string DocumentType { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public bool IsUpdated { get; set; }
        public string ListID { get; set; }
        public string ItemID { get; set; }
        public string FullId { get; set; }
        
    }

    public class ArchiveEntitiesMapping : ClassMapper<ArchiveEntities>
    {
        public ArchiveEntitiesMapping()
        {
            Table("ArchiveEntities");

            Map(f => f.Id).Key(KeyType.Identity);
            Map(f => f.Barcode).Key(KeyType.NotAKey);
            Map(f => f.Url).Key(KeyType.NotAKey);
            Map(f => f.EditUrl).Key(KeyType.NotAKey);
            Map(f => f.Created).Key(KeyType.NotAKey);
            Map(f => f.CreatedBy).Key(KeyType.NotAKey);
            Map(f => f.IsArchiveElement).Key(KeyType.NotAKey);
            Map(f => f.DocumentType).Key(KeyType.NotAKey);
            Map(f => f.Description).Key(KeyType.NotAKey);
            Map(f => f.Status).Key(KeyType.NotAKey);
            Map(f => f.IsUpdated).Key(KeyType.NotAKey);
            Map(f => f.ListID).Key(KeyType.NotAKey);
            Map(f => f.ItemID).Key(KeyType.NotAKey);
            Map(f => f.LP).Key(KeyType.NotAKey);
            Map(f => f.SE).Key(KeyType.NotAKey);
            Map(f => f.Contractor).Key(KeyType.NotAKey);
            Map(f => f.SumPlusVAT).Key(KeyType.NotAKey);
            Map(f => f.DocumentKind).Key(KeyType.NotAKey);
            Map(f => f.DocumentDate).Key(KeyType.NotAKey);
            Map(f => f.RegNumber).Key(KeyType.NotAKey);
            Map(f => f.Currency).Key(KeyType.NotAKey);
            Map(f => f.BPStatus).Key(KeyType.NotAKey);
            Map(f => f.FullId).Key(KeyType.NotAKey);
        }
    }
}