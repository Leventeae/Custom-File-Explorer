using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FileExplorer
{
    public partial class Form1 : Form
    {
        private ImageList imageList = new ImageList();
        private string currentDirectory;

        public Form1()
        {
            InitializeComponent();
            InitializeFileExplorer();
        }

        private void InitializeFileExplorer()
        {
            // Set up the ImageList and add custom icons
            imageList.ImageSize = new Size(32, 32);
            imageList.Images.Add("drive", Image.FromFile("drive.png"));   // Drive Icon
            imageList.Images.Add("folder", Image.FromFile("folder.png")); // Folder Icon
            imageList.Images.Add("txt", Image.FromFile("txt.png"));  // Text Icon
            imageList.Images.Add("exe", Image.FromFile("exe.png"));// Executable Icon
            imageList.Images.Add("file", Image.FromFile("file.png"));     // Default File Icon

            listView1.SmallImageList = imageList;
            listView1.View = View.Details;
            listView1.Columns.Add("Name", 250);
            listView1.Columns.Add("Type", 100);
            listView1.Columns.Add("Size", 100);

            LoadDrives();
        }

        private void LoadDrives()
        {
            listView1.Items.Clear();

            // Get all available drives
            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                if (drive.IsReady)
                {
                    // Add the drive to ListView
                    ListViewItem item = new ListViewItem(drive.Name, "drive");
                    item.Tag = drive.Name;  // Store the drive path in Tag for navigation
                    item.SubItems.Add("Drive");
                    item.SubItems.Add("");
                    listView1.Items.Add(item);
                }
            }

            currentDirectory = null;
        }

        private void LoadDirectories(string path)
        {
            try
            {
                listView1.Items.Clear();
                DirectoryInfo dir = new DirectoryInfo(path);

                // Go back to parent directory
                if (dir.Parent != null)
                {
                    ListViewItem upItem = new ListViewItem("..", "folder");
                    upItem.Tag = dir.Parent.FullName;
                    upItem.SubItems.Add("Parent Directory");
                    listView1.Items.Add(upItem);
                }
                else
                {
                    // If no parent go back to drives
                    ListViewItem upItem = new ListViewItem("..", "folder");
                    upItem.Tag = "drives";
                    upItem.SubItems.Add("Drives");
                    listView1.Items.Add(upItem);
                }

                // Load directories
                foreach (var directory in dir.GetDirectories())
                {
                    ListViewItem item = new ListViewItem(directory.Name, "folder");
                    item.Tag = directory.FullName;  // Store full path in Tag for navigation
                    item.SubItems.Add("Directory");
                    item.SubItems.Add("");  // No size for directories
                    listView1.Items.Add(item);
                }

                // Load files
                foreach (var file in dir.GetFiles())
                {
                    string iconKey = GetFileIcon(file.Extension);
                    ListViewItem item = new ListViewItem(file.Name, iconKey);
                    item.Tag = file.FullName;  // Store full file path for opening
                    item.SubItems.Add(file.Extension);
                    item.SubItems.Add(file.Length.ToString());
                    listView1.Items.Add(item);
                }

                // Update the current directory
                currentDirectory = path;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading directories: " + ex.Message);
            }
        }

        private string GetFileIcon(string extension)
        {
            switch (extension.ToLower())
            {
                case ".txt":
                    return "txt";
                case ".exe":
                    return "exe";
                default:
                    return "file"; // Default file icon
            }
        }

        private void OpenTextFile(string filePath)
        {
            try
            {
                // Create and show the TextEditor
                TextEditor editorForm = new TextEditor(filePath);
                editorForm.ShowDialog();  // Show the form as a dialog
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening file: {ex.Message}");
            }
        }

        private void listView1_ItemActivate_1(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0) return;

            ListViewItem selectedItem = listView1.SelectedItems[0];
            string fullPath = selectedItem.Tag.ToString();

            if (fullPath == "drives")
            {
                // Special case: if Tag is "drives", go back to the drive view
                LoadDrives();
            }
            else if (Directory.Exists(fullPath))
            {
                // Load the contents of the selected folder
                LoadDirectories(fullPath);
            }
            else if (File.Exists(fullPath))
            {
                string extension = Path.GetExtension(fullPath);
                if (extension.ToLower() == ".txt")
                {
                    // Handle opening text files
                    OpenTextFile(fullPath);
                }
                else if (extension.ToLower() == ".exe")
                {
                    Process.Start(fullPath);
                }
                // Add more conditions for other file types (e.g., .exe)
            }
        }
    }
}
