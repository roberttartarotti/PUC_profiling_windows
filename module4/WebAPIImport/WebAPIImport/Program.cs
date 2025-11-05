using System.Diagnostics;
using WebAPIImport.DAO;
using Microsoft.EntityFrameworkCore;
using WebAPIImport.DAO.Interface;
using WebAPIImport.Adapters.Interface;
using WebAPIImport.Adapters;
using WebAPIImport.Logic;
using WebAPIImport.Logic.Interface;
using WebAPIImport;

var builder = WebApplication.CreateBuilder(args);

if (!EventLog.SourceExists("WebAPIImportApp"))
{
    EventLog.CreateEventSource("WebAPIImportApp", "Application");
}

builder.Logging.AddEventLog(eventLogSettings =>
{
    eventLogSettings.SourceName = "WebAPIImportApp";
    eventLogSettings.LogName = "Application";
});

WebAPIImportApp.Log.ProcessingStarted();

var configuration = builder.Configuration;

var connection = configuration.GetConnectionString("SqlConnectionString");

builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlServer(connection)
);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddTransient<IRecordDataAdapter, RecordDataAdapter>();
builder.Services.AddTransient<IRecordDataDataBase, RecordDataDataBase>();
builder.Services.AddTransient<IRecordDataLogic, RecordDataLogic>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
