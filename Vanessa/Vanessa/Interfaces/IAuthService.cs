namespace Vanessa.Interfaces
{
    public interface IAuthService
    {
        string ConvertirContraseña(string contraseña);
        bool VerificarContraseña(string hashCompleto, string contraseña);
        bool EsContraseñaSegura(string contraseña);
    }
}
