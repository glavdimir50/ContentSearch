using CallPython;
using Newtonsoft.Json;
using System.Configuration;
using System.Data;
using System.Text;

namespace ContentSearch
{
    public partial class Form1 : Form
    {
        private string _historyFolder = @"D:\TestDir";
        private string _destinationFolder = @"D:\PubMedText";
        private string _jsonPath = string.Empty;
        private string _keywordSearchResult = string.Empty;

        public Form1()
        {
            InitializeComponent();

            comboBox1.SelectedIndex = 0;
            comboBox2.SelectedIndex = 0;
            comboBox3.SelectedIndex = 1;

            ClearResult();

            UpdateData();
        }

        private void ClearResult()
        {
            var resultPath = Path.Combine(_historyFolder, "result.json");
            if (File.Exists(resultPath))
            {
                File.Delete(resultPath);
            }
        }

        private void UpdateData()
        {
            GetPushedFiles();

            Statistics();

            CreateContentForms();

            LoadJsonDataToDataGridView();

            LoadWordCountDataGridView();
        }

        #region Events
        /// <summary>
        /// 選取檔案後自動上傳並執行分析事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void selectFileBtn_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Multiselect = true,
                Filter = "All Files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                if (openFileDialog.FileNames.Count() == 0)
                {
                    MessageBox.Show("請選擇上傳檔案");
                    return;
                }

                AutoPushFiles(openFileDialog.FileNames);

