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

namespace Ezreal___The_prodigal_explorer
{
  class Program
  {
    private static Orbwalking.Orbwalker Orbwalker;

    private static Spell Q;

    private static Spell W;

    private static Spell E;

    private static Spell R;

    private static SpellSlot FlashSlot = SpellSlot.Unknown;

    public static float FlashRange = 450f;

    public static Vector2 oWp;

    public static Vector2 nWp;

    private static Menu Config;

    public static Obj_AI_Hero Player = ObjectManager.Player;

    static void Main(string[] args)
    {
      CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
    }
    static void Game_OnGameLoad(EventArgs args)
    {
      Notifications.AddNotification("Ezreal - The Prodigal Explorer by DanZ Loaded!", 1000);
      FlashSlot = Player.GetSpellSlot("SummonerFlash");

      Q = new Spell(SpellSlot.Q, 1200);
      W = new Spell(SpellSlot.W, 1050);
      E = new Spell(SpellSlot.E, 475);
      R = new Spell(SpellSlot.R, 20000);

      Q.SetSkillshot(0.25f, 60f, 2000f, true, SkillshotType.SkillshotLine);
      W.SetSkillshot(0.25f, 80f, 1600f, false, SkillshotType.SkillshotLine);

      Config = new Menu("Ezreal", "ezreal_menu", true);
      var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
      TargetSelector.AddToMenu(targetSelectorMenu);
      Config.AddSubMenu(targetSelectorMenu);
      Config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
      Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));

      Config.AddSubMenu(new Menu("Combo", "Combo"));
      Config.SubMenu("Combo").AddItem(new MenuItem("UseQCombo", "Use Q")).SetValue(true);
      Config.SubMenu("Combo").AddItem(new MenuItem("UseWCombo", "Use W")).SetValue(true);
      Config.SubMenu("Combo").AddItem(new MenuItem("UseECombo", "Use E offensively")).SetValue(false);
      Config.SubMenu("Combo").AddItem(new MenuItem("UseRCombo", "Use R if killable")).SetValue(true);

      Config.AddSubMenu(new Menu("Harass", "Harass"));
      Config.SubMenu("Harass").AddItem(new MenuItem("UseQHarass", "Use Q")).SetValue(true);
      Config.SubMenu("Harass").AddItem(new MenuItem("UseWHarass", "Use W")).SetValue(true);
      
      Config.AddSubMenu(new Menu("Ult", "Ult"));
      Config.SubMenu("Ult").AddItem(new MenuItem("ForceRCast", "Force R")).SetValue(new KeyBind("G".ToCharArray()[0], KeyBindType.Press));

      Config.AddSubMenu(new Menu("Farming", "Farming"));
      Config.SubMenu("Farming").AddItem(new MenuItem("QLast", "Use Q to Last Hit")).SetValue(true);
      Config.SubMenu("Farming").AddItem(new MenuItem("QClear", "Use Q to Clear Wave")).SetValue(true);
      Config.SubMenu("Farming").AddItem(new MenuItem("QClearmana", "Min. Mana % to Q Clear")).SetValue(new Slider(30, 0, 100));
      Config.SubMenu("Farming").AddItem(new MenuItem("QLastmana", "Min. Mana % to Q Last Hit")).SetValue(new Slider(50, 0, 100));


      Config.AddSubMenu(new Menu("KS", "KS"));
      Config.SubMenu("KS").AddItem(new MenuItem("UseQKS", "Use Q")).SetValue(true);
      Config.SubMenu("KS").AddItem(new MenuItem("UseWKS", "Use W")).SetValue(true);
      Config.SubMenu("KS").AddItem(new MenuItem("UseRKS", "Use R (NOT IMPLEMENTED!!)")).SetValue(true);
      Config.SubMenu("KS").AddItem(new MenuItem("RKSRange", "R KS Range")).SetValue(new Slider(1000, 1000, 4000));


      Config.AddSubMenu(new Menu("Drawings", "Drawings"));
      Config.SubMenu("Drawings").AddItem(new MenuItem("drawEnable", "Enable Drawing")).SetValue(true);
      Config.SubMenu("Drawings").AddItem(new MenuItem("drawpred", "Draw skillshot line prediction")).SetValue(true);
      Config.SubMenu("Drawings").AddItem(new MenuItem("drawQ", "Draw Q")).SetValue(true);
      Config.SubMenu("Drawings").AddItem(new MenuItem("drawW", "Draw W")).SetValue(true);
      Config.SubMenu("Drawings").AddItem(new MenuItem("drawE", "Draw E")).SetValue(true);
      Config.AddToMainMenu();

