using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Models
{
    [Newtonsoft.Json.JsonObject(IsReference =true)]
    public class Menu
    {
        public decimal MENU_ID { get; set; }
        public string MENU_URL { get; set; }
        public string MENU_NAME { get; set; }
        public string MENU_ICON { get; set; }
        public string MENU_SORT { get; set; }
        public decimal PARENT_MENU_ID { get; set; }
        public decimal MENU_TYPE { get; set; }
    }
}
