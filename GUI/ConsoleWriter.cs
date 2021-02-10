﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Telltale_Script_Editor.GUI
{
    public class ConsoleWriter : TextWriter
    {
        public bool verbosity;

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
            Application.Current.Dispatcher.Invoke(() => textBox.AppendText(x.ToString()));
            //textBox.AppendText(x.ToString());
        }

        public override void Write(string x)
        {
            Application.Current.Dispatcher.Invoke(() => textBox.AppendText(x));
            //textBox.AppendText(x);
        }

        public override Encoding Encoding
        {
            get { return Encoding.UTF8; }
        }
    }
}
