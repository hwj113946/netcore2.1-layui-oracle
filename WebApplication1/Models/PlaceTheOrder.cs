using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Models
{
    public class PlaceTheOrder
    {
        public string PERSON_NAME { get; set; }
        public string PERSON_ID { get; set; }
        public string LOCATOR_ID { get; set; }
        public string SA_SUPPLY_TIME_ID { get; set; }
        public string USER_ID  { get; set; }
        public string NOTE { get; set; }
        public string PERSON_CODE { get; set; }
        public string ACCOUNT_ID { get; set; }
        public List<details> details { get; set; }
    }

    public class details
    {
        public string ITEM_ID { get; set; }
        public string SA_ITEM_NAME { get; set; }
        public string UOM { get; set; }
        public decimal SA_PRICE { get; set; }
        public int SA_QTY { get; set; }
        public string NOTE { get; set; }
    }


}
