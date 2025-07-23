namespace Sellsys.Domain.Common
{
    /// <summary>
    /// 时间辅助工具类
    /// </summary>
    public static class TimeHelper
    {
        /// <summary>
        /// 获取北京时间（UTC+8）
        /// </summary>
        /// <returns>当前北京时间</returns>
        public static DateTime GetBeijingTime()
        {
            try
            {
                // 尝试使用中国标准时间时区
                var chinaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("China Standard Time");
                return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, chinaTimeZone);
            }
            catch
            {
                try
                {
                    // 如果上面失败，尝试使用Asia/Shanghai时区（Linux系统）
                    var shanghaiTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Shanghai");
                    return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, shanghaiTimeZone);
                }
                catch
                {
                    // 如果都失败，手动添加8小时
                    return DateTime.UtcNow.AddHours(8);
                }
            }
        }
    }
}
