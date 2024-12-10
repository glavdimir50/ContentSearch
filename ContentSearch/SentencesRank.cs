using CallPython;
using System.Configuration;
using System.Text;
using System.Text.RegularExpressions;

namespace ContentSearch
{
    public partial class SentencesRank : Form
    {
        private string selectedFiles {  get; set; }
        public SentencesRank(string selectedFiles)
        {
            this.selectedFiles = selectedFiles;
            InitializeComponent();
            GetData();
        }

        private void GetData()
        {
            var resultPath = GetPythonResult();
            string cleanFilePath = Regex.Replace(resultPath.Trim(), @"[\r\n]+", "");
            //resultPath.Replace("\r\n", "");

            if (!File.Exists(cleanFilePath))
            {
                MessageBox.Show(cleanFilePath);

                textBox1.Text = "ERROR：" + cleanFilePath;
            }
            else
            {
                textBox1.Text = File.ReadAllText(cleanFilePath, encoding: Encoding.UTF8);
            }
        }

        /// <summary>
        /// 取得Python結果
        /// </summary>
        /// <returns></returns>
        private string GetPythonResult()
        {
            string exePath = ConfigurationManager.AppSettings["exePath"];
            string filePath = ConfigurationManager.AppSettings["sentenceRankPath"];

            if (!File.Exists(exePath) || !File.Exists(filePath))
            {
                MessageBox.Show("找不到Python應用程式與執行檔，\r\n" +
                    "請在App.config檔中設定\r\n" +
                    "該電腦的python應用程式路徑與\r\n" +
                    "Text.py執行檔路徑");
                return "EXE PATH OR PYTHON PATH FAILED";
            }

            var arg = "";

            if (File.Exists(selectedFiles))
            {
                arg = selectedFiles;
            }

            return ProcessClass.StartPython(exePath, filePath, arg);
        }
    }
}
