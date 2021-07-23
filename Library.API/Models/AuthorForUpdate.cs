using System.ComponentModel.DataAnnotations;

namespace Library.API.Models
{
    public class AuthorForUpdate
    {
        /// <summary>
        ///     The first name of the author
        /// </summary>
        //Adding the same data annotation class present in the entity is important for documentation.
        [Required]
        [MaxLength(150)]
        public string FirstName { get; set; }

        /// <summary>
        ///     The last name of the author
        /// </summary>
        [Required]
        [MaxLength(150)]
        public string LastName { get; set; }
    }
}
