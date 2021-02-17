using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Telltale_Script_Editor.GUI;

namespace Telltale_Script_Editor.FileManagement
{
    public class FileTreeManager
    {
        private TreeView treeView;
        private ProgressBar progressBar;
        public EditorPanelManager editorPanelManager;

        private DirectoryInfo mDirectory;

        private bool allowItemCheck = false;

        /// <summary>
        /// Manages the file tree.
        /// </summary>
        /// <remarks>
        /// TODO:
        /// add support for verbose console output setting - as of now it just prints *everything*
        /// 
        /// This method is really, really broken.
        /// I'm not even sure why - don't ask. Threading completely breaks the program and causes exceptions with little to no explanation or widespread fixes.
        /// It's probably ill-advised to try to fix the optimization here even though it might be bad on paper.
        /// 
        /// Hours Wasted Here: 20
        /// 
        /// </remarks>  
        public FileTreeManager(TreeView treeView, string directory, EditorPanelManager editorPanelManager, ProgressBar progressBar = null, bool allowItemCheck = false)
        {
            this.treeView = treeView;
            this.mDirectory = new DirectoryInfo(directory);
            this.editorPanelManager = editorPanelManager;
            this.progressBar = progressBar;
            this.allowItemCheck = allowItemCheck;

            var b = PopulateFileTree();
            treeView.Items.Add(b);
            b.IsExpanded = true;
        }

        public void RepopulateFileTree()
        {
            var fileTree = PopulateFileTree();

            treeView.Items.Clear();
            treeView.Items.Add(fileTree);

            fileTree.IsExpanded = true;
        }

        /// <summary>
        /// (Re)populates the file tree
        /// </summary>
        public TreeViewItem PopulateFileTree()
        {
            if(progressBar != null) 
            {
                progressBar.Value = 0;
                progressBar.Maximum = Directory.GetFiles(mDirectory.FullName, "*.*", SearchOption.AllDirectories).Length + Directory.GetDirectories(mDirectory.FullName, "**", SearchOption.AllDirectories).Length;
            }

            TreeViewItem root = CreateTreeViewItem(mDirectory.Name, true, false);
            PopulateFiles(mDirectory.FullName, root);
            PopulateDirectories(mDirectory.FullName, root);

            return root;
        }
       
        /// <summary>
        /// Populate the files within the directory & its subdirectories.
        /// </summary>
        private void PopulateFiles(string directory, TreeViewItem directoryTreeViewItem)
        {
            string[] dFiles = Directory.GetFiles(directory, "*.*");

            foreach (string file in dFiles)
            {
                FileInfo fi = new FileInfo(file);
                directoryTreeViewItem.Items.Add(CreateTreeViewItem(fi.Name, false, false, fi.FullName));
                UpdateProgress();
            }
        }

        /// <summary>
        /// Populate the directories within the directory
        /// </summary>
        /// <remarks>
        /// Recursive.
        /// </remarks>
        private void PopulateDirectories(string directory, TreeViewItem directoryTreeViewItem)
        {
            string[] dDirectories = Directory.GetDirectories(directory);

            foreach (string subdirectory in dDirectories)
            {
                DirectoryInfo di = new DirectoryInfo(subdirectory);
                TreeViewItem created = CreateTreeViewItem(di.Name, true, allowItemCheck, di.FullName);
                directoryTreeViewItem.Items.Add(created);

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
        }

        /// <summary>
        /// Wrapper for TreeViewItem to allow for easier reading.
        /// </summary>
        private TreeViewItem CreateTreeViewItem(string itemName, bool isDirectory, bool includesCheckbox = false, string fullDirectory = "")
        {
            //Console.WriteLine($"Creating TVI!\n\nName: {x}\n\nIs Directory?: {y}");

            TreeViewItem tc = new TreeViewItem();
            
            if (includesCheckbox)
                tc.Header = new CheckBox() { Content = itemName };
            else
                tc.Header = itemName;

            if (isDirectory) 
                tc.Tag = new string[] { "Directory", fullDirectory }; 
            else 
                tc.Tag = new string[] { "File", fullDirectory };

            if(!allowItemCheck)
                tc.MouseDoubleClick += TreeViewDoubleClick;
            
            return tc;
        }

        
        /// <summary>
        /// Double click event for TreeViewItems
        /// </summary>
        private void TreeViewDoubleClick(object eventObject, MouseButtonEventArgs eventArg)
        {
            var treeViewItem = eventObject as TreeViewItem;
            var treeViewItemTag = treeViewItem.Tag as string[];

            if (treeViewItemTag[0] == "File")
            {
                var fileType = Path.GetExtension(treeViewItemTag[1]).ToLower();

                if (fileType == ".tseproj")
                {
                    editorPanelManager.OpenProjectFile(treeViewItemTag[1]);
                }
                else if (fileType == ".lua")
                {
                    if (editorPanelManager.OpenTextFile(treeViewItemTag[1]))
                        editorPanelManager.SetSyntaxHighlighting("Lua");
                }
                else if(fileType == ".txt")
                {
                    if(editorPanelManager.OpenTextFile(treeViewItemTag[1]))
                        editorPanelManager.SetSyntaxHighlighting();
                }
                else if (fileType == ".dds" || fileType == ".d3dtx" || fileType == ".header")
                {
                    editorPanelManager.OpenImageFile(treeViewItemTag[1]);
                }
                //else if (fType == ".wav")
                //{
                //    editorPanelManager.OpenImageFile(treeViewItemTag[1]);
                //}
            }

            editorPanelManager.mainWindow.UpdateUI();
        }

        public string TreeView_GetSelectedFilePath(ref bool isDirectory)
        {
            string selectedItemPath = "";

            TreeViewItem treeViewItem = (TreeViewItem)treeView.SelectedItem;

            if(treeViewItem.Tag != null)
            {
                string[] treeViewItemTag = (string[])treeViewItem.Tag;

                if (treeViewItemTag[0].Equals("Directory"))
                {
                    isDirectory = true;

                    selectedItemPath = treeViewItemTag[1];
                }
                else if (treeViewItemTag[0].Equals("File"))
                {
                    isDirectory = false;

                    selectedItemPath = treeViewItemTag[1];
                }
            }

            return selectedItemPath;
        }

        /// <summary>
        /// Clears the Tree View & destroys the manager.
        /// </summary>
        public void Destroy()
        {
            treeView.Items.Clear();
            mDirectory = null;
            progressBar = null;
            treeView = null;

            Console.WriteLine("Destroyed file tree manager!");
        }
    }
}