namespace ContentSearch
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            keywordTxtBox = new TextBox();
            label2 = new Label();
            label4 = new Label();
            selectFileBtn = new Button();
            SelectedFileListBox = new ListBox();
            label1 = new Label();
            dataGridView1 = new DataGridView();
            flowLayoutPanel1 = new FlowLayoutPanel();
            label6 = new Label();
            dataGridView2 = new DataGridView();
            label7 = new Label();
            KeywordSearchbtn = new Button();
            comboBox1 = new ComboBox();
            label3 = new Label();
            comboBox2 = new ComboBox();
            label5 = new Label();
            comboBox3 = new ComboBox();
            label8 = new Label();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dataGridView2).BeginInit();
            SuspendLayout();
            // 
            // keywordTxtBox
            // 
            keywordTxtBox.Location = new Point(186, 262);
            keywordTxtBox.Margin = new Padding(4);
            keywordTxtBox.Name = "keywordTxtBox";
            keywordTxtBox.Size = new Size(294, 27);
            keywordTxtBox.TabIndex = 3;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(57, 267);
            label2.Margin = new Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new Size(121, 19);
            label2.TabIndex = 4;
            label2.Text = "KeywordSearch:";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(57, 367);
            label4.Margin = new Padding(4, 0, 4, 0);
            label4.Name = "label4";
            label4.Size = new Size(55, 19);
            label4.TabIndex = 9;
            label4.Text = "Result:";
            // 
            // selectFileBtn
            // 
            selectFileBtn.Location = new Point(384, 30);
            selectFileBtn.Margin = new Padding(4);
            selectFileBtn.Name = "selectFileBtn";
            selectFileBtn.Size = new Size(96, 29);
            selectFileBtn.TabIndex = 11;
            selectFileBtn.Text = "SelectFile";
            selectFileBtn.UseVisualStyleBackColor = true;
            selectFileBtn.Click += selectFileBtn_Click;
            // 
            // SelectedFileListBox
            // 
            SelectedFileListBox.FormattingEnabled = true;
            SelectedFileListBox.ItemHeight = 19;
            SelectedFileListBox.Location = new Point(57, 73);
            SelectedFileListBox.Margin = new Padding(4);
            SelectedFileListBox.Name = "SelectedFileListBox";
            SelectedFileListBox.ScrollAlwaysVisible = true;
            SelectedFileListBox.SelectionMode = SelectionMode.MultiExtended;
            SelectedFileListBox.Size = new Size(424, 175);
            SelectedFileListBox.TabIndex = 12;
            SelectedFileListBox.SelectedIndexChanged += SelectedFileListBox_SelectedIndexChanged;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(57, 35);
            label1.Margin = new Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new Size(98, 19);
            label1.TabIndex = 13;
            label1.Text = "Upload Files:";
            // 
            // dataGridView1
            // 
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.Location = new Point(509, 73);
            dataGridView1.Margin = new Padding(4);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.RowHeadersWidth = 51;
            dataGridView1.Size = new Size(791, 287);
            dataGridView1.TabIndex = 21;
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.AutoScroll = true;
            flowLayoutPanel1.BackColor = SystemColors.ActiveBorder;
            flowLayoutPanel1.BorderStyle = BorderStyle.Fixed3D;
            flowLayoutPanel1.Location = new Point(59, 398);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Padding = new Padding(10);
            flowLayoutPanel1.Size = new Size(1241, 607);
            flowLayoutPanel1.TabIndex = 24;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(510, 35);
            label6.Name = "label6";
            label6.Size = new Size(73, 19);
            label6.TabIndex = 26;
            label6.Text = "Statistics:";
            // 
            // dataGridView2
            // 
            dataGridView2.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView2.Location = new Point(1319, 73);
            dataGridView2.Name = "dataGridView2";
            dataGridView2.RowHeadersWidth = 51;
            dataGridView2.Size = new Size(521, 930);
            dataGridView2.TabIndex = 27;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(1319, 35);
            label7.Name = "label7";
            label7.Size = new Size(98, 19);
            label7.TabIndex = 28;
            label7.Text = "Word Count:";
            // 
            // KeywordSearchbtn
            // 
            KeywordSearchbtn.Location = new Point(387, 295);
            KeywordSearchbtn.Name = "KeywordSearchbtn";
            KeywordSearchbtn.Size = new Size(94, 29);
            KeywordSearchbtn.TabIndex = 30;
            KeywordSearchbtn.Text = "Search";
            KeywordSearchbtn.UseVisualStyleBackColor = true;
            KeywordSearchbtn.Click += KeywordSearchbtn_Click;
            // 
            // comboBox1
            // 
            comboBox1.FormattingEnabled = true;
            comboBox1.Items.AddRange(new object[] { "AND", "OR" });
            comboBox1.Location = new Point(186, 298);
            comboBox1.Margin = new Padding(4);
            comboBox1.Name = "comboBox1";
            comboBox1.Size = new Size(86, 27);
            comboBox1.TabIndex = 31;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(57, 305);
            label3.Margin = new Padding(4, 0, 4, 0);
            label3.Name = "label3";
            label3.Size = new Size(100, 19);
            label3.TabIndex = 32;
            label3.Text = "SearchMode:";
            // 
            // comboBox2
            // 
            comboBox2.FormattingEnabled = true;
            comboBox2.Items.AddRange(new object[] { "false", "true" });
            comboBox2.Location = new Point(186, 333);
            comboBox2.Margin = new Padding(4);
            comboBox2.Name = "comboBox2";
            comboBox2.Size = new Size(59, 27);
            comboBox2.TabIndex = 33;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(59, 339);
            label5.Margin = new Padding(4, 0, 4, 0);
            label5.Name = "label5";
            label5.Size = new Size(127, 19);
            label5.TabIndex = 34;
            label5.Text = "Word2VecMode:";
            // 
            // comboBox3
            // 
            comboBox3.FormattingEnabled = true;
            comboBox3.Items.AddRange(new object[] { "CBOW", "SG" });
            comboBox3.Location = new Point(387, 333);
            comboBox3.Name = "comboBox3";
            comboBox3.Size = new Size(93, 27);
            comboBox3.TabIndex = 35;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(260, 339);
            label8.Margin = new Padding(4, 0, 4, 0);
            label8.Name = "label8";
            label8.Size = new Size(124, 19);
            label8.TabIndex = 36;
            label8.Text = "AlgorithmMode:";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(9F, 19F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoScroll = true;
            ClientSize = new Size(1862, 1055);
            Controls.Add(label8);
            Controls.Add(comboBox3);
            Controls.Add(label5);
            Controls.Add(comboBox2);
            Controls.Add(label3);
            Controls.Add(comboBox1);
            Controls.Add(KeywordSearchbtn);
            Controls.Add(label7);
            Controls.Add(dataGridView2);
            Controls.Add(label6);
            Controls.Add(flowLayoutPanel1);
            Controls.Add(dataGridView1);
            Controls.Add(label1);
            Controls.Add(SelectedFileListBox);
            Controls.Add(selectFileBtn);
            Controls.Add(label4);
            Controls.Add(label2);
            Controls.Add(keywordTxtBox);
            Margin = new Padding(4);
            Name = "Form1";
            Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            ((System.ComponentModel.ISupportInitialize)dataGridView2).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private TextBox keywordTxtBox;
        private Label label2;
        private Label label4;
        private Button selectFileBtn;
        private ListBox SelectedFileListBox;
        private Label label1;
        private DataGridView dataGridView1;
        private FlowLayoutPanel flowLayoutPanel1;
        private Label label6;
        private Label label7;
        public DataGridView dataGridView2;
        private Button KeywordSearchbtn;
        private ComboBox comboBox1;
        private Label label3;
        private ComboBox comboBox2;
        private Label label5;
        private ComboBox comboBox3;
        private Label label8;
    }
}
