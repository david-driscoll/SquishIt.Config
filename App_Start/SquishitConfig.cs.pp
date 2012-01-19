[assembly: WebActivator.PostApplicationStartMethod(typeof($rootnamespace$.App_Start.SquishItConfigStartup), "PostStart")]
using SqusihIt.Config;
namespace $rootnamespace$.App_Start {
    public static class SquishItConfigStartup {
        public static void PostStart() {
            SqusihIt.Config.Startup.Load(new SqusihIt.Config.Settings()
			{
				ConfigFiles = new string[] {
					String.Format("{0}{1}", System.AppDomain.CurrentDomain.BaseDirectory, "squishit.config.yaml")
				},
			});
		}
    }
}