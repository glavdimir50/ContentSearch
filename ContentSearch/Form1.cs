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

        public Form1()
        {
            InitializeComponent();

            UpdateData();
        }

        public void UpdateData()
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
        /// 輸入關鍵字事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void keywordTxtBox_TextChanged(object sender, EventArgs e)
        {
            HighlightSearchKeywordFromContent();
        }

        /// <summary>
        /// ListBox選擇檔案顯示分析結果
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectedFileListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            CreateContentForms();

            LoadJsonDataToDataGridView();

            LoadWordCountDataGridView();
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

            foreach (var item in SelectedFileListBox.Items)
            {
                files.Add(item.ToString());
            }

            string pythonArguments = string.Format("{0}|{1}", "", string.Join('&', files));
            string keywordSearchResult = GetKeywordSearchIRResult(pythonArguments);
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

        private void CreateContentForms()
        {
            if (SelectedFileListBox.Items.Count == 0)
            {
                return;
            }

            List<PythonResult> jsonObjs = new List<PythonResult>();

            if (File.Exists(_jsonPath))
            {
                var content = File.ReadAllText(_jsonPath, encoding: Encoding.UTF8);

                jsonObjs = JsonConvert.DeserializeObject<List<PythonResult>>(content);
            }

            #region 顯示內文
            flowLayoutPanel1.Controls.Clear();

            if (SelectedFileListBox.SelectedItems.Count > 0)
            {
                jsonObjs = jsonObjs.Where(x => SelectedFileListBox.SelectedItems.Contains(x.FileName)).ToList();
                foreach (var item in SelectedFileListBox.SelectedItems)
                {
                    var file = item as string;
                    string content = string.Empty;

                    if (File.Exists(file))
                    {
                        content = jsonObjs.Where(x => file.Contains(x.FileName)).Select(x => x.Content).FirstOrDefault();
                    }

                    Content contentForm = new Content(Path.GetFileName(file), content);
                    contentForm.TopLevel = false;
                    flowLayoutPanel1.Controls.Add(contentForm);
                    contentForm.Show();
                }
            }
            else
            {
                foreach (var item in SelectedFileListBox.Items)
                {
                    var file = item as string;
                    string content = string.Empty;

                    if (File.Exists(file))
                    {
                        content = jsonObjs.Where(x => file.Contains(x.FileName)).Select(x => x.Content).FirstOrDefault();
                    }

                    Content contentForm = new Content(Path.GetFileName(file), content);
                    contentForm.TopLevel = false;
                    flowLayoutPanel1.Controls.Add(contentForm);
                    contentForm.Show();
                }
            }
            #endregion
        }

        /// <summary>
        /// 取得內文分析結果
        /// </summary>
        private void LoadJsonDataToDataGridView()
        {
            try
            {
                if (!File.Exists(_jsonPath))
                {
                    return;
                }

                var jsonData = File.ReadAllText(_jsonPath);

                var records = JsonConvert.DeserializeObject<List<PythonResult>>(jsonData);

                if (SelectedFileListBox.SelectedItems.Count > 0)
                {
                    records = records.Where(x => SelectedFileListBox.SelectedItems.Contains(x.FileName)).ToList();
                }

                dataGridView1.DataSource = null;
                dataGridView1.DataSource = records.Select(record => new DataGridDto
                {
                    FileName = Path.GetFileName(record.FileName),
                    CharCountIncludingSpaces = record.CharCountIncludingSpaces,
                    CharCountExcludingSpaces = record.CharCountExcludingSpaces,
                    SentenceCount = record.SentenceCount,
                    WordsCount = record.WordsCount,
                    NonAsciiChars = record.NonAsciiChars,
                    NonAsciiWords = record.NonAsciiWords
                }).ToList();
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
            if (!File.Exists(_jsonPath))
            {
                return;
            }

            var jsonData = File.ReadAllText(_jsonPath);

            var records = JsonConvert.DeserializeObject<List<PythonResult>>(jsonData);

            if (SelectedFileListBox.SelectedItems.Count > 0)
            {
                records = records.Where(x => SelectedFileListBox.SelectedItems.Contains(x.FileName)).ToList();
            }

            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("Word", typeof(string));
            dataTable.Columns.Add("Count", typeof(int));
            dataTable.Columns.Add("FileName", typeof(string));

            foreach (var record in records)
            {
                foreach (var data in record.FrequencyWords)
                {
                    dataTable.Rows.Add(data.Key, data.Value, Path.GetFileName(record.FileName));
                }
            }

            //dataGridView2.DataSource = null;
            dataGridView2.DataSource = dataTable;
        }

        /// <summary>
        /// 關鍵字搜尋
        /// </summary>
        /// <returns></returns>
        string GetKeywordSearchIRResult(string keyword)
        {
            string exePath = ConfigurationManager.AppSettings["exePath"];
            string filePath = ConfigurationManager.AppSettings["filePath"];

            string arguments = keyword;

            if (!File.Exists(exePath) || !File.Exists(filePath))
            {
                MessageBox.Show("找不到Python應用程式與執行檔，\r\n" +
                    "請在App.config檔中設定\r\n" +
                    "該電腦的python應用程式路徑與\r\n" +
                    "Text.py執行檔路徑");
                return "EXE PATH OR PYTHON PATH FAILED";
            }

            #region 取得Python執行結果
            string pythonResult = ProcessClass.StartPython(exePath, filePath, arguments);

            if (!File.Exists(pythonResult))
            {
                return "ERROR：" + pythonResult;
            }
            #endregion

            _jsonPath = pythonResult;

            ///讀取本次結果
            string result = File.ReadAllText(pythonResult, encoding: Encoding.UTF8);

            return result;
        }
        #endregion
    }
}

