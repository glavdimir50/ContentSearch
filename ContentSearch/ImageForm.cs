namespace ContentSearch
{
    public partial class ImageForm : Form
    {
        public ImageForm(string imagePath)
        {
            InitializeComponent();
            using (FileStream fs = new FileStream(imagePath, FileMode.Open, FileAccess.Read))
            {
                Image image = Image.FromStream(fs);

                pictureBox1.Image = new Bitmap(image);
            }

            // 設定表單屬性
            this.Text = Path.GetFileName(imagePath);
            this.Size = new System.Drawing.Size(1200, 960);
        }
    }
}
