using System.Reflection;
using G9Common.Abstract;
using G9Common.Configuration;
using G9Common.Interface;
using G9SuperNetCoreServer.Abstarct;
using G9SuperNetCoreServer.AbstractServer;
using G9SuperNetCoreServer.Config;

namespace G9SuperNetCoreServer
{
    public class G9SuperNetCoreSocketServer<TAccount, TSession> : AG9SuperNetCoreServerBase<TAccount, TSession>
        where TAccount : AServerAccount<TSession>, new()
        where TSession : AServerSession, new()
    {
        public G9SuperNetCoreSocketServer(G9ServerConfig superNetCoreConfig, Assembly commandAssembly,
            IG9Logging customLogging = null) : base(superNetCoreConfig, commandAssembly, customLogging)
        {
        }
    }
}