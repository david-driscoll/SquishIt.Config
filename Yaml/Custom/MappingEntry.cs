﻿using System;
using System.Collections.Generic;
using System.Text;

namespace SquishIt.ConfigYaml.Grammar
{
    public partial class MappingEntry
    {
        public override string ToString()
        {
            return String.Format("{{Key:{0}, Value:{1}}}", Key, Value);
        }
    }
}
