using System;

namespace G9Common.Interface
{
    public interface IG9CommandWithSend : IG9CommandName
    {
        /// <summary>
        ///     Specify type of send
        /// </summary>
        Type TypeOfSend { get; }
    }
}