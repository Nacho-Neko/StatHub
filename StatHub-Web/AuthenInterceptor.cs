using Grpc.Core;
using Grpc.Core.Interceptors;
using StatHub.Web.Model;

namespace StatHub.Web
{
    public class AuthenInterceptor : Interceptor
    {
        private FarmerModel farmerModel;
        public AuthenInterceptor(FarmerModel farmerModel)
        {
            this.farmerModel = farmerModel;
        }
        public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
            TRequest request, ServerCallContext context,
            UnaryServerMethod<TRequest, TResponse> continuation)
        {
            // 在请求处理之前执行的逻辑，比如日志记录
            // Console.WriteLine($"Receiving request of type {typeof(TRequest).Name}");

            string farmName = context.RequestHeaders.FirstOrDefault(h => h.Key == "famer")?.Value;
            string uuid = context.RequestHeaders.FirstOrDefault(h => h.Key == "uuid")?.Value;

            await farmerModel.GetFarm(farmName);
            if (uuid.Contains(farmerModel.Guid))
            {
                Console.WriteLine($"farm start name: {farmName}  uuid : {uuid}");
            }
            else
            {
                Console.WriteLine($"farm run name: {farmName}  uuid : {uuid}");
            }

            // 调用实际的服务方法
            var response = await continuation(request, context);

            // 在请求处理之后执行的逻辑
            // Console.WriteLine($"Sending response of type {typeof(TResponse).Name}");

            return response;
        }
    }
}
