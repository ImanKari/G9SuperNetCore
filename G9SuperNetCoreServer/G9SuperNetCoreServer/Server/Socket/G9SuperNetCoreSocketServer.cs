using System.Reflection;
using G9SuperNetCoreCommon.HelperClass;
using G9SuperNetCoreCommon.Interface;
using G9SuperNetCoreServer.Abstarct;
using G9SuperNetCoreServer.AbstractServer;
using G9SuperNetCoreServer.Config;

namespace G9SuperNetCoreServer.Server.Socket
{
    public class G9SuperNetCoreSocketServer<TAccount, TSession> : AG9SuperNetCoreServerBase<TAccount, TSession>
        where TAccount : AServerAccount<TSession>, new()
        where TSession : AServerSession, new()
    {
        public G9SuperNetCoreSocketServer(G9ServerConfig superNetCoreConfig, Assembly[] commandAssemblies = null,
            IG9Logging customLogging = null, G9SslCertificate sslCertificate = null) : base(superNetCoreConfig,
            commandAssemblies, customLogging, sslCertificate)
        {
        }
    }
}