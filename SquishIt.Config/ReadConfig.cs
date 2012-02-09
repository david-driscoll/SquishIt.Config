using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SquishIt.Config.Yaml.Grammar;

namespace SquishIt.Config
{
    class ReadConfig
    {
        public static IEnumerable<GroupConfig> Read(string file)
        {
            var yaml = YamlParser.Load(file);

            var listConfigGroup = new List<GroupConfig>();

            foreach (var entry in (yaml.Documents.First().Root as Mapping).Enties)
            {
                var files = (entry.Value as Mapping).Enties.Where(x =>
                        x.Key.ToString().ToLower() == "files"
                    ).First().Value as Sequence;

                var type = ((entry.Value as Mapping).Enties.Where(x =>
                        x.Key.ToString().ToLower() == "type"
                    ).First().Value as Scalar).Text.ToLower();

                var minifyScalar = ((entry.Value as Mapping).Enties.Where(x =>
                        x.Key.ToString().ToLower() == "minify"
                    ).FirstOrDefault());
                bool minify = false;
                if (minifyScalar != null)
                    minify = (minifyScalar.Value as Scalar).Text.ToLower() == "false" ? false : true;

                var modeScalar = ((entry.Value as Mapping).Enties.Where(x =>
                        x.Key.ToString().ToLower() == "mode"
                    ).FirstOrDefault());
                string mode = null;
                if (modeScalar != null)
                    mode = (modeScalar.Value as Scalar).Text.ToLower();

                var cacheScalar = ((entry.Value as Mapping).Enties.Where(x =>
                        x.Key.ToString().ToLower() == "cache"
                    ).FirstOrDefault());
                string cache = null;
                if (cacheScalar != null)
                    cache = (cacheScalar.Value as Scalar).Text.ToLower();

                var key = entry.Key.ToString();

                var c = SquishItCache.Unset;
                if (cache != null && (cache == "memory" || cache == "file" || cache == "dynamic" || cache == "static"))
                {
                    if (cache == "dynamic")
                        c = SquishItCache.NamedDynamic;
                    else if (cache == "static")
                        c = SquishItCache.NamedStatic;
                    else if (cache == "file")
                        c = SquishItCache.Named;
                    else if (cache == "memory")
                        c = SquishItCache.Cached;
                }

                var squishitMode = SquishItForce.None;
                if (mode == "release")
                    squishitMode = SquishItForce.Release;
                else if (mode == "debug")
                    squishitMode = SquishItForce.Debug;

                var configType = SquishItType.None;
                if (type == "css")
                    configType = SquishItType.Css;
                if (type == "js")
                    configType = SquishItType.JavaScript;

                listConfigGroup.Add(new GroupConfig()
                    {
                        Files = files.Enties.Where(x => x is Scalar).Cast<Scalar>().Select(x => x.Text),
                        Cache = c,
                        Minify = minify,
                        Mode = squishitMode,
                        Type = configType,
                        Key = key,
                        DisableCache = cache == "disable",
                    });
            }

            return listConfigGroup;
        }
    }
}
