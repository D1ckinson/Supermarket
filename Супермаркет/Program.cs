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
            CustomerFabric customerFabric = new CustomerFabric(productFabric.GetProducts());
            Shop shop = new Shop(productFabric);

            shop.Open(customerFabric.CreateCustomers());
        }
    }

    class Customer
    {
        private Dictionary<string, int> _wishList;
        private List<Product> _products = new List<Product>();
        private int _money;

        public Customer(int money, Dictionary<string, int> wishList)
        {
            _money = money;
            _wishList = wishList;
        }

        public bool IsThisProductINeed(string productName) => _wishList.Keys.Contains(productName);

        public int TellDesiredQuantityOfProduct(string productName) => _wishList[productName];

        public void BuyProducts(List<Product> productsToReturn)
        {
            if (_products.Any())
            {
                Console.WriteLine($"У покупателя {_money} денег, он набрал товаров на {CalculateProductsPrice()}.");
            }
            else
            {
                Console.WriteLine("Покупатель не нашел нужных товаров и ушел.");

                return;
            }

            while (CalculateProductsPrice() > _money)
            {
                productsToReturn.Add(RemoveRandomProduct());

                Console.WriteLine($"Покупателю не хватает денег для оплаты и он убрал {productsToReturn.Last().Name}, теперь он должен заплатить {CalculateProductsPrice()}.");
            }

            _money -= CalculateProductsPrice();

            if (CalculateProductsPrice() > 0)
                Console.WriteLine($"Покупатель заплатил {CalculateProductsPrice()} и пошел по своим делам.\n");
            else
                Console.WriteLine("Покупателю не хватило денег ни на один продукт, он ушел грустным.\n");
        }

        public void TakeProducts(List<Product> products) => _products.AddRange(products);

        public int CalculateProductsPrice()
        {
            int moneyToPay = 0;

            _products.ForEach(product => moneyToPay += product.Price);

            return moneyToPay;
        }

        private Product RemoveRandomProduct()
        {
            Product productToReturn = _products[RandomUtility.GiveNumber(_products.Count)];

            _products.Remove(productToReturn);

            return productToReturn;
        }
    }

    class CustomerFabric
    {
        private int _minMoney = 500;
        private int _maxMoney = 2000;
        private int _minWishValue = -5;
        private int _maxWishValue = 10;
        private int _maxCustomers = 15;
        private List<Product> _products;

        public CustomerFabric(List<Product> products)
        {
            _products = products;
        }

        public Queue<Customer> CreateCustomers()
        {
            Queue<Customer> customers = new Queue<Customer>();

            for (int i = 0; i < _maxCustomers; i++)
                customers.Enqueue(new Customer(RandomUtility.GiveNumber(_minMoney, _maxMoney), CreateWishList()));

            return customers;
        }

        private Dictionary<string, int> CreateWishList()
        {
            int wishValue;
            Dictionary<string, int> wishList = new Dictionary<string, int>();

            for (int i = 0; i < _products.Count; i++)
            {
                wishValue = RandomUtility.GiveNumber(_minWishValue, _maxWishValue);

                if (wishValue > 0)
                    wishList.Add(_products[i].Name, wishValue);
            }

            if (wishList.Count == 0)
                wishList.Add(_products[RandomUtility.GiveNumber(_products.Count)].Name, 1);

            return wishList;
        }
    }

    class Product
    {
        public Product(string name, int price)
        {
            Name = name;
            Price = price;
        }

        public string Name { get; private set; }
        public int Price { get; private set; }

        public Product Clone() => new Product(Name, Price);
    }

    class ProductFabric
    {
        private List<Product> _products;

        public ProductFabric()
        {
            _products = GetProducts();

            List<string> productNames = new List<string>();

            _products.ForEach(product => productNames.Add(product.Name));
        }

        public List<Product> CreateProducts(string name, int quantity = 1)
        {
            List<Product> products = new List<Product>();
            Product product;

            product = _products.Find(desiredProduct => desiredProduct.Name == name);

            for (int i = 0; i < quantity; i++)
                products.Add(product.Clone());

            return products;
        }

        public List<Product> GetProducts()
        {
            List<Product> products = new List<Product>
            {
                new Product("Картошка", 30),
                new Product("Сыр", 335),
                new Product("Колбаса", 541)
            };

            return products;
        }
    }

    class Shop
    {
        private List<Product> _storage = new List<Product>();
        private Queue<Customer> _customers;
        private List<Shelf> _shelves = new List<Shelf>();
        private ProductFabric _productFabric;

        private int _money = 0;
        private int _maxProductTypeInStorage = 40;

        public Shop(ProductFabric productFabric)
        {
            _productFabric = productFabric;

            _productFabric.GetProducts().ForEach(product => _shelves.Add(new Shelf(product.Name)));

            ReplenishStorage();

            PutProductOnShelves();
        }

        public void Open(Queue<Customer> customers)
        {
            TakeCustomers(customers);

            LetCustomersBuy();

            ReplenishStorage();

            PutProductOnShelves();

            Console.WriteLine("Баланс магазина :" + _money);
        }

        private void TakeCustomers(Queue<Customer> customers)
        {
            _customers = customers;

            Console.WriteLine("Покупатели зашли в магазин.");
        }

        private void LetCustomersBuy()
        {
            List<Product> productsToReturn = new List<Product>();

            while (_customers.Any())
            {
                Customer customer = _customers.Dequeue();

                for (int i = 0; i < _shelves.Count; i++)
                {
                    string productName = _shelves[i].ProductName;

                    if (customer.IsThisProductINeed(productName))
                    {
                        int desiredProductValue = customer.TellDesiredQuantityOfProduct(productName);

                        List<Product> products = _shelves[i].GiveProducts(desiredProductValue);

                        customer.TakeProducts(products);
                    }
                }

                customer.BuyProducts(productsToReturn);

                _money += customer.CalculateProductsPrice();

                if (productsToReturn.Any())
                    ReturnProductsOnShelves(productsToReturn);
            }
        }

        private void ReplenishStorage()
        {
            List<Product> baseProducts = _productFabric.GetProducts();

            for (int i = 0; i < baseProducts.Count(); i++)
            {
                int productValueInStorage = _storage.FindAll(product => product.Name == baseProducts[i].Name).Count;
                int productsValueToBuy = _maxProductTypeInStorage - productValueInStorage;

                if (productsValueToBuy > 0)
                    _storage.AddRange(_productFabric.CreateProducts(baseProducts[i].Name, productsValueToBuy));
            }
        }

        private void PutProductOnShelves()
        {
            foreach (Shelf shelf in _shelves)
            {
                List<Product> products = new List<Product>();

                for (int i = 0; i < shelf.FreePlaces; i++)
                    products.Add(_storage.Find(product => product.Name == shelf.ProductName));

                products.ForEach(product => _storage.Remove(product));

                shelf.TakeProducts(products);
            }

        }

        private void ReturnProductsOnShelves(List<Product> products)
        {
            while (products.Any())
            {
                Product productToFind = products.First();

                List<Product> productsOfSameType = products.FindAll(product => product.Name == productToFind.Name);

                products.RemoveAll(product => product.Name == productToFind.Name);

                _shelves.Find(shelf => shelf.ProductName == productToFind.Name).TakeProducts(productsOfSameType);

                if (productsOfSameType.Any())
                    _storage.AddRange(productsOfSameType);
            }
        }
    }

    class Shelf
    {
        private int _maxProducts = 20;
        private List<Product> _products = new List<Product>();

        public Shelf(string productName, List<Product> products = null)
        {
            ProductName = productName;

            if (products != null)
                TakeProducts(products);
        }

        private bool IsFull => _products.Count == _maxProducts;
        public int FreePlaces => _maxProducts - _products.Count;
        public string ProductName { private set; get; }

        public void TakeProducts(List<Product> products)
        {
            while (IsFull == false && products.Any(product => product.Name == ProductName))
            {
                _products.Add(products.Find(product => product.Name == ProductName));

                products.Remove(_products.Last());
            }
        }

        public List<Product> GiveProducts(int quantity = 1)
        {
            if (quantity > _products.Count)
                quantity = _products.Count;

            List<Product> products = _products.GetRange(0, quantity);

            _products.RemoveRange(0, quantity);

            return products;
        }
    }

    static class RandomUtility
    {
        static private Random s_random = new Random();

        public static int GiveNumber(int value) => s_random.Next(value);

        public static int GiveNumber(int minValue, int maxValue) => s_random.Next(minValue, maxValue);
    }
}
