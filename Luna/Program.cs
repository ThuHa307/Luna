using Luna.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Luna.Hubs;
using Microsoft.Extensions.DependencyInjection;
using Luna.Models;
using Luna.Services;
using Luna.Areas.Customer.Models;
using System.Configuration;
using Luna.Areas.Customer.Controllers;
using Luna.Areas.Customer.Controllers.VNPaylib.Services;
using Microsoft.ML;
using Hangfire;
using Hangfire.SqlServer;
using Hangfire.Dashboard;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;
var mailSettings = configuration.GetSection("MailSettings");
builder.Services.AddOptions(); // Kích hoạt Options
builder.Services.Configure<MailSetting>(mailSettings);
builder.Services.AddTransient<IEmailSender, SendMailService>();
builder.Services.AddSingleton<GlobalService>();
//thao huong
builder.Services.Configure<MailSettings>(mailSettings);
builder.Services.AddControllersWithViews();
builder.Services.AddTransient<IMailService, MailService>();
// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddIdentity<IdentityUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders();
builder.Services.Configure<IdentityOptions>(options => {

    // Thiết lập về Password
    options.Password.RequireDigit = false; // Không bắt phải có số
    options.Password.RequireLowercase = false; // Không bắt phải có chữ thường
    options.Password.RequireNonAlphanumeric = false; // Không bắt ký tự đặc biệt
    options.Password.RequireUppercase = false; // Không bắt buộc chữ in
    options.Password.RequiredLength = 3; // Số ký tự tối thiểu của password
    options.Password.RequiredUniqueChars = 1; // Số ký tự riêng biệt

    // Cấu hình về User.
    options.User.AllowedUserNameCharacters = // các ký tự đặt tên user
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = true;  // Email là duy nhất

    // Cấu hình đăng nhập.
    options.SignIn.RequireConfirmedEmail = true;            // Cấu hình xác thực địa chỉ email (email phải tồn tại)
    options.SignIn.RequireConfirmedPhoneNumber = false;     // Xác thực số điện thoại
    options.SignIn.RequireConfirmedAccount = true;       // Xác thực tài khoản

});

// test ml.net
// Đăng ký dịch vụ SentimentAnalysisService
builder.Services.AddSingleton<SentimentAnalysisService>();







builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = $"/Identity/Account/Login";
    options.LogoutPath = $"/Identity/Account/Logout";
    options.AccessDeniedPath = $"/Identity/Account/AccessDenied";
});

builder.Services.AddAuthentication()
                .AddGoogle(options =>
                {
                    var ggConfig = configuration.GetSection("Authentication:Google");
                    options.ClientId = ggConfig["ClientId"];
                    options.ClientSecret = ggConfig["ClientSecret"];
                    options.CallbackPath = "/dang-nhap-tu-google";
                });
builder.Services.AddRazorPages();
builder.Services.AddSignalR();
//thao huong
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options => {
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.IsEssential = true;
});
//Tan VNPay
builder.Services.AddSingleton<IVnPayService, VnPayService>();

builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection"), new SqlServerStorageOptions
    {
        CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
        SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
        QueuePollInterval = TimeSpan.Zero,
        UseRecommendedIsolationLevel = true,
        DisableGlobalLocks = true
    }));

builder.Services.AddHangfireServer();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
//thao huong
app.UseSession();
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();

app.MapControllerRoute(
    name: "default",
pattern: "{area=Customer}/{controller=Home}/{action=Index}/{id?}");
// Cấu hình dashboard cho Hangfire
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = (IEnumerable<Hangfire.Dashboard.IDashboardAuthorizationFilter>)(new[]
    {
        new HangfireDashboardAuthorizationFilter()
    })
});


// Đăng ký job
HangfireJobs.ScheduleJobs();

app.MapHub<ChatHub>("/hubs/chat");
app.Run();

public class HangfireDashboardAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        return httpContext.User.Identity.IsAuthenticated && httpContext.User.IsInRole("Admin");
    }
}