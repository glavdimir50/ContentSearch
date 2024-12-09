namespace ContentSearch
{
    public class PythonResult : DataGridDto
    {
        public Dictionary<string, int> FrequencyWordsWithStemming { get; set; }
        public Dictionary<string, int> FrequencyWords { get; set; }
        public string Content { get; set; }
        public List<string> Stopwords { get; set; }
    }
}
