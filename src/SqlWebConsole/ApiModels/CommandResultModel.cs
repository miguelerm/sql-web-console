using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlWebConsole.ApiModels
{
    public class CommandResultModel
    {
        public int RecordsAffected { get; set; }
        public IDictionary<string, ColumnModel> Columns { get; set; }
        public IList<IDictionary<string, object>> Items { get; set; }

        public CommandResultModel()
        {
            Columns = new Dictionary<string, ColumnModel>();
            Items = new List<IDictionary<string, object>>();
        }

    }
}
