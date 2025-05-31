var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.Configure<Peer.Config>(builder.Configuration.GetSection("PeerConfig"));

string dirPath = Path.Combine("wwwroot", "peer");
if (Directory.Exists(dirPath))
{
    Directory.Delete(dirPath, true);
}
Directory.CreateDirectory(dirPath);

dirPath = Path.Combine("data", "text");
if (Directory.Exists(dirPath))
{
    Directory.Delete(dirPath, true);
}
Directory.CreateDirectory(dirPath);

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAuthorization();
app.MapControllers();
app.Run();