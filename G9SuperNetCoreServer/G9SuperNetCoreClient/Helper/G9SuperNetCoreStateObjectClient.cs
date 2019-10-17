using G9Common.Abstract;

namespace G9SuperNetCoreClient.Helper
{
    // State object for reading client data asynchronously  
    internal class G9SuperNetCoreStateObjectClient : AG9SuperNetCoreStateObjectBase
    {
        public G9SuperNetCoreStateObjectClient(int oBufferSize, long oSessionIdentity) : base(oBufferSize, oSessionIdentity)
        {
        }
    }
}