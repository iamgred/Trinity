//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
using Microsoft.EntityFrameworkCore;
using TeamServer.Data;
using TeamServer.ExceptionHandlers;
using TeamServer.Repositories;
using TeamServer.Services;
using TeamServer.Services.Factories;
using Trinity.Shared.Configurations;
using Trinity.Shared.Interfaces;

namespace TeamServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Services 
            builder.Services.AddScoped<ListenerService>();
            builder.Services.AddScoped<CommandService>();
            builder.Services.AddScoped<PayloadService>();
            builder.Services.AddScoped<AgentService>();
            builder.Services.AddScoped<TaskService>();
            builder.Services.AddScoped<OperatorService>();
            builder.Services.AddScoped<CampaignService>();
            builder.Services.AddScoped<AdminService>();
            builder.Services.AddSingleton<ListenerFactory>();
            builder.Services.AddSingleton<HttpModuleFactory>();
            builder.Services.AddSingleton<CommandFactory>();
            builder.Services.AddSingleton<HttpListenerManager>();
            builder.Services.AddScoped<DatabaseService>();
            builder.Services.AddControllers();
            builder.Services.AddSwaggerGen(options =>
            {
                options.EnableAnnotations();
            });
            builder.Services.AddDbContextPool<Context>(opt =>
                opt.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));

            // Repositories
            builder.Services.AddScoped<ListenerRepository>();
            builder.Services.AddScoped<ProtocolRepository>();
            builder.Services.AddScoped<TaskStatusRepository>();
            builder.Services.AddScoped<CommandRepository>();
            builder.Services.AddScoped<AgentRepository>();
            builder.Services.AddScoped<TaskRepository>();
            builder.Services.AddScoped<AdminRepository>();
            builder.Services.AddScoped<CampaignBridgeRepository>();
            builder.Services.AddScoped<OperatorRepository>();

            // Exception handler
            builder.Services.AddSingleton<GlobalExceptionHandler>();
            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
            builder.Services.AddProblemDetails();

            // Server configuration 
            ServerNetworkSettings serverNetworkSettings = new ServerNetworkSettings()
                .ResolveIpV4Address()
                .ResolveIpV6Address();

            builder.Services.AddSingleton(serverNetworkSettings);
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
                    options.RoutePrefix = String.Empty;
                });
            }

            app.UseExceptionHandler();
            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
    }
}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//