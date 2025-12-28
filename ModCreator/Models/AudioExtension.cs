namespace ModCreator.Models
{
    /// <summary>
    /// Audio extension model
    /// </summary>
    public class AudioExtension
    {
        public string Name { get; set; }
        public string Extension { get; set; }
        public string Desc { get; set; }

        public override string ToString() => Extension;
    }
}
