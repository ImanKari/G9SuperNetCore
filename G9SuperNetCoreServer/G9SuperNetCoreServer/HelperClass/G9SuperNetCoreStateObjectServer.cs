using G9SuperNetCoreCommon.Abstract;

namespace G9SuperNetCoreServer.HelperClass
{
    // State object for reading client data asynchronously  
    internal class G9SuperNetCoreStateObjectServer : AG9SuperNetCoreStateObjectBase
    {
        public G9SuperNetCoreStateObjectServer(ushort oBufferSize, uint oSessionIdentity) : base(oBufferSize,
            oSessionIdentity)
        {
        }
    }
}