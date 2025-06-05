var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.APIEvents>("apievents");

builder.Build().Run();
