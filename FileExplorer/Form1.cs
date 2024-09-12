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
using System.Diagnostics;
using Microsoft.VisualBasic;

namespace FileExplorer
{
    public partial class Form1 : Form
    {
        private ImageList imageList = new ImageList();
        private ContextMenuStrip contextMenuStrip1;
        private string currentDirectory;

        private List<string> protectedFiles = new List<string>
        {
            // Some hard coded protection :)
            @"C:\Windows",
            @"C:\test\important.txt",
            @"C:\System32",
            @"C:\Windows\system32",
            @"C:\Windows\SysWOW64",
            @"C:\Windows\WINSxS",
            @"C:\Windows\Fonts",
            @"C:\bootmgr",
            @"C:\Boot\BCD",
            @"C:\boot\bcd.log",
            @"C:\Windows\Boot\EFI",
            @"C:\Boot\BCD",
            @"C:\Windows\System32\config\SYSTEM",
            @"C:\Windows\System32\config\SOFTWARE",
            @"C:\Windows\System32\config\SAM",
            @"C:\Windows\System32\config\SECURITY",
            @"C:\Windows\System32\config\DEFAULT",
            @"C:\Users",
            @"C:\Users\Default",
            @"C:\Program Files",
            @"C:\Program Files (x86)",
            @"C:\Windows\System32\ntoskrnl.exe",
            @"C:\Windows\System32\winload.exe",
            @"C:\Windows\System32\hal.dll",
            @"C:\Windows\System32\lsass.exe",
            @"C:\Windows\System32\svchost.exe",
            @"C:\Recovery",
            @"C:\Windows\System32\Recovery",
            @"C:\Windows\System32\winre.wim",
            @"C:\pagefile.sys",
            @"C:\hiberfil.sys",
        };

        // Better be safe than sorry :D
        private bool IsProtectedFile(string fullPath)
        {
            // Check if the file or directory is in the protected list
            if (protectedFiles.Contains(fullPath, StringComparer.OrdinalIgnoreCase))
            {
                return true; // File is protected
            }

            // Check if the file has read-only, system, or hidden attributes
            FileAttributes attributes;
            if (File.Exists(fullPath))
            {
                attributes = File.GetAttributes(fullPath);
            }
            else if (Directory.Exists(fullPath))
            {
                attributes = File.GetAttributes(fullPath);
            }
            else
            {
                return false; // Path does not exist, so it's not protected
            }

            if ((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly ||
                (attributes & FileAttributes.System) == FileAttributes.System ||
                (attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
            {
                return true; // File is protected due to its attributes
            }

            return false; // File is not protected
        }


        public Form1()
        {
            InitializeComponent();
            InitializeFileExplorer();
            listView1.Dock = DockStyle.Fill;
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

        private void listView1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                ListViewItem clickedItem = listView1.HitTest(e.Location).Item;

                if (clickedItem != null)
                {
                    clickedItem.Selected = true;

                    contextMenuStrip1 = new ContextMenuStrip();

                    ToolStripMenuItem openMenuItem = new ToolStripMenuItem("Open");
                    ToolStripMenuItem deleteMenuItem = new ToolStripMenuItem("Delete");
                    ToolStripMenuItem renameMenuItem = new ToolStripMenuItem("Rename");

                    openMenuItem.Click += new EventHandler(OpenMenuItem_Click);
                    deleteMenuItem.Click += new EventHandler(DeleteMenuItem_Click);
                    renameMenuItem.Click += new EventHandler(RenameMenuItem_Click);

                    contextMenuStrip1.Items.AddRange(new ToolStripItem[] { openMenuItem, deleteMenuItem, renameMenuItem });
                    contextMenuStrip1.Show(listView1, e.Location);
                }
            }
        }

        private void OpenMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0) return;

            ListViewItem selectedItem = listView1.SelectedItems[0];
            string fullPath = selectedItem.Tag.ToString();

            if (File.Exists(fullPath))
            {
                string extension = Path.GetExtension(fullPath);
                if (extension.ToLower() == ".txt")
                {
                    OpenTextFile(fullPath);
                }
                else if (extension.ToLower() == ".exe")
                {
                    Process.Start(fullPath);
                }
            }
            else if (Directory.Exists(fullPath))
            {
                LoadDirectories(fullPath);
            }
        }

        private void DeleteMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0) return;

            ListViewItem selectedItem = listView1.SelectedItems[0];
            string fullPath = selectedItem.Tag.ToString();

            if (IsProtectedFile(fullPath))
            {
                MessageBox.Show("This file or directory is protected and cannot be deleted.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            else if (File.Exists(fullPath) || Directory.Exists(fullPath))
            {
                DialogResult result = MessageBox.Show($"Are you sure you want to delete {fullPath}?",
                    "Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    if (Directory.Exists(fullPath))
                        Directory.Delete(fullPath, true); // Delete the directory
                    else
                        File.Delete(fullPath); // Delete the file

                    listView1.Items.Remove(selectedItem);
                }
            }
        }

        private void RenameMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0) return;

            ListViewItem selectedItem = listView1.SelectedItems[0];
            string fullPath = selectedItem.Tag.ToString();

            if (IsProtectedFile(fullPath))
            {
                MessageBox.Show("This file or directory is protected and cannot be renamed.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            else
            {
                // Ask the user for a new name
                string newName = Microsoft.VisualBasic.Interaction.InputBox("Enter new name:", "Rename", Path.GetFileName(fullPath));
                if (!string.IsNullOrWhiteSpace(newName))
                {
                    string newFullPath = Path.Combine(Path.GetDirectoryName(fullPath), newName);

                    if (File.Exists(fullPath))
                    {
                        File.Move(fullPath, newFullPath); // Rename file
                    }
                    else if (Directory.Exists(fullPath))
                    {
                        Directory.Move(fullPath, newFullPath); // Rename directory
                    }
                    // Update name and tag
                    selectedItem.Text = newName;
                    selectedItem.Tag = newFullPath;
                }
            }
        }
    }
}
