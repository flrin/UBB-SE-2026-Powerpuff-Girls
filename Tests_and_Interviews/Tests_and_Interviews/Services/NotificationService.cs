using Microsoft.Toolkit.Uwp.Notifications;
using System;

namespace Tests_and_Interviews.Services
{
    public class NotificationService
    {
        public void ShowBookingConfirmed(string companyName, string jobTitle, DateTime startTime, DateTime endTime)
        {
            try
            {
                new ToastContentBuilder()
                    .AddText("Interview confirmed")
                    .AddText($"{companyName} - {jobTitle}")
                    .AddText($"{startTime:MMM dd yyyy h:mm tt} - {endTime:h:mm tt}")
                    .AddButton(new ToastButtonDismiss("Close"))
                    .Show();
            }
            catch { }
        }

        public void ShowSimpleNotification(string title, string message)
        {
            try
            {
                new ToastContentBuilder()
                    .AddText(title)
                    .AddText(message)
                    .AddButton(new ToastButtonDismiss("Close"))
                    .Show();
            }
            catch { }
        }
    }
}