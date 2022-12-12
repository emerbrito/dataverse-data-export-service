using EmBrito.Dataverse.DataExport.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmBrito.Dataverse.DataExport.Scripts
{
    public class SqlScript
    {
        readonly private TableDefinition _tableDefinition;
        private readonly List<string> _description;

        public string Type { get; private set; }
        public IEnumerable<string> Description { get => _description; }
        public TableDefinition TableDefinition { get => _tableDefinition;  }
        public string TableName { get => _tableDefinition.Name;  }
        public string Script { get; set; }

        public SqlScript(string type, string script, TableDefinition tableDefinition)
        {
            _tableDefinition = tableDefinition;
            _description = new List<string>();
            Type = type;
            Script = script;            
        }

        public SqlScript(string type, string script, TableDefinition tableDefinition, string description)
            : this(type, script, tableDefinition)
        {
            _description.Add(description);
        }

        public SqlScript(string type, string script, TableDefinition tableDefinition, IEnumerable<string> description) 
            : this(type, script, tableDefinition)
        {
            _description.AddRange(description);
        }

    }
}
