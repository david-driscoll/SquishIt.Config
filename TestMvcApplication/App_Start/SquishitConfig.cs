using System;
using SquishIt.Config;
[assembly: WebActivator.PostApplicationStartMethod(typeof(TestMvcApplication.App_Start.SquishItConfigStartup), "PostStart")]
namespace TestMvcApplication.App_Start
{
    public static class SquishItConfigStartup
    {
        public static void PostStart()
        {
            SquishIt.Config.Startup.Load(new SquishItConfigSettings() { CacheMode = SquishItCache.NamedDynamic });
		}
    }
}