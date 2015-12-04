namespace Alba.StaticFiles
{
    public interface IStaticFiles
    {
        IStaticFile Find(string relativeUrl);
    }
}