using System.Collections.Generic;

namespace ModCreator.Models
{
    public class ModConfValueGroup
    {
        public string Name { get; set; }
        public List<ModConfValue> Values { get; set; }

        public ModConfValueGroup()
        {
            Values = new List<ModConfValue>();
        }
    }
}
