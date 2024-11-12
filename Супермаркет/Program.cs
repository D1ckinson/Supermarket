using System;
using System.Collections.Generic;
using System.Linq;

namespace Супермаркет
{
    class Program
    {
        static void Main()
        {
            CustomerFactory customerFabric = new CustomerFactory();
            Shop shop = new Shop();

            shop.Work(customerFabric.Create());
        }
    }

    class Shop
    {
        private int _money = 0;

        public void Work(Queue<Customer> customers)
        {
            Console.WriteLine($"В магазин зашло {customers.Count} покупателей.");

            while (customers.Count > 0)
            {
                Console.WriteLine($"осталось {customers.Count} покупателей\n");
                Customer customer = customers.Dequeue();

                while (customer.IsMoneyEnough == false)
                {
                    Console.WriteLine("Покупателю не хватает денег для оплаты");
                    customer.RemoveRandomProduct();
                }

                if (customer.BasketSize == 0)
                {
                    Console.WriteLine("Покупателю не хватает ни на что денег и он ушел");

                    return;
                }

                _money += customer.GiveMoney();
                customer.TakeProducts();

                Console.WriteLine($"Покупатель купил продукты и ушел. Теперь у вас {_money} денег\n");
            }
        }
    }

    class Customer
    {
        private List<Product> _basket = new List<Product>();
        private List<Product> _bag = new List<Product>();
        private int _money;

        public Customer(int money, List<Product> basket)
        {
            _money = money;
            _basket = basket;
        }

        public bool IsMoneyEnough => _money >= Price;
        public int BasketSize => _basket.Count;
        private int Price => _basket.Sum(product => product.Price);

        public int GiveMoney()
        {
            _money -= Price;

            return Price;
        }

        public void TakeProducts()
        {
            _basket.ForEach(product => _bag.Add(product));
            _basket.Clear();
        }

        public void RemoveRandomProduct()
        {
            int index = RandomUtility.GenerateValue(_basket.Count);

            Console.WriteLine($"Покупатель убирает {_basket[index].Name} из корзины\n");
            _basket.RemoveAt(index);
        }
    }

    class CustomerFactory
    {
        private int _minMoney = 500;
        private int _maxMoney = 2000;

        private int _minProductsQuantity = -5;
        private int _maxProductsQuantity = 10;

        private int _customersQuantity = 15;
        private List<Product> _products;

        public CustomerFactory()
        {
            _products = FillProducts();
        }

        public Queue<Customer> Create()
        {
            Queue<Customer> customers = new Queue<Customer>();

            for (int i = 0; i < _customersQuantity; i++)
                customers.Enqueue(new Customer(CreateMoney(), CreateBasket()));

            return customers;
        }

        private int CreateMoney() =>
            RandomUtility.GenerateValue(_minMoney, _maxMoney);

        private List<Product> CreateBasket()
        {
            List<Product> basket = new List<Product>();

            for (int i = 0; i < _products.Count; i++)
            {
                int productsQuantity = RandomUtility.GenerateValue(_minProductsQuantity, _maxProductsQuantity);

                AddProductToBasket(basket, i, productsQuantity);
            }

            if (basket.Count == 0)
                AddProductToBasket(basket);

            return basket;
        }

        private void AddProductToBasket(List<Product> basket, int index = -1, int quantity = 1)
        {
            if (index == -1)
            {
                index = RandomUtility.GenerateValue(_products.Count);
            }

            for (int i = 0; i < quantity; i++)
            {
                basket.Add(_products[index].Clone());
            }
        }

        private List<Product> FillProducts() =>
            new List<Product>
            {
                new Product("Картошка", 30),
                new Product("Сыр", 335),
                new Product("Колбаса", 541)
            };
    }

    class Product
    {
        public Product(string name, int price)
        {
            Name = name;
            Price = price;
        }

        public string Name { get; }
        public int Price { get; }

        public Product Clone() =>
            new Product(Name, Price);
    }

    static class RandomUtility
    {
        static private Random s_random = new Random();

        static public int GenerateValue(int maxValue) =>
            s_random.Next(maxValue);

        static public int GenerateValue(int minValue, int maxValue) =>
            s_random.Next(minValue, maxValue);
    }
}
