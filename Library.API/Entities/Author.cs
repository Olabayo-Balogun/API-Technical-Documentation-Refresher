using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Library.API.Entities
{
    //The line of code can be used to disable warnings relating to missing line of code, note that it can also apply to other warnings once you know the code (which can be found in the error list).
#pragma warning disable CS1591
    [Table("Authors")]
    public class Author
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(150)]
        public string LastName { get; set; }

        public ICollection<Book> Books { get; set; } = new List<Book>();
    }

    //You can restore warnings after a block of code using the line of code below
#pragma warning restore CS1591
}
