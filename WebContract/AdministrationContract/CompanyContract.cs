using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text;

namespace WebContract
{
    [DataContract]
    public class CompanyContract
    {
        public int Id { get; set; }
        [DataMember]
        [Required]
        [StringLength(30, MinimumLength = 2, ErrorMessage = "The company name is required and must be minimum 2 charachters !")]
        [DisplayName("Company Name")]
        public string Cname { get; set; }
        [DataMember]
        [StringLength(30, MinimumLength = 9, ErrorMessage = "The Phone number is required and must be minimum 19 numbers !")]
        [DisplayName("Company Phone")]
        public string Ctel { get; set; }
        [DataMember]
        [DisplayName("Address")]
        public string Caddress { get; set; }
        [DataMember]
        [Required]
        [DataType(DataType.Password)]
        [StringLength(30, MinimumLength = 4, ErrorMessage = "The Password is required and must be minimum 4 characters !")]
        public string Password { get; set; }
        [DataMember]
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        public byte[] Avatar { get; set; }
    }
}
