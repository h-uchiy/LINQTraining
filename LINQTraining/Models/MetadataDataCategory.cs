#nullable disable

namespace LINQTraining.Models
{
    public class MetadataDataCategory
    {
        public long Id { get; set; }
        public long MetadataId { get; set; }
        public Metadata Metadata { get; set; }
        public long DataCategoryId { get; set; }
        public DataCategory DataCategory { get; set; }
    }
}