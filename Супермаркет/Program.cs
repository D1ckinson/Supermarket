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
            Shop shop = new Shop(productFabric);

            shop.Open(customerFabric.CreateCustomers());
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
        private int _maxCustomers = 15;

        private List<string> _productsNames;
        private Random _random = new Random();

        public CustomerFabric(List<string> productsNames) => _productsNames = productsNames;

        public Queue<Customer> CreateCustomers()
        {
            Queue<Customer> customers = new Queue<Customer>();

            for (int i = 0; i < _maxCustomers; i++)
                customers.Enqueue(new Customer(_random.Next(_minMoney, _maxMoney), CreateWishList()));

            return customers;
        }

        private Dictionary<string, int> CreateWishList()
        {
            int wishValue;
            Dictionary<string, int> wishList = new Dictionary<string, int>();

            for (int i = 0; i < _productsNames.Count; i++)
            {
                wishValue = _random.Next(_minWishValue, _maxWishValue);

                if (wishValue > 0)
                    wishList.Add(_productsNames[i], wishValue);
            }

            if (wishList.Count == 0)
                wishList.Add(_productsNames[_random.Next(_productsNames.Count)], 1);

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
        private List<Product> _storage = new List<Product>();
        private Queue<Customer> _customers = new Queue<Customer>();
        private List<Shelf> _shelves = new List<Shelf>();
        private List<string> _productsNames = new List<string>();
        private ProductFabric _productFabric;

        private int _money = 0;
        private int _maxProductTypeInStorage = 40;

        public Shop(ProductFabric productFabric)
        {
            _productFabric = productFabric;
            _productsNames = _productFabric.ProductsNames;

            _productsNames.ForEach(name => _shelves.Add(new Shelf(name)));

            ReplenishStorage();

            PutProductOnShelves();
        }

        public void Open(Queue<Customer> customers)
        {
            GetCustomers(customers);

            LetCustomersBuy();

            ReplenishStorage();

            PutProductOnShelves();

            Console.WriteLine("Баланс магазина :" + _money);
        }

        private void GetCustomers(Queue<Customer> customers) => _customers = customers;

        private void LetCustomersBuy()
        {
            Customer customer;
            List<Product> productsToReturn = new List<Product>();

            for (int i = 0; i < _customers.Count; i++)
            {
                customer = _customers.Dequeue();

                customer.TakeProducts(_shelves);

                _money += customer.BuyProducts(productsToReturn);
            }

            if (productsToReturn.Any())
                ReturnProductsOnShelves(productsToReturn);
        }

        private void ReplenishStorage()
        {
            for (int i = 0; i < _productsNames.Count; i++)
            {
                int productValueInStorage = _storage.FindAll(product => product.Name == _productsNames[i]).Count;
                int productsValueToBuy = _maxProductTypeInStorage - productValueInStorage;

                if (productsValueToBuy > 0)
                    _storage.AddRange(_productFabric.CreateProducts(_productsNames[i], _maxProductTypeInStorage - productValueInStorage));
            }
        }

        private void PutProductOnShelves()
        {
            _shelves.ForEach(shelf => shelf.GetProducts(_storage));
        }

        private void ReturnProductsOnShelves(List<Product> products)
        {
            if (products.Any())
            {
                Product productToFind = products.First();

                List<Product> productsOfSameType = products.FindAll(product => product == productToFind);

                _shelves.Find(shelf => shelf.ProductName == productToFind.Name).GetProducts(productsOfSameType);
            }
        }
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
