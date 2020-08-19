using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Models
{
    public class GetPerson
    {
        public string PERSON_NAME { get; set; }
        public int PAGE { get; set; }
        public int LIMIT { get; set; }
    }
}
