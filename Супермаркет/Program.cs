using System;
using System.Collections.Generic;
using System.Linq;

namespace Супермаркет
{
    internal class Program
    {
        static void Main()
        {
            ProductFabric productFabric = new ProductFabric();
            CustomerFabric customerFabric = new CustomerFabric(productFabric.ProductsNames);
            Customer customer = customerFabric.CreateCustomer();
            Shop shop = new Shop(productFabric);
        }
    }

    class Customer
    {
        private Dictionary<string, int> _wishList;
        private List<Product> _products = new List<Product>();
        private int _money;
        private int _moneyToPay = 0;

        public Customer(int money, Dictionary<string, int> wishList)
        {
            _money = money;
            _wishList = wishList;
        }

        public int BuyProducts(List<Product> productsToReturn)
        {
            _products.ForEach(product => _moneyToPay += product.Price);

            while (_moneyToPay > _money)
                RemoveProduct(productsToReturn);

            _money -= _moneyToPay;

            return _moneyToPay;
        }

        public void TakeProducts(List<Shelf> shelves)
        {
            for (int i = 0; i < _wishList.Count; i++)
                if (FindProduct(shelves[i]))
                    _products.AddRange(shelves[i].GiveProducts(_wishList[shelves[i].ProductName]));
        }

        private void RemoveProduct(List<Product> productsToReturn)
        {
            _moneyToPay -= _products.Last().Price;

            productsToReturn.Add(_products.Last());

            _products.RemoveAt(_products.Count - 1);
        }

        private bool FindProduct(Shelf shelf) => _wishList.Keys.Contains(shelf.ProductName);
    }

    class CustomerFabric
    {
        private int _minMoney = 500;
        private int _maxMoney = 5000;
        private int _minWishValue = -5;
        private int _maxWishValue = 10;

        private List<string> _products;
        private Random _random = new Random();

        public CustomerFabric(List<string> products) => _products = products;

        public Customer CreateCustomer() => new Customer(_random.Next(_minMoney, _maxMoney), CreateWishList());

        private Dictionary<string, int> CreateWishList()
        {
            int wishValue;
            Dictionary<string, int> wishList = new Dictionary<string, int>();

            for (int i = 0; i < _products.Count; i++)
            {
                wishValue = _random.Next(_minWishValue, _maxWishValue);

                if (wishValue > 0)
                    wishList.Add(_products[i], wishValue);
            }

            if (wishList.Count == 0)
                wishList.Add(_products[_random.Next(_products.Count)], 1);

            return wishList;
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

        public object Clone() => MemberwiseClone();
    }

    class ProductFabric
    {
        public List<string> ProductsNames = new List<string>();
        private List<Product> _products = new List<Product> { new Product("Картошка", 30), new Product("Сыр", 335) };

        public ProductFabric() => _products.ForEach(product => ProductsNames.Add(product.Name));

        public List<Product> CreateProducts(string name, int quantity = 1)
        {
            List<Product> products = new List<Product>();
            Product product;

            product = _products.Find(desiredProduct => desiredProduct.Name == name);

            for (int i = 0; i < quantity; i++)
                products.Add((Product)product.Clone());

            return products;
        }
    }

    class Shop
    {
        private int _money = 0;
        private int _productQuantity = 10;

        private List<string> _productsNames = new List<string>();
        private List<Shelf> _shelves = new List<Shelf>();
        private List<Product> _storage = new List<Product>();
        private List<Customer> _customers = new List<Customer>();
        private ProductFabric _productFabric;

        public Shop(ProductFabric productFabric)
        {
            _productFabric = productFabric;
            _productsNames = _productFabric.ProductsNames;

            _productsNames.ForEach(name => _shelves.Add(new Shelf(name)));

            _shelves.ForEach(shelf => shelf.GetProducts(_productFabric.CreateProducts(shelf.ProductName, _productQuantity)));
        }

        public void GetProducts(List<Product> products) => _storage.AddRange(products);

        public void LetCustomersIn(List<Customer> customers) => _customers.AddRange(customers);
    }

    class Shelf
    {
        private int _maxProducts = 20;
        private bool _isFull = false;
        private List<Product> _products = new List<Product>();

        public Shelf(string productName, List<Product> products = null)
        {
            ProductName = productName;

            if (products != null)
                GetProducts(products);
        }

        public string ProductName { private set; get; }

        public void ChangeProductName(string productName, List<Product> products)
        {
            products.AddRange(_products);
            _products.Clear();
            ProductName = productName;
        }

        public void GetProducts(List<Product> products)
        {
            while (!_isFull && products.Any(product => product.Name == ProductName))
            {
                _products.Add(products.Find(product => product.Name == ProductName));

                products.Remove(_products.Last());

                SetFullness();
            }
        }

        public List<Product> GiveProducts(int quantity = 1)
        {
            List<Product> products = new List<Product>();

            if (quantity > _maxProducts)
                quantity = _maxProducts;

            for (int i = 0; i < quantity; i++)
            {
                if (_products.Any())
                {
                    products.Add(_products[0]);
                    _products.RemoveAt(0);
                }
                else
                {
                    break;
                }
            }

            SetFullness();

            return products;
        }

        private void SetFullness()
        {
            if (_products.Count == _maxProducts)
                _isFull = true;
            else
                _isFull = false;
        }
    }
}
