using System.Threading.RateLimiting;
using Index.Repository;
using Index.Service;
using Details.Repository;
using Details.Service;
using Terms.Repository;
using Terms.Service;
public partial class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        string connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        builder.Services.AddMemoryCache();
        builder.Services.AddControllersWithViews();
        builder.Services.AddScoped<IndexService>();
        builder.Services.AddScoped<DetailsService>();
        builder.Services.AddScoped<TermsService>();
        builder.Services.AddScoped<IndexRepository>();
        builder.Services.AddScoped<DetailsRepository>();
        builder.Services.AddScoped<TermsRepository>();

        builder.Services.AddRateLimiter(options =>
        {
            options.AddPolicy("ip-rate-limit", context =>
            {
                var ipAddress = context.Connection.RemoteIpAddress?.ToString();
                
                // IPアドレス取得失敗時はリクエスト拒否
                if (string.IsNullOrEmpty(ipAddress))
                {
                    return RateLimitPartition.GetNoLimiter("unknown");
                }
                
                return RateLimitPartition.GetSlidingWindowLimiter(
                    partitionKey: ipAddress,
                    factory: _ => new SlidingWindowRateLimiterOptions
                    {
                        Window = TimeSpan.FromSeconds(60),
                        PermitLimit = 100,
                        SegmentsPerWindow = 12,
                        AutoReplenishment = true,
                    }
                );
            });
        });
        var app = builder.Build();
        app.UseHsts();
        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseRateLimiter();
        app.UseAuthorization();

        app.Use(async (context, next) => 
        {
            // Clickjacking 対策
            context.Response.Headers.Append("X-Frame-Options", "DENY");
    
            // MIME-type Sniffing 対策
            context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    
            // Referrer-Policy
            context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    
            // キャッシュ制御
            context.Response.Headers.Append("Cache-Control", "no-cache, no-store, must-revalidate");
            context.Response.Headers.Append("Pragma", "no-cache");
            context.Response.Headers.Append("Expires", "0");
    
            // HSTS 明示設定
            context.Response.Headers.Append("Strict-Transport-Security", "max-age=31536000; includeSubDomains; preload");
            await next();
        });

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}"
        );

        app.Run();
    }
}