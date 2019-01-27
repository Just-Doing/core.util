using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace core.util.Models
{
    public class FunctionTitle
    {
        public string title { get; set; }
        public List<FunctionDec> content { get; set; }

    }
    public class FunctionDec
    {
        public string nvId { get; set; }
        public string nvTitle { get; set; }
        public string name { get; set; }
        public string dec { get; set; }
        public string para { get; set; }
        public string ret { get; set; }
        public string eg { get; set; }
    }
}
