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
        JavaScript
    }

    public class Startup
    {
        private static Startup staticStartup;
        public readonly SquishItConfigSettings Settings;
        private IDictionary<string, ConfigBundle> configBundles;
        private Dictionary<string, bool> mobileKeys;

        #region Static Methods
        public static void Load(SquishItConfigSettings settings = null)
        {
            if (settings == null)
                settings = new SquishItConfigSettings() { CacheMode = SquishItCache.Cached };

            if (staticStartup == null)
            {
                staticStartup = new Startup(settings);
            }
            staticStartup.Init();
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
            configBundles = new Dictionary<string, ConfigBundle>();
            mobileKeys = new Dictionary<string, bool>();
        }

        public void Init()
        {
            // Defered execution... so wonderful yet so annoying.
            // Without ToDictionary this doesnt get called until after the foreach below.
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
                    var lastModified = File.GetLastWriteTime(config);
                    var groups = ReadConfig.Read(config);
                    foreach (var group in groups)
                    {
                        var cacheName = String.Format("{0}-{1}", group.Type, group.Key).ToLower();

                        if (group.Type == SquishItType.Css)
                        {
                            LoadAddConfigBundle<CSSBundle>(cacheName, group, Bundle.Css());
                        }
                        else if (group.Type == SquishItType.JavaScript)
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
                    effectedBundles = configBundles
                        .Where(x => modifiedBundles
                            .Any(z => x.Value.BundledFiles.Any(c => c.ToLower() == z.Key))
                        )
                        .ToDictionary(x => x.Key, x => x.Value);
                    effectedBundles.ForEach(x =>
                    {
                        if (x.Value.Config.Type == SquishItType.Css)
                            (x.Value as ConfigBundle<CSSBundle>).Bundle = Bundle.Css();
                        else if (x.Value.Config.Type == SquishItType.JavaScript)
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
            bundle.BundledFiles.ForEach(x =>
            {
                if (configBundles.ContainsKey(x.ToLower()))
                {
                    LoadIntoBundle<T>(configBundles.FirstOrDefault(z => z.Key == x.ToLower()), bundledItem);
                }
                else
                {
                    bundledItem.Add(x);
                }
            });

            if (!bundledItemIsNull)
                return;

            if (bundle.IsCached)
                bundle.CacheBundle();
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
            if (configBundles.ContainsKey(key))
            {
                return (configBundles[key] as ConfigBundle<T>).GetBundle(force);
            }
            throw new Exception(String.Format("Bundle {0} does not exist!", key));
        }
    }
}
