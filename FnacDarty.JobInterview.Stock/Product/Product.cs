namespace FnacDarty.JobInterview.Stock.Product
{
    public class Product
    {
        public string Ean { get; set; }

        public override string ToString()
        {
            return Ean;
        }
    }
}
