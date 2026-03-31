namespace F1Store.Models.Favorite
{
    public class FavoriteVM
    {
        // Използваме списък от обекти за визуализация
        public IEnumerable<FavoriteItemVM> Items { get; set; } = new List<FavoriteItemVM>();
    }

    public class FavoriteItemVM
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public string Picture { get; set; } = null!;
        public decimal Price { get; set; }
    }
}