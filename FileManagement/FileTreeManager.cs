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

        private TreeView      treeView;
        private ProgressBar   progressBar;
        private EditorPanelManager epManager;

        private DirectoryInfo mDirectory;


        private bool allowItemCheck = false;

        /// <summary>
        /// Manages the file tree.
        /// </summary>
        /// <param name="x">The TreeView to use</param>
        /// <param name="y">The directory to monitor</param>
        /// <param name="z">The ProgressBar to use</param>
        /// <param name="a">Should items be checkable?</param>
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
        public FileTreeManager(TreeView w, string x, EditorPanelManager y, ProgressBar z = null, bool a = false)
        {
            this.treeView = w;
            this.mDirectory = new DirectoryInfo(x);
            this.epManager = y;
            this.progressBar = z;
            this.allowItemCheck = a;

            treeView.Items.Add(PopulateFileTree());
        }

        /// <summary>
        /// (Re)populates the file tree
        /// </summary>
        public TreeViewItem PopulateFileTree()
        {

            if(progressBar != null) 
            {
                progressBar.Value = 0;
                progressBar.Maximum = 
                    Directory.GetFiles(mDirectory.FullName, "*.*", SearchOption.AllDirectories).Length 
                    + Directory.GetDirectories(mDirectory.FullName, "**", SearchOption.AllDirectories).Length;
            }

            TreeViewItem root = CreateTVItem(mDirectory.Name, true, false);
            PopulateFiles(mDirectory.FullName, root);
            PopulateDirectories(mDirectory.FullName, root);

            return root;
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
                y.Items.Add(CreateTVItem(fi.Name, false, false, fi.FullName));
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
                TreeViewItem created = CreateTVItem(di.Name, true, allowItemCheck, di.FullName);
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
        }

        /// <summary>
        /// Wrapper for TreeViewItem to allow for easier reading.
        /// </summary>
        /// <param name="x">Item Name</param>
        /// <param name="y">Is Directory?</param>
        /// <param name="z">Includes Checkbox?</param>
        /// <param name="a">Full Directory</param>
        private TreeViewItem CreateTVItem(string x, bool y, bool z = false, string a = "")
        {
            //Console.WriteLine($"Creating TVI!\n\nName: {x}\n\nIs Directory?: {y}");

            TreeViewItem tc =
                new TreeViewItem();
            
            if (z)
                tc.Header = new CheckBox() { Content = x };
            else
                tc.Header = x;

            if (y) tc.Tag = new string[] { "Directory", a }; else tc.Tag = new string[] { "File", a };

            if(!allowItemCheck)
                tc.MouseDoubleClick += TreeViewDoubleClick;
            
            return tc;
        }

        
        /// <summary>
        /// Double click event for TreeViewItems
        /// </summary>
        /// <param name="x">The TreeViewItem where the event originated.</param>
        /// <param name="y">MouseButtonEventArgs</param>
        private void TreeViewDoubleClick(object x, MouseButtonEventArgs y)
        {
            var tc = x as TreeViewItem;
            var tag = tc.Tag as string[];

            if (tag[0] == "File")
            {
                var fType = Path.GetExtension(tag[1]).ToLower();
                if (fType == ".tseproj")
                {
                    epManager.OpenProjectFile(tag[1]);
                }
                else if (fType == ".lua")
                {
                    if (epManager.OpenTextFile(tag[1]))
                        epManager.SetSyntaxHighlighting("Lua");
                }
                else if(fType == ".txt")
                {
                    if(epManager.OpenTextFile(tag[1]))
                        epManager.SetSyntaxHighlighting();
                }
                else if (fType == ".dds")
                {
                    epManager.OpenImageFile(tag[1]);
                }
            }
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