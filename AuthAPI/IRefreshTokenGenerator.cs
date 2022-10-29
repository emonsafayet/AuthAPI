namespace AuthAPI
{
    public interface IRefreshTokenGenerator
    {
        string GenerateToken(string username);
    }
}
