using System.Reflection;
using G9Common.Abstract;
using G9Common.Configuration;
using G9Common.Interface;
using G9SuperNetCoreClient.Abstract;
using G9SuperNetCoreClient.AbstractClient;
using G9SuperNetCoreClient.Config;

namespace G9SuperNetCoreClient.Client.Socket
{
    public class G9SuperNetCoreSocketClient<TAccount, TSession> : AG9SuperNetCoreClientBase<TAccount, TSession>
        where TAccount : AClientAccount<TSession>, new()
        where TSession : AClientSession, new()
    {
        public G9SuperNetCoreSocketClient(G9ClientConfig clientConfig, Assembly commandAssembly,
            IG9Logging customLogging = null) : base(clientConfig, commandAssembly, customLogging)
        {
        }
    }
}
