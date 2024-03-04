using Grpc.Core;
using Grpc.Core.Interceptors;

namespace StatHub.Control
{
    public class AuthInterceptor : Interceptor
    {
        private readonly string _token;
        private readonly string _uuid;
        public AuthInterceptor(string token,string uuid)
        {
            this._token = token;
            _uuid = uuid; 
        }
        public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(
                TRequest request, ClientInterceptorContext<TRequest, TResponse> context,
                AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
        {
            var headers = context.Options.Headers;
            if (headers == null)
            {
                headers = new Metadata();
                context = new ClientInterceptorContext<TRequest, TResponse>(
                    context.Method, context.Host, context.Options.WithHeaders(headers));
            }
            headers.Add("famer", _token);
            headers.Add("uuid", _uuid);
            return continuation(request, context);
        }
    }
}
