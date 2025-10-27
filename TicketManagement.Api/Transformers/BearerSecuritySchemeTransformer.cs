using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace TicketManagement.Api.Transformers;

internal sealed class BearerSecuritySchemeTransformer(
    IAuthenticationSchemeProvider authenticationSchemeProvider)
    : IOpenApiDocumentTransformer
{
    public async Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        // Lấy danh sách các Authentication Schemes
        var authenticationSchemes = await authenticationSchemeProvider.GetAllSchemesAsync();

        // Nếu trong app có Bearer authentication
        if (authenticationSchemes.Any(authScheme => authScheme.Name == "Bearer"))
        {
            // Định nghĩa security scheme
            var requirements = new Dictionary<string, OpenApiSecurityScheme>
            {
                ["Bearer"] = new()
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    In = ParameterLocation.Header,
                    BearerFormat = "JWT",
                    Description = "Enter JWT Bearer token only (without 'Bearer ' prefix)"
                }
            };

            // Thêm vào components
            document.Components ??= new OpenApiComponents();
            document.Components.SecuritySchemes = requirements;

            // Thêm security requirement cho tất cả endpoints
            foreach (var operation in document.Paths.Values.SelectMany(p => p.Operations))
            {
                operation.Value.Security ??= new List<OpenApiSecurityRequirement>();
                operation.Value.Security.Add(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Id = "Bearer",
                                Type = ReferenceType.SecurityScheme
                            }
                        },
                        new List<string>()
                    }
                });
            }
        }
    }
}