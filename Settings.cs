using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SquishIt.Config
{
    public class Settings
    {
        public Settings()
        {
            LastModified = new Dictionary<string, DateTime>();
        }

        public virtual string[] ConfigFiles { get; set; }
        public virtual IDictionary<string, DateTime> LastModified { get; set; }

        private string _javaScriptPath = "~/Scripts";
        public virtual string JavaScriptPath
        {
            get
            {
                return _javaScriptPath;
            }
            set
            {
                _javaScriptPath = value;
            }
        }
        public virtual string JavaScriptRelativePath { get { return _javaScriptPath.Substring(1); } }

        private string _cssPath = "~/Content";
        public virtual string CssPath
        {
            get
            {
                return _cssPath;
            }
            set
            {
                _cssPath = value;
            }
        }
        public virtual string CssRelativePath { get { return _cssPath.Substring(1); } }

        private string _assetsPath = "~/Assets/";
        public virtual string AssetsPath
        {
            get
            {
                return _assetsPath;
            }
            set
            {
                _assetsPath = value;
            }
        }
        public virtual string AssetsRelativePath { get { return _assetsPath.Substring(1); } }

        public virtual SquishItCache CacheMode { get; set; }
    }
}
