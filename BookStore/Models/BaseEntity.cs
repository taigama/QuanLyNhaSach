using System.ComponentModel.DataAnnotations;
namespace BookStore.Models
{
    public class BaseEntity
    {
        [ScaffoldColumn(false)]
        public int ID { get; set; }
        [ScaffoldColumn(false)]
        public string Code { get; set; }
    }
}