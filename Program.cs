using FerramentariaTest.DAL;
using FerramentariaTest.ModifiedServices;
using FerramentariaTest.Services.Interfaces;
using FerramentariaTest.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Serilog;
using FerramentariaTest.DAL.Demo;
using FerramentariaTest.Services.Demo;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

if (builder.Environment.IsEnvironment("Demo"))
{

    builder.Services.AddDbContext<DemoDB>(options => options.UseInMemoryDatabase("PortfolioDemoDb"));

    builder.Services.AddScoped<IUserService, UserServiceDemo>();
    builder.Services.AddScoped<IFerramentariaService, FerramentariaServiceDemo>();
    builder.Services.AddScoped<IReservationService, ReservationServiceDemo>();
    builder.Services.AddScoped<IUserContextService, UserContextService>();
    builder.Services.AddScoped<IEmployeeService, EmployeeServiceDemo>();
    builder.Services.AddScoped<IAuditService, AuditServiceDemo>();
    builder.Services.AddScoped<IRetiradaService, RetiradaServiceDemo>();
    builder.Services.AddScoped<IConsultReservationRetirada, ConsultReservationRetiradaServiceDemo>();
    builder.Services.AddScoped<IHistoryAlocadoService, HistoryAlocadoServiceDemo>();

    builder.Services.AddHostedService<DemoDataSeeder>();

}
else
{
    var connectionString = builder.Configuration.GetConnectionString("FerramentariaConnection") ?? throw new InvalidOperationException("Connection string 'FerramentariaConnection' not found.");
    var connectionStringBS = builder.Configuration.GetConnectionString("BSConnection") ?? throw new InvalidOperationException("Connection string 'BSConnection' not found.");
    var connectionStringSeekEmployee = builder.Configuration.GetConnectionString("SeekEmployeeConnection") ?? throw new InvalidOperationException("Connection string 'SeekEmployeeConnection' not found.");
    var connectionStringRM = builder.Configuration.GetConnectionString("RMConnection") ?? throw new InvalidOperationException("Connection string 'RMConnection' not found.");

    builder.Services.AddDbContext<ContextoBanco>(options => options.UseSqlServer(connectionString));
    builder.Services.AddDbContext<ContextoBancoBS>(options => options.UseSqlServer(connectionStringBS));
    builder.Services.AddDbContext<ContextoBancoRM>(options => options.UseSqlServer(connectionStringRM));
    builder.Services.AddDbContext<ContextoBancoSeek>(options => options.UseSqlServer(connectionStringSeekEmployee));

    builder.Services.AddScoped<IUserContextService, UserContextService>();
    builder.Services.AddScoped<IReservationService, ReservationService>();
    builder.Services.AddScoped<IFerramentariaService, FerramentariaService>();
    builder.Services.AddScoped<IEmployeeService, EmployeeService>();
    builder.Services.AddScoped<IRetiradaService, RetiradaService>();
    builder.Services.AddScoped<IConsultReservationRetirada, ConsultReservationRetiradaService>();
    builder.Services.AddScoped<IHistoryAlocadoService, HistoryAlocadoService>();
    builder.Services.AddScoped<IAuditService, AuditService>();
    builder.Services.AddScoped<IAdminService, AdminService>();
    builder.Services.AddScoped<ICatalogService, CatalogService>();

}





// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddMvc();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                                .AddCookie(options =>
                                {
                                    options.LoginPath = "/Home/Login";
                                    options.AccessDeniedPath = "/Home/AccessDenied";
                                    options.ExpireTimeSpan = TimeSpan.FromMinutes(15);
                                    options.SlidingExpiration = true;
                                    options.Cookie.IsEssential = true;
                                    options.Cookie.SameSite = SameSiteMode.Lax;
                                    options.Cookie.HttpOnly = true;
                                    options.Cookie.SecurePolicy = CookieSecurePolicy.None;
                                    options.Events = new CookieAuthenticationEvents
                                    {
                                        OnRedirectToLogin = context =>
                                        {
                                            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                                            var user = context.HttpContext.User.Identity?.Name ?? "Anonymous";

                                            logger.LogWarning("Access denied - Redirecting to login: User={User}, Path={Path}, ReturnUrl={ReturnUrl}",
                                                               user,
                                                               context.Request.Path,
                                                               context.RedirectUri);

                                            //context.Response.Redirect($"/Home/Ferramentaria?message=Please log in to continue&returnUrl={context.RedirectUri}");
                                            context.Response.Redirect($"/Home/Login?message=Please login to continue");
                                            return Task.CompletedTask;
                                        },
                                        OnRedirectToAccessDenied = context =>
                                        {
                                            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                                            var user = context.HttpContext.User.Identity?.Name ?? "Anonymous";

                                            logger.LogWarning("Access denied - Redirecting to access denied page: User={User}, Path={Path}, Status={Status}",
                                                        user,
                                                        context.Request.Path,
                                                        context.HttpContext.Response.StatusCode);

                                            context.Response.Redirect(context.RedirectUri);
                                            return Task.CompletedTask;
                                        }
                                    };
                                });

builder.Host.UseSerilog((context, services, configuration) =>
{
    var correlationService = services.GetRequiredService<ICorrelationIdService>();

    configuration.ReadFrom.Configuration(context.Configuration)
                        .Enrich.With(new RequestContextEnricher(services))
                        .Enrich.With(new CorrelationIdLogEnricher(correlationService));
});

builder.Services.AddScoped<IAuditLogger>(provider =>
{
    var correlationService = provider.GetRequiredService<ICorrelationIdService>();
    var configuration = provider.GetRequiredService<IConfiguration>();

    var auditLogger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration, "AuditLog")
        .Enrich.With(new RequestContextEnricher(provider))
        .Enrich.With(new CorrelationIdLogEnricher(correlationService)).CreateLogger();

    return new AuditLoggerService(auditLogger); // Pass the auditLogger instance directly
});

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(15);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.None;
    options.Cookie.Name = "Ferramentaria.Session";
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddControllersWithViews();

builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddSingleton<ICorrelationIdService, CorrelationIdService>();



var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

//app.UseHttpsRedirection();

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseCookiePolicy();
app.UseSession();

app.UseRouting();
app.UseAuthentication();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
	pattern: "{controller=Home}/{action=Login}");



app.Run();
