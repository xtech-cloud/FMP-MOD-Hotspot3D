
using XTC.FMP.MOD.Hotspot3D.App.Service;
public static class MyProgram
{
    public static void PreBuild(WebApplicationBuilder? _builder)
    {
        _builder?.Services.AddSingleton<SingletonServices>();
    }

    public static void PreRun(WebApplication? _app)
    {
    }
}
