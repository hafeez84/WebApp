using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace WebContract
{
    [DataContract]
    public class UserContract
    {
        public int Id { get; set; }
        [DataMember]
        [Required]
        [StringLength(20)]
        [DisplayName("First Name")]
        public string Fname { get; set; }
        [DataMember]
        [Required]
        [StringLength(20)]
        [DisplayName("Last Name")]
        public string Lname { get; set; }
        [DataMember]
        [Phone]
        public string Tel { get; set; }
        [DataMember]
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        public string Address { get; set; }
        [DataMember]
        [Required]
        [StringLength(30, MinimumLength = 4, ErrorMessage = "The Password is required and must be minimum 4 characters !")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        public byte[] Avatar { get; set; }
    }
}
