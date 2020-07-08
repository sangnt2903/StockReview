using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PreviewSPA_Wx3.Models
{
    public class SAPQtyModel
    {
        public int Unrestricted { get; set; }
        public int QualityInspection { get; set; }
        public int Blocked { get; set; }
        public int TransitAndTransfer { get; set; }
        public int AllStock => Unrestricted + QualityInspection + Blocked;
    }
}
