using System.Reflection;
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
            IG9Logging customLogging, string privateKeyForSslConnection, string clientUniqueIdentity) : base(
            clientConfig, commandAssembly, customLogging, privateKeyForSslConnection, privateKeyForSslConnection)
        {
        }

        public G9SuperNetCoreSocketClient(G9ClientConfig clientConfig, Assembly commandAssembly,
            IG9Logging customLogging) : base(clientConfig, commandAssembly, customLogging)
        {
        }

        public G9SuperNetCoreSocketClient(G9ClientConfig clientConfig, Assembly commandAssembly)
            : base(clientConfig, commandAssembly)
        {
        }

        public G9SuperNetCoreSocketClient(G9ClientConfig clientConfig, Assembly commandAssembly,
            string privateKeyForSslConnection, string clientUniqueIdentity) : base(clientConfig, commandAssembly,
            privateKeyForSslConnection, clientUniqueIdentity)
        {
        }
    }
}