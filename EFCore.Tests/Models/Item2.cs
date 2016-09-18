using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace EFCore.Tests.Models
{
    [Table("Item2")]
    public class Item2
    {
        [Key]
        public int Id { get; set; }


        public int IdRef { get; set; }

        [ForeignKey("IdRef")]
        public virtual Item Item { get; set; }

        public string Name { get; set; }
    }
}
