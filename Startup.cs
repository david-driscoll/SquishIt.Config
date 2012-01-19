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
using System.Web.Mvc;

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
        public readonly Settings Settings;
        private IDictionary<string, ConfigBundle> configBundles;
        private Dictionary<string, bool> mobileKeys;

        #region Static Methods
        public static void Load(Settings settings)
        {
            if (staticStartup == null)
            {
                staticStartup = new Startup(settings);
            }
            staticStartup.Init();
        }

        public static Startup StaticStartup()
        {
            return staticStartup;
        }

        public static void UpdateBundles(IDictionary<string, ConfigBundle> configBundles)
        {
            staticStartup.configBundles = configBundles;
        }
        #endregion

        #region Startup
        public Startup(Settings settings)
        {
            Settings = settings;
            configBundles = new Dictionary<string, ConfigBundle>();
            mobileKeys = new Dictionary<string, bool>();
        }

        public void Init()
        {
            var runSetup = false;
            Settings.ConfigFiles.ForEach(x =>
            {
                if (!Settings.LastModified.ContainsKey(x))
                {
                    Settings.LastModified.Add(x, File.GetLastWriteTime(x));
                    runSetup = true;
                }
            });
            if (!runSetup && !Settings.ConfigFiles.Any(x => File.GetLastWriteTime(x) > Settings.LastModified[x]))
                return;

            foreach (var config in Settings.ConfigFiles)
            {
                Settings.LastModified[config] = File.GetLastWriteTime(config);
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
                }
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

            foreach (var bundle in configBundles)
            {
                if (bundle.Value is ConfigBundle<JavaScriptBundle>)
                    LoadIntoBundle<JavaScriptBundle>(bundle);
                else if (bundle.Value is ConfigBundle<CSSBundle>)
                    LoadIntoBundle<CSSBundle>(bundle);
            }

            Startup.UpdateBundles(configBundles);
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

        public MvcHtmlString GetBundle<T>(string key, bool mobile = false, SquishItForce force = SquishItForce.None)
            where T : SquishIt.Framework.Base.BundleBase<T>
        {
            if (configBundles.ContainsKey(key))
            {
                return MvcHtmlString.Create((configBundles[key] as ConfigBundle<T>).GetBundle(force));
            }
            throw new Exception(String.Format("Bundle {0} does not exist!", key));
        }
    }
}
