using System;
using System.Collections;
using System.Collections.Generic;
using G9Common.Interface;
using G9LogManagement.Enums;
using UnityEngine;

public class G9Logging4Unity : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static void LogException(Exception ex, string message = null, string identity = null, string title = null)
    {
    }

    public static void LogError(string message, string identity = null, string title = null)
    {
    }

    public static void LogWarning(string message, string identity = null, string title = null)
    {
    }

    public static void LogInformation(string message, string identity = null, string title = null)
    {
    }

    public static void LogEvent(string message, string identity = null, string title = null)
    {
    }

    public static bool CheckLoggingIsActive(LogsType type)
    {
        return false;
    }

    public static bool CheckConsoleLoggingIsActive(LogsType type)
    {
        return false;
    }

    public static bool CheckFileLoggingIsActive(LogsType type)
    {
        return false;
    }
}
