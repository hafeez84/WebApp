using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace WebContract
{
    [DataContract]
    public class UserContract
    {
        [DataMember]
        public int Id { get; set; }
        [Required]
        [StringLength(20)]
        [DisplayName("First Name")]
        public string Fname { get; set; }
        [Required]
        [StringLength(20)]
        [DisplayName("Last Name")]
        public string Lname { get; set; }
        [Required]
        [Phone]
        public string Tel { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        public string Address { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        public byte[] Avatar { get; set; }
    }
}
