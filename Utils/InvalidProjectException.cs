using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telltale_Script_Editor.Utils
{
    public class InvalidProjectException : Exception
    {
        public InvalidProjectException() { }

        public InvalidProjectException(string msg) : base(msg) { } 
    }
}
