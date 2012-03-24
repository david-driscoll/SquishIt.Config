using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SquishIt.Framework.JavaScript;
using SquishIt.Framework.Css;
using System.Web;
using SquishIt.Config.Extensions;
using System.IO;
using SquishIt.Framework;
using System.Collections;
using SquishIt.Framework.Base;
using SquishIt.Config.Yaml.Grammar;

namespace SquishIt.Config
{
    public enum SquishItForce
    {
        None,
        Release,
        Debug,
        ReleaseUnminified,
    }

    public enum SquishItCache
    {
        Named,
        Cached,
        NamedDynamic,
        NamedStatic,
        Unset
    }

    public enum SquishItType
    {
        None,
        Css,
        JavaScript,
        EmbeddedCss,
        EmbeddedJavaScript
    }

    public class Startup
    {
        private static Startup staticStartup;
        public readonly SquishItConfigSettings Settings;
        private IDictionary<string, ConfigBundle> configBundles;
        private Dictionary<string, bool> mobileKeys;

        private readonly object configBundlesLock = new object();

        #region Static Methods
        public static void Load(SquishItConfigSettings settings = null)
        {
            if (settings == null)
                settings = new SquishItConfigSettings() { CacheMode = SquishItCache.Cached };

            if (staticStartup == null)
            {
                staticStartup = new Startup(settings);
            }
            //staticStartup.Init();
        }

        public static Startup StaticStartup()
        {
            if (staticStartup == null)
            {
                Startup.Load();
            }
            return staticStartup;
        }

        public static void UpdateBundles(IDictionary<string, ConfigBundle> configBundles)
        {
            staticStartup.configBundles = configBundles;
        }
        #endregion

        #region Startup
        public Startup(SquishItConfigSettings settings)
        {
            Settings = settings;

            lock (configBundlesLock)
            {
                configBundles = new Dictionary<string, ConfigBundle>();
                mobileKeys = new Dictionary<string, bool>();
            }
        }

        public void Init()
        {
            if (!(HttpContext.Current.IsDebuggingEnabled || !configBundles.Any()))
                return;
            // Defered execution... so wonderful yet so annoying.
            // Without ToDictionary this doesnt get called until after the foreach below.
            lock (configBundlesLock)
            {
                var modifiedBundles = configBundles
                    .Where(z => Settings.ConfigFiles.Any(x => z.Value.IsModified()))
                    .ToDictionary(x => x.Key, x => x.Value);

                if (configBundles.Any() && !modifiedBundles.Any())
                    return;

                if (!configBundles.Any() || modifiedBundles.Any())
                {
                    var modifiedConfigs = modifiedBundles.Select(x => x.Value.File);
                    if (!modifiedConfigs.Any())
                        modifiedConfigs = Settings.ConfigFiles.AsEnumerable();

                    foreach (var config in modifiedConfigs)
                    {
                        var lastModified = config.Contains("://") ? DateTime.MinValue : File.GetLastWriteTime(config);
                        var groups = ReadConfig.Read(config);
                        foreach (var group in groups)
                        {
                            var cacheType = group.RealType;
                            var cacheName = String.Format("{0}-{1}", cacheType, group.Key).ToLower();

                            if (cacheType == SquishItType.Css)
                            {
                                LoadAddConfigBundle<CSSBundle>(cacheName, group, Bundle.Css());
                            }
                            else if (cacheType == SquishItType.JavaScript)
                            {
                                LoadAddConfigBundle<JavaScriptBundle>(cacheName, group, Bundle.JavaScript());
                            }
                            configBundles[cacheName].LastModified = lastModified;
                            configBundles[cacheName].File = config;
                        }
                    }

                    IDictionary<string, ConfigBundle> effectedBundles = null;
                    if (!modifiedBundles.Any())
                    {
                        effectedBundles = configBundles;
                    }
                    else if (modifiedBundles.Count() != configBundles.Count())
                    {
                        // search all configBundles for modified bundle keys
                        effectedBundles = configBundles.Where(x => modifiedConfigs.Any(z => x.Value.File.ToLower() == z.ToLower())).ToDictionary(x => x.Key, x => x.Value); ;
                        effectedBundles.ForEach(x =>
                        {
                            if (x.Value.Config.RealType == SquishItType.Css)
                                (x.Value as ConfigBundle<CSSBundle>).Bundle = Bundle.Css();
                            else if (x.Value.Config.RealType == SquishItType.JavaScript)
                                (x.Value as ConfigBundle<JavaScriptBundle>).Bundle = Bundle.JavaScript();
                        });
                        effectedBundles = effectedBundles
                            .Union(modifiedBundles
                                .Where(x => !effectedBundles.ContainsKey(x.Key)))
                            .ToDictionary(x => x.Key, x => x.Value);
                    }

                    //Bundle.Css().ClearGroupBundlesCache();
                    //Bundle.JavaScript().ClearGroupBundlesCache();
                    Bundle.Css().ClearCache();
                    Bundle.JavaScript().ClearCache();

                    var keys = new List<string>();
                    foreach (DictionaryEntry c in HttpRuntime.Cache)
                        if (c.Key is string)
                            keys.Add(c.Key as string);
                    keys.ForEach(x => HttpRuntime.Cache.Remove(x));

                    foreach (var bundle in effectedBundles)
                    {
                        if (bundle.Value is ConfigBundle<JavaScriptBundle>)
                            LoadIntoBundle<JavaScriptBundle>(bundle);
                        else if (bundle.Value is ConfigBundle<CSSBundle>)
                            LoadIntoBundle<CSSBundle>(bundle);
                    }

                    Startup.UpdateBundles(configBundles);
                }
            }
        }

