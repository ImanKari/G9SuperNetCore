using System;
using G9LogManagement.Enums;

namespace G9Common.Interface
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

        #region LogException

        void LogException(Exception ex, string message = null, string identity = null, string title = null);

        #endregion

        /// <summary>
        ///     Handle error log
        /// </summary>
        /// <param name="message">Error message</param>
        /// <param name="identity">Insert identity if need found easy in logs</param>
        /// <param name="title">Custom title for log</param>

        #region LogError

        void LogError(string message, string identity = null, string title = null);

        #endregion

        /// <summary>
        ///     Handle warning log
        /// </summary>
        /// <param name="message">Warning message</param>
        /// <param name="identity">Insert identity if need found easy in logs</param>
        /// <param name="title">Custom title for log</param>

        #region LogWarning

        void LogWarning(string message, string identity = null, string title = null);

        #endregion

        /// <summary>
        ///     Handle information log
        /// </summary>
        /// <param name="message">Information message</param>
        /// <param name="identity">Insert identity if need found easy in logs</param>
        /// <param name="title">Custom title for log</param>

        #region LogInformation

        void LogInformation(string message, string identity = null, string title = null);

        #endregion

        /// <summary>
        ///     Handle event log
        /// </summary>
        /// <param name="message">Event message</param>
        /// <param name="identity">Insert identity if need found easy in logs</param>
        /// <param name="title">Custom title for log</param>

        #region LogEvent

        void LogEvent(string message, string identity = null, string title = null);

        #endregion

        /// <summary>
        ///     Check active logging by log type
        /// </summary>
        /// <param name="type">Specify type of log</param>
        /// <returns>If active logging for specified type return true</returns>

        #region LogIsActive

        bool LogIsActive(LogsType type);

        #endregion
    }
}