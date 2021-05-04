using System.Net.Http;
using System.Threading.Tasks;
using IdentityModel.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ScarletMVC.Services
{
    public class TokenService : ITokenService
    {
        private readonly ILogger<TokenService> logger;
        private readonly IOptions<IdentityServerSettings> identityServerSettings;
        private readonly DiscoveryDocumentResponse discoveryDocument;

        public TokenService(
            ILogger<TokenService> logger,
            IOptions<IdentityServerSettings> identityServerSettings)
        {
            this.logger = logger;
            this.identityServerSettings = identityServerSettings;

            using var httpClient = new HttpClient();

            discoveryDocument = httpClient.GetDiscoveryDocumentAsync(identityServerSettings.Value.DiscoveryUrl).GetAwaiter().GetResult();
            if (discoveryDocument.IsError)
            {
                logger.LogError($"Unable to get discovery document. Error: {discoveryDocument.Exception}");
                throw new System.Exception($"Unable to get discovery document. Error: {discoveryDocument.Exception}");
            }
        }

        public async Task<TokenResponse> GetToken(string scope)
        {
            using var httpClient = new HttpClient();

            var tokenResponse = await httpClient.RequestClientCredentialsTokenAsync(
                new ClientCredentialsTokenRequest {
                    Address = discoveryDocument.TokenEndpoint,
                    ClientId = identityServerSettings.Value.ClientName,
                    ClientSecret = identityServerSettings.Value.ClientPassword,
                    Scope = scope
                }
            );

            if (tokenResponse.IsError)
            {
                logger.LogError($"Unable to get token. Error: {discoveryDocument.Exception}");
                throw new System.Exception($"Unable to get token. Error: {discoveryDocument.Exception}");
            }

            return tokenResponse;
        }
    }
}