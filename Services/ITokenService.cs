using System.Threading.Tasks;
using IdentityModel.Client;

namespace ScarletMVC.Services
{
    public interface ITokenService
    {
        Task<TokenResponse> GetToken(string scope);
    }
}