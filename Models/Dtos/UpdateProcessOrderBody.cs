namespace GBSWarehouse.Models.Dtos
{
    public class UpdateProcessOrderBody
    {
        public long? processOrderId { get; set; }
        public int? newQuantity { get; set; }
        public string appLang { get; set; } = "en";
    }
}
