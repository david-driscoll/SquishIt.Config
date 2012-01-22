using System;
using SquishIt.Config;
[assembly: WebActivator.PostApplicationStartMethod(typeof($rootnamespace$.App_Start.SquishItConfigStartup), "PostStart")]
namespace $rootnamespace$.App_Start {
    public static class SquishItConfigStartup {
        public static void PostStart() {
            SquishIt.Config.Startup.Load();
		}
    }
}