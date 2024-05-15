using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using GameHubShared.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();
builder.Services.AddResponseCompression(opts => {
    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        new[] { "application/octet-stream" });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "AllowAllOrigins",
                    builder  =>
                    {
                        builder.AllowAnyOrigin() // allows any origin
                            .AllowAnyHeader()
                            .AllowAnyMethod();
                    });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer( options => {
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters() {
            ValidateIssuer = true,
            ValidateLifetime = true,
            ValidateAudience = false,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
        options.Events = new JwtBearerEvents() {
            OnAuthenticationFailed = async context => {
                var ex = context.Exception;
                Console.WriteLine(ex.Message);
            }
        };
    });
builder.Services.AddAuthorization(options => {
    options.AddPolicy("PolicyOfTruth", policy => policy.RequireRole("LoggedInUser"));
});

builder.Services.AddDbContext<GameHubApi.DataBase.Context>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")!));
builder.Services.AddSingleton(new List<LobbyModel>() {
    new WaitingLobbyModel(Guid.NewGuid(), 2, "Tic Tac Toe", "A lobby for playing Tic Tac Toe."),
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    try
    {
        var context = services.GetRequiredService<GameHubApi.DataBase.Context>();
        context.Database.Migrate();
    }
    catch (Exception _)
    {
        scope.ServiceProvider.GetRequiredService<ILogger<Program>>().LogError(_, "An error occurred while migrating or initializing the database.");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
        c.RoutePrefix = "";
    });
}

app.UseRouting();
app.UseCors("AllowAllOrigins");

app.UseResponseCompression();
app.UseAuthentication();
app.UseAuthorization();

#pragma warning disable ASP0014 // Suggest using top level route registrations
app.UseEndpoints( endpoints => {
    endpoints.MapHub<GameHubApi.Hubs.GameHub>("/lobbyHub");
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}"
    );
});
#pragma warning restore ASP0014 // Suggest using top level route registrations


app.Run();
