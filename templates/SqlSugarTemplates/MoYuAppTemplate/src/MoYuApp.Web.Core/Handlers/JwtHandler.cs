using MoYu.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace MoYuApp.Web.Core;

public class JwtHandler : AppAuthorizeHandler
{
    public override Task<bool> PipelineAsync(AuthorizationHandlerContext context, DefaultHttpContext httpContext)
    {
        // ����д������Ȩ�ж��߼�����Ȩͨ������ true�����򷵻� false

        return Task.FromResult(true);
    }
}
