using Grpc.Net.Client;
using StatHub.Control;
using StatHub.Control.Agent;
using StatHub.Control.Model;
using Subspace.Agent.Core.Model;
using YamlDotNet.Serialization;

/// 上报当前进程状态 (启动/停止) 每秒 一次
/// 上报当前CPU  使用状态 每秒一次
/// 上报当前内存 使用状态 每秒一次
/// 上报当前带宽 使用状态 每秒一次
/// 上报当前P盘进度  更新时上报
GrpcChannel? channel = GrpcChannel.ForAddress("http://master/");

var yamlDeserializer = new DeserializerBuilder().Build();
var yaml = File.ReadAllText("subspace.yaml");
var config = yamlDeserializer.Deserialize<FarmSetting>(yaml);

Agent nodeAgent;
if (config.node_agent.HasValue && config.node_agent == true)
{
    IDeserializer deserializer = new DeserializerBuilder().Build();
    string str = File.ReadAllText("config.yaml");
    ConfigModel configModel = yamlDeserializer.Deserialize<ConfigModel>(str);

    nodeAgent = new Agent();
    config.node_rpc_url = "ws://127.0.0.1:9943";
    await nodeAgent.StartAsync(configModel);
}

Console.WriteLine($"farm: {config.farm}");
Console.WriteLine($"reward_address: {config.reward_address}");
foreach (var path in config.paths)
{
    Console.WriteLine($"path: {path}");
}

string currentDirectory = Directory.GetCurrentDirectory();
string logDirectory = Path.Combine(currentDirectory, "log");
Directory.CreateDirectory(logDirectory);

while (true)
{
    string? md5Hash = await ProcessWatich.Init();
    ProcessWatich processWatich = null;
    var interceptor = new AuthInterceptor(config.farm, ProcessWatich.uuid);
    // var interceptedChannel = channel.Intercept(interceptor);
    // FarmRpc.FarmRpcClient farmRpcClient = new FarmRpc.FarmRpcClient(interceptedChannel);
    processWatich = new ProcessWatich(logDirectory);
    string arguments = await processWatich.ConstrucAsync(config);
    processWatich.CheckUpdate(md5Hash);
    await processWatich.StartAsync(arguments);
    await Task.Delay(10000);
}