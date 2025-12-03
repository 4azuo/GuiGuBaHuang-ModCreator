namespace ModCreator.Models
{
    /// <summary>
    /// Variable type model
    /// </summary>
    public class VarType
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Desc { get; set; }
        public bool IsHidden { get; set; } = false;

        public override string ToString() => Type;
    }
}
