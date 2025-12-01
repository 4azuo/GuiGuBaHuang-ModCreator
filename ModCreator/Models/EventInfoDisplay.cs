namespace ModCreator.Models
{
    public class ModEventItemDisplay : EventInfo
    {
        public string DisplayText => string.IsNullOrEmpty(Category) ? DisplayName : $"{Category} - {DisplayName}";
    }
}
