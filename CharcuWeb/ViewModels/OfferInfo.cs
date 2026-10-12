namespace TradicioCarnica.ViewModels
{
    public class OfferInfo
    {
        public decimal? NewPrice { get; set; }   // null en ofertas tipo "compra X y llévate Y"
        public string Badge { get; set; } = "";
    }
}