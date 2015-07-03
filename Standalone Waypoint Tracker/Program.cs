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

namespace Standalone_Waypoint_Tracker
{
  class Program
  {
    private static Menu Config;
    public static Obj_AI_Hero Player = ObjectManager.Player;
    public static Vector2 Wp1;
    public static Vector2 Wp2;

    static void Main(string[] args)
    {
      CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
    }
    static void Game_OnGameLoad(EventArgs args)
    {
      Notifications.AddNotification("Standalone Waypoint Tracker by DanZ", 1000);
      Config = new Menu("Waypoint Tracker", "wp_menu", true);
      Config.AddSubMenu(new Menu("On", "On"));
      Config.SubMenu("On").AddItem(new MenuItem("Toggle", "Toggle")).SetValue(true);
      Config.AddToMainMenu();

      Drawing.OnDraw += OnDraw;
    }
    private static void DrawCross(float x, float y, float size, float thickness, Color color)
    {
      var topLeft = new Vector2(x - 10 * size, y - 10 * size);
      var topRight = new Vector2(x + 10 * size, y - 10 * size);
      var botLeft = new Vector2(x - 10 * size, y + 10 * size);
      var botRight = new Vector2(x + 10 * size, y + 10 * size);

      Drawing.DrawLine(topLeft.X, topLeft.Y, botRight.X, botRight.Y, thickness, color);
      Drawing.DrawLine(topRight.X, topRight.Y, botLeft.X, botLeft.Y, thickness, color);
    }
    private static void OnDraw(EventArgs args)
    {
      var myPos = Drawing.WorldToScreen(Player.Position);
      var enemy = TargetSelector.GetTarget(1500, TargetSelector.DamageType.Magical);
      var enemyPos = Drawing.WorldToScreen(enemy.ServerPosition);
      List<Vector2> waypoints = enemy.GetWaypoints();
      for (int i = 0; i < waypoints.Count - 1; i++)
      {


        Wp1 = Drawing.WorldToScreen(waypoints[i].To3D());
        Wp2 = Drawing.WorldToScreen(waypoints[i + 1].To3D());
        if (!waypoints[i].IsOnScreen() && !waypoints[i + 1].IsOnScreen())
        {
          continue;
        }

        if (Config.Item("Toggle").GetValue<bool>())
        {
          Drawing.DrawLine(enemyPos.X, enemyPos.Y, Wp2[0], Wp2[1], 2, Color.Red);
          DrawCross(Wp2[0], Wp2[1], 1, 2, Color.Lime);
        }
      }
    }
  }
}
