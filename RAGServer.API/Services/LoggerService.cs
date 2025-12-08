
using System.Text;
using NLog;
namespace RAGSERVERAPI.Services;

public interface ILogger
{
    void LogInfo(string message);
    void LogWarn(string message);
    void LogDebug(string message);
    void LogError(string message);
    void LogException(Exception ex);
    void LogLocationWithException(string location, Exception ex);
}

public class LoggerService : ILogger
{
    private static NLog.ILogger logger = LogManager.GetCurrentClassLogger();
    public void LogInfo(string message) => logger.Info(message);
    public void LogWarn(string message) => logger.Error(message);
    public void LogDebug(string message) => logger.Debug(message);
    public void LogError(string message) => logger.Error(message);

    public void LogLocationWithException(string location, Exception ex)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("Location: " + location);
        sb.AppendLine("Error log: " + DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt"));
        sb.AppendLine("Associated exception message: " + ex.Message);
        sb.AppendLine("Exception Inner: " + ex.InnerException);
        sb.AppendLine("Exception class: " + ex.GetType().ToString());
        sb.AppendLine("Exception source: " + ex.Source.ToString());
        sb.AppendLine("Exception method: " + ex.TargetSite.Name.ToString());
        sb.AppendLine("Exception Stack Trace : " + ex.StackTrace);
        logger.Error(sb.ToString());
    }
    public void LogException(Exception ex)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("Error log: " + DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt"));
        sb.AppendLine("Associated exception message: " + ex.Message);
        sb.AppendLine("Exception Inner: " + ex.InnerException);
        sb.AppendLine("Exception class: " + ex.GetType().ToString());
        sb.AppendLine("Exception source: " + ex.Source.ToString());
        sb.AppendLine("Exception method: " + ex.TargetSite.Name.ToString());
        sb.AppendLine("Exception Stack Trace : " + ex.StackTrace);
        logger.Error(sb.ToString());
    }
}
