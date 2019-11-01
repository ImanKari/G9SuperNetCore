using System;
using System.ComponentModel;
using G9Common.Interface;
using G9LogManagement;
using G9LogManagement.Enums;

namespace G9SuperNetCoreClient.Logging
{
    public class G9LoggingClient : IG9Logging
    {

        /// <inheritdoc />
        public void LogException(Exception ex, string message = null, string identity = null, string title = null)
        {
            G9Logging4Unity.LogException(ex, message, identity, title);
        }

        /// <inheritdoc />
        public void LogError(string message, string identity = null, string title = null)
        {
            G9Logging4Unity.LogError(message, identity, title);
        }

        /// <inheritdoc />
        public void LogWarning(string message, string identity = null, string title = null)
        {
            G9Logging4Unity.LogWarning(message, identity, title);
        }

        /// <inheritdoc />
        public void LogInformation(string message, string identity = null, string title = null)
        {
            G9Logging4Unity.LogInformation(message, identity, title);
        }

        /// <inheritdoc />
        public void LogEvent(string message, string identity = null, string title = null)
        {
            G9Logging4Unity.LogEvent(message, identity, title);
        }

        /// <inheritdoc />
        public bool CheckLoggingIsActive(LogsType type)
        {
            return G9Logging4Unity.CheckLoggingIsActive(type);
        }

        /// <inheritdoc />
        public bool CheckConsoleLoggingIsActive(LogsType type)
        {
            return G9Logging4Unity.CheckConsoleLoggingIsActive(type);
        }

        /// <inheritdoc />
        public bool CheckFileLoggingIsActive(LogsType type)
        {
            return G9Logging4Unity.CheckFileLoggingIsActive(type);
        }
    }
}