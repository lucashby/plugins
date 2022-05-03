using System.Reflection;
using System.Runtime.CompilerServices;

using McMaster.NETCore.Plugins;

using PluginShared;

AppContext.SetSwitch("Switch.Microsoft.Data.SqlClient.UseManagedNetworkingOnWindows", true);

var pluginPath = Path.GetFullPath("../../../../SqlPlugin/bin/Debug/net6.0/SqlPlugin.dll");

TryExecutePlugin(pluginPath, out var weakReference);

// Force a GC collect to ensure unloaded has completed
for (var i = 0; weakReference.IsAlive && i < 10000; i++)
{
    GC.Collect();
    GC.WaitForPendingFinalizers();
}

Console.WriteLine($"{nameof(weakReference)} IsAlive? {weakReference.IsAlive}");

[MethodImpl(MethodImplOptions.NoInlining)]
void TryExecutePlugin(string path, out WeakReference weakRef)
{
    var loader = PluginLoader.CreateFromAssemblyFile(pluginPath, typeof(ISqlPlugin).Assembly.GetTypes(), cfg => { cfg.IsUnloadable = true; });
    var assembly = loader.LoadDefaultAssembly();
    var sqlPluginType = assembly.GetTypes().Single(t => !t.IsAbstract && typeof(ISqlPlugin).IsAssignableFrom(t));

    if (sqlPluginType == null) throw new ApplicationException("Type from assembly can't be null");

    using (var sqlPlugin = Activator.CreateInstance(sqlPluginType) as ISqlPlugin)
    {
        sqlPlugin?.DoWork();
    }

    loader.Dispose();

    weakRef = new WeakReference(assembly);
}
