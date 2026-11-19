using PCStore.Data;
using PCStore.Repositories.Builder;
using PCStore.Services.Builder;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddSingleton<SqlConnectionFactory>();
builder.Services.AddSingleton<ISqlConnectionFactory>(sp => sp.GetRequiredService<SqlConnectionFactory>());
builder.Services.AddScoped<IBuildPcRepository, BuildPcRepository>();
builder.Services.AddSingleton<ICompatibilityService, CompatibilityService>();
builder.Services.AddScoped<IBuildPcService, BuildPcService>();
builder.Services.AddSingleton<IBuildPcExportService, BuildPcExportService>();

var app = builder.Build();
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
// TODO: Thêm UseAuthentication() sau khi triển khai đăng nhập.
app.UseAuthorization();

// Phải đặt route Area trước route mặc định.
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.Run();
