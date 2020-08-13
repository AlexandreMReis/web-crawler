using NLog;

namespace WC.Logger
{
    /// <summary>
    /// The 'LogEngine' class
    /// </summary>
    public static class LogEngine
    {
        /// <summary>
        /// The crawler logger
        /// </summary>
        public static readonly NLog.Logger CrawlerLogger = LogManager.GetLogger("CrawlerLogger");
    }
}
