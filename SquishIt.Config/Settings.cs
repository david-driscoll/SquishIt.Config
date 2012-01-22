using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SquishIt.Config.Extensions;
using System.IO;

namespace SquishIt.Config
{
    public class Settings
    {
        public Settings()
        {
            LastModified = new Dictionary<string, DateTime>();
            
            var configFiles = Directory.GetFiles(System.AppDomain.CurrentDomain.BaseDirectory, "*.sic.yaml", SearchOption.AllDirectories);
            _configFiles = configFiles.ToList();
        }

        private IList<string> _configFiles;
        public virtual string[] ConfigFiles
        {
            get
            {
                return _configFiles.ToArray();
            }
            set
            {
                value.ForEach(x => { var s = x.Replace("~/", System.AppDomain.CurrentDomain.BaseDirectory).Replace("/", "\\"); if (!_configFiles.Any(z => z == s)) _configFiles.Add(s); });
            }
        }
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
