using FramRpcService;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using StatHub.Web.Model;

internal class FarmGrpcService : FarmRpc.FarmRpcBase
{
    private readonly FarmerModel farmerModel;
    public FarmGrpcService(FarmerModel farmerModel)
    {
        this.farmerModel = farmerModel;
    }
    /// <summary>
    ///  下发启动的参数
    /// </summary>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public override async Task<StarArgReply> StarArg(StarArgRequest request, ServerCallContext context)
    {
        StarArgReply starArgReply = new StarArgReply();
        if (!request.Mainet)
        {
            starArgReply.BootstrapNodes = "";
            starArgReply.ReservedPeers.Add("/ip4/192.168.0.101/udp/30533/quic-v1/p2p/12D3KooWAjrV465cPfy1N9ByXUKs7rM7G9E98nAf7VKotD9ksCC3");
        }
        foreach (var farmPath in farmerModel.farmPaths)
        {
            starArgReply.Path.Add($"path={farmPath.Path},size={farmPath.Size}");
        }
        starArgReply.AllowPrivateIps = true;
        starArgReply.NodeRpcUrl = "ws://192.168.0.237:9944";
        starArgReply.RewardAddress = "st9ZjdhzNUpUssD1DNwZFeg5wcuNN5gcNCebkz5zmX5UNxern";
        /// 测试阶段必须关闭
        starArgReply.FarmDuringInitialPlotting = false;
        return starArgReply;
    }

    public override Task<StatusReply> Status(StatusRequest request, ServerCallContext context)
    {
        StatusReply statusReply = new StatusReply();
        return Task.FromResult(statusReply);
    }

    public override Task<Empty> Logger(LoggerRequest request, ServerCallContext context)
    {
        return Task.FromResult(new Empty());
    }
    public override Task<PathReply> Path(PathRequest request, ServerCallContext context)
    {
        PathReply pathReply = new PathReply();
        return Task.FromResult(pathReply);
    }
    public override Task<Empty> SectorIndex(SectorRequest request, ServerCallContext context)
    {
        return Task.FromResult(new Empty());
    }
    public override Task<Empty> PieceCache(PieceRequest request, ServerCallContext context)
    {
        return Task.FromResult(new Empty());
    }
}