using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Супермаркет
{
    internal class Program
    {
        static void Main()
        {

        }
    }

    class Customer
    {
        private Dictionary<string, int> _productsWishList;
        private List<Product> _products;
        private int _money;

        public Customer(int minMoney, int maxMoney)
        {
            _money = new Random().Next(minMoney, maxMoney);
        }
    }

    class Product : ICloneable
    {
        public Product(string name, int price)
        {
            Name = name;
            Price = price;
        }

        public string Name { get; set; }
        public int Price { get; set; }

        public void Check(string name, int price)
        {
            Name = name;
            Price = price;
        }

        public object Clone() => MemberwiseClone();
    }

    class ProductFabric
    {
        public IReadOnlyList<Product> Products = new List<Product> { new Product("Картошка", 30), new Product("Сыр", 335) };

        public Product CreateProduct(string name) => (Product)Products.First(product => product.Name == name).Clone();
    }
}
