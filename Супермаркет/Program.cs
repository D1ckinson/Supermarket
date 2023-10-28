﻿using System;
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

            List<Product> products = productFabric.CreateProducts("Сыр", 21);
            List<Product> products2 = new List<Product>();

            Shelf shelf = new Shelf();
            Shelf shelf1 = new Shelf(products);

            products2.AddRange(shelf.GiveProducts("Сыр"));
            products2.AddRange(shelf1.GiveProducts("Сыр", 70));

            shelf.GetProducts(products);
        }
    }

    class Customer
    {
        private Dictionary<string, int> _wishList;
        private List<Product> _products;
        private int _money;

        public Customer(int money, Dictionary<string, int> wishList)
        {
            _money = money;
            _wishList = wishList;
        }
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
                wishList.Add(_products[0], 1);

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

        public void Check(string name, int price)
        {
            Name = name;
            Price = price;
        }

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
        private List<Product> _productList = new List<Product>();
    }

    class Shelf
    {
        private int _maxProducts = 20;
        private bool _isFull = false;
        private List<Product> _products = new List<Product>();

        public Shelf(List<Product> products = null)
        {
            if (products != null)
                GetProducts(products);
        }

        public void GetProducts(List<Product> products)
        {
            while (!_isFull && products.Any())
            {
                _products.Add(products[0]);

                products.RemoveAt(0);

                SetFullness();
            }
        }

        public List<Product> GiveProducts(string productName, int quantity = 1)
        {
            List<Product> products = new List<Product>();

            if (quantity > _maxProducts)
                quantity = _maxProducts;

            for (int i = 0; i < quantity; i++)
            {
                if (_products.Exists(product => product.Name == productName))
                {
                    products.Add(_products.Find(product => product.Name == productName));
                    _products.Remove(products.Last());
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
