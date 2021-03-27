namespace G9SuperNetCoreCommon.Interface
{
    public interface IG9CommandName
    {
        /// <summary>
        ///     Specify name of command
        ///     call server and client with this name
        /// </summary>
        string CommandName { get; }
    }
}