using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using SquishIt.Config.Extensions;
using System.IO;
using System.Reflection;

namespace SquishIt.Config
{
    public class SquishItConfigSettings
    {
        public SquishItConfigSettings(bool ignoreEmbedded = false)
        {
            var configFiles = Directory.GetFiles(System.AppDomain.CurrentDomain.BaseDirectory, "*.sic.yaml", SearchOption.AllDirectories);
            _configFiles = configFiles.ToList();
            FileFilters = new List<string>() { ".qunit.js", ".junit.js" };

            if (!ignoreEmbedded)
            {
                assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(x => !x.IsDynamic);
                foreach (var assembly in assemblies)
                {
                    var configs = assembly.GetManifestResourceNames().Where(x => x.EndsWith(".sic.embedded.yaml"));
                    if (configs.Count() > 0)
                        foreach (var c in configs)
                            _configFiles.Add(String.Format("{0}://{1}", assembly.GetName().Name, c));
                }
            }
        }

        public readonly IEnumerable<Assembly> assemblies;

        public virtual IList<string> FileFilters { get; set; }

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
        public virtual bool UseCDN { get; set; }
    }
}
