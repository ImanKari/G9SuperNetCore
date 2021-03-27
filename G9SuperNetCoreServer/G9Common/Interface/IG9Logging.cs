using System;
using System.Runtime.CompilerServices;
using G9LogManagement.Enums;

namespace G9SuperNetCoreCommon.Interface
{
    public interface IG9Logging
    {
        /// <summary>
        ///     Handle exception log
        /// </summary>
        /// <param name="ex">Exception</param>
        /// <param name="message">Additional message</param>
        /// <param name="identity">Insert identity if need found easy in logs</param>
        /// <param name="title">Custom title for log</param>
        /// <param name="customCallerPath">Custom caller path</param>
        /// <param name="customCallerName">Custom caller name</param>
        /// <param name="customLineNumber">Custom line number</param>

        #region LogException

        void LogException(Exception ex, string message = null, string identity = null, string title = null,
            [CallerFilePath] string customCallerPath = null,
            [CallerMemberName] string customCallerName = null,
            [CallerLineNumber] int customLineNumber = 0);

        #endregion

        /// <summary>
        ///     Handle error log
        /// </summary>
        /// <param name="message">Error message</param>
        /// <param name="identity">Insert identity if need found easy in logs</param>
        /// <param name="title">Custom title for log</param>
        /// <param name="customCallerPath">Custom caller path</param>
        /// <param name="customCallerName">Custom caller name</param>
        /// <param name="customLineNumber">Custom line number</param>

        #region LogError

        void LogError(string message, string identity = null, string title = null,
            [CallerFilePath] string customCallerPath = null,
            [CallerMemberName] string customCallerName = null,
            [CallerLineNumber] int customLineNumber = 0);

        #endregion

        /// <summary>
        ///     Handle warning log
        /// </summary>
        /// <param name="message">Warning message</param>
        /// <param name="identity">Insert identity if need found easy in logs</param>
        /// <param name="title">Custom title for log</param>
        /// <param name="customCallerPath">Custom caller path</param>
        /// <param name="customCallerName">Custom caller name</param>
        /// <param name="customLineNumber">Custom line number</param>

        #region LogWarning

        void LogWarning(string message, string identity = null, string title = null,
            [CallerFilePath] string customCallerPath = null,
            [CallerMemberName] string customCallerName = null,
            [CallerLineNumber] int customLineNumber = 0);

        #endregion

        /// <summary>
        ///     Handle information log
        /// </summary>
        /// <param name="message">Information message</param>
        /// <param name="identity">Insert identity if need found easy in logs</param>
        /// <param name="title">Custom title for log</param>
        /// <param name="customCallerPath">Custom caller path</param>
        /// <param name="customCallerName">Custom caller name</param>
        /// <param name="customLineNumber">Custom line number</param>

        #region LogInformation

        void LogInformation(string message, string identity = null, string title = null,
            [CallerFilePath] string customCallerPath = null,
            [CallerMemberName] string customCallerName = null,
            [CallerLineNumber] int customLineNumber = 0);

        #endregion

        /// <summary>
        ///     Handle event log
        /// </summary>
        /// <param name="message">Event message</param>
        /// <param name="identity">Insert identity if need found easy in logs</param>
        /// <param name="title">Custom title for log</param>
        /// <param name="customCallerPath">Custom caller path</param>
        /// <param name="customCallerName">Custom caller name</param>
        /// <param name="customLineNumber">Custom line number</param>

        #region LogEvent

        void LogEvent(string message, string identity = null, string title = null,
            [CallerFilePath] string customCallerPath = null,
            [CallerMemberName] string customCallerName = null,
            [CallerLineNumber] int customLineNumber = 0);

        #endregion

        /// <summary>
        ///     Check active logging by log type
        ///     Check console logging or file logging
        /// </summary>
        /// <param name="type">Specify type of log</param>
        /// <returns>If active console logging or file logging for specified type return true</returns>

        #region CheckLoggingIsActive

        bool CheckLoggingIsActive(LogsType type);

        #endregion

        /// <summary>
        ///     Check active console logging by log type
        /// </summary>
        /// <param name="type">Specify type of log</param>
        /// <returns>If active console logging for specified type return true</returns>

        #region CheckConsoleLoggingIsActive

        bool CheckConsoleLoggingIsActive(LogsType type);

        #endregion

        /// <summary>
        ///     Check active file logging by log type
        /// </summary>
        /// <param name="type">Specify type of log</param>
        /// <returns>If active file logging for specified type return true</returns>

        #region CheckFileLoggingIsActive

        bool CheckFileLoggingIsActive(LogsType type);

        #endregion
    }
}