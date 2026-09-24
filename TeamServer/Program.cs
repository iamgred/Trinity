//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
using Microsoft.EntityFrameworkCore;
using TeamServer.Data;
using TeamServer.ExceptionHandlers;
using TeamServer.Services;
using TeamServer.Services.Factories;
using Trinity.Shared.Configurations;

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
            builder.Services.AddSingleton<HttpListenerFactory>();
            builder.Services.AddSingleton<HttpCommModuleFactory>();
            builder.Services.AddScoped<DatabaseService>();
            builder.Services.AddControllers();
            builder.Services.AddSwaggerGen(options =>
            {
                options.EnableAnnotations();
            });
            builder.Services.AddDbContextPool<Context>(opt =>
                opt.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));

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

            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;

                try
                {
                    var context = services.GetRequiredService<Context>();
                    context.Database.Migrate();
                }
                catch (Exception ex)
                {

                    throw;
                }
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