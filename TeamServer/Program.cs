using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TeamServer.Data;
using TeamServer.ExceptionHandlers;
using TeamServer.Handlers.CheckinHandler;
using TeamServer.Handlers.CheckinHandler.Strategies;
using TeamServer.NotificationDispatchers;
using TeamServer.Repositories;
using TeamServer.Services;
using TeamServer.Services.Factories;
using TeamServer.Validators;
using TeamServer.Validators.Commands;
using TeamServer.Validators.Listeners.Http;
using TeamServer.Validators.Listeners.Smb;
using Trinity.Shared.Configurations;
using Trinity.Shared.DTOs.Command;
using Trinity.Shared.DTOs.Listener.Http;
using Trinity.Shared.DTOs.Listener.Smb;
using Trinity.Shared.Enums;
using Trinity.Shared.Enums.Tasks;
using Trinity.Shared.Interfaces;

namespace TeamServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Services
            builder.Services.AddScoped<IListenerService, ListenerService>();
            builder.Services.AddScoped<ICommandService, CommandService>();
            builder.Services.AddScoped<IPayloadService, PayloadService>();
            builder.Services.AddScoped<IPayloadBuilder, CMakePayloadBuilder>();
            builder.Services.AddScoped<IAgentService, AgentService>();
            builder.Services.AddScoped<ITaskService, TaskService>();
            builder.Services.AddScoped<IOperatorService, OperatorService>();
            builder.Services.AddScoped<ICampaignService, CampaignService>();
            builder.Services.AddScoped<IAdminService, AdminService>();
            builder.Services.AddScoped<IToolService, ToolService>();
            builder.Services.AddScoped<IDownloadService, DownloadService>();
            builder.Services.AddScoped<IUploadService, UploadService>();
            builder.Services.AddScoped<IKeyloggerService, KeyloggerService>();

            // Handlers 
            builder.Services.AddScoped<ICheckinResponseHandler, CheckinResponseHandler>();
            builder.Services.AddScoped<SuccessStrategy>();
            builder.Services.AddScoped<RunningStrategy>();
            builder.Services.AddScoped<ErrorStrategy>();
            builder.Services.AddScoped<UploadChunkStrategy>();
            builder.Services.AddScoped<DownloadChunkStrategy>();
            builder.Services.AddScoped<KeyloggerStrategy>();

            // Validators
            builder.Services.AddValidatorsFromAssemblyContaining<OperatorValidator>();

            /// Singletons
            builder.Services.AddSingleton<ListenerFactory>();
            builder.Services.AddSingleton<HttpModuleFactory>();
            builder.Services.AddSingleton<CommandFactory>();
            builder.Services.AddSingleton<AgentFactory>();
            builder.Services.AddSingleton<TaskResultFactory>();
            builder.Services.AddSingleton<HttpListenerManager>();
            builder.Services.AddSingleton<IHostFactory, HostFactory>();
            builder.Services.AddSingleton<ITaskRequestFactory, TaskRequestFactory>();
            builder.Services.AddSingleton<IToolFactory, ToolFactory>();

            builder.Services.AddSingleton<INotificationDispatcher, NotificationDispatcher>();

            builder.Services.AddControllers(options =>
            {
                options.Filters.Add<ValidationFilter>();
            });

            builder.Services.AddSwaggerGen(options =>
            {
                options.EnableAnnotations();
            });

            builder.Services.AddDbContextPool<Context>(opt => opt.UseNpgsql(
                builder.Configuration.GetConnectionString("Postgres"),
                o => o.MapEnum<Status>("status")
                      .MapEnum<TaskType>("taskType")
                      .MapEnum<TaskCode>("taskCode")
                      .MapEnum<ToolType>("toolType")
                      .MapEnum<Arch>("arch")
                      .MapEnum<ListenerType>("listenerType")
                      .MapEnum<RotationStrategy>("rotationStrategy")
                      .MapEnum<PayloadType>("payloadType")));

            // Repositories
            builder.Services.AddScoped<ListenerRespository>();
            builder.Services.AddScoped<ProtocolRespository>();
            builder.Services.AddScoped<TaskStatusRepository>();
            builder.Services.AddScoped<CommandRepository>();
            builder.Services.AddScoped<AgentRepository>();
            builder.Services.AddScoped<TaskRepository>();
            builder.Services.AddScoped<TaskStatusRepository>();
            builder.Services.AddScoped<TaskResultRepository>();
            builder.Services.AddScoped<PayloadRepository>();
            builder.Services.AddScoped<CampaignRepository>();
            builder.Services.AddScoped<OperatorRepository>();
            builder.Services.AddScoped<CampaignBridgeRepository>();
            builder.Services.AddScoped<AdminRepository>();
            builder.Services.AddScoped<HostRepository>();
            builder.Services.AddScoped<ToolRepository>();

            // Validators 
            builder.Services.AddScoped<IValidator<CreateHttpListenerRequest>, CreateHttpListenerRequestValidator>();
            builder.Services.AddScoped<IValidator<CreateSmbRequest>, CreateSmbRequestValidator>();
            builder.Services.AddScoped<IValidator<BofDTO>, BofDTOValidator>();
            builder.Services.AddScoped<IValidator<CancelDownloadDTO>, CancelDownloadDTOValidator>();
            builder.Services.AddScoped<IValidator<DotNetAsmDTO>, DotNetAsmDTOValidator>();
            builder.Services.AddScoped<IValidator<DownloadDTO>, DownloadDTOValidator>();
            builder.Services.AddScoped<IValidator<EscalateDTO>, EscalateDTOValidator>();
            builder.Services.AddScoped<IValidator<KillProcessDTO>, KillProcessDTOValidator>();
            builder.Services.AddScoped<IValidator<PowerShellDTO>, PowerShellDTOValidator>();
            builder.Services.AddScoped<IValidator<RunAsDTO>, RunAsDTOValidator>();
            builder.Services.AddScoped<IValidator<RunDTO>, RunDTOValidator>();
            builder.Services.AddScoped<IValidator<RunAsDTO>, RunAsDTOValidator>();
            builder.Services.AddScoped<IValidator<RunUDTO>, RunUDTOValidator>();
            builder.Services.AddScoped<IValidator<SetSleepDTO>, SetSleepDTOValidator>();
            builder.Services.AddScoped<IValidator<UpdateHostsDTO>, UpdateHostsDTOValidator>();
            builder.Services.AddScoped<IValidator<ShellDTO>, ShellDTOValidator>();
            builder.Services.AddScoped<IValidator<SpawnToDTO>, SpawnToDTOValidator>();
            builder.Services.AddScoped<IValidator<UploadDTO>, UploadDTOValidator>();

            // SignalR
            builder.Services.AddSignalR();

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
                    options.RoutePrefix = string.Empty;
                });
            }

            app.MapHub<NotificationHub>("/register");
            app.UseExceptionHandler();
            if (app.Configuration.GetValue<bool>("EnableHttpsRedirection"))
            {
                app.UseHttpsRedirection();
            }
            app.UseAuthorization();
            app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
            app.MapControllers();
            app.Run();
        }
    }
}