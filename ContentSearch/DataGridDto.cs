namespace ContentSearch
{
    public class DataGridDto
    {
        public string FileName { get; set; }
        public int CharCountIncludingSpaces { get; set; }
        public int CharCountExcludingSpaces { get; set; }
        public int SentenceCount { get; set; }
        public int WordsCount { get; set; }
        public int NonAsciiChars { get; set; }
        public int NonAsciiWords { get; set; }
        public int? KeywordSetCount { get; set; }
        public List<string>? keywordSet { get; set; }
    }
}
