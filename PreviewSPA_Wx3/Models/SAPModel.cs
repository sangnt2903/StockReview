using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PreviewSPA_Wx3.Models
{
    public class SAPModel
    {
        public string SPA_ID { get; set; }
        public string MaterialCode { get; set; }
        public string MaterialDesc { get; set; }
        public string LocalStorage { get; set; }
        public string Batch { get; set; }
        public SAPQtyModel Quantity { get; set; }
    }
}
