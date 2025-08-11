namespace LearnDotNet7.Service
{
    public interface IRefreshHandler
    {
        Task<string> GenerateToken(string username);
    }
}
