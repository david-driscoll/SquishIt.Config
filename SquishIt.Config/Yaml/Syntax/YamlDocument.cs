using System;
using System.Collections.Generic;
using System.Text;

namespace SquishIt.Config.Yaml.Grammar
{
    public partial class YamlDocument
    {
        public DataItem Root;

        public List<Directive> Directives = new List<Directive>();

    }
}
