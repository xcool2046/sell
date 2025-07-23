using Microsoft.EntityFrameworkCore;
using Sellsys.Infrastructure.Data;
using Sellsys.WebApi.Middleware;
using Sellsys.Application.Interfaces;
using Sellsys.Application.Services;
var builder = WebApplication.CreateBuilder(args);

// 1. Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Health Checks
builder.Services.AddHealthChecks();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// 2. Configure DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<SellsysDbContext>(options =>
    options.UseSqlite(connectionString));
// 3. Register application services
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IAfterSalesService, AfterSalesService>();
builder.Services.AddScoped<IFinanceService, FinanceService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<IDepartmentGroupService, DepartmentGroupService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Register seed services
builder.Services.AddScoped<RoleSeedService>();


var app = builder.Build();

// 2.5. Ensure database is created and migrated
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<SellsysDbContext>();
    context.Database.EnsureCreated();

    // Initialize seed data
    var roleSeedService = scope.ServiceProvider.GetRequiredService<RoleSeedService>();
    await roleSeedService.SeedRolesAsync();
}

// 3. Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

app.UseCors("AllowAll");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Map Health Check endpoints
app.MapHealthChecks("/health");

app.Run();
