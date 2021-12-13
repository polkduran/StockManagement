using System;
using System.Collections.Generic;
using System.Linq;

namespace FnacDarty.JobInterview.Stock.Stock
{
    public interface IStockRepository
    {
        void AddStockMovement(StockMovement stockMovement);
        void AddStockMovements(IEnumerable<StockMovement> stockMovements);
        StockMovement GetLatestInventoryMovement(Product.Product product);
        IOrderedEnumerable<StockMovement> GetStockMovements(Product.Product product, DateTime fromDateTime, DateTime untilDateTime);
        IEnumerable<StockMovement> GetStockMovements();
    }
}