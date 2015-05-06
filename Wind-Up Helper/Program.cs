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

namespace Wind_Up_Helper
{
  class Program
  {
    public static double windup;

    private static Menu Config;

    private static Orbwalking.Orbwalker Orbwalker;

    public static Obj_AI_Hero Player
    {
      get
      {
        return ObjectManager.Player;
      }
    }
    static void Main(string[] args)
    {
      CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
    }
    static void Game_OnGameLoad(EventArgs args)
    {
     // Notifications.AddNotification("Wind-Up Helper by DanZ and Drunkenninja loaded!", 1000);
      Config = new Menu("Wind-Up", "winduphelper", true);
      Config.AddSubMenu(new Menu("On", "On"));
      //Config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
      //Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));
      Config.SubMenu("On").AddItem(new MenuItem("On", "On")).SetValue(true);
      Config.AddToMainMenu();

      Game.OnUpdate += OnGameUpdate;
      Drawing.OnDraw += OnDraw;

    }
    private static void OnGameUpdate(EventArgs args)
    {
      NotificationHandler.Update();

      if (Config.Item("On").GetValue<bool>())
      {
        {
          windup = Game.Ping * 1.5;
        }
      }
    }
    private static void OnDraw(EventArgs args)
    {
      //Drawing.DrawText(10, 10, Color.Red, windup.ToString());
    }
  }
}

