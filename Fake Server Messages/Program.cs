using System;
using System.Collections;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using System.Drawing;
using Color = System.Drawing.Color;
using System.Collections.Generic;
using System.Threading;

namespace Fake_Server_Messages
{
  class Program
  {
    public static Obj_AI_Hero Player = ObjectManager.Player;
    private static Menu Config;
    private static string premsg = "";
    private static float timestampX1;
    private static string sender = "[Server Message]";
    private static bool allX = true;
    private static string message;


    static void Main(string[] args)
    {
      CustomEvents.Game.OnGameLoad += Game_OnGameLoad;

    }
    static void Game_OnGameLoad(EventArgs args)
    {
      Notifications.AddNotification("Fake Server Messages by DanZ Loaded!", 1000);
      Config = new Menu("Fake Server Messages", "FSM", true);
      Config.SubMenu("Instructions:");
      Config.SubMenu("-------");
      Config.SubMenu("-Use .msg to define the new line message                                                                 Example: .msg Server will shutdown in 5 minute)");
      Config.SubMenu("---------");
      Config.SubMenu("-Use .all 0 or 1 to configure if the message is sent to team or to all.                  Example: .timestamp 1 ");
      Config.SubMenu("-----------");

      Config.AddToMainMenu();
      Game.OnUpdate += OnGameUpdate;
      Game.OnChat += OnChatSender;

    }
    private static void OnGameUpdate(EventArgs args)
    {
      //Game.Say(premsg.ToString());
    }
    private static void OnChatSender(GameChatEventArgs args)
    {
      if (!args.Sender.IsMe)
      {
        return;
      }
      if (args.Message.StartsWith(".premessage"))
      {
        args.Process = false;
      }
      if (args.Message.StartsWith(".sender"))
      {
        sender = args.Message.Substring(args.Message.IndexOf(" ") + 1);
        args.Process = false;
      }
      if (args.Message.StartsWith(".all 1"))
      {
        allX = true;
        args.Process = false;

      }
      else if (args.Message.StartsWith(".all 0"))
      {
        allX = false;
        args.Process = false;

      }
      if (args.Message.StartsWith(".msg"))
      {
        var fakemsg = args.Message.Substring(args.Message.IndexOf(" ") + 1);
        if (allX == true)
        {
          var message = string.Format("/all {0}{1}  {2}  {3}", premsg, new string('-', 45 + sender.Length), sender, fakemsg);
          Game.Say(message);
          args.Process = false;

        }
        else if (allX == false)
        {
          var message = string.Format("     {0}{1}  {2}  {3}", premsg, new string('-', 50 + sender.Length), sender, fakemsg);
          Game.Say(message);
          args.Process = false;

        }
      }

    }
  }
}
