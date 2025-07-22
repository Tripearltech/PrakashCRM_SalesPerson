using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Data;
using System.ComponentModel.DataAnnotations;

namespace PrakashCRM.Data.Models
{
    public class SPPostedSalesInvoiceList
    {
        public string No { get; set; }

        public string Posting_Date { get; set; }

        public string Due_Date { get; set; }

        public decimal Amount { get; set; }

        public decimal Amount_Including_VAT { get; set; }

        public decimal Remaining_Amount { get; set; }

        public string Closed { get; set; }

    }

}
