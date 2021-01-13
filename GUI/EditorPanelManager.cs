using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Xml;

namespace Telltale_Script_Editor.GUI
{
    public class EditorPanelManager
    {
        private MainWindow mw;

        private string currentlyOpenFile;

        /// <summary>
        /// Helps with the management of the multiple editor panels.
        /// </summary>
        public EditorPanelManager()
        {
            mw = (MainWindow)Application.Current.MainWindow;
        }

        /// <summary>
        /// Set syntax highlighting
        /// </summary>
        /// <param name="x">The syntax highlighting definition to load</param>
        public void SetSyntaxHighlighting(string x = null)
        {
            if (x == null)
            {
                mw.textEditor.SyntaxHighlighting = null;
                return;
            }

            using (Stream s = Assembly.GetExecutingAssembly().GetManifestResourceStream($"Telltale_Script_Editor.Resources.{x}.xshd"))
            {
                using (XmlTextReader reader = new XmlTextReader(s))
                {
                    mw.textEditor.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
                }
            }
        }

        /// <summary>
        /// Opens & displays a file in the editor as text.
        /// </summary>
        /// <param name="x">The location of the file to open.</param>
        public bool OpenTextFile(string x)
        {
            if (!ResetEditorView())
                return false;

            currentlyOpenFile = x;
            mw.textEditor.Text = File.ReadAllText(x);
            mw.textEditor.IsModified = false;
            mw.textEditor.IsEnabled = true;
            mw.textEditor.Visibility = Visibility.Visible;
            return true;
        }

        /// <summary>
        /// Opens & displays a DDS image file in the editor.
        /// </summary>
        /// <param name="x">The location of the file to open.</param>
        public bool OpenImageFile(string x)
        {
            if (!ResetEditorView())
                return false;
            
            mw.imageViewer.Source = new BitmapImage(new Uri(x));

            mw.imageViewer.IsEnabled = true;
            mw.imageViewer.Visibility = Visibility.Visible;

            return true;
        }

        /// <summary>
        /// Displays the editor for .tseproj files.
        /// </summary>
        /// <param name="x">The location of the file to open.</param>
        public void OpenProjectFile(string x)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Closes / disables all editor panels.
        /// </summary>
        private bool ResetEditorView()
        {
            if(mw.textEditor.IsEnabled && mw.textEditor.IsModified && currentlyOpenFile != null)
            {
                var x = MessageBox.Show("Would you like to save your changes?", "Save?", MessageBoxButton.YesNoCancel);
                if (x == MessageBoxResult.Yes)
                    File.WriteAllText(currentlyOpenFile, mw.textEditor.Text);
                else if (x == MessageBoxResult.Cancel)
                    return false;
            }

            mw.textEditor.IsEnabled = false;
            mw.textEditor.Visibility = Visibility.Hidden;
            mw.textEditor.IsModified = false;

            mw.imageViewer.IsEnabled = false;
            mw.imageViewer.Visibility = Visibility.Hidden;
            mw.imageViewer.Source = null;
            
            currentlyOpenFile = null;
            return true;
        }

        public bool Destroy()
        {
            return ResetEditorView();
        }
    }
}
