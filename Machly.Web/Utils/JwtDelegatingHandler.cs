using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace Machly.Web.Utils
{
    public class JwtDelegatingHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _context;

        public JwtDelegatingHandler(IHttpContextAccessor context)
        {
            _context = context;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var httpContext = _context.HttpContext;

            if (httpContext != null && httpContext.User.Identity!.IsAuthenticated)
            {
                // Obtener token desde AuthenticationProperties
                // El token está almacenado en AuthenticationProperties usando StoreTokens
                // y estos están serializados en la cookie de autenticación "MachlyAuth"
                // GetTokenAsync lee desde AuthenticationProperties que están en la cookie "MachlyAuth"
                var token = await httpContext.GetTokenAsync("access_token");

                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Authorization =
                        new AuthenticationHeaderValue("Bearer", token);
                }
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
