using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class UserProducts
    {
        public User UserV { get; set; }
        public List<Product> ProductsV { get; set; }
        public List<Sold_products> Bought_Prod { get; set; }
    }
}