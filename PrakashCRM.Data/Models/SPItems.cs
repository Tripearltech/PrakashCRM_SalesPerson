using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrakashCRM.Data.Models
{
    public class SPItemList
    {
        public string No { get; set; }

        public string Description { get; set; }

        public string Base_Unit_of_Measure { get; set; }
    }

    public class SPItemPackingStyleDetails
    {
        public string Item_No { get; set; }
        public string Item_Description { get; set; }
        public string Packing_Style_Code { get; set; }
        public string Packing_Style_Description { get; set; }
        public int PCPL_Purchase_Cost { get; set; }
    }

}
