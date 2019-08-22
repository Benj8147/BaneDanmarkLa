namespace BaneDanmarkLa.Pn.Abstractions
{
    public interface IBaneDanmarkLaLocalizationService
    {
        string GetString(string key);
        string GetString(string format, params object[] args);
    }
}