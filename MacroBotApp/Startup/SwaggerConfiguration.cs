namespace MacroBot.Startup;

public static class SwaggerConfiguration
{
    public static void ConfigureSwagger(this IApplicationBuilder app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Macro Bot API");
            c.RoutePrefix = "";
        });
    } 
}