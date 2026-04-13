using MoYu.RemoteRequest;

namespace MoYu.Application;

[BaseAddress("https://localhost:44316")]
public interface IHttp : IBase
{
    [Post("/api/test-module/upload-file", ContentType = "multipart/form-data")]
    Task<HttpResponseMessage> TestSingleFileProxyAsync(HttpFile file);

    [Post("/api/test-module/upload-muliti-file", ContentType = "multipart/form-data")]
    Task<HttpResponseMessage> TestMultiFileProxyAsync(HttpFile[] files);


    [Post("/api/test-module/upload-file", ContentType = "multipart/form-data")]
    Task<HttpResponseModel<string>> TestHttpResponseModel(HttpFile file);
}

public interface IBase : IHttpDispatchProxy
{
    [MoYu.RemoteRequest.Interceptor(InterceptorTypes.Request)]
    static void OnRequest(HttpClient client, HttpRequestMessage req)
    {
    }
}