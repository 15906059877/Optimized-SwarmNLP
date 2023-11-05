namespace SwarmNLP
{
    partial class WindowForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WindowForm));
            this.RangeList = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.MinBox = new System.Windows.Forms.TextBox();
            this.MaxBox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.ScaleBox = new System.Windows.Forms.TextBox();
            this.SetButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // RangeList
            // 
            this.RangeList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.RangeList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3});
            this.RangeList.FullRowSelect = true;
            this.RangeList.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.RangeList.HideSelection = false;
            this.RangeList.Location = new System.Drawing.Point(12, 12);
            this.RangeList.MultiSelect = false;
            this.RangeList.Name = "RangeList";
            this.RangeList.Size = new System.Drawing.Size(225, 141);
            this.RangeList.TabIndex = 0;
            this.RangeList.UseCompatibleStateImageBehavior = false;
            this.RangeList.View = System.Windows.Forms.View.Details;
            this.RangeList.SelectedIndexChanged += new System.EventHandler(this.RangeList_SelectedIndexChanged);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Dimension";
            this.columnHeader1.Width = 80;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Min";
            this.columnHeader2.Width = 61;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Max";
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(26, 162);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(24, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Min";
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(139, 162);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(27, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Max";
            // 
            // MinBox
            // 
            this.MinBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.MinBox.Location = new System.Drawing.Point(56, 159);
            this.MinBox.Name = "MinBox";
            this.MinBox.Size = new System.Drawing.Size(65, 20);
            this.MinBox.TabIndex = 4;
            this.MinBox.TextChanged += new System.EventHandler(this.MinBox_TextChanged);
            // 
            // MaxBox
            // 
            this.MaxBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.MaxBox.Location = new System.Drawing.Point(172, 159);
            this.MaxBox.Name = "MaxBox";
            this.MaxBox.Size = new System.Drawing.Size(65, 20);
            this.MaxBox.TabIndex = 5;
      