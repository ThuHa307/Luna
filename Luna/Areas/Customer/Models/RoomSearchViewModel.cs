namespace Luna.Models
{
    public class RoomSearchViewModel
    {
        public int? RoomId { get; set; }
        public string? RoomStatus { get; set; }
        public int? TypeId { get; set; }
        public string? TypeName { get; set; }
        public decimal? TypePrice { get; set; }
        public string? Description { get; set; }
        public string? TypeImg { get; set; }
        public int? TotalEmpty { get; set; }
    }
}
