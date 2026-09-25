using Wissian.Host;
using Wissian.Host.Modules;

await HostApplication.Build(args, [CounterpartiesEndpoints.Registration, DocumentsEndpoints.Registration]).RunAsync();
