using Client.Components;
using Client.Interpreter;
using Client.Interpreter.Strategies;
using Client.Services;
using MudBlazor.Services;
using Refit;
using Trinity.Shared.Interfaces;

namespace Client
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            // Registers MudBlazor Components
            builder.Services.AddMudServices();

            // Runtime-configured TeamServer connection + typed API clients (DTO-based).
            builder.Services.AddScoped<TeamServerConnection>();
            builder.Services.AddHttpClient<AgentApiClient>();
            builder.Services.AddHttpClient<TaskApiClient>();
            builder.Services.AddHttpClient<AuthApiClient>();
            builder.Services.AddHttpClient<ListenerApiClient>();
            builder.Services.AddHttpClient<CommandApiClient>();
            builder.Services.AddHttpClient<ServerApiClient>();

            // Named TeamServer HttpClient (fixed BaseUrl from appsettings).
            builder.Services.AddHttpClient("TeamServer", client =>
            {
                client.BaseAddress = new Uri(builder.Configuration["TeamServer:BaseUrl"]!);
            });

            // App Services
            builder.Services.AddScoped<AuthService>();
            builder.Services.AddScoped<AgentService>();
            builder.Services.AddScoped<ListenerService>();
            builder.Services.AddScoped<CampaignService>();
            builder.Services.AddScoped<OperatorService>();
            builder.Services.AddScoped<AdminService>();
            builder.Services.AddScoped<PayloadService>();
            builder.Services.AddScoped<TaskApiService>();
            builder.Services.AddScoped<CommandInterpreter>();
            builder.Services.AddScoped<PowerShellService>();

            builder.Services.AddScoped<ICommandInterpreter, CommandInterpreter>();
            builder.Services.AddScoped<PowerShellStrategy>();
            builder.Services.AddScoped<ShellStrategy>();
            builder.Services.AddScoped<RunAsStrategy>();
            builder.Services.AddScoped<RunStrategy>();
            builder.Services.AddScoped<RunUStrategy>();
            builder.Services.AddScoped<KillProcessStrategy>();
            builder.Services.AddScoped<EscalateStrategy>();
            builder.Services.AddScoped<DotNetAsmStrategy>();
            builder.Services.AddRefitClient<ICommandAPI>()
                .ConfigureHttpClient(client =>
                {
                    client.BaseAddress = new Uri("https://localhost:7251");
                });


            var app = builder.Build(); // <- Build happens here, after all services are registered

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
            app.UseHttpsRedirection();
            app.UseAntiforgery();
            app.MapStaticAssets();
            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            app.Run();
        }
    }
}
