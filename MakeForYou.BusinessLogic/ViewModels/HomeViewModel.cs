using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MakeForYou.BusinessLogic.Entities;

namespace MakeForYou.ViewModels
{
    public class HomeViewModel
    {
       
        public List<Product> FeaturedProducts { get; set; } = new List<Product>();

        public List<Category> Categories { get; set; } = new List<Category>();

        
        public List<Seller> FeaturedArtisans { get; set; } = new List<Seller>();
    }
}
