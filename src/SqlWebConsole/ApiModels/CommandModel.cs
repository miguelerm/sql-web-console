using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlWebConsole.ApiModels
{
    public class CommandModel
    {
        public string CommandText { get; set; }
        public ConnectionModel Connection { get; set; }
    }
}
