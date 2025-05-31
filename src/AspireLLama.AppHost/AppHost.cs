var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache");

var ollama = builder.AddOllama("ollama")
                    //.AddModel("llama3")
                    //.WithContainerRuntimeArgs("--device", "nvidia.com/gpu=all")
                    .WithDataVolume()
                    .WithOpenWebUI()
                    .AddModel("chat", "phi3.5");


var apiService = builder.AddProject<Projects.AspireLLama_ApiService>("api")
    .WithHttpHealthCheck("/health")
    .WithEnvironment("GOOGLE_API_KEY",builder.Configuration["GoogleApiKey"])
    .WithReference(ollama)
    .WaitFor(ollama);

var web = builder.AddViteApp("web", "../ui/aspire-react")
    .WithYarnPackageInstallation()
    .WithReference(apiService).WaitFor(apiService)
    .WithEnvironment("BROWSER", "none") // Disable opening browser on npm start
    .WithExternalHttpEndpoints();

builder.Build().Run();
