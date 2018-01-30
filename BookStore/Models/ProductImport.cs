using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BookStore.Models
{
    public class ProductImport : BaseEntity
    {
        public ProductImport()
        {
        }

        [Display(Name = "Tên")]
        public string Name { get; set; }

        // Trạng thái
        public ImportStatus Status { get; set; }

        // Ngày tạo
        public DateTime CreatedDay { get; set; }

        public virtual ICollection<ImportDetail> ImportDetails { get; set; }
    }
}