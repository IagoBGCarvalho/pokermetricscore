using PokerMetricsCore.Web.Components;
using PokerMetricsCore.Web.Data;
using PokerMetricsCore.Web.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

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

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
