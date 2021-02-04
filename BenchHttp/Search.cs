using System.Collections.Generic;

namespace BenchHttp
{
    public class Search
    {
        public int numFound { get; set; }
        public int start { get; set; }
        public List<Doc> docs { get; set; }
        public int num_found { get; set; }
    }
}