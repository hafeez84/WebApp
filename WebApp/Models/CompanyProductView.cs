using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class CompanyProductView
    {
        public List<Product> ProductView { get; set; }
        public Company CompanyView { get; set; }
    }
}