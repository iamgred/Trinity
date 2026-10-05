using MudBlazor.Services;
using Client.Components;
using Client.Services;

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
            builder.Services.AddScoped<TeamServerConnection>();
            builder.Services.AddHttpClient<AgentApiClient>();
            builder.Services.AddHttpClient<TaskApiClient>();
            builder.Services.AddHttpClient<AuthApiClient>();
            builder.Services.AddHttpClient<ListenerApiClient>();
            builder.Services.AddHttpClient<CommandApiClient>();
            builder.Services.AddHttpClient<ServerApiClient>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
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
        }
    }
}
