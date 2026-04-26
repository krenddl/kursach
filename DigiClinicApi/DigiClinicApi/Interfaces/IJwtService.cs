namespace DigiClinicApi.Interfaces
{
    public interface IJwtService
    {
        string GenerateToken(Models.User user);
    }
}
