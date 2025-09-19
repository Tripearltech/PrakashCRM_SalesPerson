using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrakashCRM.Data.Models
{
    public class SPOutstandingPaymentList
    {
        public string Posting_Date { get; set; }
        public string Document_No { get; set; }
        public string PCPL_Customer_Name { get; set; }
        public string Description { get; set; }
        public double Amount_LCY { get; set; }
        public double Remaining_Amt_LCY { get; set; }
        public string Due_Date { get; set; }

    }
}
