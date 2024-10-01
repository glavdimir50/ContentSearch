namespace ContentSearch
{
    public partial class Content : Form
    {
        public Content(string fileName, string content)
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.None;
            this.label1.Text = fileName;
            this.richTextBox1.Text = content;
        }

        public void Highlight(string keyword)
        {
            int start = 0;
            int last = richTextBox1.Text.LastIndexOf(keyword);

            richTextBox1.SelectAll();
            richTextBox1.SelectionBackColor = Color.White;

            while (start < last)
            {
                richTextBox1.Find(keyword, start, richTextBox1.TextLength, RichTextBoxFinds.MatchCase);
                richTextBox1.SelectionBackColor = Color.Yellow;
                start = richTextBox1.Text.IndexOf(keyword, start) + 1;
            }

            if (String.IsNullOrEmpty(keyword))
            {
                richTextBox1.BackColor = Color.White;
                richTextBox1.SelectionBackColor = Color.White;
                return;
            }
        }
    }
}
