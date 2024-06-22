var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOrchardCms();
var app = builder.Build();

app.UseOrchardCore();
app.Run();