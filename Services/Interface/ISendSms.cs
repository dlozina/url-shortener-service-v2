namespace Shortener.Service.Services.Interface
{
    public interface ISendSms
    {
        void NotifyTargetUrlIsShortened();

        void SendDailyNotification();

        void SendWeeklyReportNotification();

        void SendMonthlyReportNotification();
    }
}