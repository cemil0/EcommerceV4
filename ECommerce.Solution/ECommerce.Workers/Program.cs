var builder = Host.CreateApplicationBuilder(args);

// Worker services will be added here later
// builder.Services.AddHostedService<OutboxProcessorService>();

var host = builder.Build();
host.Run();
