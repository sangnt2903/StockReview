using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PreviewSPA_Wx3.Models
{
    public class WX3Model
    {
        public string WX3ID { get; set; }
        public string ProductCode { get; set; }
        public string ProductDesc { get; set; }
        public string VendorCode { get; set; }
        public string LotNo { get; set; }
        public int AvailQty { get; set; }
        public int ResvQty { get; set; }
        public int AllStock => AvailQty + ResvQty;
    }
}
