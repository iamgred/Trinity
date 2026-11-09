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

            var teamServerUrl = builder.Configuration["TeamServer:BaseUrl"]!;

            // Refit typed HTTP clients — all targeting TeamServer
            builder.Services.AddRefitClient<ICommandAPI>()
                .ConfigureHttpClient(c => c.BaseAddress = new Uri(teamServerUrl));
            builder.Services.AddRefitClient<IAgentAPI>()
                .ConfigureHttpClient(c => c.BaseAddress = new Uri(teamServerUrl));
            builder.Services.AddRefitClient<IListenerAPI>()
                .ConfigureHttpClient(c => c.BaseAddress = new Uri(teamServerUrl));
            builder.Services.AddRefitClient<ICampaignAPI>()
                .ConfigureHttpClient(c => c.BaseAddress = new Uri(teamServerUrl));
            builder.Services.AddRefitClient<IOperatorAPI>()
                .ConfigureHttpClient(c => c.BaseAddress = new Uri(teamServerUrl));
            builder.Services.AddRefitClient<IAdminAPI>()
                .ConfigureHttpClient(c => c.BaseAddress = new Uri(teamServerUrl));
            builder.Services.AddRefitClient<IPayloadAPI>()
                .ConfigureHttpClient(c => c.BaseAddress = new Uri(teamServerUrl));
            builder.Services.AddRefitClient<IAuthAPI>()
                .ConfigureHttpClient(c => c.BaseAddress = new Uri(teamServerUrl));
            builder.Services.AddRefitClient<IServerAPI>()
                .ConfigureHttpClient(c => c.BaseAddress = new Uri(teamServerUrl));
            builder.Services.AddRefitClient<ITaskAPI>()
                .ConfigureHttpClient(c => c.BaseAddress = new Uri(teamServerUrl));
            builder.Services.AddRefitClient<IArmouryAPI>()
                .ConfigureHttpClient(c => c.BaseAddress = new Uri(teamServerUrl));
            builder.Services.AddRefitClient<IFilesAPI>()
                .ConfigureHttpClient(c => c.BaseAddress = new Uri(teamServerUrl));

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