      Game.OnUpdate += OnGameUpdate;
      Drawing.OnDraw += OnDraw;
    }
    private static void OnDraw(EventArgs args)
    {
      var myPos = Drawing.WorldToScreen(Player.Position);

      if (Config.Item("drawEnable").GetValue<bool>())
      {
        if (Config.Item("drawQ").GetValue<bool>())
        {

          Render.Circle.DrawCircle(ObjectManager.Player.Position, Q.Range, Color.Aqua, 1);
        }

        if (Config.Item("drawW").GetValue<bool>())
        {

          Render.Circle.DrawCircle(ObjectManager.Player.Position, W.Range, Color.Aqua, 1);
        }
        if (Config.Item("drawE").GetValue<bool>())
        {

          Render.Circle.DrawCircle(ObjectManager.Player.Position, E.Range, Color.Aqua, 1);
        }

      }

      var enemy = TargetSelector.GetTarget(1500, TargetSelector.DamageType.Magical);
      List<Vector2> waypoints = enemy.GetWaypoints();
      for (int i = 0; i < waypoints.Count - 1; i++)
      {
        oWp = Drawing.WorldToScreen(waypoints[i].To3D());
        nWp = Drawing.WorldToScreen(waypoints[i + 1].To3D());
        if (!waypoints[i].IsOnScreen() && !waypoints[i + 1].IsOnScreen())
        {
          continue;
        }

        if (Config.Item("drawpred").GetValue<bool>())
        {
          Drawing.DrawLine(myPos.X - 15, myPos.Y - 15, nWp[0] - 15, nWp[1] - 15, 1, Color.Red);
          Drawing.DrawLine(myPos.X + 15, myPos.Y + 15, nWp[0] + 15, nWp[1] + 15, 1, Color.Red);
        }
      }
    }
    private static void Combo()
    {
      var target = TargetSelector.GetTarget(1300, TargetSelector.DamageType.Magical);
      if (Q.IsReady() && (Config.Item("UseQCombo").GetValue<bool>()))
      {
        Q.CastIfHitchanceEquals(target, HitChance.Dashing, true);
        Q.CastIfHitchanceEquals(target, HitChance.Immobile, true);
        var Qprediction = Q.GetPrediction(target);

        if (Qprediction.Hitchance >= HitChance.High && Qprediction.CollisionObjects.Count(h => h.IsEnemy && !h.IsDead && h is Obj_AI_Minion) < 2)
        {
          Q.Cast(Qprediction.CastPosition);
        }
        if (W.IsReady() && (Config.Item("UseWCombo").GetValue<bool>()))
        {
          W.CastIfHitchanceEquals(target, HitChance.Dashing, true);
          W.CastIfHitchanceEquals(target, HitChance.Immobile, true);
          var Wprediction = W.GetPrediction(target);

          if (Wprediction.Hitchance >= HitChance.High)
          {
            W.Cast(Wprediction.CastPosition);
          }
        }
      }
      if (E.IsReady() && (Config.Item("UseECombo").GetValue<bool>()))
      {

        E.Cast(Game.CursorPos.Extend(target.ServerPosition, E.Range));
      }
      if (R.IsReady() && (Config.Item("UseRCombo").GetValue<bool>()))
      {
        R.CastIfHitchanceEquals(target, HitChance.Dashing, true);
        R.CastIfHitchanceEquals(target, HitChance.Immobile, true);
        var Rprediction = R.GetPrediction(target);

        if (Rprediction.Hitchance >= HitChance.High)
        {
          R.Cast(Rprediction.CastPosition);
        }
      }

    }
    private static void ForceR()
    {
      var target = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);

      R.CastIfHitchanceEquals(target, HitChance.Dashing, true);
      R.CastIfHitchanceEquals(target, HitChance.Immobile, true);
      var Rprediction = R.GetPrediction(target);

      if (Rprediction.Hitchance >= HitChance.High)
      {
        R.Cast(Rprediction.CastPosition);
      }
    }
    private static void Harass()
    {
      var target = TargetSelector.GetTarget(1300, TargetSelector.DamageType.Magical);
      if (Q.IsReady() && (Config.Item("UseQHarass").GetValue<bool>()))
      {
        Q.CastIfHitchanceEquals(target, HitChance.Dashing, true);
        Q.CastIfHitchanceEquals(target, HitChance.Immobile, true);
        var Qprediction = Q.GetPrediction(target);

        if (Qprediction.Hitchance >= HitChance.High && Qprediction.CollisionObjects.Count(h => h.IsEnemy && !h.IsDead && h is Obj_AI_Minion) < 2)
        {
          Q.Cast(Qprediction.CastPosition);
        }
        if (W.IsReady() && (Config.Item("UseWHarass").GetValue<bool>()))
        {
          W.CastIfHitchanceEquals(target, HitChance.Dashing, true);
          W.CastIfHitchanceEquals(target, HitChance.Immobile, true);
          var Wprediction = W.GetPrediction(target);

          if (Wprediction.Hitchance >= HitChance.High)
          {
            W.Cast(Wprediction.CastPosition);
          }
        }
      }
    }
    private static void LaneClear()
    {
      if (Player.Mana / Player.MaxMana * 100 >= Config.Item("QClearmana").GetValue<Slider>().Value)
      {
        var minions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.NotAlly);
        bool jungleMobs = minions.Any(x => x.Team == GameObjectTeam.Neutral);
        if ((Config.Item("QClear").GetValue<bool>() || jungleMobs))
        {
          MinionManager.FarmLocation farmLocation = Q.GetLineFarmLocation(minions);
          if (farmLocation.Position.IsValid())
            if (farmLocation.MinionsHit >= 1 || jungleMobs)
              Q.Cast(farmLocation.Position);
        }
      }
    }
    private static void LastHit()
    {
      if (Player.Mana / Player.MaxMana * 100 >= Config.Item("QLastmana").GetValue<Slider>().Value)
      {
        var minions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.NotAlly);
        bool jungleMobs = minions.Any(x => x.Team == GameObjectTeam.Neutral);
        if ((Config.Item("QLast").GetValue<bool>() || jungleMobs))
        {
          foreach (var minion in MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.Enemy).
                                     Where(x => Q.GetDamage(x) >= x.Health))
          {
            if (Q.CanCast(minion))
              Q.CastOnUnit(minion, true);
          }
        }
      }
    }
    private static void QKS()
    {
      foreach (Obj_AI_Hero hero in ObjectManager.Get<Obj_AI_Hero>().Where(unit => unit.IsValidTarget(Q.Range)))
      {
        var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
        if (target == null) return;

        var predictionQ = Q.GetPrediction(target);

        if (Q.IsReady())
        {

          if (target.Health < Q.GetDamage(target))
          {
            if (predictionQ.Hitchance >= HitChance.High && predictionQ.CollisionObjects.Count(h => h.IsEnemy && !h.IsDead && h is Obj_AI_Minion) < 1)
            {
              Q.Cast(predictionQ.CastPosition);
            }

          }
        }
      }
    }
    private static void WKS()
    {
      foreach (Obj_AI_Hero hero in ObjectManager.Get<Obj_AI_Hero>().Where(unit => unit.IsValidTarget(W.Range)))
      {
        var target = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Magical);
        if (target == null) return;

        var predictionW = W.GetPrediction(target);

        if (Q.IsReady())
        {

          if (target.Health < Q.GetDamage(target))
          {
            if (predictionW.Hitchance >= HitChance.High)
            {
              Q.Cast(predictionW.CastPosition);
            }

          }
        }
      }
    }
    private static void RKS()
    {
    }
    private static void OnGameUpdate(EventArgs args)
    {
      var target = TargetSelector.GetTarget(1300, TargetSelector.DamageType.Magical, true);
      if (Config.Item("ForceRCast").GetValue<KeyBind>().Active)
      {
        ForceR();
      }
      if (Config.Item("UseQKS").GetValue<bool>())
      {
        QKS();
      }
      if (Config.Item("UseWKS").GetValue<bool>())
      {
        WKS();
      }
      if (Config.Item("UseRKS").GetValue<bool>())
      {
        RKS();
      }


      switch (Orbwalker.ActiveMode)
      {
        case Orbwalking.OrbwalkingMode.Combo:
          Combo();
          break;
        case Orbwalking.OrbwalkingMode.Mixed:
          Harass();
          break;
        case Orbwalking.OrbwalkingMode.LaneClear:
          LaneClear();
          break;
        case Orbwalking.OrbwalkingMode.LastHit:
          LastHit();
          break;
      }
    }
  }
}
