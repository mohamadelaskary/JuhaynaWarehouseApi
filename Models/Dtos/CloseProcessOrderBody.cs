namespace GBSWarehouse.Models.Dtos
{
    public class CloseProcessOrderBody
    {
        public long? processOrderId { get; set; }
        public string appLang { get; set; } = "en";
    }
}
