using System.Reflection;
using System.Runtime.Loader;

namespace Alba;

internal static class JasperFxEnvironmentAutoStartHost
{
    private static string JasperFxDllPath =>
        Path.Combine(AppContext.BaseDirectory, "JasperFx.dll");

    private static readonly Lazy<Action> AutoStartHostEnabler = new(() =>
    {
        var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(JasperFxDllPath);
        var environment = assembly.GetType("JasperFx.CommandLine.JasperFxEnvironment");
        var autoStartHost = environment?.GetProperty(
            "AutoStartHost",
            BindingFlags.Static | BindingFlags.Public);

        return () => autoStartHost?.SetValue(null, true);
    });

    public static void Enable()
    {
        if (AutoStartHostEnabler.IsValueCreated)
        {
            AutoStartHostEnabler.Value.Invoke();
            return;
        }

        if (File.Exists(JasperFxDllPath))
            AutoStartHostEnabler.Value.Invoke();
    }
}
