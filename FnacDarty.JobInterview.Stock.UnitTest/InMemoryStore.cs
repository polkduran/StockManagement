using System.Collections.Concurrent;
using System.Collections.Generic;
using FnacDarty.JobInterview.Stock.Stock;

namespace FnacDarty.JobInterview.Stock.UnitTest
{
    public class InMemoryStore
    {
        public ConcurrentDictionary<string, Product.Product> ProductsByEan { get; } =
            new ConcurrentDictionary<string, Product.Product>();

        public ConcurrentDictionary<Product.Product, SortedSet<StockMovement>> ProductStockMovements { get; }
            = new ConcurrentDictionary<Product.Product, SortedSet<StockMovement>>();
    }
}
