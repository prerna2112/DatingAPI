using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DatingAPI.Dtos
{
    public class UserForRegisterDto
    {   [Required]
        public  string Username { get; set; }

        [Required]
        [StringLength(8, MinimumLength =4, ErrorMessage ="password length must be between 8 and 4")]
        public string Password { get; set; }
        [Required]
        public string KnownAs { get; set; }
        [Required]
        public DateTime DateOFBirth { get; set; }
        [Required]
        public string City { get; set; }
        [Required]
        public string Country { get; set; }
        [Required]
        public DateTime Created { get; set; }
        public DateTime LastActive { get; set; }
        public UserForRegisterDto()
        {
            Created = DateTime.Now;
            LastActive = DateTime.Now;
        }
    }
}
