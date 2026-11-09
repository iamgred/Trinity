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

            // Runtime-configured TeamServer connection (used by old API client wrappers).
            builder.Services.AddScoped<TeamServerConnection>();

            // Scoped per Blazor circuit — attaches Bearer token to every outgoing request.
            builder.Services.AddScoped<AuthTokenHandler>();

            var teamServerUrl = builder.Configuration["TeamServer:BaseUrl"]!;
            var refitSettings = new RefitSettings();

            // Refit typed HTTP clients — all scoped, all share the circuit's AuthTokenHandler.
            // Using RestService.For<T> (not IHttpClientFactory) so the scoped handler is
            // resolved from the correct Blazor circuit scope, not the root DI container.
            TApi MakeRefit<TApi>(IServiceProvider sp)
            {
                var handler = sp.GetRequiredService<AuthTokenHandler>();
                var client  = new HttpClient(handler, disposeHandler: false)
                              { BaseAddress = new Uri(teamServerUrl) };
                return RestService.For<TApi>(client, refitSettings);
            }

            builder.Services.AddScoped(MakeRefit<ICommandAPI>);
            builder.Services.AddScoped(MakeRefit<IAgentAPI>);
            builder.Services.AddScoped(MakeRefit<IListenerAPI>);
            builder.Services.AddScoped(MakeRefit<ICampaignAPI>);
            builder.Services.AddScoped(MakeRefit<IOperatorAPI>);
            builder.Services.AddScoped(MakeRefit<IAdminAPI>);
            builder.Services.AddScoped(MakeRefit<IPayloadAPI>);
            builder.Services.AddScoped(MakeRefit<IAuthAPI>);
            builder.Services.AddScoped(MakeRefit<IServerAPI>);
            builder.Services.AddScoped(MakeRefit<ITaskAPI>);
            builder.Services.AddScoped(MakeRefit<IArmouryAPI>);
            builder.Services.AddScoped(MakeRefit<IFilesAPI>);

            // Thin wrappers that pages inject directly (internally use Refit interfaces)
            builder.Services.AddScoped<AgentApiClient>();
            builder.Services.AddScoped<AuthApiClient>();
            builder.Services.AddScoped<ListenerApiClient>();
            builder.Services.AddScoped<CommandApiClient>();
            builder.Services.AddScoped<ServerApiClient>();
            builder.Services.AddScoped<FilesApiClient>();

            // App Services
            builder.Services.AddScoped<EventLogService>();
            builder.Services.AddScoped<AuthService>();
            builder.Services.AddScoped<AgentService>();
            builder.Services.AddScoped<ListenerService>();
            builder.Services.AddScoped<CampaignService>();
            builder.Services.AddScoped<OperatorService>();
            builder.Services.AddScoped<AdminService>();
            builder.Services.AddScoped<PayloadService>();
            builder.Services.AddScoped<TaskApiService>();
            builder.Services.AddScoped<ArmouryService>();
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
            builder.Services.AddScoped<DownloadStrategy>();
            builder.Services.AddScoped<CancelDownloadStrategy>();
            builder.Services.AddScoped<UploadStrategy>();
            builder.Services.AddScoped<SpawnToStrategy>();
            builder.Services.AddScoped<UpdateHostsStrategy>();
            builder.Services.AddScoped<KillStrategy>();
            builder.Services.AddScoped<GetUIDStrategy>();
            builder.Services.AddScoped<SetSleepStrategy>();
            builder.Services.AddScoped<HelpStrategy>();

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
