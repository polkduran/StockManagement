namespace FnacDarty.JobInterview.Stock.Product
{
    public interface IProductRepository
    {
        void AddProduct(Product product);
        Product GetProductByEan(string ean);
    }
}