                UpdateData();
            }
        }

        /// <summary>
        /// ListBox選擇檔案顯示分析結果
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectedFileListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            //CreateContentForms();

            //LoadJsonDataToDataGridView();

            //LoadWordCountDataGridView();
        }

        /// <summary>
        /// 搜尋關鍵字
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void KeywordSearchbtn_Click(object sender, EventArgs e)
        {
            CloseAllImageForms();

            Statistics();

            CreateContentForms();

            LoadJsonDataToDataGridView();

            LoadWordCountDataGridView();

            //HighlightSearchKeywordFromContent();
        }
        #endregion

        #region Functions
        /// <summary>
        /// 取得已上傳檔案
        /// </summary>
        private void GetPushedFiles()
        {
            string[] filePaths = Directory.GetFiles(_destinationFolder);

            if (filePaths.Length.Equals(0))
            {
                return;
            }

            SelectedFileListBox.Items.Clear();

            foreach (var file in filePaths)
            {
                if (File.Exists(file))
                {
                    SelectedFileListBox.Items.Add(file);
                }
            }

            label1.Text = string.Format("Upload Files: {0}",  filePaths.Length);
        }

        /// <summary>
        /// 選取後自動上傳檔案
        /// </summary>
        private void AutoPushFiles(string[] newFiles)
        {
            if (!Directory.Exists(_destinationFolder))
            {
                Directory.CreateDirectory(_destinationFolder);
            }

            foreach (var filePath in newFiles)
            {
                if (File.Exists(filePath))
                {
                    try
                    {
                        string fileName = Path.GetFileName(filePath);
                        string destinationPath = Path.Combine(_destinationFolder, fileName);
                        File.Copy(filePath, destinationPath, true);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"檔案 {filePath} 上傳失敗: {ex.Message}");
                        return;
                    }
                }
                else
                {
                    MessageBox.Show($"檔案 {filePath} 不存在");
                    return;
                }
            }

            MessageBox.Show("所有檔案已成功上傳");
        }

        /// <summary>
        /// 分析結果方法
        /// </summary>
        private void Statistics()
        {
            List<string> files = new List<string>();

            if (SelectedFileListBox.SelectedItems.Count > 0)
            {
                foreach (var item in SelectedFileListBox.SelectedItems)
                {
                    files.Add(item.ToString());
                }
            }
            else
            {
                foreach (var item in SelectedFileListBox.Items)
                {
                    files.Add(item.ToString());
                }
            }

            var sg = 0;
            switch (comboBox3.Text)
            {
                case "SG":
                    sg = 1;
                    break;
                case "CBOW":
                    sg = 0;
                    break;
                default:
                    // 這裡是當 comboBox3.Text 不是 "SG" 或 "CBOW" 時的處理
                    // 可以給 sg 一個預設值或處理其他邏輯
                    break;
            }

            string pythonArguments = string.Format("{0}|{1}", string.Format("{0}&{1}&{2}&{3}", keywordTxtBox.Text, comboBox1.Text, comboBox2.Text.ToLower(), sg), string.Join('&', files));

            string filePath = Path.Combine(_historyFolder, "selectFiles.txt");

            try
            {
                File.WriteAllText(filePath, pythonArguments);
                Console.WriteLine("文件已成功儲存到 " + filePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine("儲存文件時發生錯誤: " + ex.Message);
            }

            _keywordSearchResult = GetKeywordSearchIRResult(filePath);
        }

        /// <summary>
        /// 根據關鍵字在文章中Highlight顯示
        /// </summary>
        private void HighlightSearchKeywordFromContent()
        {
            string allowLowerKeyword = keywordTxtBox.Text;

            if (flowLayoutPanel1.Controls.Count > 0)
            {
                foreach (Content obj in flowLayoutPanel1.Controls)
                {
                    obj.Highlight(allowLowerKeyword);
                }
            }
        }

        /// <summary>
        /// 顯示內文
        /// </summary>
        private void CreateContentForms()
        {
            if (SelectedFileListBox.Items.Count == 0)
            {
                return;
            }

            var jsonObjs = getJsonData();

            List<string> keyWords = new List<string>();

            //if (!string.IsNullOrEmpty(keywordTxtBox.Text))
            //{
            //    jsonObjs.IRResult = jsonObjs.IRResult.Where(x => x.Content.ToLower().Contains(keywordTxtBox.Text.ToLower())).ToList();
            //}

            #region 顯示內文
            flowLayoutPanel1.Controls.Clear();

            if (SelectedFileListBox.SelectedItems.Count > 0)
            {
                jsonObjs.IRResult = jsonObjs.IRResult.Where(x => SelectedFileListBox.SelectedItems.Contains(x.FileName)).ToList();

                foreach (var item in SelectedFileListBox.SelectedItems)
                {
                    var file = item as string;
                    PythonResult? irResult = null;

                    if (File.Exists(file))
                    {
                        irResult = jsonObjs.IRResult.FirstOrDefault(x => file.Contains(x.FileName));
                    }

                    if (irResult == null)
                    {
                        continue;
                    }

                    Content contentForm = new Content(Path.GetFileName(file), irResult.Content);
                    contentForm.TopLevel = false;
                    flowLayoutPanel1.Controls.Add(contentForm);
                    contentForm.Show();
                    if (irResult.KeywordSetCount > 0 && irResult.keywordSet != null)
                    {
                        keyWords.AddRange(irResult.keywordSet);
                        contentForm.Highlight(irResult.keywordSet);
                    }
                }
            }
            else
            {
                int count = 0;
                foreach (var item in SelectedFileListBox.Items)
                {
                    if (count >= 50) break;
                    var file = item as string;
                    //string content = string.Empty;
                    PythonResult? irResult = null;

                    if (File.Exists(file))
                    {
                        irResult = jsonObjs.IRResult.FirstOrDefault(x => file.Contains(x.FileName));
                    }

                    if (irResult == null)
                    {
                        continue;
                    }

                    if (string.IsNullOrEmpty(irResult.Content))
                    {
                        continue;
                    }

                    Content contentForm = new Content(Path.GetFileName(file), irResult.Content);
                    contentForm.TopLevel = false;
                    flowLayoutPanel1.Controls.Add(contentForm);
                    contentForm.Show();
                    if (irResult.KeywordSetCount>0 && irResult.keywordSet!=null)
                    {
                        keyWords.AddRange(irResult.keywordSet);
                        contentForm.Highlight(irResult.keywordSet);
                    }
                    count++;
                }
            }
            #endregion

            label4.Text = string.Format("Result: {0}", string.Join(", ", keyWords.Distinct()));
        }

        /// <summary>
        /// 取得內文分析結果
        /// </summary>
        private void LoadJsonDataToDataGridView()
        {
            try
            {
                var records = getJsonData();

                if (SelectedFileListBox.SelectedItems.Count > 0)
                {
                    records.IRResult = records.IRResult.Where(x => SelectedFileListBox.SelectedItems.Contains(x.FileName)).ToList();
                }

                dataGridView1.DataSource = null;
                dataGridView1.DataSource = records.IRResult.Select(record => new DataGridDto
                {
                    FileName = Path.GetFileName(record.FileName),
                    CharCountIncludingSpaces = record.CharCountIncludingSpaces,
                    CharCountExcludingSpaces = record.CharCountExcludingSpaces,
                    SentenceCount = record.SentenceCount,
                    WordsCount = record.WordsCount,
                    NonAsciiChars = record.NonAsciiChars,
                    NonAsciiWords = record.NonAsciiWords,
                    keywordSet = record.keywordSet,
                    KeywordSetCount = record.KeywordSetCount
                }).ToList();

                label6.Text = string.Format("Statistics: {0}", records.IRResult.Count);
                //var nullFiles = records.IRResult.Where(x=>x.SentenceCount.Equals(0)).Select(x=>x.FileName).ToList();
                //var files = Directory.GetFiles("D:\\PubMedText");
                //foreach (var file in files)
                //{
                //    if (nullFiles.Contains(file) && File.Exists(file))
                //    {
                //        File.Delete(file);
                //    }
                //}
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// 取得單字統計結果
        /// </summary>
        private void LoadWordCountDataGridView()
        {
            GetDataGridViewsDataByFrequencyWords();
        }

        private void GetDataGridViewsDataByFrequencyWords()
        {
            var records = getJsonData();

            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("Word", typeof(string));
            dataTable.Columns.Add("Count", typeof(int));

            foreach (var wordEntry in records.Frequencies.OrderByDescending(x => x.Value).ThenBy(x => x.Key))
            {
                dataTable.Rows.Add(wordEntry.Key, wordEntry.Value);
            }

            dataGridView2.DataSource = null;
            dataGridView2.DataSource = dataTable;

            label7.Text = string.Format("Word Count: {0}", records.Frequencies.Count);
        }

        private void GetDataGridViewsDataByFileName()
        {
            var records = getJsonData();

            if (SelectedFileListBox.SelectedItems.Count > 0)
            {
                records.IRResult = records.IRResult.Where(x => SelectedFileListBox.SelectedItems.Contains(x.FileName)).ToList();
            }

            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("Word", typeof(string));
            dataTable.Columns.Add("Count", typeof(int));
            dataTable.Columns.Add("FileName", typeof(string));

            foreach (var record in records.IRResult)
            {
                foreach (var data in record.FrequencyWords)
                {
                    dataTable.Rows.Add(data.Key, data.Value, Path.GetFileName(record.FileName));
                }
            }

            dataGridView2.DataSource = null;
            dataGridView2.DataSource = dataTable;
        }

        /// <summary>
        /// 取得Json物件
        /// </summary>
        /// <returns></returns>
        private JsonData? getJsonData()
        {
            try
            {
                if (string.IsNullOrEmpty(_keywordSearchResult))
                {
                    return null;
                }

                return JsonConvert.DeserializeObject<JsonData>(_keywordSearchResult);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                throw;
            }
        }

        /// <summary>
        /// 開啟圖檔
        /// </summary>
        /// <param name="path"></param>
        private void OpenImage(string path)
        {
            ImageForm imageForm = new ImageForm(path);

            imageForm.Show();
        }

        /// <summary>
        /// 關鍵字搜尋
        /// </summary>
        /// <returns></returns>
        private string GetKeywordSearchIRResult(string keyword)
        {
            string exePath = ConfigurationManager.AppSettings["exePath"];
            string filePath = ConfigurationManager.AppSettings["filePath"];

            if (!File.Exists(exePath) || !File.Exists(filePath))
            {
                MessageBox.Show("找不到Python應用程式與執行檔，\r\n" +
                    "請在App.config檔中設定\r\n" +
                    "該電腦的python應用程式路徑與\r\n" +
                    "Text.py執行檔路徑");
                return "EXE PATH OR PYTHON PATH FAILED";
            }

            #region 取得Python執行結果
            string pythonResult = ProcessClass.StartPython(exePath, filePath, keyword);
            var filePaths = pythonResult.Split("|");
            if (!File.Exists(filePaths[0]) || !File.Exists(filePaths[1]) || !File.Exists(filePaths[2]))
            {
                MessageBox.Show(pythonResult);
                return "ERROR：" + pythonResult;
            }
            #endregion
            OpenImage(filePaths[1]);
            OpenImage(filePaths[2]);
            if (filePaths.Length>3 && File.Exists(filePaths[3])) {
                OpenImage(filePaths[3]);
            }

            _jsonPath = filePaths[0];

            return File.ReadAllText(filePaths[0], encoding: Encoding.UTF8);
        }

        /// <summary>
        /// 關閉已開啟的圖檔
        /// </summary>
        private void CloseAllImageForms()
        {
            List<Form> openImageForms = new List<Form>();

            foreach (Form form in Application.OpenForms)
            {
                if (form is ImageForm)
                {
                    openImageForms.Add(form);
                }
            }

            foreach (Form imageForm in openImageForms)
            {
                imageForm.Close();
            }
        }
        #endregion
    }
}

