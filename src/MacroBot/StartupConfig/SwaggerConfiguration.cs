using MacroBot.Core.Runtime;
using Microsoft.OpenApi.Models;

namespace MacroBot.StartupConfig;

public static class SwaggerConfiguration
{

    public static void AddSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                In = ParameterLocation.Header,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                Description = "API Key Authorization header",
            });
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    new string[] {}
                }
            });
        });
    }
    
    public static void ConfigureSwagger(this IApplicationBuilder app)
    {
        if (MacroBotEnvironment.IsProduction)
        {
            return;
        }
        
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Macro Bot API");
            c.RoutePrefix = "";
        });
    } 
}