using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using G9LogManagement.Enums;
using G9LogManagement.Structures;
using UnityEngine;

// ReSharper disable once CheckNamespace
public class G9Logging4Unity : MonoBehaviour
{
    #region Start Fields And Properties

    // Use for enable and disable Logs
    public bool IsEnableExceptionLogging = true;
    public bool IsEnableErrorLogging = true;
    public bool IsEnableWarningLogging = true;
    public bool IsEnableInformationLogging = false;
    public bool IsEnableEventLogging = false;

    // Use for static methods
    private static bool StaticIsEnableExceptionLogging = true;
    private static bool StaticIsEnableErrorLogging = true;
    private static bool StaticIsEnableWarningLogging = true;
    private static bool StaticIsEnableInformationLogging;
    private static bool StaticIsEnableEventLogging;

    // Logging colors
    private static readonly Color32[] loggingColors =
    {
        // Green
        new Color32(53, 170, 53, 1),
        // Blue
        new Color32(63, 127, 191, 1),
        // Orange
        new Color32(216, 133, 74, 1),
        // Red
        new Color32(224, 96, 107, 1),
        // Dark Red
        new Color32(239, 25, 25, 1)
    };

    private static readonly LogType[] _unityLogTypes =
    {
        LogType.Log, // 0 => for event
        LogType.Log, // 1 => for information
        LogType.Warning, // 2 => for warning
        LogType.Error, // 3 => for error
        LogType.Exception // 4 => for exception
    };

    /// <summary>
    ///     Queue for save log and show
    /// </summary>
    private static readonly Queue<G9LogItem> _logQueueForShow = new Queue<G9LogItem>();

    #endregion End Fields And Properties

    #region Start Methods

    #region Start Normal Methods

    /// <summary>
    ///     Static constructor
    /// </summary>

    #region G9Logging4Unity

    static G9Logging4Unity()
    {
        LogEvent("Start Logging", "G9Log", "G9LogManagement", "None", "None", -1);
    }

    #endregion

    /// <summary>
    ///     Start is called before the first frame update
    /// </summary>

    #region Start

    private void Start()
    {
        StaticIsEnableExceptionLogging = IsEnableExceptionLogging;
        StaticIsEnableErrorLogging = IsEnableErrorLogging;
        StaticIsEnableWarningLogging = IsEnableWarningLogging;
        StaticIsEnableInformationLogging = IsEnableInformationLogging;
        StaticIsEnableEventLogging = IsEnableEventLogging;
    }

    #endregion

    /// <summary>
    ///     Update is called once per frame
    /// </summary>

    #region Update

    private void Update()
    {
        StaticIsEnableExceptionLogging = IsEnableExceptionLogging;
        StaticIsEnableErrorLogging = IsEnableErrorLogging;
        StaticIsEnableWarningLogging = IsEnableWarningLogging;
        StaticIsEnableInformationLogging = IsEnableInformationLogging;
        StaticIsEnableEventLogging = IsEnableEventLogging;

        if (_logQueueForShow.Any())
            ConsoleLogging(_logQueueForShow.Dequeue());
    }

    #endregion

    #endregion End Normal Methods

    #region Start Static Logging Methods

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

    public static void LogException(Exception ex, string message = null, string identity = null, string title = null,
        string customCallerPath = null,
        string customCallerName = null,
        int customLineNumber = 0)
    {
        // Check enable => if disable return;
        if (!StaticIsEnableExceptionLogging) return;

        // Show Log
        _logQueueForShow.Enqueue(new G9LogItem(LogsType.EXCEPTION, identity, title, ExceptionErrorGenerate(ex, message),
            customCallerPath, customCallerName,
            customLineNumber.ToString(), DateTime.Now));
    }

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

    public static void LogError(string message, string identity = null, string title = null,
        string customCallerPath = null,
        string customCallerName = null,
        int customLineNumber = 0)
    {
        // Check enable => if disable return;
        if (!StaticIsEnableErrorLogging) return;

        // Show Log
        _logQueueForShow.Enqueue(new G9LogItem(LogsType.ERROR, identity, title, message, customCallerPath,
            customCallerName,
            customLineNumber.ToString(), DateTime.Now));
    }

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

    public static void LogWarning(string message, string identity = null, string title = null,
        string customCallerPath = null,
        string customCallerName = null,
        int customLineNumber = 0)
    {
        // Check enable => if disable return;
        if (!StaticIsEnableWarningLogging) return;

        // Show Log
        _logQueueForShow.Enqueue(new G9LogItem(LogsType.WARN, identity, title, message, customCallerPath,
            customCallerName,
            customLineNumber.ToString(), DateTime.Now));
    }

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

    public static void LogInformation(string message, string identity = null, string title = null,
        string customCallerPath = null,
        string customCallerName = null,
        int customLineNumber = 0)
    {
        // Check enable => if disable return;
        if (!StaticIsEnableInformationLogging) return;

        // Show Log
        _logQueueForShow.Enqueue(new G9LogItem(LogsType.INFO, identity, title, message, customCallerPath,
            customCallerName,
            customLineNumber.ToString(), DateTime.Now));
    }

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

