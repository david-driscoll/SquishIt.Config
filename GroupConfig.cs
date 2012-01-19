using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqusihIt.Config
{
    public class GroupConfig
    {
        public GroupConfig()
        {
            Files = new List<string>();
        }

        public virtual IEnumerable<string> Files { get; set; }
        public virtual string Key { get; set; }
        public virtual SquishItType Type { get; set; }
        public virtual bool Minify { get; set; }
        public virtual SquishItForce Mode { get; set; }
        public virtual SquishItCache Cache { get; set; }
    }
}
