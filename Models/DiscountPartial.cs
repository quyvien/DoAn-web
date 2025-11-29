using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace DoAn_web.Models
{
    // ap dung cho lop metadata class Discount
    [MetadataType(typeof(DiscountMetadata))]
    // dung tu khoa partial de mo rong lop Discount
    public partial class Discount
    {
        // them cac thuoc tinh anh xa
        public bool IsExpired
        {
            get { return this.EndDate < DateTime.Now; }
        }
    }
}