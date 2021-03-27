using G9SuperNetCoreCommon.Abstract;

namespace G9SuperNetCoreClient.Helper
{
    // State object for reading client data asynchronously  
    internal class G9SuperNetCoreStateObjectClient : AG9SuperNetCoreStateObjectBase
    {
        public G9SuperNetCoreStateObjectClient(ushort oBufferSize, uint oSessionIdentity) : base(oBufferSize, oSessionIdentity)
        {
        }

        public void ChangeBufferSize(ushort newBufferSize)
        {
            BufferSize = newBufferSize;
            Buffer = new byte[BufferSize];
        }
    }
}