using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SquishIt.Framework.JavaScript;
using SquishIt.Config;
using SquishIt.Framework.Css;

namespace SquishIt.Config.Extensions
{
    public class HelperExtensions
    {
        private static Startup Startup = null;
        private static string IncludeResource<T>(string format, bool mobile, SquishItForce force)
            where T : SquishIt.Framework.Base.BundleBase<T>
        {
            if (Startup == null)
                Startup = Startup.StaticStartup();

            return Startup.GetBundle<T>(format.ToLower(), mobile, force);
        }

        public static string JavaScript(bool mobile, string key, SquishItForce force = SquishItForce.None)
        {
            return IncludeResource<JavaScriptBundle>(String.Format("JavaScript-{0}", key), mobile, force);
        }

        public static string Css(bool mobile, string key, SquishItForce force = SquishItForce.None)
        {
            return IncludeResource<CSSBundle>(String.Format("Css-{0}", key), mobile, force);
        }

    }
}
