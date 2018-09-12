using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class CompanyProductUpload
    {
        public Company CompanyM { get; set; }
        public Product ProductM { get; set; }
        public Brand BrandM { get; set; }
        public Category CategoryM { get; set; }
        public Model ProductModelM { get; set; }
        public List<P_photo> P_photos { get; set; }
        public List<Comment> P_Comments { get; set; }

    }
}