namespace TradicioCarnica.ViewModels
{
    public class PagerViewModel
    {
        public int Current { get; set; }
        public int TotalPages { get; set; }
        public string? Fragment { get; set; }
        public Dictionary<string, object?> Query { get; set; } = new();
    }
}