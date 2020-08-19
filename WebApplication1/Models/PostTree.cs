using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Models
{
    public class PostTree
    {
        public decimal value { get; set; }
        public string name { get; set; }
        public decimal Parent_Post_Id { get; set; }
        public List<PostTree> children { get; set; }
    }
}
