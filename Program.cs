using Employee_jwt_Webapi.Interface;
using Employee_jwt_Webapi.Models;
using Employee_jwt_Webapi.Repository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using System.Text;
using static System.Net.WebRequestMethods;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<DatabaseContext>
    (options => options.UseSqlServer(builder.Configuration.GetConnectionString("dbConnection")));
builder.Services.AddTransient<IEmployees, EmployeeRepository>();
builder.Services.AddControllers();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
            {
                context.Response.Headers.Add("Token-Expired", "true");
            }
            return Task.CompletedTask;
        }
    };
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var _policyName = "CorsPolicy";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: _policyName,
    builder =>
    {
        builder.WithOrigins("http://localhost:4200","https://localhost:7051/api/Employee")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                  .AllowCredentials();
    });
    options.AddPolicy("AllowHeaders", builder =>
    {
        builder.WithOrigins("http://localhost:4200","https://localhost:7051/api/Employee")
           .WithHeaders(HeaderNames.ContentType, HeaderNames.Server, 
           HeaderNames.AccessControlAllowHeaders, HeaderNames.AccessControlExposeHeaders,
           "x-custom-header", "x-path", "x-record-in-use", HeaderNames.ContentDisposition);
    });
    //enable single domain
    //enable multiple domain
    //any domain

});
var app = builder.Build();


// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();

//}
app.UseSwagger();
app.UseSwaggerUI();
app.UseCors(_policyName);
//the below is Cors policy name
//app.UseCors(_policyName);

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
