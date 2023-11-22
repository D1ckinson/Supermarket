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
            CustomerFabric customerFabric = new CustomerFabric(productFabric.ProductsNames.ToList());
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

            if (_products.Any())
            {
                Console.WriteLine($"У покупателя {_money} денег, он набрал товаров на {_moneyToPay}.");
            }
            else
            {
                Console.WriteLine("Покупатель не нашел нужных товаров и ушел.");

                return 0;
            }

            while (_moneyToPay > _money)
                RemoveProduct(productsToReturn);

            _money -= _moneyToPay;

            if (_moneyToPay > 0)
                Console.WriteLine($"Покупатель заплатил {_moneyToPay} и пошел по своим делам.\n");
            else
                Console.WriteLine("Покупателю не хватило денег ни на один продукт, он ушел грустным.\n");

            return _moneyToPay;
        }

        public void TakeProducts(List<Shelf> shelves)
        {
            for (int i = 0; i < shelves.Count; i++)
                if (IsWishProductOnShelf(shelves[i]))
                    _products.AddRange(shelves[i].GiveProducts(_wishList[shelves[i].ProductName]));

            Console.Write("Покупатель выбрал продукты: ");

            _products.ForEach(product => Console.Write(product.Name + ", "));
        }

        private void RemoveProduct(List<Product> productsToReturn)
        {
            Product productToReturn = _products[RandomUtility.GiveNumber(_products.Count)];

            _moneyToPay -= productToReturn.Price;

            Console.WriteLine($"Покупателю не хватает денег для оплаты и он убрал {productToReturn.Name}, теперь он должен заплатить {_moneyToPay}.");

            productsToReturn.Add(productToReturn);

            _products.Remove(productToReturn);
        }

        private bool IsWishProductOnShelf(Shelf shelf) => _wishList.Keys.Contains(shelf.ProductName);
    }

    class CustomerFabric
    {
        private int _minMoney = 500;
        private int _maxMoney = 2000;
        private int _minWishValue = -5;
        private int _maxWishValue = 10;
        private int _maxCustomers = 15;

        private List<string> _productsNames;

        public CustomerFabric(List<string> productsNames) => _productsNames = productsNames;

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

            for (int i = 0; i < _productsNames.Count; i++)
            {
                wishValue = RandomUtility.GiveNumber(_minWishValue, _maxWishValue);

                if (wishValue > 0)
                    wishList.Add(_productsNames[i], wishValue);
            }

            if (wishList.Count == 0)
                wishList.Add(_productsNames[RandomUtility.GiveNumber(_productsNames.Count)], 1);

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
        private List<Product> _products = new List<Product>();
        public IReadOnlyList<string> ProductsNames;
        public ProductFabric()
        {
            AddProducts();

            List<string> productNames = new List<string>();

            _products.ForEach(product => productNames.Add(product.Name));

            ProductsNames = productNames;
        }

        public List<Product> CreateProducts(string name, int quantity = 1)
        {
            List<Product> products = new List<Product>();
            Product product;

            product = _products.Find(desiredProduct => desiredProduct.Name == name);

            for (int i = 0; i < quantity; i++)
                products.Add((Product)product.Clone());

            return products;
        }

        private void AddProducts()
        {
            _products.Add(new Product("Картошка", 30));
            _products.Add(new Product("Сыр", 335));
            _products.Add(new Product("Колбаса", 541));
        }
    }

    class Shop
    {
        private List<Product> _storage = new List<Product>();
        private Queue<Customer> _customers;
        private List<Shelf> _shelves = new List<Shelf>();
        private List<string> _productsNames;
        private ProductFabric _productFabric;

        private int _money = 0;
        private int _maxProductTypeInStorage = 40;

        public Shop(ProductFabric productFabric)
        {
            _productFabric = productFabric;
            _productsNames = _productFabric.ProductsNames.ToList();

            _productsNames.ForEach(name => _shelves.Add(new Shelf(name)));

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

                customer.TakeProducts(_shelves);

                _money += customer.BuyProducts(productsToReturn);

                if (productsToReturn.Any())
                    ReturnProductsOnShelves(productsToReturn);
            }
        }

        private void ReplenishStorage()
        {
            for (int i = 0; i < _productsNames.Count; i++)
            {
                int productValueInStorage = _storage.FindAll(product => product.Name == _productsNames[i]).Count;
                int productsValueToBuy = _maxProductTypeInStorage - productValueInStorage;

                if (productsValueToBuy > 0)
                    _storage.AddRange(_productFabric.CreateProducts(_productsNames[i], productsValueToBuy));
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
