SquishIt.Config
===============
SquishIt.Config is a tool to bring configuration files to define your resource sets.  Similar to how Combres works, but to expand upon a couple areas.

Combres?
--------
If you haven't ever used Combres it's an extremely powerful resource minification tool.  If you don't need all the power, like filters for example, there are other alternatives like SquishIt.  The best part about SquishIt is the simplicity of use, but for large web applications, where you could have 10's of 10's of JavaScript files to resource that could take extra time.  I created SquishIt.Config to allow you to group those files in one central location and give some MVC extensions to make it super simple to use.

Differences to Combres
----------------------
The first difference is fairly obvious, instead of using XML for the file format SquishIt.Config uses YAML.  In the future I want refactor and interface that out and create a few projects that can produce the proper configuration from any format, XML, YAML, JSON, etc.

The second difference above Combres is the ability to define wildcards in the configuration file, to select a ton of different files at once. You can also define other resource groups, to nest them together.

The last major difference is you're allowed multiple configuration files.  You can either supply them at app start or follow the convention (*.sic.yaml) and they will be loaded.  You can also mix and match if you wish.

How to use?
===========
If you're going to use Web Activator, all you need to do is let it do it's magic.
For debugging, and changing files on the fly without having to restart IIS/IIS Express/Cassini make a call off to SquishIt.Config.Startup.StaticStartup().Init() as seen in the TestMvcApplication.  It's best not to call this method in production as it does have to at the very least, look at each config files last modified date.

Your best bet is look at the sample app and play around with it.

Cache Modes
-----------
Cached: Uses the Assets Controller supplied with SquishIt.Config.Mvc, and works very similarly to combres. ( This mode has not been tested against for a while )

Named: Files are cached to the server, and named after the Group Name.  Default behaviour for debug mode is NamedDynamic, when in realese is NamedStatic

NamedDynamic: Files are cached to the server, with a dynamic hash at the end in the script tag.

NamedStatic: Files are cached to the server, with the hash tag in the file name. ( NamedStatic does not currently do any file clean up, so any generated files must be cleared up from time to time )

