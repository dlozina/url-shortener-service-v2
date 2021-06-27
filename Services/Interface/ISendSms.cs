using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
