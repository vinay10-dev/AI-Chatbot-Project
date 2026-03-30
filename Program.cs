var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// Railway se Port uthao, agar nahi hai toh 8080 use karo
var builder = WebApplication.CreateBuilder(args);

// Railway Port Setup
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// Services add karein
builder.Services.AddControllers();

var app = builder.Build();
// Swagger (optional but useful)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Static files (Frontend UI)
app.UseDefaultFiles();
app.UseStaticFiles();

// Routing
app.MapControllers();

app.Run();
