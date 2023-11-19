using OpenAIApp.Configurations;
using OpenAIApp.Helpers.Files;
using OpenAIApp.Helpers.OpenAi;
using OpenAIApp.Managers;
using OpenAIApp.Repository.FileRepo;
using OpenAIApp.Repository.FileTagRepo;
using OpenAIApp.Repository.TagRepo;
using OpenAIApp.Services;
using OpenAIApp.Services.FileProcessing;
using OpenAIApp.Services.Jobs;
using Serilog;
using Supabase;

namespace OpenAIApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Host.UseSerilog((context, configuration) => configuration.ReadFrom.Configuration(context.Configuration));


            CreateSupabaseDI(builder);

            // Add services to the container.

            builder
                .Services
                .AddControllers()
                .AddNewtonsoftJson(options =>
                {
                    // Configure Newtonsoft.Json options here, if needed
                    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft
                        .Json
                        .ReferenceLoopHandling
                        .Ignore;
                });

            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddSingleton<IFileRepo, FileRepo>();
            builder.Services.AddSingleton<ITagRepo, TagRepo>();
            builder.Services.AddSingleton<IFileTagRepo, FileTagRepo>();

            builder.Services.AddSingleton<OpenAiConfig>();
            builder.Services.AddSingleton<IOpenAiHelper, OpenAiHelper>();

            builder.Services.AddSingleton<IService, JobService>();

            builder.Services.AddSingleton<FileProcessingService>();
            builder
                .Services
                .AddSingleton<IFileProcesssingService>(
                    x => x.GetRequiredService<FileProcessingService>()
                );
            builder
                .Services
                .AddSingleton<IService>(x => x.GetRequiredService<FileProcessingService>());

            builder.Services.AddHostedService<ServiceManager>();

            var app = builder.Build();


            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseSerilogRequestLogging();

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }

        private static void CreateSupabaseDI(WebApplicationBuilder builder)
        {
            var url = Environment.GetEnvironmentVariable("SUPABASE_URL");
            var key = Environment.GetEnvironmentVariable("SUPABASE_KEY");

            var options = new SupabaseOptions
            {
                AutoRefreshToken = true,
                AutoConnectRealtime = true,
                // SessionHandler = new SupabaseSessionHandler() <-- This must be implemented by the developer
            };

            _ = builder.Services.AddSingleton(provider => new Client(url, key, options));
        }
    }
}
