using Azure.AI.OpenAI;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.IdentityModel.Tokens;
using MijnKeuken.Infrastructure;
using MijnKeuken.Infrastructure.Persistence;
using MijnKeuken.Web.Auth;
using MijnKeuken.Web.Components;
using MijnKeuken.Web.Services;
using MudBlazor.Services;
using OpenAI;
using System.ClientModel;
using System.Net;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddControllers();
builder.Services.AddMudServices();
builder.Services.AddInfrastructure();

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(
        typeof(MijnKeuken.Application.Users.Commands.LoginUser.LoginUserCommand).Assembly));

var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET")
    ?? "MijnKeuken-Dev-Secret-Key-Min-32-Characters!!";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = "MijnKeuken",
            ValidateAudience = true,
            ValidAudience = "MijnKeuken",
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
        };
    });

builder.Services.AddScoped<JwtAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
    sp.GetRequiredService<JwtAuthenticationStateProvider>());
builder.Services.AddAuthorizationCore();

builder.Services.AddHttpClient();

var openAiEndpoint = builder.Configuration.GetValue<string>("OPENAI_ENDPOINT") ?? "";
var openAiApiKey = builder.Configuration.GetValue<string>("OPENAI_KEY") ?? "";
var openAiModel = builder.Configuration.GetValue<string>("OPENAI_MODEL") ?? "";

builder.Services.AddSingleton<IChatClient>(_ =>
    new AzureOpenAIClient(new Uri(openAiEndpoint), new ApiKeyCredential(openAiApiKey))
        .GetChatClient(openAiModel)
        .AsIChatClient());

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ITagService, TagService>();
builder.Services.AddScoped<IStorageLocationService, StorageLocationService>();
builder.Services.AddScoped<IIngredientService, IngredientService>();
builder.Services.AddScoped<IRecipeService, RecipeService>();
builder.Services.AddScoped<ScrapedRecipeState>();
builder.Services.AddScoped<IMenuService, MenuService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapControllers();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
