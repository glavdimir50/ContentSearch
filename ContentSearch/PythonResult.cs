using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentSearch
{
    public class PythonResult : DataGridDto
    {
        public Dictionary<string, int> FrequencyWords { get; set; }
        public string Content { get; set; }
        public List<string> Stopwords { get; set; }
    }
}
