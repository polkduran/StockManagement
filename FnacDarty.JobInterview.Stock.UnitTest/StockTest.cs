using System;
using System.Collections.Generic;
using System.Linq;
using FnacDarty.JobInterview.Stock.Stock;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FnacDarty.JobInterview.Stock.UnitTest
{
    [TestClass]
    public class StockTest
    {
        private readonly InMemoryProductRepository _productRepository;
        private readonly InMemoryStockRepository _stockMovementRepository;
        private readonly StockService _stockService;

        public StockTest()
        {
            var store = new InMemoryStore();
            _productRepository = new InMemoryProductRepository(store);
            _stockMovementRepository = new InMemoryStockRepository(store);
            _stockService = new StockService(_stockMovementRepository, _productRepository);
        }

        [TestMethod]
        public void AddPositiveStockMovements()
        {
            var product = new Product.Product {Ean = "EAN00001"};
            _productRepository.AddProduct(product);

            _stockService.AddStockMovement(DateTime.Today.AddDays(-5), "Achat N°1", product, 10);
            _stockService.AddStockMovement(DateTime.Today.AddDays(-4), "Achat N°2", product, 3);
            _stockService.AddStockMovement(DateTime.Today.AddDays(-3), "Achat N°3", product, 1);

            var currentStock = _stockService.GetCurrentProductStock(product);
            Assert.AreEqual(14, currentStock);
        }

        [TestMethod]
        public void AddNegativeStockMovements()
        {
            var product = new Product.Product { Ean = "EAN00001" };
            _productRepository.AddProduct(product);

            _stockService.AddStockMovement(DateTime.Today.AddDays(-5), "Cmd N°1", product, -1);
            _stockService.AddStockMovement(DateTime.Today.AddDays(-4), "Cmd N°2", product, -3);
            _stockService.AddStockMovement(DateTime.Today.AddDays(-3), "Cmd N°3", product, -2);

            var currentStock = _stockService.GetCurrentProductStock(product);
            Assert.AreEqual(-6, currentStock);
        }

        [TestMethod]
        public void AddPositiveAndNegativeStockMovements()
        {
            var product = new Product.Product { Ean = "EAN00001" };
            _productRepository.AddProduct(product);

            _stockService.AddStockMovement(DateTime.Today.AddDays(-5), "Achat N°1", product, 10);
            _stockService.AddStockMovement(DateTime.Today.AddDays(-4), "Cmd N°2", product, -3);
            _stockService.AddStockMovement(DateTime.Today.AddDays(-3), "Cmd N°2", product, -1);

            var currentStock = _stockService.GetCurrentProductStock(product);
            Assert.AreEqual(6, currentStock);
        }

        [TestMethod]
        public void RetrieveProductStockAtAnyDate()
        {
            var product = new Product.Product { Ean = "EAN00001" };
            _productRepository.AddProduct(product);

            _stockService.AddStockMovement(DateTime.Today.AddDays(-5), "Achat N°1", product, 10);
            _stockService.AddStockMovement(DateTime.Today.AddDays(-4), "Cmd N°2", product, -3);
            _stockService.AddStockMovement(DateTime.Today.AddDays(-3), "Cmd N°3", product, -1);

            var expectedStockByDate = new[]
            {
                (10, DateTime.Today.AddDays(-5)),
                (7, DateTime.Today.AddDays(-4)),
                (6, DateTime.Today.AddDays(-3)),
                (6, DateTime.Today.AddDays(-2)),
                (6, DateTime.Today)
            };

            foreach (var (expectedStock, date) in expectedStockByDate)
            {
                var realStock = _stockService.GetProductStock(product, date);
                Assert.AreEqual(expectedStock, realStock);
            }
        }

        [TestMethod]
        public void RetrieveProductStockVariations()
        {
            var product = new Product.Product { Ean = "EAN00001" };
            _productRepository.AddProduct(product);

            _stockService.AddStockMovement(DateTime.Today.AddDays(-5), "Achat N°1", product, 10);
            _stockService.AddStockMovement(DateTime.Today.AddDays(-4), "Cmd N°1", product, -3);
            _stockService.AddStockMovement(DateTime.Today.AddDays(-3), "Cmd N°2", product, -1);
            _stockService.AddStockMovement(DateTime.Today.AddDays(-2), "Achat N°2", product, 2);

            var variation = _stockService.GetProductStockVariation(product, DateTime.MinValue, DateTime.Today.AddDays(-5));
            Assert.AreEqual(10, variation);

             variation = _stockService.GetProductStockVariation(product, DateTime.Today.AddDays(-5), DateTime.Today.AddDays(-3));
            Assert.AreEqual(-4, variation);

            variation = _stockService.GetProductStockVariation(product, DateTime.Today.AddDays(-4), DateTime.Today);
            Assert.AreEqual(1, variation);
        }

        [TestMethod]
        public void AddStockMovementsForMultipleProductsWithSameLabelAndDate()
        {
            var sharedLabel = "Commande 13";
            var date = DateTime.Today.AddDays(-5);

            var product1 = new Product.Product { Ean = "EAN00001" };
            _productRepository.AddProduct(product1);

            var product2 = new Product.Product { Ean = "EAN00002" };
            _productRepository.AddProduct(product2);

            var product3 = new Product.Product { Ean = "EAN00003" };
            _productRepository.AddProduct(product3);

            _stockService.AddStockMovement(date, sharedLabel, 
                new []{(product1, 2), (product2, 3) , (product3, 4) });

            var stockMovements1 = _stockMovementRepository.GetStockMovements(product1, date, date);
            Assert.AreEqual(sharedLabel, stockMovements1.Single().Label);

            var stockMovements2 = _stockMovementRepository.GetStockMovements(product2, date, date);
            Assert.AreEqual(sharedLabel, stockMovements2.Single().Label);

            var stockMovements3 = _stockMovementRepository.GetStockMovements(product3, date, date);
            Assert.AreEqual(sharedLabel, stockMovements3.Single().Label);
        }

        [TestMethod]
        public void GetCurrentProductsInStock()
        {
            var sharedLabel = "Commande 13";
            var date = DateTime.Today.AddDays(-5);

            var product1 = new Product.Product { Ean = "EAN00001" };
            _productRepository.AddProduct(product1);

            var product2 = new Product.Product { Ean = "EAN00002" };
            _productRepository.AddProduct(product2);

            var product3 = new Product.Product { Ean = "EAN00003" };
            _productRepository.AddProduct(product3);

            _stockService.AddStockMovement(date, sharedLabel,
                new[] { (product1, 2), (product2, -3), (product3, 4) });

            var productsInStock = _stockService.GetProductsInStock();
            Assert.AreEqual(2, productsInStock.Count);
            Assert.AreEqual(product1.Ean, productsInStock.Single(x => x.Ean == product1.Ean).Ean);
            Assert.AreEqual(product3.Ean, productsInStock.Single(x => x.Ean == product3.Ean).Ean);
        }

        [TestMethod]
        public void GetCurrentStock()
        {
            var sharedLabel = "Commande 13";
            var date = DateTime.Today.AddDays(-5);

            var product1 = new Product.Product { Ean = "EAN00001" };
            _productRepository.AddProduct(product1);

            var product2 = new Product.Product { Ean = "EAN00002" };
            _productRepository.AddProduct(product2);

            var product3 = new Product.Product { Ean = "EAN00003" };
            _productRepository.AddProduct(product3);

            _stockService.AddStockMovement(date, sharedLabel,
                new[] { (product1, 2), (product2, -3)});

            _stockService.AddInventoryMovement(product2, 4);

            var totalStock = _stockService.GetAllProductsStock();
            Assert.AreEqual(6, totalStock);
        }

        [TestMethod]
        public void AddStockMovementsToUnknownProductCreatesIt()
        {
            var ean = "EAN00001";

            var product = _productRepository.GetProductByEan(ean);
            Assert.IsNull(product);

            _stockService.AddStockMovement(DateTime.Today.AddDays(-5), "Achat N°1", ean, 10);

            product = _productRepository.GetProductByEan(ean);
            Assert.IsNotNull(product);

            var currentStock = _stockService.GetCurrentProductStock(product);
            Assert.AreEqual(10, currentStock);
        }

        [TestMethod]
        public void StockInventoryFixesTheStock()
        {
            var product = new Product.Product { Ean = "EAN00001" };
            _productRepository.AddProduct(product);

            _stockService.AddStockMovement(DateTime.Today.AddDays(-5), "Achat N°1", product, 10);
            _stockService.AddStockMovement(DateTime.Today.AddDays(-4), "Achat N°2", product, -3);
            _stockService.AddStockMovement(DateTime.Today.AddDays(-3), "Cmd N°2", product, -1);

            var currentStock = _stockService.GetCurrentProductStock(product);
            Assert.AreEqual(6, currentStock);

            _stockService.AddInventoryMovement(product, 7);
            currentStock = _stockService.GetCurrentProductStock(product);
            Assert.AreEqual(7, currentStock);

            _stockService.AddStockMovement(DateTime.Today.AddDays(1), "Cmd N°3", product, -2);
            currentStock = _stockService.GetProductStock(product, DateTime.Today.AddDays(1));
            Assert.AreEqual(5, currentStock);
        }

        [TestMethod]
        public void CannotAddStockPriorOrEqualToInventoryDate()
        {
            var product = new Product.Product { Ean = "EAN00001" };
            _productRepository.AddProduct(product);

            _stockService.AddStockMovement(DateTime.Today.AddDays(-5), "Achat N°1", product, 10);
            _stockService.AddStockMovement(DateTime.Today.AddDays(-4), "Cmd N°1", product, -3);
            _stockService.AddInventoryMovement(product, 7);

            Assert.ThrowsException<BusinessException>(() => 
                _stockService.AddStockMovement(DateTime.Today.AddDays(-5), "Cmd N°3", product, -2));

            Assert.ThrowsException<BusinessException>(() =>
                _stockService.AddStockMovement(DateTime.Today, "Cmd N°3", product, -2));
        }

        [TestMethod]
        public void StockInventoryIsUnique()
        {
            var product = new Product.Product { Ean = "EAN00001" };
            _productRepository.AddProduct(product);

            _stockService.AddStockMovement(DateTime.Today.AddDays(-5), "Achat N°1", product, 10);
            _stockService.AddStockMovement(DateTime.Today.AddDays(-4), "Cmd N°1", product, -3);

            _stockService.AddInventoryMovement(product, 7);

            Assert.ThrowsException<BusinessException>(() =>
                _stockService.AddInventoryMovement(product, 6));
        }

        [TestMethod]
        public void StockInventoryCannotBeNegative()
        {
            var product = new Product.Product { Ean = "EAN00001" };
            _productRepository.AddProduct(product);

            _stockService.AddStockMovement(DateTime.Today.AddDays(-5), "Achat N°1", product, 10);
            _stockService.AddStockMovement(DateTime.Today.AddDays(-4), "Cmd N°1", product, -3);

            Assert.ThrowsException<BusinessException>(() =>
                _stockService.AddInventoryMovement(product, -6));
        }

        [TestMethod]
        public void MultipleStockMovementsFailForAll()
        {
            var sharedLabel = "Commande 13";
            var date = DateTime.Today.AddDays(-5);

            var product1 = new Product.Product { Ean = "EAN00001" };
            _productRepository.AddProduct(product1);

            var product2 = new Product.Product { Ean = "EAN00002" };
            _productRepository.AddProduct(product2);

            var product3 = new Product.Product { Ean = "EAN00003" };
            _productRepository.AddProduct(product3);

            Assert.AreEqual(0, _stockService.GetProductsInStock().Count);
            Assert.AreEqual(0, _stockService.GetAllProductsStock());

            _stockService.AddInventoryMovement(product2, 2);

            Assert.AreEqual(product2.Ean, _stockService.GetProductsInStock().Single().Ean);
            Assert.AreEqual(2, _stockService.GetAllProductsStock());

            // Null product in list
            Assert.ThrowsException<BusinessException>(() =>
                _stockService.AddStockMovement(date, sharedLabel, new[] { (product1, 2), (null, 3), (product3, 4) }));
            Assert.AreEqual(product2.Ean, _stockService.GetProductsInStock().Single().Ean);
            Assert.AreEqual(2, _stockService.GetAllProductsStock());

            // Stock movement prior to inventory for product 2
            Assert.ThrowsException<BusinessException>(() =>
                _stockService.AddStockMovement(date, sharedLabel, new[] { (product1, 2), (product2, 3), (product3, 4) }));
            Assert.AreEqual(product2.Ean, _stockService.GetProductsInStock().Single().Ean);
            Assert.AreEqual(2, _stockService.GetAllProductsStock());
        }
    }
}
