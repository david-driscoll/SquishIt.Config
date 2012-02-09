using System;
using SquishIt.Config;
using System.Web;
[assembly: WebActivator.PreApplicationStartMethod(typeof($rootnamespace$.SquishItConfigStartup), "Prestart")]
namespace $rootnamespace$
{
    /// <summary>
    /// SquishIt.Config WebActivator
    /// </summary>
    public static class SquishItConfigStartup
    {
        /// <summary>
        /// Called just before the application starts
        /// </summary>
        public static void Prestart()
        {
            SquishIt.Config.Startup.Load(new SquishItConfigSettings() { CacheMode = SquishItCache.NamedDynamic });
            // Register our module
            Microsoft.Web.Infrastructure.DynamicModuleHelper.DynamicModuleUtility.RegisterModule(typeof(StartupBeginRequest));
        }
    }

    /// <summary>
    /// SquishIt.Config WebActivator BegeinRequest Handler
    /// </summary>
    class StartupBeginRequest : IHttpModule
    {
        /// <summary>
        /// Called to bind to the application
        /// </summary>
        public void Init(HttpApplication context)
        {
            if (context == null)
                throw new ArgumentNullException("context", "must be defined");
            context.BeginRequest += new EventHandler(OnBeginRequest);
        }

        /// <summary>
        /// Called with every request, goes through all the config files to see if there are any changes.
        /// </summary>
        void OnBeginRequest(object sender, EventArgs e)
        {
            if (HttpContext.Current.IsDebuggingEnabled)
                SquishIt.Config.Startup.StaticStartup().Init();
        }

        /// <summary>
        /// Dispose of the application event.
        /// </summary>
        public void Dispose()
        {
        }
    }
}