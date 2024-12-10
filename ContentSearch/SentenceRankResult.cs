namespace ContentSearch
{
    public class SentenceRankResult
    {
        public string filePath { get; set; }
        public string title { get; set; }
        //public string content { get; set; }
        //public bool fileStatus { get; set; }
        //public bool analyzed { get; set; }
        public List<KeyValuePair<double, string>> SentenceVector { get; set; }
        public List<KeyValuePair<double, string>> SentenceBERT { get; set; }
    }
}
