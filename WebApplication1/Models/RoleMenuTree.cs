using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Models
{
    public class RoleMenuTree
    {
        public decimal id { get; set; }
        public decimal pid { get; set; }
        public string title { get; set; }
        public decimal menu_type { get; set; }
        public string @checked { get; set; }
        public string spread { get; set; }
    }
}
