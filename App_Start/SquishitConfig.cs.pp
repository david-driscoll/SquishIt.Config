[assembly: WebActivator.PostApplicationStartMethod(typeof($rootnamespace$.App_Start.SquishItConfigStartup), "PostStart")]
using SquishIt.Config.;
namespace $rootnamespace$.App_Start {
    public static class SquishItConfigStartup {
        public static void PostStart() {
            SquishIt.Config.Startup.Load(new SquishIt.Config.Settings()
			{
				ConfigFiles = new string[] {
					String.Format("{0}{1}", System.AppDomain.CurrentDomain.BaseDirectory, "SquishIt.Config.yaml")
				},
			});
		}
    }
}