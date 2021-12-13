using System;
using System.Collections.Generic;
using System.Linq;
using FnacDarty.JobInterview.Stock.Stock;

namespace FnacDarty.JobInterview.Stock.UnitTest
{
    public class InMemoryStockRepository : IStockRepository
    {
        private readonly InMemoryStore _store;
        private readonly StockMovementComparer _stockMovementComparer;

        public InMemoryStockRepository(InMemoryStore store)
        {
            _store = store;
            _stockMovementComparer = new StockMovementComparer();
        }

        public void AddStockMovement(StockMovement stockMovement)
        {
           var productStockMovements= _store.ProductStockMovements.GetOrAdd(stockMovement.Product,
                _ => new SortedSet<StockMovement>(_stockMovementComparer));
           productStockMovements.Add(stockMovement);
        }

        public void AddStockMovements(IEnumerable<StockMovement> stockMovements)
        {
            foreach (var stockMovement in stockMovements)
            {
                AddStockMovement(stockMovement);
            }
        }

        public StockMovement GetLatestInventoryMovement(Product.Product product)
        {
            if (_store.ProductStockMovements.TryGetValue(product, out var stockMovements))
            {
                return stockMovements.LastOrDefault(x => x.MovementType == StockMovementType.Inventory);
            }

            return null;
        }

        public IOrderedEnumerable<StockMovement> GetStockMovements(Product.Product product, DateTime fromDateTime, DateTime untilDateTime)
        {
            if (_store.ProductStockMovements.TryGetValue(product, out var stockMovements))
            {
                return stockMovements
                    .Where(x => x.MovementDate >= fromDateTime && x.MovementDate <= untilDateTime)
                    .ToList()
                    .OrderBy(x => x.MovementDate);
            }

            return Enumerable.Empty<StockMovement>().OrderBy(x => x.MovementDate);
        }

        public IEnumerable<StockMovement> GetStockMovements()
        {
            return _store.ProductStockMovements
                .SelectMany(x => x.Value)
                .ToList();
        }

        private class StockMovementComparer : IComparer<StockMovement>
        {
            public int Compare(StockMovement x, StockMovement y)
            {
                if (ReferenceEquals(x, y))
                {
                    return 0;
                }

                if (ReferenceEquals(null, y))
                {
                    return 1;
                }

                if (ReferenceEquals(null, x))
                {
                    return -1;
                }

                return x.MovementDate.CompareTo(y.MovementDate);
            }
        }
    }
}
