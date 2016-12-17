using System;
using System.IO;
using System.Reflection;
using System.Diagnostics;

namespace TeamNote
{
  class Debug
  {
    private static StreamWriter m_sLogWriter = null;

    public static void Setup(string filename = "")
    {
      m_sLogWriter = new StreamWriter(filename, true);
      WriteLog("S", "Starting application log.");
    }

    [Conditional("DEBUG")]
    public static void Log(string format, params object[] args)
    {
      WriteLog("D", format, args);
    }

    public static void Warn(string format, params object[] args)
    {
      WriteLog("W", format, args);
    }

    public static void Error(string format, params object[] args)
    {
      WriteLog("E", format, args);
    }

    public static void Exception(Exception ex)
    {
      WriteLog("P", "{0}{1}{2}.", ex.Message, Environment.NewLine, ex.StackTrace);
    }

    private static void WriteLog(string type, string format, params object[] args)
    {
      // if (m_sLogWriter == null)
      //  return;

      StackFrame currentFrame = new StackFrame(2);
      MethodBase methodBase = currentFrame.GetMethod();

      if (methodBase == null)
        return;

      m_sLogWriter?.WriteLine("{0} {1} [{2} {3}] {4}", type, DateTime.Now.ToLongTimeString(), methodBase.DeclaringType, methodBase.Name, string.Format(format, args));
      m_sLogWriter?.Flush();
    }
  }
}