        private void LoadAddConfigBundle<T>(string cacheName, GroupConfig group, BundleBase<T> bundle)
            where T : SquishIt.Framework.Base.BundleBase<T>
        {
            if (configBundles.ContainsKey(cacheName))
            {
                var configBundle = GetConfigBundle<T>(cacheName);
                configBundle.Bundle = bundle;
                configBundle.Config = group;
            }
            else
            {
                configBundles.Add(cacheName, new ConfigBundle<T>(Settings)
                {
                    Name = group.Key,
                    Config = group,
                    Bundle = bundle,
                });
            }
        }

        private void LoadIntoBundle<T>(KeyValuePair<string, ConfigBundle> b, T bundledItem = null)
            where T : SquishIt.Framework.Base.BundleBase<T>
        {
            var bundle = b.Value as ConfigBundle<T>;
            var bundledItemIsNull = false;
            if (bundledItem == null)
            {
                bundledItem = bundle.Bundle as T;
                bundledItemIsNull = true;
            }

            var key = b.Key;
            var embedded = b.Value.Config.Embedded;
            bundle.BundledFiles.ForEach(x =>
            {
                if (configBundles.ContainsKey(x.ToLower()))
                {
                    LoadIntoBundle<T>(configBundles.FirstOrDefault(z => z.Key == x.ToLower()), bundledItem);
                }
                else
                {
                    if (ConfigBundle.Url.Match(x).Success)
                    {
                        var local = "";
                        var remote = "";
                        var loaded = false;
                        if (x.Contains("|"))
                        {
                            local = x.Substring(0, x.IndexOf("|")).Trim();
                            remote = x.Substring(x.IndexOf("|") + 1).Trim();
                            // Potential group resource
                            if (!System.IO.File.Exists(local.Replace("~/", System.AppDomain.CurrentDomain.BaseDirectory).Replace("/", "\\")))
                            {
                                if (configBundles.ContainsKey((typeof(T) == typeof(JavaScriptBundle) ? "javascript-" : "css-") + local.ToLower())
                                    && (HttpContext.Current.IsDebuggingEnabled
                                    || !Settings.UseCDN))
                                {
                                    LoadIntoBundle<T>(configBundles.FirstOrDefault(z => z.Key == (typeof(T) == typeof(JavaScriptBundle) ? "javascript-" : "css-") + local.ToLower()), bundledItem);
                                    loaded = true;
                                }
                                else
                                {
                                    local = Settings.JavaScriptPath + remote.Substring(remote.LastIndexOf("/")).Trim();
                                }
                            }
                            if (!loaded)
                            {
                                if (Settings.UseCDN)
                                    bundledItem.AddRemote(local, remote);
                                else
                                    AddPossiblyEmbeddedItem<T>(b, bundledItem, local);
                            }
                        }
                        else
                        {
                            local = Settings.JavaScriptPath + x.Substring(x.LastIndexOf("/")).Trim();
                            remote = x;
                            bundledItem.AddRemote(local, remote);
                        }
                    }
                    else
                    {
                        AddPossiblyEmbeddedItem<T>(b, bundledItem, x);
                    }
                }
            });

            if (!bundledItemIsNull)
                return;

            if (bundle.IsCached)
                bundle.CacheBundle();
        }

        private void AddPossiblyEmbeddedItem<T>(KeyValuePair<string, ConfigBundle> b, T bundledItem, string x)
            where T : SquishIt.Framework.Base.BundleBase<T>
        {
            var embedded = b.Value.Config.Embedded;
            if (embedded)
            {
                var valueFolders = x.Substring(2).Split('/');
                var realPath = x.Replace("~/", System.AppDomain.CurrentDomain.BaseDirectory).Replace("/", "\\");
                var currentFolder = System.AppDomain.CurrentDomain.BaseDirectory;
                foreach (var f in valueFolders.Take(valueFolders.Count() - 1))
                {
                    currentFolder = currentFolder + "\\" + f;
                    if (!System.IO.Directory.Exists(currentFolder))
                        System.IO.Directory.CreateDirectory(currentFolder);
                }

                var embeddedPath = String.Format("{0}://{1}", b.Value.Config.Assembly, x.Replace("~/", "").Replace("/", "."));
                bundledItem.AddEmbeddedResource(x, embeddedPath);
            }
            else
            {
                if (!System.IO.File.Exists(x.Replace("~/", System.AppDomain.CurrentDomain.BaseDirectory).Replace("/", "\\")))
                    throw new FileNotFoundException(String.Format("Resource \"{0}\" could not be found", x), x, null);
                bundledItem.Add(x);
            }
        }
        #endregion

        private ConfigBundle<T> GetConfigBundle<T>(string key)
            where T : SquishIt.Framework.Base.BundleBase<T>
        {
            if (configBundles.ContainsKey(key))
                return configBundles[key] as ConfigBundle<T>;
            return null;
        }

        public string GetBundle<T>(string key, bool mobile = false, SquishItForce force = SquishItForce.None)
            where T : SquishIt.Framework.Base.BundleBase<T>
        {
            lock (configBundlesLock)
            {
                if (configBundles.ContainsKey(key))
                {
                    return (configBundles[key] as ConfigBundle<T>).GetBundle(force);
                }
                throw new Exception(String.Format("Bundle {0} does not exist!", key));
            }
        }
    }
}
