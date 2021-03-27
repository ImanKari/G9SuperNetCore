using System;
using System.Runtime.CompilerServices;
using G9SuperNetCoreCommon.Interface;
using G9LogManagement;
using G9LogManagement.Enums;

namespace G9SuperNetCoreClient.Logging
{
    public class G9LoggingClient : IG9Logging
    {
#if !UNITY_2018_1_OR_NEWER
        // Instance logging system
        private readonly G9Log _logging = new G9Log("Client");

        /// <inheritdoc />
        public void LogException(Exception ex, string message = null, string identity = null, string title = null,
            [CallerFilePath] string customCallerPath = null,
            [CallerMemberName] string customCallerName = null,
            [CallerLineNumber] int customLineNumber = 0)
        {
            _logging.G9LogException(ex, message, identity, title, customCallerPath, customCallerName, customLineNumber);
        }

        /// <inheritdoc />
        public void LogError(string message, string identity = null, string title = null,
            [CallerFilePath] string customCallerPath = null,
            [CallerMemberName] string customCallerName = null,
            [CallerLineNumber] int customLineNumber = 0)
        {
            _logging.G9LogError(message, identity, title, customCallerPath, customCallerName, customLineNumber);
        }

        /// <inheritdoc />
        public void LogWarning(string message, string identity = null, string title = null,
            [CallerFilePath] string customCallerPath = null,
            [CallerMemberName] string customCallerName = null,
            [CallerLineNumber] int customLineNumber = 0)
        {
            _logging.G9LogWarning(message, identity, title, customCallerPath, customCallerName, customLineNumber);
        }

        /// <inheritdoc />
        public void LogInformation(string message, string identity = null, string title = null,
            [CallerFilePath] string customCallerPath = null,
            [CallerMemberName] string customCallerName = null,
            [CallerLineNumber] int customLineNumber = 0)
        {
            _logging.G9LogInformation(message, identity, title, customCallerPath, customCallerName, customLineNumber);
        }

        /// <inheritdoc />
        public void LogEvent(string message, string identity = null, string title = null,
            [CallerFilePath] string customCallerPath = null,
            [CallerMemberName] string customCallerName = null,
            [CallerLineNumber] int customLineNumber = 0)
        {
            _logging.G9LogEvent(message, identity, title, customCallerPath, customCallerName, customLineNumber);
        }

        /// <inheritdoc />
        public bool CheckLoggingIsActive(LogsType type)
        {
            return _logging.CheckEnableConsoleLoggingOrFileLoggingByType(type);
        }

        /// <inheritdoc />
        public bool CheckConsoleLoggingIsActive(LogsType type)
        {
            return _logging.CheckEnableConsoleLoggingByType(type);
        }

        /// <inheritdoc />
        public bool CheckFileLoggingIsActive(LogsType type)
        {
            return _logging.CheckEnableFileLoggingByType(type);
        }
#else
        /// <inheritdoc />
        public void LogException(Exception ex, string message = null, string identity = null, string title = null,
            [CallerFilePath] string customCallerPath = null,
            [CallerMemberName] string customCallerName = null,
            [CallerLineNumber] int customLineNumber = 0)
        {
            G9Logging4Unity.LogException(ex, message, identity, title, customCallerPath, customCallerName, customLineNumber);
        }

        /// <inheritdoc />
        public void LogError(string message, string identity = null, string title = null,
            [CallerFilePath] string customCallerPath = null,
            [CallerMemberName] string customCallerName = null,
            [CallerLineNumber] int customLineNumber = 0)
        {
            G9Logging4Unity.LogError(message, identity, title, customCallerPath, customCallerName, customLineNumber);
        }

        /// <inheritdoc />
        public void LogWarning(string message, string identity = null, string title = null,
            [CallerFilePath] string customCallerPath = null,
            [CallerMemberName] string customCallerName = null,
            [CallerLineNumber] int customLineNumber = 0)
        {
            G9Logging4Unity.LogWarning(message, identity, title, customCallerPath, customCallerName, customLineNumber);
        }

        /// <inheritdoc />
        public void LogInformation(string message, string identity = null, string title = null,
            [CallerFilePath] string customCallerPath = null,
            [CallerMemberName] string customCallerName = null,
            [CallerLineNumber] int customLineNumber = 0)
        {
            G9Logging4Unity.LogInformation(message, identity, title, customCallerPath, customCallerName, customLineNumber);
        }

        /// <inheritdoc />
        public void LogEvent(string message, string identity = null, string title = null,
            [CallerFilePath] string customCallerPath = null,
            [CallerMemberName] string customCallerName = null,
            [CallerLineNumber] int customLineNumber = 0)
        {
            G9Logging4Unity.LogEvent(message, identity, title, customCallerPath, customCallerName, customLineNumber);
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
#endif
    }
}