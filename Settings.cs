using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqusihIt.Config
{
    public class Settings
    {
        public virtual string[] ConfigFiles { get; set; }

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

        public virtual SquishItCache CacheMode { get; set; }
    }
}
