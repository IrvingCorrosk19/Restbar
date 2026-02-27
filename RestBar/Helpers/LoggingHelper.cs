using Microsoft.Extensions.Logging;
using System;

namespace RestBar.Helpers
{
    /// <summary>
    /// Helper para logging unificado que combina Console.WriteLine e ILogger
    /// </summary>
    public static class LoggingHelper
    {
        /// <summary>
        /// Log de información con emoji para fácil identificación
        /// </summary>
        public static void LogInfo(ILogger? logger, string className, string methodName, string message)
        {
            var logMessage = $"🔍 [{className}] {methodName}() - {message}";
            Console.WriteLine(logMessage);
            logger?.LogInformation(logMessage);
        }

        /// <summary>
        /// Log de éxito
        /// </summary>
        public static void LogSuccess(ILogger? logger, string className, string methodName, string message)
        {
            var logMessage = $"✅ [{className}] {methodName}() - {message}";
            Console.WriteLine(logMessage);
            logger?.LogInformation(logMessage);
        }

        /// <summary>
        /// Log de advertencia
        /// </summary>
        public static void LogWarning(ILogger? logger, string className, string methodName, string message)
        {
            var logMessage = $"⚠️ [{className}] {methodName}() - {message}";
            Console.WriteLine(logMessage);
            logger?.LogWarning(logMessage);
        }

        /// <summary>
        /// Log de error con excepción
        /// </summary>
        public static void LogError(ILogger? logger, string className, string methodName, Exception ex, string? contextMessage = null)
        {
            var logMessage = $"❌ [{className}] {methodName}() - Error: {ex.Message}";
            if (!string.IsNullOrEmpty(contextMessage))
            {
                logMessage = $"❌ [{className}] {methodName}() - {contextMessage} - Error: {ex.Message}";
            }
            
            Console.WriteLine(logMessage);
            Console.WriteLine($"🔍 [{className}] {methodName}() - StackTrace: {ex.StackTrace}");
            
            logger?.LogError(ex, logMessage);
        }

        /// <summary>
        /// Log de datos/estadísticas
        /// </summary>
        public static void LogData(ILogger? logger, string className, string methodName, string message)
        {
            var logMessage = $"📊 [{className}] {methodName}() - {message}";
            Console.WriteLine(logMessage);
            logger?.LogDebug(logMessage);
        }

        /// <summary>
        /// Log de comunicación HTTP/AJAX
        /// </summary>
        public static void LogHttp(ILogger? logger, string className, string methodName, string message)
        {
            var logMessage = $"📡 [{className}] {methodName}() - {message}";
            Console.WriteLine(logMessage);
            logger?.LogDebug(logMessage);
        }

        /// <summary>
        /// Log de envío de datos
        /// </summary>
        public static void LogSend(ILogger? logger, string className, string methodName, string message)
        {
            var logMessage = $"📤 [{className}] {methodName}() - {message}";
            Console.WriteLine(logMessage);
            logger?.LogDebug(logMessage);
        }

        /// <summary>
        /// Log de parámetros de entrada
        /// </summary>
        public static void LogParams(ILogger? logger, string className, string methodName, string parameters)
        {
            var logMessage = $"📋 [{className}] {methodName}() - {parameters}";
            Console.WriteLine(logMessage);
            logger?.LogDebug(logMessage);
        }
    }
}

