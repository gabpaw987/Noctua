namespace Algorithm_Interface_Prototype
{
    partial class Form1
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.CSVFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.CSVBrowseButton = new System.Windows.Forms.Button();
            this.CSVTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.AlgorithmBrowseButton = new System.Windows.Forms.Button();
            this.AlgrithmFileTextBox = new System.Windows.Forms.TextBox();
            this.AlgorithmFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.startButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // CSVFileDialog
            // 
            this.CSVFileDialog.FileName = "CSVFileDialog";
            // 
            // CSVBrowseButton
            // 
            this.CSVBrowseButton.Location = new System.Drawing.Point(255, 50);
            this.CSVBrowseButton.Name = "CSVBrowseButton";
            this.CSVBrowseButton.Size = new System.Drawing.Size(75, 23);
            this.CSVBrowseButton.TabIndex = 0;
            this.CSVBrowseButton.Text = "browse";
            this.CSVBrowseButton.UseVisualStyleBackColor = true;
            this.CSVBrowseButton.Click += new System.EventHandler(this.CSVBrowseButton_Click);
            // 
            // CSVTextBox
            // 
            this.CSVTextBox.Location = new System.Drawing.Point(12, 51);
            this.CSVTextBox.Name = "CSVTextBox";
            this.CSVTextBox.Size = new System.Drawing.Size(237, 22);
            this.CSVTextBox.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 31);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(179, 17);
            this.label1.TabIndex = 2;
            this.label1.Text = "Choose the Input Data File:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 92);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(173, 17);
            this.label2.TabIndex = 3;
            this.label2.Text = "Choose the Algorithm File:";
            // 
            // AlgorithmBrowseButton
            // 
            this.AlgorithmBrowseButton.Location = new System.Drawing.Point(255, 112);
            this.AlgorithmBrowseButton.Name = "AlgorithmBrowseButton";
            this.AlgorithmBrowseButton.Size = new System.Drawing.Size(75, 23);
            this.AlgorithmBrowseButton.TabIndex = 4;
            this.AlgorithmBrowseButton.Text = "browse";
            this.AlgorithmBrowseButton.UseVisualStyleBackColor = true;
            this.AlgorithmBrowseButton.Click += new System.EventHandler(this.AlgorithmBrowseButton_Click);
            // 
            // AlgrithmFileTextBox
            // 
            this.AlgrithmFileTextBox.Location = new System.Drawing.Point(13, 113);
            this.AlgrithmFileTextBox.Name = "AlgrithmFileTextBox";
            this.AlgrithmFileTextBox.Size = new System.Drawing.Size(236, 22);
            this.AlgrithmFileTextBox.TabIndex = 5;
            // 
            // AlgorithmFileDialog
            // 
            this.AlgorithmFileDialog.FileName = "openFileDialog1";
            // 
            // startButton
            // 
            this.startButton.Location = new System.Drawing.Point(13, 161);
            this.startButton.Name = "startButton";
            this.startButton.Size = new System.Drawing.Size(317, 35);
            this.startButton.TabIndex = 6;
            this.startButton.Text = "Start Calculation";
            this.startButton.UseVisualStyleBackColor = true;
            this.startButton.Click += new System.EventHandler(this.startButton_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(342, 208);
            this.Controls.Add(this.startButton);
            this.Controls.Add(this.AlgrithmFileTextBox);
            this.Controls.Add(this.AlgorithmBrowseButton);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.CSVTextBox);
            this.Controls.Add(this.CSVBrowseButton);
            this.Name = "Form1";
            this.Text = "Backtesting Software";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.OpenFileDialog CSVFileDialog;
        private System.Windows.Forms.Button CSVBrowseButton;
        private System.Windows.Forms.TextBox CSVTextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button AlgorithmBrowseButton;
        private System.Windows.Forms.TextBox AlgrithmFileTextBox;
        private System.Windows.Forms.OpenFileDialog AlgorithmFileDialog;
        private System.Windows.Forms.Button startButton;
    }
}

