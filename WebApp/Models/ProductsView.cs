using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class ProductsView
    {
        public List<Product> Products { get; set; }
        public List<Brand> Product_b { get; set; }
        public List<Category> Product_c { get; set; }
        public List<Model> Product_m { get; set; }
        public List<P_photo> P_Photos { get; set; }
    }
}