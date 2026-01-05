using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace BlogPlatform.Api.OpenApi;

/// <summary>
/// Adds Bearer token authentication to the OpenAPI document.
/// </summary>
public sealed class BearerSecuritySchemeTransformer : IOpenApiDocumentTransformer
{
    private readonly IAuthenticationSchemeProvider _authenticationSchemeProvider;

    public BearerSecuritySchemeTransformer(IAuthenticationSchemeProvider authenticationSchemeProvider)
    {
        _authenticationSchemeProvider = authenticationSchemeProvider;
    }

    public async Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        var authenticationSchemes = await _authenticationSchemeProvider.GetAllSchemesAsync();
        
        if (authenticationSchemes.Any(authScheme => authScheme.Name == "Bearer"))
        {
            // Add security scheme
            var securityScheme = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "Enter your JWT token.\n\nExample: \"eyJhbGciOiJIUzI1NiIs...\""
            };
            
            document.Components ??= new OpenApiComponents();
            document.Components.SecuritySchemes["Bearer"] = securityScheme;
            
            // Create a reference to the security scheme
            var schemeReference = new OpenApiSecuritySchemeReference("Bearer");
            
            // Add security requirement to all operations
            var securityRequirement = new OpenApiSecurityRequirement
            {
                [schemeReference] = new List<string>()
            };
            
            // Add security requirement at document level for all endpoints
            if (document.Paths != null)
            {
                foreach (var path in document.Paths.Values)
                {
                    if (path?.Operations != null)
                    {
                        foreach (var operation in path.Operations.Values)
                        {
                            operation.Security ??= new List<OpenApiSecurityRequirement>();
                            operation.Security.Add(securityRequirement);
                        }
                    }
                }
            }
        }
    }
}

