namespace G9Common.HelperClass
{
    public static class G9CommandChecker
    {
        /// <summary>
        ///     Generate standard command name
        ///     check command size and add or remove character
        /// </summary>
        /// <param name="commandName">Command name</param>
        /// <param name="commandSize">Specify command size => 16, 32, ect</param>
        /// <returns>Standard command name</returns>

        #region GenerateStandardCommandName

        public static string GenerateStandardCommandName(this string commandName, int commandSize)
        {
            return commandName.PadLeft(commandSize, '9').Substring(0, commandSize);
        }

        #endregion
    }
}