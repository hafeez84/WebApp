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
        [DisplayName("First Name")]
        public string Fname { get; set; }
        [DisplayName("Last Name")]
        public string Lname { get; set; }
        [DisplayName("Tel")]
        public string Tel { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