    public static void LogEvent(string message, string identity = null, string title = null,
        string customCallerPath = null,
        string customCallerName = null,
        int customLineNumber = 0)
    {
        // Check enable => if disable return;
        if (!StaticIsEnableEventLogging) return;

        // Show Log
        _logQueueForShow.Enqueue(new G9LogItem(LogsType.EVENT, identity, title, message, customCallerPath,
            customCallerName,
            customLineNumber.ToString(), DateTime.Now));
    }

    #endregion

    /// <summary>
    ///     Check active logging by log type
    ///     Check console logging or file logging
    /// </summary>
    /// <param name="type">Specify type of log</param>
    /// <returns>If active console logging or file logging for specified type return true</returns>

    #region CheckLoggingIsActive

    public static bool CheckLoggingIsActive(LogsType type)
    {
        switch (type)
        {
            case LogsType.EVENT:
                return StaticIsEnableEventLogging;
            case LogsType.INFO:
                return StaticIsEnableInformationLogging;
            case LogsType.WARN:
                return StaticIsEnableWarningLogging;
            case LogsType.ERROR:
                return StaticIsEnableErrorLogging;
            case LogsType.EXCEPTION:
                return StaticIsEnableExceptionLogging;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }

    #endregion

    /// <summary>
    ///     Check active console logging by log type
    /// </summary>
    /// <param name="type">Specify type of log</param>
    /// <returns>If active console logging for specified type return true</returns>

    #region CheckConsoleLoggingIsActive

    public static bool CheckConsoleLoggingIsActive(LogsType type)
    {
        switch (type)
        {
            case LogsType.EVENT:
                return StaticIsEnableEventLogging;
            case LogsType.INFO:
                return StaticIsEnableInformationLogging;
            case LogsType.WARN:
                return StaticIsEnableWarningLogging;
            case LogsType.ERROR:
                return StaticIsEnableErrorLogging;
            case LogsType.EXCEPTION:
                return StaticIsEnableExceptionLogging;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }

    #endregion

    /// <summary>
    ///     Check active file logging by log type
    /// </summary>
    /// <param name="type">Specify type of log</param>
    /// <returns>If active file logging for specified type return true</returns>

    #region CheckFileLoggingIsActive

    public static bool CheckFileLoggingIsActive(LogsType type)
    {
        switch (type)
        {
            case LogsType.EVENT:
                return StaticIsEnableEventLogging;
            case LogsType.INFO:
                return StaticIsEnableInformationLogging;
            case LogsType.WARN:
                return StaticIsEnableWarningLogging;
            case LogsType.ERROR:
                return StaticIsEnableErrorLogging;
            case LogsType.EXCEPTION:
                return StaticIsEnableExceptionLogging;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }

    #endregion

    /// <summary>
    ///     Ready log data for show in the console
    /// </summary>
    /// <param name="logItem">Specify log item for show</param>

    #region ConsoleLogging

    private void ConsoleLogging(G9LogItem logItem)
    {
        // Set Log type number
        var logTypeNumber = (byte)logItem.LogType;

        try
        {
            // Ignore if null
            if (string.IsNullOrEmpty(logItem.Body)) return;

            // Show console log
            Debug.LogFormat(_unityLogTypes[logTypeNumber],
                LogOption.NoStacktrace,
                null,
                $"<color=#{0:X2}{loggingColors[logTypeNumber].g:X2}{loggingColors[logTypeNumber].b:X2}> | ### Log Type: {logItem.LogType} ### | \nDate & Time: {logItem.LogDateTime:yyyy/MM/ss HH:mm:ss.fff}\nIdentity: {logItem.Identity}\tTitle: {logItem.Title}\nBody: {new Regex("[^a-zA-Z0-9 -]").Replace(logItem.Body, string.Empty)}\nPath: {logItem.FileName}\nMethod: {logItem.MethodBase}\tLine: {logItem.LineNumber}\n</color>"
            );
        }
        catch (Exception ex)
        {
            // Ignore
            Debug.LogWarning(ex.StackTrace);
        }
    }

    #endregion

    /// <summary>
    ///     Generate message from exception
    /// </summary>
    /// <param name="ex">Exception</param>
    /// <param name="additionalMessage">Additional message</param>
    /// <returns>Generated message</returns>

    #region ExceptionErrorGenerate

    private static string ExceptionErrorGenerate(Exception ex, string additionalMessage)
    {
        var exceptionMessage = new StringBuilder();
        exceptionMessage.Append(Environment.NewLine);

        // Add additional
        if (!string.IsNullOrEmpty(additionalMessage))
            exceptionMessage.Append(
                $"| Additional Message | {additionalMessage}{Environment.NewLine}");
        // Add exception message
        exceptionMessage.Append(
            $"| Exception Message | {ex.Message}{Environment.NewLine}");
        // Add stack trace if exists
        if (!string.IsNullOrEmpty(ex.StackTrace))
            exceptionMessage.Append(
                $"| StackTrace | {ex.StackTrace}{Environment.NewLine}");
        // Add inner exception if exists
        if (ex.InnerException != null)
            exceptionMessage.Append(
                $"| Inner Exception | {ExceptionErrorGenerate(ex.InnerException, null)}{Environment.NewLine}");
        return exceptionMessage.ToString();
    }

    #endregion

    #endregion Static Logging Methods

    #endregion End Methods
}