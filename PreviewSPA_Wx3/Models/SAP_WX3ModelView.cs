using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PreviewSPA_Wx3.Models
{
    public class SAP_WX3ModelView
    {
        public string MaterialCode { get; set; }
        public string MaterialDesc { get; set; }
        public string LocalStorage { get; set; }
        public string Batch { get; set; }
        public int AllStockSAP { get; set; }
        public int AllStockWX3 { get; set; }
        public int Diff => AllStockSAP - AllStockWX3;
    }
}
