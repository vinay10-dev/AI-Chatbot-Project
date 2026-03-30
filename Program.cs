var builder = WebApplication.CreateBuilder(args);

// 1. Railway Port Setup (Sirf ek baar builder use karein)
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// 2. Services add karein
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 3. CORS Policy (Mobile aur n8n ke liye zaroori hai)
builder.Services.AddCors(options => {
    options.AddPolicy("AllowAll", policy => {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build(); // YE SIRF EK BAAR HONA CHAHIYE

// 4. Middleware Setup
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// CORS apply karein
app.UseCors("AllowAll");

// Frontend files dikhane ke liye
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();

app.Run();
