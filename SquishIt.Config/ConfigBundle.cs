using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SquishIt.Framework.Base;
using System.IO;
using System.Web;
using SquishIt.Config.Extensions;
using SquishIt.Framework.JavaScript;
using System.Text.RegularExpressions;

namespace SquishIt.Config
{
    public class ConfigBundle<T> : ConfigBundle
        where T : SquishIt.Framework.Base.BundleBase<T>
    {
        public ConfigBundle(SquishItConfigSettings settings)
            : base(settings)
        {
        }

        public virtual BundleBase<T> Bundle { get; set; }

        public string GetBundle(SquishItForce force)
        {
            var bundle = Bundle as T;

            if (!IsCached || Config.DisableCache)
                CacheBundle(force);
            var returnTags = RenderBundle();
            if (returnTags == String.Empty)
            {
                CacheBundle(force);
                returnTags = RenderBundle();
            }
            return returnTags;
        }

        private SquishItCache GetCacheMode()
        {
            return Config.Cache == SquishItCache.Unset ? _settings.CacheMode : Config.Cache;
        }

        private string GetBundlePath()
        {
            return typeof(T) == typeof(JavaScriptBundle) ? _settings.JavaScriptPath : _settings.CssPath;
        }

        private string GetBundleExtension()
        {
            return typeof(T) == typeof(JavaScriptBundle) ? "js" : "css";
        }

        public void CacheBundle(SquishItForce force = SquishItForce.None)
        {
            var cacheMode = GetCacheMode();
            var extension = GetBundleExtension();
            var path = GetBundlePath();
            var key = Name.ToLower();
            ForceBundle(force);

            if (cacheMode == SquishItCache.Cached)
                Bundle.AsCached(key, String.Format("{0}/{1}/{2}", _settings.AssetsPath, extension, key));
            else if (cacheMode == SquishItCache.Named && HttpContext.Current.IsDebuggingEnabled || cacheMode == SquishItCache.NamedDynamic)
                Bundle.AsNamed(key, String.Format("{0}/{1}.squishit.{2}", path, key, extension));
            else if (cacheMode == SquishItCache.Named && !HttpContext.Current.IsDebuggingEnabled || cacheMode == SquishItCache.NamedStatic)
                Bundle.AsNamed(key, String.Format("{0}/{1}.#.squishit.{2}", path, key, extension));
            if (!IsCached)
                IsCached = true;
        }

        public void ForceBundle(SquishItForce force)
        {
            force = force == SquishItForce.None ? Config.Mode : force;
            if (force == SquishItForce.Release || force == SquishItForce.ReleaseUnminified)
                Bundle.ForceRelease();
            if (force == SquishItForce.Debug)
                Bundle.ForceDebug();
            //if (force == SquishItForce.ReleaseUnminified)
            //    bundle.DisableMinify();
        }

        public string RenderBundle()
        {
            var cacheMode = GetCacheMode();
            var extension = GetBundleExtension();
            var path = GetBundlePath();
            var key = Name.ToLower();
            if (cacheMode == SquishItCache.Cached)
            {
                return Bundle.RenderCachedAssetTag(key);
            }
            else if (cacheMode == SquishItCache.Named || cacheMode == SquishItCache.NamedDynamic || cacheMode == SquishItCache.NamedStatic)
            {
                var cacheKey = String.Format("{0}/{1}.#.squishit.{2}", path, key, extension);
                if (cacheMode == SquishItCache.Named && HttpContext.Current.IsDebuggingEnabled || cacheMode == SquishItCache.NamedDynamic)
                    cacheKey = cacheKey.Replace(".#", "");

                return Bundle.Render(cacheKey);
            }
            return null;
        }
    }

    public class ConfigBundle
    {
        protected readonly SquishItConfigSettings _settings;
        public static Regex Url = new Regex("(http|https)://", RegexOptions.Compiled);
        public ConfigBundle(SquishItConfigSettings settings)
        {
            _settings = settings;
        }

        public virtual string Name { get; set; }
        private GroupConfig _config;
        public virtual GroupConfig Config
        {
            get
            {
                return _config;
            }
            set
            {
                files = null;
                _config = value;
            }
        }
        public virtual bool IsCached { get; set; }
        public virtual DateTime LastModified { get; set; }
        public virtual string File { get; set; }

