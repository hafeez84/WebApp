using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text;

namespace WebContract
{
    [DataContract]
    class CompanyContract
    {
        public int Id { get; set; }
        [DataMember]
        [Required]
        [StringLength(30)]
        [DisplayName("Company Name")]
        public string Cname { get; set; }
        [DataMember]
        [StringLength(20)]
        [DisplayName("Company Phone")]
        public string Ctel { get; set; }
        [DataMember]
        [DisplayName("Address")]
        public string Caddress { get; set; }
        [DataMember]
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [DataMember]
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
