using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SquishIt.Config.Yaml.Grammar;
using System.IO;

namespace SquishIt.Config
{
    class ReadConfig
    {
        public static IEnumerable<GroupConfig> Read(string file)
        {
            YamlStream yaml = null;
            var replacedTabs = false;
            while (yaml == null)
            {
                try
                {
                    if (file.Contains("://"))
                    {
                        var split = file.Split(new[] { "://" }, StringSplitOptions.None);
                        var assemblyName = split.ElementAt(0);
                        var assembly = AppDomain.CurrentDomain.GetAssemblies().SingleOrDefault(x => x.GetName().Name == assemblyName);
                        var resourceName = split.ElementAt(1);
                        using (var stream = assembly.GetManifestResourceStream(resourceName))
                        {
                            if (stream == null) throw new InvalidOperationException(String.Format("Embedded resource not found: {0}", file));

                            string contents;
                            using (var sr = new StreamReader(stream))
                            {
                                contents = sr.ReadToEnd();
                            }
                            TextInput input = new TextInput(contents);
                            YamlParser parser = new YamlParser();
                            bool success;
                            yaml = parser.ParseYamlStream(input, out success);
                            if (!success)
                                throw new Exception(String.Format("Could not parse embedded YAML file \"{0}\"", file));
                        }
                    }
                    else
                    {
                        yaml = YamlParser.Load(file);
                    }
                }
                catch (Exception e)
                {
                    if (replacedTabs)
                        throw new Exception(String.Format("Could not parse YAML file \"{0}\"", file), e);
                    File.WriteAllLines(file, File.ReadAllLines(file).Select(x => x.Replace('\t'.ToString(), "    ").Replace('\u0009'.ToString(), "    ")));
                    replacedTabs = true;
                }
            }

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

                var assemblyScalar = ((entry.Value as Mapping).Enties.Where(x =>
                        x.Key.ToString().ToLower() == "assembly"
                    ).FirstOrDefault());
                string assembly = null;
                if (assemblyScalar != null)
                    assembly = (assemblyScalar.Value as Scalar).Text;

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
                if (type == "embedded-css")
                    configType = SquishItType.EmbeddedCss;
                if (type == "embedded-js")
                    configType = SquishItType.EmbeddedJavaScript;


                listConfigGroup.Add(new GroupConfig()
                    {
                        Files = files.Enties.Where(x => x is Scalar).Cast<Scalar>().Select(x => x.Text),
                        Cache = c,
                        Minify = minify,
                        Mode = squishitMode,
                        Type = configType,
                        Key = key,
                        Assembly = assembly,
                        DisableCache = cache == "disable",
                    });
            }

            return listConfigGroup;
        }
    }
}
