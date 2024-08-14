using ColoradoExplorer;
using ExplorerLibrary.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.CommandLine;

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

    using IHost host = Host.CreateDefaultBuilder(args)
        .ConfigureServices(services =>
        {
            services.AddSingleton<IDataExplorer, ColoradoDataExplorer>();
            services.AddSingleton<IDataAccess, AzureBlobStorageAccess>();
            services.AddSingleton<App>();
        })
        .Build();
    var app = host.Services.GetService<App>();
    await app!.Run(cacheFolder, connectionString);

    await host.StopAsync();
    await host.WaitForShutdownAsync();

}, connectionStringOption, cacheFolderOption);

await rootCommand.InvokeAsync(args);
