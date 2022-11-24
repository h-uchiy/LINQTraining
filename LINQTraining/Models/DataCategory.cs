using System.Collections.Generic;

namespace LINQTraining.Models
{
    public class DataCategory
    {
        public long Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public ICollection<MetadataDataCategory> MetadataDataCategory { get; set; }
    }
}