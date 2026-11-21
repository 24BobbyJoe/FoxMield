using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using FoxMield.Components;
using FoxMield.Components.Services;
using FoxMield.App_Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Antiforgery;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

BsonSerializer.RegisterSerializer(
    new GuidSerializer(GuidRepresentation.Standard)
);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddScoped<CardService>();
builder.Services.AddScoped<DeckService>();
builder.Services.AddSingleton<MongoDbContext>();
builder.Services.AddScoped<AccountService>();
builder.Services.AddScoped<CardAnalysisService>();

builder.Services.AddAuthentication(
    CookieAuthenticationDefaults.AuthenticationScheme
)
.AddCookie(options =>
{
    options.LoginPath = "/login";
    options.LogoutPath = "/logout";
    options.Cookie.Name = "FoxMield.Auth";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.SlidingExpiration = true;
});

builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();

builder.Services.AddHttpClient<ScryfallService>(client =>
{
    client.BaseAddress = new Uri("https://api.scryfall.com/");
    client.DefaultRequestHeaders.Add("User-Agent", "FoxMield/1.0");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute(
    "/not-found",
    createScopeForStatusCodePages: true
);

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

// Registration endpoint
app.MapPost("/account/register", async (
    HttpContext context,
    AccountService accountService,
    IAntiforgery antiforgery) =>
{
    await antiforgery.ValidateRequestAsync(context);

    var form = await context.Request.ReadFormAsync();

    string username = form["Username"].ToString();
    string email = form["Email"].ToString();
    string password = form["Password"].ToString();

    try
    {
        await accountService.Register(username, email, password);

        return Results.Redirect("/login");
    }
    catch (Exception ex)
    {
        return Results.BadRequest(ex.Message);
    }
});

// Login endpoint
app.MapPost("/account/login", async (
    HttpContext context,
    AccountService accountService,
    IAntiforgery antiforgery) =>
{
    await antiforgery.ValidateRequestAsync(context);

    var form = await context.Request.ReadFormAsync();

    string email = form["Email"].ToString();
    string password = form["Password"].ToString();

    var user = await accountService.Login(email, password);

    if (user == null)
    {
        return Results.BadRequest("Invalid email or password.");
    }

    var claims = new List<Claim>
    {
        new(ClaimTypes.NameIdentifier, user.Id),
        new(ClaimTypes.Name, user.Username),
        new(ClaimTypes.Email, user.Email)
    };

    var identity = new ClaimsIdentity(
        claims,
        CookieAuthenticationDefaults.AuthenticationScheme
    );

    var principal = new ClaimsPrincipal(identity);

    await context.SignInAsync(
        CookieAuthenticationDefaults.AuthenticationScheme,
        principal
    );

    return Results.Redirect("/");
});

// Logout endpoint
app.MapPost("/account/logout", async (
    HttpContext context,
    IAntiforgery antiforgery) =>
{
    await antiforgery.ValidateRequestAsync(context);

    await context.SignOutAsync(
        CookieAuthenticationDefaults.AuthenticationScheme
    );

    return Results.Redirect("/");
});

app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
