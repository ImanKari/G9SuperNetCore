using System.Reflection;
using G9SuperNetCoreCommon.Interface;
using G9SuperNetCoreClient.Abstract;
using G9SuperNetCoreClient.AbstractClient;
using G9SuperNetCoreClient.Config;

namespace G9SuperNetCoreClient.Client.Socket
{
    public class G9SuperNetCoreSocketClient<TAccount, TSession> : AG9SuperNetCoreClientBase<TAccount, TSession>
        where TAccount : AClientAccount<TSession>, new()
        where TSession : AClientSession, new()
    {
        public G9SuperNetCoreSocketClient(G9ClientConfig clientConfig, IG9Logging customLogging = null,
            string privateKeyForSslConnection = null, string clientUniqueIdentity = null,
            Assembly[] commandAssemblies = null,
            TAccount customAccount = null, TSession customSession = null) : base(clientConfig, customLogging,
            privateKeyForSslConnection, clientUniqueIdentity, commandAssemblies, customAccount, customSession)
        {
        }
    }
}