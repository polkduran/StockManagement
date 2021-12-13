using System;

namespace FnacDarty.JobInterview.Stock.Stock
{
    public class StockMovement
    {
        public DateTime MovementDate { get; set; }
        public string Label { get; set; }
        public StockMovementType MovementType { get; set; }

        // We could store only the product's Ean whether we want a relational constraint or not
        public Product.Product Product { get; set; }
        public int Quantity { get; set; }
    }
}
