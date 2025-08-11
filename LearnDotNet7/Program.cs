
using AutoMapper;
using LearnDotNet7.Container;
using LearnDotNet7.helper;
using LearnDotNet7.Modal;
using LearnDotNet7.Repos;
using LearnDotNet7.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;

namespace LearnDotNet7
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            //builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddScoped<ICustomerService,CustomerService>();
            builder.Services.AddTransient<IRefreshHandler, RefreshHandler>();

            builder.Services.AddDbContext<LearndataContext>(o =>
                                                         o.UseSqlServer(builder.Configuration.GetConnectionString("apicon")));



            var automapper = new MapperConfiguration(item => item.AddProfile(new AutoMapperHandler()));
            IMapper mapper = automapper.CreateMapper();
            builder.Services.AddSingleton(mapper);



            var _authkey = builder.Configuration.GetValue<string>("JwtSettings:securitykey");
            builder.Services.AddAuthentication(item =>
            {
                item.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                item.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(item =>
            {
                item.RequireHttpsMetadata = true;
                item.SaveToken = true;
                item.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_authkey)),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                };

            });


            builder.Services.AddRateLimiter(_ => _.AddFixedWindowLimiter(policyName: "fixedwindow", options =>
            {
                options.Window = TimeSpan.FromSeconds(100000);
                options.PermitLimit = 5;
                options.QueueLimit = 0;
                options.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
            }).RejectionStatusCode = 401);


            string logpath = builder.Configuration.GetSection("Logging:Logpath").Value;
            var _logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("microsoft", Serilog.Events.LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.File(logpath)
                .CreateLogger();
            builder.Logging.AddSerilog(_logger);

            var _jwtsetting = builder.Configuration.GetSection("JwtSettings");
            builder.Services.Configure<JwtSettings>(_jwtsetting);   

            var app = builder.Build();
            app.UseRateLimiter();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthentication();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
