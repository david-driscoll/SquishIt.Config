﻿using System;
using System.Collections.Generic;
using System.Text;

namespace SquishIt.ConfigYaml.Grammar
{
    public partial class Scalar : DataItem
    {
        public Scalar()
        {
            this.Text = String.Empty;
        }

        public override string ToString()
        {
            return Text;
        }
    }
}
