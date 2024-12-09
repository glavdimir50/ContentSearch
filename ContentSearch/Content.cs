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

        public void Highlight(List<string> keywords)
        {
            // 清除之前的高亮顯示
            richTextBox1.SelectAll();
            richTextBox1.SelectionBackColor = Color.White;

            // 如果關鍵字列表為空或沒有任何關鍵字，則返回
            if (keywords == null || keywords.Count == 0)
            {
                return;
            }

            // 將 RichTextBox 的內容轉換成小寫，方便不區分大小寫的比對
            string richText = richTextBox1.Text.ToLower();

            // 對每個關鍵字進行查找並高亮
            foreach (string keyword in keywords)
            {
                if (string.IsNullOrEmpty(keyword))
                {
                    continue; // 跳過空關鍵字
                }

                // 將關鍵字轉換為小寫
                string lowerKeyword = keyword.ToLower();

                int start = 0;
                int last = richText.LastIndexOf(lowerKeyword);

                while (start <= last && last != -1)
                {
                    // 找到關鍵字並設置高亮，不分大小寫
                    richTextBox1.Find(lowerKeyword, start, richTextBox1.TextLength, RichTextBoxFinds.None);
                    richTextBox1.SelectionBackColor = Color.Yellow;

                    // 更新 start 為下一個關鍵字開始的位置
                    start = richText.IndexOf(lowerKeyword, start) + 1;

                    // 如果找不到下一個關鍵字，跳出循環
                    if (start <= 0)
                    {
                        break;
                    }
                }
            }
        }

    }
}
