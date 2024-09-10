using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FileExplorer
{
    public partial class TextEditor : Form
    {
        private string filePath;

        public TextEditor(string filePath)
        {
            InitializeComponent();
            this.filePath = filePath;
            LoadFile();
        }

        private void LoadFile()
        {
            try
            {
                richTextBox1.Text = File.ReadAllText(filePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading file: {ex.Message}");
            }
        }

        private void Save_Click(object sender, EventArgs e)
        {
            try
            {
                File.WriteAllText(filePath, richTextBox1.Text);
                MessageBox.Show("File saved successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving file: {ex.Message}");
            }
        }
    }
}
