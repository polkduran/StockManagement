using System;
using FnacDarty.JobInterview.Stock.Product;

namespace FnacDarty.JobInterview.Stock.UnitTest
{
    public class InMemoryProductRepository : IProductRepository
    {
        private readonly InMemoryStore _store;

        public InMemoryProductRepository(InMemoryStore store)
        {
            _store = store;
        }

        public void AddProduct(Product.Product product)
        {
            if (product == null)
            {
                throw new ArgumentNullException(nameof(product));
            }

            if (string.IsNullOrWhiteSpace(product.Ean))
            {
                throw new ArgumentException($"Cannot add a product to the store with an empty Ean");
            }

            if (!_store.ProductsByEan.TryAdd(product.Ean, product))
            {
                throw new InvalidOperationException($"Cannot add {product} to the store");
            }
        }

        public Product.Product GetProductByEan(string ean)
        {
            if (string.IsNullOrWhiteSpace(ean))
            {
                throw new ArgumentException($"Cannot get product from store for an empty ean");
            }

            _store.ProductsByEan.TryGetValue(ean, out var product);
            return product;
        }
    }
}
