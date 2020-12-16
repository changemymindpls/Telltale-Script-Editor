using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Telltale_Script_Editor.FileManagement
{
    public class FileTreeManager
    {

        private TreeView      treeView;
        private ProgressBar   progressBar;

        private DirectoryInfo mDirectory;

        /// <summary>
        /// Manages the file tree.
        /// </summary>
        /// <param name="x">The TreeView to use</param>
        /// <param name="y">The directory to monitor</param>
        public FileTreeManager(TreeView x, string y, ProgressBar z = null)
        {
            this.treeView = x;
            this.mDirectory = new DirectoryInfo(y);
            this.progressBar = z;

            PopulateFileTree();
        }

        /// <summary>
        /// (Re)populates the file tree
        /// </summary>
        public void PopulateFileTree()
        {

            if(progressBar != null) 
            {
                progressBar.Value = 0;
                progressBar.Maximum = 
                    Directory.GetFiles(mDirectory.FullName, "*.*", SearchOption.AllDirectories).Length 
                    + Directory.GetDirectories(mDirectory.FullName, "**", SearchOption.AllDirectories).Length;
            }

            TreeViewItem root = CreateTVItem(mDirectory.Name, true);
            treeView.Items.Add(root);

            PopulateFiles(mDirectory.FullName, root);
            PopulateDirectories(mDirectory.FullName, root);
        }
       
        /// <summary>
        /// Populate the files within the directory & its subdirectories.
        /// </summary>
        /// <param name="x">The directory which you wish to populate</param>
        /// <param name="y">The TreeViewItem representing said directory</param>
        private void PopulateFiles(string x, TreeViewItem y)
        {
            string[] dFiles = Directory.GetFiles(x, "*.*");

            foreach (string file in dFiles)
            {
                FileInfo fi = new FileInfo(file);
                y.Items.Add(CreateTVItem(fi.Name, false));
                UpdateProgress();
            }
        }

        /// <summary>
        /// Populate the directories within the directory
        /// </summary>
        /// <param name="x">The root directory which you wish to populate</param>
        /// <param name="y">The TreeViewItem representing said directory</param>
        /// <remarks>
        /// Recursive.
        /// </remarks>
        private void PopulateDirectories(string x, TreeViewItem y)
        {
            string[] dDirectories = Directory.GetDirectories(x);
            foreach (string subdirectory in dDirectories)
            {
                DirectoryInfo di = new DirectoryInfo(subdirectory);
                TreeViewItem created = CreateTVItem(di.Name, true);
                y.Items.Add(created);

                PopulateFiles(subdirectory, created);
                PopulateDirectories(subdirectory, created);
                UpdateProgress();
            }
        }

        /// <summary>
        /// Update the operation progress.
        /// </summary>
        private void UpdateProgress()
        {
            if (progressBar != null)
            { 
                double x = (((double)progressBar.Value / (double)progressBar.Maximum) * 100);
                double percent = Math.Round(x, 1);
                if (progressBar.Value < progressBar.Maximum)
                {
                    progressBar.Value++;
                }
            }
            //Application.DoEvents();
        }

        /// <summary>
        /// Wrapper for TreeViewItem to allow for easier reading.
        /// </summary>
        /// <param name="x">Item Name</param>
        /// <param name="y">Is Directory?</param>
        private TreeViewItem CreateTVItem(string x, bool y)
        {
            Console.WriteLine($"Creating TVI!\n\nName: {x}\n\nIs Directory?: {y}");
            TreeViewItem tc =
                new TreeViewItem();

            tc.Header = x;
            if (y) tc.Tag = "Directory"; else tc.Tag = "File";
            
            return tc;
        }

        /// <summary>
        /// Clears the Tree View & destroys the manager.
        /// </summary>
        public void Destroy()
        {

        }

    }
}
