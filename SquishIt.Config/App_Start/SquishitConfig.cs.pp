using System;
using SquishIt.Config;
using System.Web;
using TestMvcApplication;
[assembly: WebActivator.PreApplicationStartMethod(typeof($rootnamespace$.App_Start.SquishItConfigStartup), "PreStart")]
namespace $rootnamespace$.App_Start
{
    public static class SquishItConfigStartup
    {
        public static void PreStart()
        {
            SquishIt.Config.Startup.Load(new SquishItConfigSettings() { CacheMode = SquishItCache.NamedDynamic });
            // Register our module
            Microsoft.Web.Infrastructure.DynamicModuleHelper.DynamicModuleUtility.RegisterModule(typeof(StartupBeginRequest));
		}

        public static void BeginRequest(object obj, EventArgs args)
        {
            if (HttpContext.Current.IsDebuggingEnabled)
                SquishIt.Config.Startup.StaticStartup().Init();
        }

        public class StartupBeginRequest : IHttpModule
        {
            public void Init(HttpApplication application)
            {
                application.BeginRequest += new EventHandler(OnBeginRequest);
            }

            void OnBeginRequest(object sender, EventArgs e)
            {
                if (HttpContext.Current.IsDebuggingEnabled)
                    SquishIt.Config.Startup.StaticStartup().Init();
            }

            public void Dispose()
            {
            }
        }
    }
}