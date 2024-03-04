using Microsoft.AspNetCore.Server.Kestrel.Core;
using SqlSugar;
using StatHub.Web;
using StatHub.Web.Model;

var builder = WebApplication.CreateBuilder(args);

/*
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    // ���ö˿ں�IP
    serverOptions.ListenLocalhost(5000, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http2;
    });
    serverOptions.ListenAnyIP(5001, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
    });
    // ��������������С
    serverOptions.Limits.MaxRequestBodySize = 10 * 1024;
});
*/


// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddAntDesign();
builder.Services.AddSingleton<WeatherForecastService>();
builder.Services.AddScoped<FarmGrpcService>();
builder.Services.AddScoped<FarmerModel>();
builder.Services.AddScoped<FarmPathModel>();
builder.Services.AddSingleton<ISqlSugarClient>(s =>
{
    SqlSugarScope sqlSugar = new SqlSugarScope(new ConnectionConfig()
    {
        ConnectionString = "server=localhost;Database=famer;Uid=famer;Pwd=",
        DbType = DbType.MySql,
        InitKeyType = InitKeyType.Attribute,//�����Զ�ȡ��������������Ϣ
        IsAutoCloseConnection = true,
    });
    return sqlSugar;
});

builder.Services.AddGrpc(options =>
{
    options.Interceptors.Add<AuthenInterceptor>();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();

app.UseRouting();

app.UseEndpoints(endpoints =>
{
    // �������е�ӳ��
    endpoints.MapBlazorHub();
    endpoints.MapFallbackToPage("/_Host");
    // ӳ��gRPC����
    endpoints.MapGrpcService<FarmGrpcService>();
});

app.Run();
