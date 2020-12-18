using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Telltale_Script_Editor.GUI
{
    public class ConsoleWriter : TextWriter
    {
        private TextBox textBox;
        
        /// <summary>
        /// Wrapper method to allow writing console output to the GUI.
        /// </summary>
        /// <param name="x">TextBox to write to</param>
        public ConsoleWriter(TextBox x)
        {
            this.textBox = x;
        }

        public override void Write(char x)
        {
            textBox.AppendText(x.ToString());
        }

        public override void Write(string x)
        {
            textBox.AppendText(x);
        }

        public override Encoding Encoding
        {
            get { return Encoding.UTF8; }
        }
    }
}
