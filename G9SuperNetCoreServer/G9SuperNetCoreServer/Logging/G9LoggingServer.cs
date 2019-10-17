using System;
using System.ComponentModel;
using G9Common.Interface;
using G9LogManagement;
using G9LogManagement.Enums;

namespace G9SuperNetCoreServer.Logging
{
    public class G9LoggingServer : IG9Logging
    {
        // Instance logging system
        private readonly G9Log _logging = new G9Log("Server");

        /// <inheritdoc />
        public void LogException(Exception ex, string message = null, string identity = null, string title = null)
        {
            _logging.G9LogException(ex, message, identity, title);
        }

        /// <inheritdoc />
        public void LogError(string message, string identity = null, string title = null)
        {
            _logging.G9LogError(message, identity, title);
        }

        /// <inheritdoc />
        public void LogWarning(string message, string identity = null, string title = null)
        {
            _logging.G9LogWarning(message, identity, title);
        }

        /// <inheritdoc />
        public void LogInformation(string message, string identity = null, string title = null)
        {
            _logging.G9LogInformation(message, identity, title);
        }

        /// <inheritdoc />
        public void LogEvent(string message, string identity = null, string title = null)
        {
            _logging.G9LogEvent(message, identity, title);
        }

        /// <inheritdoc />
        public bool LogIsActive(LogsType type)
        {
            return type switch
            {
                LogsType.EVENT => _logging.IsEnableEventLog,
                LogsType.INFO => _logging.IsEnableInformationLog,
                LogsType.WARN => _logging.IsEnableWarningLog,
                LogsType.ERROR => _logging.IsEnableErrorLog,
                LogsType.EXCEPTION => _logging.IsEnableExceptionLog,
                _ => throw new InvalidEnumArgumentException(nameof(type), (int) type, typeof(LogsType))
            };
        }
    }
}