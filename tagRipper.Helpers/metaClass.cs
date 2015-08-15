using System.Collections.Generic;

namespace tagRipper.Helpers
{
    public class metaClass
    {
        public string etag { get; set; }

        public List<Item> items { get; set; }

        public string kind { get; set; }

        public PageInfo pageInfo { get; set; }
    }
}