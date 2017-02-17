using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDFReader
{
    public class ColumnType
    {
        private Dictionary<string, string> columnTypes;
        private Dictionary<string, string[]> columnEnums; 
        private Enum umEnum;
        public ColumnType()
        {
            columnTypes = new Dictionary<string, string>
            {
                {"line #", "number" },
                {"ordered","number"},
                { "shipped","number"},
                {"item number","single" },
                {"part number", "single" },
                {"description", "freeform"},
                {"unit price","number" },
                {"ext", "string" },
                {"um","enum"},
                {"amount","number"},
                {"ext amt", "number" },
                {"qty", "number" }
            };
            columnEnums = new Dictionary<string, string[]>
            {
                {"um", new[] {"e", "eo", "c", "ea", "ft", "lg"}}
            };
        }

        public Dictionary<string, string> columns => columnTypes;
        public Dictionary<string, string[]> enums => columnEnums;
    }
}
