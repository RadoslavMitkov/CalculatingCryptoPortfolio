using CalculatingCryptoPortfolioValue.Client;
using CalculatingCryptoPortfolioValue.Client.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddHttpClient("coinlore", client =>
{
    client.BaseAddress = new Uri("https://api.coinlore.net/api/");
});

builder.Services.AddScoped<CalculatePortfolioService>();
builder.Services.AddLogging();

await builder.Build().RunAsync();
