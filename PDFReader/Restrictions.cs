using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDFReader
{
    public class Restrictions
    {
        public Dictionary<string, string[]> restrictionSettings { get; }

        public Restrictions()
        {
            restrictionSettings = new Dictionary<string, string[]>
            {
                {"line #", new [] {"1 digit"}},
                {"um", new [] {"2 characters"} },
                {"description", new []{"freeform"} }
            };
        }
    }
}
