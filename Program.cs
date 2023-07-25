using Microsoft.EntityFrameworkCore;
using ITHelp.Models;
using ITHelp.Services;
using GSS.Authentication.CAS.AspNetCore;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages().AddRazorRuntimeCompilation();
builder.Services.AddScoped<IFileIOService, FileIOService>();
builder.Services.AddScoped<INotificationService, NotificationService>();

builder.Services.AddDbContext<ITHelpContext>(o =>
            {
                o.UseSqlServer(builder.Configuration.GetConnectionString("ServiceTrackerContext"));
                o.UseLoggerFactory(ITHelpContext.GetLoggerFactory());
            });  

builder.Services.AddScoped<IdentityService, IdentityService>();
        builder.Services.AddAuthentication("Cookies")
            .AddCookie(options =>
            {
                options.LoginPath = "/Account/Login";
                options.AccessDeniedPath = "/Account/AccessDenied";
                options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
                options.SlidingExpiration = true;
            })
            .AddCAS(o =>
            {
                o.SignInScheme = "Cookies";
                o.CasServerUrlBase = builder.Configuration["CasBaseUrl"];
                o.Events.OnCreatingTicket = async context =>
                {
                    if (context.Identity == null)
                    {
                        return;
                    }
                    var ident = (ClaimsIdentity) context.Principal.Identity;
                    var assertion = context.Assertion;
                    var kerb = assertion.PrincipalName;
                    if (string.IsNullOrWhiteSpace(kerb)) return;
                    ident.AddClaim(new Claim(ClaimTypes.NameIdentifier, assertion.PrincipalName));
                    var db = context.HttpContext.RequestServices.GetRequiredService<ITHelpContext>();
                    var user = await db.Employees.Where(e => e.KerberosId == kerb && e.Current).FirstOrDefaultAsync();
                    if(user != null)
                    {                       
                        ident.AddClaim(new Claim(ClaimTypes.Surname, user.LastName));
                        ident.AddClaim(new Claim(ClaimTypes.GivenName, user.FirstName));
                        ident.AddClaim(new Claim(ClaimTypes.Sid,user.Id));
                        ident.AddClaim(new Claim(ClaimTypes.Role, "Employee"));
                        if(!string.IsNullOrWhiteSpace(user.Role) && user.Role != "none")
                        {
                            ident.AddClaim(new Claim(ClaimTypes.Role, user.Role));                            
                        }                        
                    }
                    await Task.FromResult(0);
                };
            }); 
            
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAuthentication();
app.UseCookiePolicy();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
