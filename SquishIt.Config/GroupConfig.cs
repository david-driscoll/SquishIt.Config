using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SquishIt.Config
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
        public virtual SquishItType RealType { get { return this.Type == SquishItType.EmbeddedCss ? SquishItType.Css : this.Type == SquishItType.EmbeddedJavaScript ? SquishItType.JavaScript : this.Type; } }
        public virtual string Assembly { get; set; }
        public virtual bool Minify { get; set; }
        public virtual SquishItForce Mode { get; set; }
        public virtual SquishItCache Cache { get; set; }
        public virtual bool DisableCache { get; set; }
        public virtual bool Embedded { get { return this.Type == SquishItType.EmbeddedJavaScript || this.Type == SquishItType.EmbeddedCss; } }
    }
}
