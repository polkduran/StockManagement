using System;
using System.Collections.Generic;
using System.Linq;
using FnacDarty.JobInterview.Stock.Product;

namespace FnacDarty.JobInterview.Stock.Stock
{
    public class StockService
    {
        private readonly IStockRepository _stockRepository;
        private readonly IProductRepository _productRepository;

        public StockService(IStockRepository stockRepository, IProductRepository productRepository)
        {
            _stockRepository = stockRepository;
            _productRepository = productRepository;
        }

        public void AddInventoryMovement(Product.Product product, int quantity)
        {
            if (product == null)
            {
                throw new ArgumentNullException(nameof(product));
            }

            if (quantity < 0)
            {
                throw new BusinessException($"Inventory for product {product} shall not be negative, the value provided was {quantity}");
            }

            var latestInventory = _stockRepository.GetLatestInventoryMovement(product);
            if (latestInventory != null && latestInventory.MovementDate == DateTime.Today)
            {
                throw new BusinessException($"Cannot add an inventory movement for product {product} at {DateTime.Today} as one is registered already");
            }

            var inventoryMovement = new StockMovement
            {
                Label = "inventaire",
                MovementDate = DateTime.Today,
                MovementType = StockMovementType.Inventory,
                Product = product,
                Quantity = quantity
            };

            _stockRepository.AddStockMovement(inventoryMovement);
        }

        public void AddStockMovement(DateTime date, string label, string productEan, int quantity)
        {
            if (string.IsNullOrWhiteSpace(productEan))
            {
                throw new ArgumentException("Product ean cannot be empty", nameof(productEan));
            }

            var product = _productRepository.GetProductByEan(productEan);
            if (product == null)
            {
                product = new Product.Product
                {
                    Ean = productEan
                };
                _productRepository.AddProduct(product);
            }

            AddStockMovement(date, label, product, quantity);
        }

        public void AddStockMovement(DateTime date, string label, Product.Product product, int quantity)
        {
            AddStockMovement(date, label, new[] { (product, quantity) });
        }

        public void AddStockMovement(DateTime date, string label, IEnumerable<(Product.Product product, int quantity)> productQuantity)
        {
            date = date.Date;
            if (date > DateTime.Today.AddDays(1))
            {
                throw new BusinessException($"Cannot add a stock movement on the future {date}");
            }

            if (productQuantity == null)
            {
                throw new ArgumentNullException(nameof(productQuantity));
            }

            if (string.IsNullOrWhiteSpace(label))
            {
                throw new BusinessException("Label on stock movement shall not be empty");
            }

            // We build first a valid collection of stock movements
            // so if one entry is not valid we don't persist any of the provided input
            // * this can be done using a unit of work pattern or at least a transaction scope
            var stockMovements = new List<StockMovement>();
            foreach (var (product, quantity) in productQuantity)
            {
                if (product == null)
                {
                    throw new BusinessException($"Product shall not be null in {nameof(productQuantity)}");
                }

                var latestInventory = _stockRepository.GetLatestInventoryMovement(product);
                if (latestInventory != null && latestInventory.MovementDate >= date)
                {
                    throw new BusinessException($"Cannot add a stock movement for product {product} at {date} as an inventory movement is registered at {latestInventory.MovementDate}");
                }
                
                var stockMovement = new StockMovement
                {
                    Label = label,
                    MovementDate = date,
                    MovementType = StockMovementType.Movement,
                    Product = product,
                    Quantity = quantity
                };

                stockMovements.Add(stockMovement);
            }

            _stockRepository.AddStockMovements(stockMovements);
        }

        public int GetProductStock(Product.Product product, DateTime date)
        {
            // This can be optimized looking for the latest inventory movement prior to the requested date
            // and applying the reduction from that fixed point (inventory)
            var stockMovements = _stockRepository.GetStockMovements(product, DateTime.MinValue, date);
            var productStock = ComputeStock(stockMovements);
            return productStock;
        }

        public int GetCurrentProductStock(Product.Product product)
        {
            return GetProductStock(product, DateTime.Today);
        }

        public int GetProductStockVariation(Product.Product product, DateTime fromDateTime, DateTime toDateTime)
        {
            if (fromDateTime > toDateTime)
            {
                throw new BusinessException($"{nameof(fromDateTime)} shall not be greater than {nameof(toDateTime)}");
            }

            // Can be optimized fetching and iterating only once the product's stock movements
            return GetProductStock(product, toDateTime) - GetProductStock(product, fromDateTime);
        }

        public IReadOnlyCollection<Product.Product> GetProductsInStock()
        {
            return GetStockByProduct()
                .Where(x => x.stock >= 0)
                .Select(x => x.product)
                .ToList();
        }

        public int GetAllProductsStock()
        {
            return GetStockByProduct()
                .Where(x => x.stock > 0)
                .Sum(x => x.stock);
        }

        private IEnumerable<(Product.Product product, int stock)> GetStockByProduct()
        {
            var allStockMovements = _stockRepository.GetStockMovements();
            var productsInStock =
                from stockMovement in allStockMovements
                group stockMovement by stockMovement.Product
                into stockMovementsByProduct
                let stock = ComputeStock(stockMovementsByProduct)
                select (stockMovementsByProduct.Key, stock);

            return productsInStock;
        }

        private int ComputeStock(IEnumerable<StockMovement> stockMovements)
        {
            var productStock = stockMovements.Aggregate(0, (currentStock, stockMovement) =>
            {
                switch (stockMovement.MovementType)
                {
                    case StockMovementType.Movement:
                        return currentStock + stockMovement.Quantity;
                    case StockMovementType.Inventory:
                        return stockMovement.Quantity;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(stockMovement.MovementType), stockMovement.MovementType, "Stock movement aggregation");
                }
            });

            return productStock;
        }
    }
}