using LeagueSharp.Common;
using SharpDX;

namespace Wind_Up_Helper
{
  class NotificationHandler
  {
    private static Notification _modeNotificationHandler;

    public static void Update()
    {
      var text = "Recommended Windup: " + Program.windup;

      if (_modeNotificationHandler == null)
      {
        _modeNotificationHandler = new Notification(text)
        {
          TextColor = new ColorBGRA(124, 252, 0, 255)
        };
        Notifications.AddNotification("By DanZ and Drunkenninja");
        Notifications.AddNotification(_modeNotificationHandler);
      }

      _modeNotificationHandler.Text = text;
    }
  }
}