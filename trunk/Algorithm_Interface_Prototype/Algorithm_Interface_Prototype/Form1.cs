using System;
using System.Reflection;
using System.Windows.Forms;

namespace Algorithm_Interface_Prototype
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void CSVBrowseButton_Click(object sender, EventArgs e)
        {
            CSVFileDialog.ShowDialog();
            CSVTextBox.Text = CSVFileDialog.FileName;
        }

        private void AlgorithmBrowseButton_Click(object sender, EventArgs e)
        {
            AlgorithmFileDialog.ShowDialog();
            AlgrithmFileTextBox.Text = AlgorithmFileDialog.FileName;
        }

        private void startButton_Click(object sender, EventArgs e)
        {
            mainClass.startCalculation(CSVTextBox.Text, AlgrithmFileTextBox.Text);
        }
    }
}