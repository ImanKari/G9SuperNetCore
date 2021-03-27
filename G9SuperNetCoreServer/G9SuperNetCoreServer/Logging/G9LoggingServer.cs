using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using G9SuperNetCoreCommon.Interface;
using G9LogManagement;
using G9LogManagement.Enums;

namespace G9SuperNetCoreServer.Logging
{
    public class G9LoggingServer : IG9Logging
    {
        // Instance logging system
        private readonly G9Log _logging = new G9Log("Server");

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
    }
}