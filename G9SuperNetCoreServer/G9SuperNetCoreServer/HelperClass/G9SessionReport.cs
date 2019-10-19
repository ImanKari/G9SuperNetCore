using System;

namespace G9SuperSocketNetCoreServer.Class.Struct
{
    public struct G9SessionReport
    {
        /// <summary>
        ///     Specify session id
        /// </summary>
        public uint SessionId;

        /// <summary>
        ///     Specify total send
        /// </summary>
        public uint TotalSend;

        /// <summary>
        ///     Specify Total receive
        /// </summary>
        public uint TotalReceive;

        /// <summary>
        ///     Specify start connection date time
        /// </summary>
        public DateTime StartTime;
    }
}