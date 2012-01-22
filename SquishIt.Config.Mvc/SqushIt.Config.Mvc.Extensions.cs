using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Web;
using System.Web.Mvc;
using SquishIt.Config.Yaml.Grammar;
using System.Collections;
using SquishIt.Config;
using SquishIt.Config.Extensions;
using System.Reflection;

namespace SquishIt.Config.Mvc
{
    public static class Extensions
    {
        public static MvcHtmlString JavaScript(this HtmlHelper helper, string key, SquishItForce force = SquishItForce.None)
        {
            return MvcHtmlString.Create(HelperExtensions.JavaScript(helper.ViewContext.HttpContext.Request.Browser.IsMobileDevice, key, force));
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
            return MvcHtmlString.Create(HelperExtensions.JavaScript(controller.ControllerContext.HttpContext.Request.Browser.IsMobileDevice, key, force));
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
            return MvcHtmlString.Create(HelperExtensions.Css(helper.ViewContext.HttpContext.Request.Browser.IsMobileDevice, key, force));
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
            return MvcHtmlString.Create(HelperExtensions.Css(controller.ControllerContext.HttpContext.Request.Browser.IsMobileDevice, key, force));
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
