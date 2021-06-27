namespace Shortener.Service.Services.Interface
{
    public interface ISendSms
    {
        void NotifyTargetUrlIsShortened();

        void SendDailyNotification();

        void SendWeeklyNotification();

        void SendMonthlyNotification();
    }
}