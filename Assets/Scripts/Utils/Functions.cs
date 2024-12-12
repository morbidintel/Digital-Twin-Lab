using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;

namespace GeorgeChew.UnityAssessment.Utils
{
    public static class Functions
    {
        public static void SetValueFronIDictionary<T>(IDictionary<object, object> dictionary,
            string key, ref T value)
        {
            if (dictionary.TryGetValue(key, out var valueObj) && valueObj is T parsedValue)
            {
                value = parsedValue;
            }
        }

        public static IEnumerator WaitForSeconds(float seconds, Action action)
        {
            yield return new WaitForSeconds(seconds);
            action?.Invoke();
        }

        public static IEnumerator WaitUntil(Func<bool> predicate, Action action)
        {
            yield return new WaitUntil(predicate);
            action?.Invoke();
        }

        public static IEnumerator WaitForEndOfFrame(Action action)
        {
            yield return new WaitForEndOfFrame();
            action?.Invoke();
        }

        public static IEnumerable<string> SplitInParts(this string s, int partLength)
        {
            if (s == null)
                throw new ArgumentNullException(nameof(s));

            if (partLength <= 0)
                throw new ArgumentException("Part length has to be positive.", nameof(partLength));

            for (var i = 0; i < s.Length; i += partLength)
                yield return s.Substring(i, Math.Min(partLength, s.Length - i));
        }

        /// <summary>
        /// Generates a log prefix string that includes the name of the method and the type where the method is declared.
        /// </summary>
        /// <returns>
        /// A string in the format "[TypeName.MethodName] " where TypeName is the name of the class and MethodName is the name of the method.
        /// </returns>
        /// <remarks>
        /// If the method name contains "MoveNext", it indicates an iterator method, and the actual method name is extracted from the declaring type's name.
        /// </remarks>
        private static string GetLogPrefix()
        {
            System.Reflection.MethodBase methodBase = new System.Diagnostics.StackTrace().GetFrame(2).GetMethod();
            string methodName = methodBase.Name;
            if (methodName.Contains("MoveNext"))
            {
                methodName = Regex.Match(methodBase.DeclaringType.Name, "<(.*)>").Groups[1].Value;
            }

            Type declaringType = methodBase.DeclaringType;
            string typeName = declaringType.Name.Contains('<') ?
                declaringType.DeclaringType.Name :
                declaringType.Name;

            return $"[{typeName}.{methodName}] ";
        }

        /// <summary>
        /// Logs a message to the Unity console in this format:
        /// <code>
        /// 	[ClassName.MethodName] message
        /// </code>
        /// </summary>
        /// <remarks>
        /// Class name and method name are extracted from the stacktrace. See <see cref="GetLogPrefix"/>.
        /// </remarks>
        /// <param name="message">The message to log.</param>
        public static void Log(string message)
        {
            string prefix = GetLogPrefix();
            Debug.Log(prefix + message);
        }

        /// <summary>
        /// Logs a warning to the Unity console in this format:
        /// <code>
        /// 	[ClassName.MethodName] message
        /// </code>
        /// </summary>
        /// <remarks>
        /// Class name and method name are extracted from the stacktrace. See <see cref="GetLogPrefix"/>.
        /// </remarks>
        /// <param name="message">The message to log.</param>
        public static void LogWarning(string message)
        {
            string prefix = GetLogPrefix();
            Debug.LogWarning(prefix + message);
        }

        /// <summary>
        /// Logs a warning to the Unity console in this format:
        /// <code>
        /// 	[ClassName.MethodName] message
        /// </code>
        /// Also sends the exception to Crashlytics.
        /// </summary>
        /// <remarks>
        /// Class name and method name are extracted from the stacktrace. See <see cref="GetLogPrefix"/>.
        /// </remarks>
        /// <param name="message">An error message to log</param>
        public static void LogError(string message)
        {
            string prefix = GetLogPrefix();
            Debug.LogError(prefix + message);
        }

        /// <summary>
        /// Logs a warning to the Unity console in this format:
        /// The format will be:
        /// <code>
        /// 	[ClassName.MethodName] customMessage
        /// 		exception.Message
        /// 		
        /// 		exception.StackTrace
        /// 		
        /// </code>
        /// Also sends the exception to Crashlytics.
        /// </summary>
        /// <remarks>
        /// Class name and method name are extracted from the stacktrace. See <see cref="GetLogPrefix"/>.
        /// </remarks>
        /// <param name="e">The exception to log.</param>
        /// <param name="customMessage">An optional error message to log.</param>
        public static void LogError(Exception e, string customMessage = "")
        {
            string prefix = GetLogPrefix();
            Debug.LogError($"{prefix}{customMessage}\n{e.Message}\n\n{e.StackTrace}\n");
        }

        /// <summary>
        /// Blocks until condition is true or timeout occurs.
        /// </summary>
        /// <param name="condition">The break condition.</param>
        /// <param name="frequency">The frequency at which the condition will be checked.</param>
        /// <param name="timeout">The timeout in milliseconds.</param>
        /// <returns></returns>
        public static async Task WaitUntilAsync(Func<bool> condition, uint frequency = 25, long timeout = long.MaxValue)
        {
            // George: don't use Application.IsPlaying, it'll throw an exception
            while (!condition() && timeout > 0)
            {
                await Task.Delay((int)frequency);
                timeout -= (int)frequency;
            }
        }

        private static ConcurrentDictionary<string, int> delayCodes = new ConcurrentDictionary<string, int>();

        /// <summary>
        /// Delays the execution of a task with an exponential backoff strategy.
        /// </summary>
        /// <param name="delayCode">A unique code representing the delay instance.</param>
        /// <param name="backoffSecondsMax">The maximum backoff time in milliseconds. Default is 60000 milliseconds (1 minute).</param>
        /// <returns>A task that represents the asynchronous delay operation.</returns>
        public static async Task DelayWithBackoff(string delayCode, int backoffSecondsMax = 60000)
        {
            if (delayCodes.ContainsKey(delayCode))
            {
                delayCodes[delayCode] = Math.Min(delayCodes[delayCode] * 2, backoffSecondsMax);
            }
            else
            {
                delayCodes[delayCode] = 500;
            }

            await Task.Delay(delayCodes[delayCode]);
        }

        /// <summary>
        /// Resets the backoff delay for a given delay code by removing it from the delayCodes collection.
        /// </summary>
        /// <param name="delayCode">The code representing the delay to be reset.</param>
        public static void DelayBackoffReset(string delayCode)
        {
            delayCodes.TryRemove(delayCode, out _);
        }

        /// <summary>
        /// Parses the query parameters from the given URI and returns them as a dictionary.
        /// </summary>
        /// <param name="uri">The URI containing the query parameters.</param>
        /// <returns>A dictionary where the keys are the parameter names and the values are the parameter values.</returns>
        public static Dictionary<string, string> GetQueryParameters(Uri uri)
        {
            var queryParams = new Dictionary<string, string>();
            string query = uri.Query.TrimStart('?');
            string[] pairs = query.Split('&');
            foreach (string pair in pairs)
            {
                string[] keyValue = pair.Split('=');
                if (keyValue.Length == 2)
                {
                    queryParams[keyValue[0]] = Uri.UnescapeDataString(keyValue[1]);
                }
            }
            return queryParams;
        }
    }
}