        public bool IsModified()
        {
            if (BundledFiles.Any(x =>
                {
                    var fileName = x.Replace("~/", System.AppDomain.CurrentDomain.BaseDirectory).Replace("/", "\\");
                    return !this.Config.Embedded && !Url.Match(x).Success && System.IO.File.Exists(fileName) && LastModifiedBundledFiles[x] < System.IO.File.GetLastWriteTime(fileName);
                }
            ))
                return true;
            return LastModified < (this.File.Contains("://") ? DateTime.MinValue : System.IO.File.GetLastWriteTime(File));
        }

        #region Bundled Files
        private string[] FindFiles(string path)
        {
            SearchOption searchOptions = SearchOption.TopDirectoryOnly;
            if (path.Contains("**"))
                searchOptions = SearchOption.AllDirectories;

            if (path.Contains("|"))
            {
                if (HttpContext.Current.IsDebuggingEnabled)
                    path = path.Substring(0, path.IndexOf("|")).Trim();
                else
                    path = path.Substring(path.IndexOf("|") + 1).Trim();
            }

            if (path != null && path != String.Empty)
            {
                if (this.Config.Embedded)
                    throw new NotSupportedException("File wild cards are not supported with Embedded scripts");
                var physicalPath = path.Replace("~/", System.AppDomain.CurrentDomain.BaseDirectory);
                physicalPath = physicalPath.Substring(0, physicalPath.LastIndexOf("/")).Replace("/", "\\");

                var extension = path.Substring(path.LastIndexOf("/") + 1);
                var files = Directory.GetFiles(physicalPath, extension, searchOptions).Where(x => !x.EndsWith("vsdoc.js"));

                var virtualFiles = new List<string>();
                foreach (var file in files)
                {
                    var pass = true;
                    foreach (var filter in _settings.FileFilters)
                    {
                        if (file.EndsWith(filter))
                            pass = false;
                    }
                    if (pass)
                        virtualFiles.Add(file.Replace(System.AppDomain.CurrentDomain.BaseDirectory, "~/").Replace("\\", "/"));
                }
                return virtualFiles.ToArray();
            }
            return new string[] { };
        }

        public bool EmbeddedFileExists(string file)
        {
            var assemblyName = this.Config.Assembly;
            var assembly = _settings.assemblies.SingleOrDefault(x => x.GetName().Name == assemblyName);
            var resourceName = assemblyName + file.Replace("~", "").Replace("/", ".");
            return assembly.GetManifestResourceNames().Any(x => x.ToLower() == resourceName.ToLower());
        }

        private void AddFile(string value)
        {
            if (value != null && value != String.Empty)
            {
                var isHtml = Url.Match(value).Success;
                if (isHtml)
                {
                    files.Add(value);
                }
                else
                {
                    if (value.Contains("|"))
                    {
                        if (HttpContext.Current.IsDebuggingEnabled)
                            value = value.Substring(0, value.IndexOf("|")).Trim();
                        else
                            value = value.Substring(value.IndexOf("|") + 1).Trim();
                    }

                    var fileExists = System.IO.File.Exists(value.Replace("~/", System.AppDomain.CurrentDomain.BaseDirectory).Replace("/", "\\"));
                    if ((!this.Config.Embedded && !fileExists) || (this.Config.Embedded && !this.EmbeddedFileExists(value)))
                    {
                        value = String.Format("{0}-{1}", Config.RealType, value);
                    }

                    if (!files.Any(x => x == value))
                    {
                        files.Add(value);
                    }
                }
            }
        }

        private List<string> files;
        public virtual IList<string> BundledFiles
        {
            get
            {
                if (files != null)
                    return files;
                files = new List<string>();
                foreach (var file in Config.Files)
                {
                    if (!this.Config.Embedded && file.Contains("*"))
                    {
                        FindFiles(file).ForEach(x => AddFile(x));
                    }
                    else
                    {
                        AddFile(file);
                    }
                }
                lastModifiedFiles = null;
                return files;
            }
        }

        private Dictionary<string, DateTime> lastModifiedFiles;
        public virtual IDictionary<string, DateTime> LastModifiedBundledFiles
        {
            get
            {
                if (lastModifiedFiles == null)
                    lastModifiedFiles = new Dictionary<string, DateTime>();
                foreach (var file in BundledFiles)
                {
                    var f = file.Replace("~/", System.AppDomain.CurrentDomain.BaseDirectory).Replace("/", "\\");
                    if (!lastModifiedFiles.ContainsKey(file) && !Url.Match(file).Success && System.IO.File.Exists(f))
                    {
                        lastModifiedFiles[file] = this.Config.Embedded ? DateTime.MinValue : System.IO.File.GetLastWriteTime(f);
                    }
                }
                return lastModifiedFiles;
            }
        }
        #endregion
    }
}
