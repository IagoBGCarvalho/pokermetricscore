using PokerMetricsCore.Web.Components;
using PokerMetricsCore.Web.Data;
using PokerMetricsCore.Web.Services;
using Microsoft.EntityFrameworkCore;

// Descobre o caminho físico e real do .exe (ex: a pasta na Área de Trabalho)
var exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName ?? throw new InvalidOperationException("Unable to determine executable path.");
var exeDir = System.IO.Path.GetDirectoryName(exePath) ?? throw new InvalidOperationException("Unable to determine executable directory.");

// Força o servidor web a usar essa pasta como a raiz do projeto
var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = exeDir
});

// Pegando a connection string do appsettings
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=pokermetrics.db";

// Configuração do BD com FACTORY
builder.Services.AddDbContextFactory<PokerMetricsCoreContext>(options =>
    options.UseSqlite(connectionString));

// Aumentando o limite do SignalR para uploads de arquivos grandes
builder.Services.AddSignalR(e => {
    e.MaximumReceiveMessageSize = 20 * 1024 * 1024; // 20MB
});

// Registrando serviços
builder.Services.AddScoped<ProcessamentoArquivoService>();
builder.Services.AddScoped<RelatorioService>();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Executa as migrações pendentes e cria o banco SQLite automaticamente
using (var scope = app.Services.CreateScope())
{
    var dbFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<PokerMetricsCoreContext>>();
    using var context = await dbFactory.CreateDbContextAsync();
    
    await context.Database.MigrateAsync();
}

// Configuração do pipeline do HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

// Ensina ao servidor o caminho exato do manifesto de arquivos estáticos
var manifestPath = System.IO.Path.Combine(exeDir, "PokerMetricsCore.Web.staticwebassets.endpoints.json");
app.MapStaticAssets(manifestPath);
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
