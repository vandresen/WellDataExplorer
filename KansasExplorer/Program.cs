using ExplorerLibrary.Services;
using KansasExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.CommandLine;
using Microsoft.Extensions.Logging;
using Serilog;

var connectionStringOption = new Option<string>(
    "--connection-string",
    description: "The connection string for the data storage.")
{
    IsRequired = true
};
connectionStringOption.AddAlias("-c");

var cacheFolderOption = new Option<string>(
    "--cache-folder",
    description: "The folder path for web cache files.")
{
    IsRequired = false
};

cacheFolderOption.AddAlias("-f");

var rootCommand = new RootCommand
{
    connectionStringOption,
    cacheFolderOption
};

rootCommand.Description = "Application with data storage connection and cache folder options";

rootCommand.SetHandler(async (string connectionString, string cacheFolder) =>
{
    Console.WriteLine($"Database Connection String: {connectionString}");
    Console.WriteLine($"Cache Folder: {cacheFolder}");

    Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.File(@"C:\temp\KansasExplorer.log")
    .CreateLogger();

    using IHost host = Host.CreateDefaultBuilder(args)
        .ConfigureServices(services =>
        {
            services.AddSingleton<IDataExplorer, KansasDataExplorer>();
            services.AddSingleton<IDataAccess, AzureBlobStorageAccess>();
            services.AddSingleton<App>();
        })
        .UseSerilog()
        .Build();

    var logger = host.Services.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("Starting application.");

    var app = host.Services.GetService<App>();
    await app!.Run(cacheFolder, connectionString);

    await host.StopAsync();
    await host.WaitForShutdownAsync();

}, connectionStringOption, cacheFolderOption);

await rootCommand.InvokeAsync(args);