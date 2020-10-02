using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using G9LogManagement.Enums;
using G9LogManagement.Structures;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(-899)]
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

    [Header("InputField UI for debug log")]
    // Text ui for log debug
    public InputField TextForDebug;

    // Use for static methods
    private static bool _staticIsEnableExceptionLogging = true;
    private static bool _staticIsEnableErrorLogging = true;
    private static bool _staticIsEnableWarningLogging = true;
    private static bool _staticIsEnableInformationLogging = true;
    private static bool _staticIsEnableEventLogging = true;

    // Logging colors
    private static readonly Color32[] LoggingColors =
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

    private static readonly LogType[] UnityLogTypes =
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
    private static readonly Queue<G9LogItem> LogQueueForShow = new Queue<G9LogItem>();

    /// <summary>
    ///     <para>The number of logs is too high, not all of them can be displayed.</para>
    ///     <para>Specified number if log for ignore</para>
    /// </summary>
    public byte NumberOfLogIgnore = 99;

    /// <summary>
    ///     <para>When core print logs, this field save count of logs receive in print time.</para>
    /// </summary>
    private static int _numberOfIgnoreInPrintTime = -1;

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

    // ReSharper disable once UnusedMember.Local
    private void Start()
    {
#if UNITY_EDITOR
        // اگر در محیط بازی سازی، بازی فعال نبود باید از اجرا اسکریپت جلوگیری کند
        if (!EditorApplication.isPlaying) return;
#endif

        _staticIsEnableExceptionLogging = IsEnableExceptionLogging;
        _staticIsEnableErrorLogging = IsEnableErrorLogging;
        _staticIsEnableWarningLogging = IsEnableWarningLogging;
        _staticIsEnableInformationLogging = IsEnableInformationLogging;
        _staticIsEnableEventLogging = IsEnableEventLogging;
    }

    #endregion

    /// <summary>
    ///     Update is called once per frame
    /// </summary>

    #region Update

    // ReSharper disable once UnusedMember.Local
    private void Update()
    {
        _staticIsEnableExceptionLogging = IsEnableExceptionLogging;
        _staticIsEnableErrorLogging = IsEnableErrorLogging;
        _staticIsEnableWarningLogging = IsEnableWarningLogging;
        _staticIsEnableInformationLogging = IsEnableInformationLogging;
        _staticIsEnableEventLogging = IsEnableEventLogging;

        if (LogQueueForShow.Any() && LogQueueForShow.Count > NumberOfLogIgnore)
        {
            // Enable ignore log and save count
            _numberOfIgnoreInPrintTime = 0;

            // Save current log count
            var currentLogCount = LogQueueForShow.Count;

            // Remove logs
            ConsoleLogging(LogQueueForShow.Dequeue());

            // Clear other logs
            LogQueueForShow.Clear();

            // Save log item for count of ignore
            var logForCountOfIgnore = new G9LogItem(LogsType.WARN, "IGNORE-LOG", "IGNORE-LOG",
                "The number of logs is too high, not all of them can be displayed.\n" +
                $"Ignore {currentLogCount + _numberOfIgnoreInPrintTime} +  Logs! ",
                nameof(G9Logging4Unity),
                $"{nameof(G9Logging4Unity)}.{nameof(Update)}", "0", DateTime.Now);

            // Reset ignore log
            _numberOfIgnoreInPrintTime = -1;

            // Print log ignore
            ConsoleLogging(logForCountOfIgnore);
        }
        else
        {
            if (!LogQueueForShow.Any()) return;
            var countOfLogs = LogQueueForShow.Count;
            // Print logs
            while (countOfLogs-- > 0)
                ConsoleLogging(LogQueueForShow.Dequeue());
        }
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
        if (!_staticIsEnableExceptionLogging) return;

        // Check if ignore not equal -1 => plus ignore item in print time.
        if (_numberOfIgnoreInPrintTime != -1)
        {
            _numberOfIgnoreInPrintTime++;
            return;
        }

        // Show Log
        LogQueueForShow.Enqueue(new G9LogItem(LogsType.EXCEPTION, identity, title, ExceptionErrorGenerate(ex, message),
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
        if (!_staticIsEnableErrorLogging) return;

        // Check if ignore not equal -1 => plus ignore item in print time.
        if (_numberOfIgnoreInPrintTime != -1)
        {
            _numberOfIgnoreInPrintTime++;
            return;
        }

        // Show Log
        LogQueueForShow.Enqueue(new G9LogItem(LogsType.ERROR, identity, title, message, customCallerPath,
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
        if (!_staticIsEnableWarningLogging) return;

        // Check if ignore not equal -1 => plus ignore item in print time.
        if (_numberOfIgnoreInPrintTime != -1)
        {
            _numberOfIgnoreInPrintTime++;
            return;
        }

        // Show Log
        LogQueueForShow.Enqueue(new G9LogItem(LogsType.WARN, identity, title, message, customCallerPath,
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
        if (!_staticIsEnableInformationLogging) return;

        // Check if ignore not equal -1 => plus ignore item in print time.
        if (_numberOfIgnoreInPrintTime != -1)
        {
            _numberOfIgnoreInPrintTime++;
            return;
        }

        // Show Log
        LogQueueForShow.Enqueue(new G9LogItem(LogsType.INFO, identity, title, message, customCallerPath,
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
        if (!_staticIsEnableEventLogging) return;

        // Check if ignore not equal -1 => plus ignore item in print time.
        if (_numberOfIgnoreInPrintTime != -1)
        {
            _numberOfIgnoreInPrintTime++;
            return;
        }

        // Show Log
        LogQueueForShow.Enqueue(new G9LogItem(LogsType.EVENT, identity, title, message, customCallerPath,
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
                return _staticIsEnableEventLogging;
            case LogsType.INFO:
                return _staticIsEnableInformationLogging;
            case LogsType.WARN:
                return _staticIsEnableWarningLogging;
            case LogsType.ERROR:
                return _staticIsEnableErrorLogging;
            case LogsType.EXCEPTION:
                return _staticIsEnableExceptionLogging;
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
                return _staticIsEnableEventLogging;
            case LogsType.INFO:
                return _staticIsEnableInformationLogging;
            case LogsType.WARN:
                return _staticIsEnableWarningLogging;
            case LogsType.ERROR:
                return _staticIsEnableErrorLogging;
            case LogsType.EXCEPTION:
                return _staticIsEnableExceptionLogging;
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
                return _staticIsEnableEventLogging;
            case LogsType.INFO:
                return _staticIsEnableInformationLogging;
            case LogsType.WARN:
                return _staticIsEnableWarningLogging;
            case LogsType.ERROR:
                return _staticIsEnableErrorLogging;
            case LogsType.EXCEPTION:
                return _staticIsEnableExceptionLogging;
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
        var logTypeNumber = (byte) logItem.LogType;

        try
        {
            // Ignore if null
            if (string.IsNullOrEmpty(logItem.Body)) return;

            // Set log data
            var log =
                $"[### Log Type: {logItem.LogType} ###]\nDate & Time: {logItem.LogDateTime:yyyy/MM/ss HH:mm:ss.fff}\nIdentity: {logItem.Identity}\tTitle: {logItem.Title}\nBody: {new Regex("[^a-zA-Z0-9 -]").Replace(logItem.Body, string.Empty)}\nPath: {logItem.FileName}\nMethod: {logItem.MethodBase}\tLine: {logItem.LineNumber}\n\n";

            // Set debug ui log if exists
            if (TextForDebug != null)
            {
                var newLog = log + TextForDebug.text;
                TextForDebug.text = newLog.Substring(0, Mathf.Clamp(newLog.Length, 0, 6000));
            }

            // Show console log
            Debug.LogFormat(UnityLogTypes[logTypeNumber],
                LogOption.NoStacktrace,
                null,
                $"<color=#{0:X2}{LoggingColors[logTypeNumber].g:X2}{LoggingColors[logTypeNumber].b:X2}>{log}</color>"
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