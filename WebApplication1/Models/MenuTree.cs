using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Models
{
    public class MenuTree
    {
        public decimal value { get; set; }
        public string name { get; set; }
        public decimal pid { get; set; }
        public List<MenuTree> children { get; set; }
    }
}
