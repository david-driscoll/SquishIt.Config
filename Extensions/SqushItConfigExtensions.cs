using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using SquishIt.Framework;
using System.Web;
using System.Web.Mvc;
using SquishIt.Framework.Base;
using SquishIt.Framework.Css;
using SquishIt.Framework.JavaScript;
using SquishIt.Config.Yaml.Grammar;
using System.Collections;
using SquishIt.Config;
using System.Reflection;

namespace SquishIt.Config.Extensions
{
    public static class SqushItConfigExtensions
    {
        private static Startup Startup = null;

        private static MvcHtmlString IncludeResource<T>(string format, bool mobile, SquishItForce force)
            where T : SquishIt.Framework.Base.BundleBase<T>
        {
            if (Startup == null)
                Startup = Startup.StaticStartup();

            return Startup.GetBundle<T>(format.ToLower(), mobile, force) as MvcHtmlString;
        }

        private static MvcHtmlString JavaScript(bool mobile, string key, SquishItForce force = SquishItForce.None)
        {
            return IncludeResource<JavaScriptBundle>(String.Format("JavaScript-{0}", key), mobile, force);
        }

        public static MvcHtmlString Css(bool mobile, string key, SquishItForce force = SquishItForce.None)
        {
            return IncludeResource<CSSBundle>(String.Format("Css-{0}", key), mobile, force);
        }

        public static MvcHtmlString JavaScript(this HtmlHelper helper, string key, SquishItForce force = SquishItForce.None)
        {
            return JavaScript(helper.ViewContext.HttpContext.Request.Browser.IsMobileDevice, key, force);
        }

        public static MvcHtmlString JavaScript(this HtmlHelper helper, IEnumerable<string> keys, SquishItForce force = SquishItForce.None)
        {
            var sb = new StringBuilder();
            foreach (var key in keys)
                sb.Append(helper.JavaScript(key, force));
            return MvcHtmlString.Create(sb.ToString());
        }

        public static MvcHtmlString JavaScript(this Controller controller, string key, SquishItForce force = SquishItForce.None)
        {
            return JavaScript(controller.ControllerContext.HttpContext.Request.Browser.IsMobileDevice, key, force);
        }

        public static MvcHtmlString JavaScript(this Controller controller, IEnumerable<string> keys, SquishItForce force = SquishItForce.None)
        {
            var sb = new StringBuilder();
            foreach (var key in keys)
                sb.Append(controller.JavaScript(key, force));
            return MvcHtmlString.Create(sb.ToString());
        }

        public static MvcHtmlString Css(this HtmlHelper helper, string key, SquishItForce force = SquishItForce.None)
        {
            return Css(helper.ViewContext.HttpContext.Request.Browser.IsMobileDevice, key, force);
        }

        public static MvcHtmlString Css(this HtmlHelper helper, IEnumerable<string> keys, SquishItForce force = SquishItForce.None)
        {
            var sb = new StringBuilder();
            foreach (var key in keys)
                sb.Append(helper.Css(key, force));
            return MvcHtmlString.Create(sb.ToString());
        }

        public static MvcHtmlString Css(this Controller controller, string key, SquishItForce force = SquishItForce.None)
        {
            return Css(controller.ControllerContext.HttpContext.Request.Browser.IsMobileDevice, key, force);
        }

        public static MvcHtmlString Css(this Controller controller, IEnumerable<string> keys, SquishItForce force = SquishItForce.None)
        {
            var sb = new StringBuilder();
            foreach (var key in keys)
                sb.Append(controller.Css(key, force));
            return MvcHtmlString.Create(sb.ToString());
        }
    }
}
