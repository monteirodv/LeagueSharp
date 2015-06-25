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

namespace Xerath___The_Magus_Ascendant
{
  class Program
  {
    private static Orbwalking.Orbwalker Orbwalker;

    public static List<Spell> SpellList = new List<Spell>();

    private static float RDMG;

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
      Notifications.AddNotification("Xerath - The Magus Ascendant by DanZ Loaded!", 1000);
      FlashSlot = Player.GetSpellSlot("SummonerFlash");

      Q = new Spell(SpellSlot.Q, 1550);
      W = new Spell(SpellSlot.W, 1000);
      E = new Spell(SpellSlot.E, 1150);
      R = new Spell(SpellSlot.R, 3200);

      Q.SetSkillshot(0.6f, 100f, float.MaxValue, false, SkillshotType.SkillshotLine);
      W.SetSkillshot(0.7f, 125f, float.MaxValue, false, SkillshotType.SkillshotCircle);
      E.SetSkillshot(0.25f, 60f, 1400f, true, SkillshotType.SkillshotLine);
      R.SetSkillshot(0.7f, 120f, float.MaxValue, false, SkillshotType.SkillshotCircle);

      Q.SetCharged("XerathArcanopulseChargeUp", "XerathArcanopulseChargeUp", 750, 1550, 1.5f);

      SpellList.Add(Q);
      SpellList.Add(W);
      SpellList.Add(E);
      SpellList.Add(R);
      Config = new Menu("Xerath", "xerath_menu", true);
      var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
      TargetSelector.AddToMenu(targetSelectorMenu);
      Config.AddSubMenu(targetSelectorMenu);
      Config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
      Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));

      Config.AddSubMenu(new Menu("Combo", "Combo"));
      Config.SubMenu("Combo").AddItem(new MenuItem("UseQCombo", "Use Q")).SetValue(true);
      Config.SubMenu("Combo").AddItem(new MenuItem("UseWCombo", "Use W")).SetValue(true);
      Config.SubMenu("Combo").AddItem(new MenuItem("UseECombo", "Use E")).SetValue(false);

      Config.AddSubMenu(new Menu("Harass", "Harass"));
      Config.SubMenu("Harass").AddItem(new MenuItem("UseQHarass", "Use Q")).SetValue(true);
      Config.SubMenu("Harass").AddItem(new MenuItem("UseWHarass", "Use W")).SetValue(true);
      Config.SubMenu("Harass").AddItem(new MenuItem("UseEHarass", "Use E")).SetValue(false);

      Config.AddSubMenu(new Menu("Farming", "Farming"));
      Config.SubMenu("Farming").AddItem(new MenuItem("QClear", "Use Q")).SetValue(true);
      Config.SubMenu("Farming").AddItem(new MenuItem("WClear", "Use W")).SetValue(true);
      Config.SubMenu("Farming").AddItem(new MenuItem("QClearmana", "Min. Mana % to Q Clear")).SetValue(new Slider(30, 0, 100));
      Config.SubMenu("Farming").AddItem(new MenuItem("WClearmana", "Min. Mana % to W Clear")).SetValue(new Slider(50, 0, 100));

      Config.AddSubMenu(new Menu("R Options", "ROptions"));
      Config.SubMenu("ROptions").AddItem(new MenuItem("BlockMove", "Block Movement while casting R")).SetValue(true);
      Config.SubMenu("ROptions").AddItem(new MenuItem("castR", "Force R Cast").SetValue(new KeyBind("G".ToCharArray()[0], KeyBindType.Press)));
      Config.SubMenu("ROptions").AddSubMenu(new Menu("Delays", "Delays"));
      for (int i = 1; i <= 3; i++)
        Config.SubMenu("ROptions").SubMenu("Delays").AddItem(new MenuItem("Delays" + i, "Delays" + i).SetValue(new Slider(0, 1500, 0)));


      Config.AddSubMenu(new Menu("KS", "KS"));
      Config.SubMenu("KS").AddItem(new MenuItem("UseQKS", "Use Q")).SetValue(true);
      Config.SubMenu("KS").AddItem(new MenuItem("UseWKS", "Use W")).SetValue(true);
      Config.SubMenu("KS").AddItem(new MenuItem("UseRKS", "Use R")).SetValue(false);

      Config.AddSubMenu(new Menu("Interrupter", "Interrupter"));
      Config.SubMenu("Interrupter").AddItem(new MenuItem("EInterrupt", "Interrupt Dangerous spells with E")).SetValue(true);

      Config.AddSubMenu(new Menu("Anti GapCloser", "AntiGapCloser"));
      Config.SubMenu("AntiGapCloser").AddItem(new MenuItem("EGC", "Use E on GapClosers")).SetValue(true);

      Config.AddSubMenu(new Menu("Drawings", "Drawings"));
      Config.SubMenu("Drawings").AddItem(new MenuItem("drawEnable", "Enable Drawing")).SetValue(true);
      Config.SubMenu("Drawings").AddItem(new MenuItem("drawpred", "Draw skillshot line prediction")).SetValue(true);
      Config.SubMenu("Drawings").AddItem(new MenuItem("drawQ", "Draw Q")).SetValue(true);
      Config.SubMenu("Drawings").AddItem(new MenuItem("drawW", "Draw W")).SetValue(true);
      Config.SubMenu("Drawings").AddItem(new MenuItem("drawE", "Draw E")).SetValue(true);
      Config.SubMenu("Drawings").AddItem(new MenuItem("drawkill", "Draw Ult damage on HP Bar(MAY CAUSE FPS DROPS, NEEDS FIX!)")).SetValue(false);
      Config.AddToMainMenu();

      Game.OnUpdate += OnGameUpdate;
      Drawing.OnDraw += OnDraw;
      Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
      AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
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
        if (Config.Item("drawkill").GetValue<bool>())
        {

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
    static void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
    {
      if (Config.Item("EInterrupt").GetValue<bool>()) return;

      if (sender.IsValidTarget(E.Range))
      {
        E.Cast(sender);
      }
    }
    private static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
    {
      if (gapcloser.Sender.IsValidTarget(E.Range))
      {
        if (Config.Item("EGC").GetValue<bool>() && E.IsReady())
        {
          E.Cast(gapcloser.Sender);
        }
      }
    }

    private static void Combo()
    {
      var target = TargetSelector.GetTarget(1300, TargetSelector.DamageType.Magical);

      if (Q.IsReady() && target.IsValid && (Config.Item("UseQCombo").GetValue<bool>()))
      {
        if (!Q.IsCharging)
        {
          Q.StartCharging();
        }
        if (Q.IsCharging)
        {
          Q.CastIfHitchanceEquals(target, HitChance.VeryHigh, true);
        }
      }
      if (W.IsReady() && (Config.Item("UseWCombo").GetValue<bool>()))
      {
        W.CastIfHitchanceEquals(target, HitChance.Dashing, true);
        W.CastIfHitchanceEquals(target, HitChance.Immobile, true);
        var Wprediction = W.GetPrediction(target);
        if (Wprediction.Hitchance >= HitChance.VeryHigh)
        {
          W.Cast(Wprediction.CastPosition);
        }
      }
      if (E.IsReady() && (Config.Item("UseECombo").GetValue<bool>()))
      {
        E.CastIfHitchanceEquals(target, HitChance.Dashing, true);
        E.CastIfHitchanceEquals(target, HitChance.Immobile, true);
        var Eprediction = E.GetPrediction(target);
        if (Eprediction.Hitchance >= HitChance.VeryHigh)
        {
          E.Cast(Eprediction.CastPosition);
        }
      }

      if (target.Health < Player.GetSpellDamage(target, SpellSlot.R) * 3)
        if (R.IsReady())
          castR();

    }

    private static void Harass()
    {
      var target = TargetSelector.GetTarget(1300, TargetSelector.DamageType.Magical);
      if (Q.IsReady() && (Config.Item("UseQHarass").GetValue<bool>()))
      {
        if (!Q.IsCharging)
        {
          Q.StartCharging();
          return;
        }
        if (Q.IsCharging)
        {
          Q.CastIfHitchanceEquals(target, HitChance.VeryHigh, true);
        }
      }
      if (W.IsReady() && (Config.Item("UseWHarass").GetValue<bool>()))
      {
        W.CastIfHitchanceEquals(target, HitChance.Dashing, true);
        W.CastIfHitchanceEquals(target, HitChance.Immobile, true);
        var Wprediction = W.GetPrediction(target);

        if (Wprediction.Hitchance >= HitChance.VeryHigh)
        {
          W.Cast(Wprediction.CastPosition);
        }
      }
      if (E.IsReady() && (Config.Item("UseEHarass").GetValue<bool>()))
      {
        E.CastIfHitchanceEquals(target, HitChance.Dashing, true);
        E.CastIfHitchanceEquals(target, HitChance.Immobile, true);
        var Eprediction = E.GetPrediction(target);

        if (Eprediction.Hitchance >= HitChance.VeryHigh)
        {
          E.Cast(Eprediction.CastPosition);
        }
      }

    }
    private static void LaneClear()
    {
      if (Player.Mana / Player.MaxMana * 100 >= Config.Item("QClearmana").GetValue<Slider>().Value)
      {
        var minions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.NotAlly);
        bool jungleMobs = minions.Any(x => x.Team == GameObjectTeam.Neutral);
        if ((Config.Item("WClear").GetValue<bool>() || jungleMobs))
        {
          MinionManager.FarmLocation farmLocation = W.GetCircularFarmLocation(minions);
          if (farmLocation.Position.IsValid())
            if (farmLocation.MinionsHit >= 1 || jungleMobs)
              W.Cast(farmLocation.Position);
        }
        if ((Config.Item("QClear").GetValue<bool>() || jungleMobs))
        {
          MinionManager.FarmLocation farmLocation = Q.GetLineFarmLocation(minions);
          if (farmLocation.Position.IsValid())
            if (farmLocation.MinionsHit >= 1 || jungleMobs)
              if (!Q.IsCharging)
              {
                Q.StartCharging();
                return;
              }
          if (Q.IsCharging)
          {
            Q.Cast(farmLocation.Position);
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

        if (Q.IsReady())
        {

          if (target.Health < Q.GetDamage(target))
          {
            if (!Q.IsCharging)
            {
              Q.StartCharging();
              return;
            }
            if (Q.IsCharging)
            {
              Q.Cast(target.ServerPosition);
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

        if (W.IsReady())
        {

          if (target.Health < W.GetDamage(target))
          {
            if (predictionW.Hitchance >= HitChance.VeryHigh)
            {
              W.Cast(predictionW.CastPosition);
            }

          }
        }
      }
    }
    private static void EKS()
    {
      foreach (Obj_AI_Hero hero in ObjectManager.Get<Obj_AI_Hero>().Where(unit => unit.IsValidTarget(E.Range)))
      {
        var target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);
        if (target == null) return;

        var predictionE = E.GetPrediction(target);

        if (E.IsReady())
        {

          if (target.Health < E.GetDamage(target))
          {
            if (predictionE.Hitchance >= HitChance.VeryHigh)
            {
              W.Cast(predictionE.CastPosition);
            }

          }
        }
      }
    }

    private static void RKS()
    {
      foreach (Obj_AI_Hero hero in ObjectManager.Get<Obj_AI_Hero>().Where(unit => unit.IsValidTarget(R.Range)))
      {
        var target = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);
        if (target == null) return;
        if (target.Health < Player.GetSpellDamage(target, SpellSlot.R) * 3)


          if (R.IsReady())
            castR();
      }
    }
    public static class RCharges
    {
      public static int CastR;
      public static int Index;
      public static Vector3 Position;
    }


    public static bool CastingR
    {
      get
      {
        return Player.HasBuff("XerathLocusOfPower2", true) ||
               (Player.LastCastedSpellName() == "XerathLocusOfPower2" &&
                Environment.TickCount - Player.LastCastedSpellT() < 500);
      }
    }
    private static void Obj_AI_Hero_OnIssueOrder(Obj_AI_Base sender, GameObjectIssueOrderEventArgs args)
    {
      if (Config.Item("BlockMove").GetValue<bool>())
      {
        if (CastingR)
        {
          args.Process = false;
        }
      }
    }


    private static void castR()
    {
      var Range1 = 2000 + (1100 * R.Level);

      var target = TargetSelector.GetTarget(Range1, TargetSelector.DamageType.Magical);
      if (Utils.TickCount - RCharges.CastR > Config.Item("Delays" + (RCharges.Index + 1)).GetValue<Slider>().Value)
      {
        R.Cast(target, true);
      }
    }
    private static void OnGameUpdate(EventArgs args)
    {
      if (Config.Item("castR").GetValue<KeyBind>().Active)
      {
        castR();
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
      }
      if (Config.Item("drawkill").GetValue<bool>())
      {
        Utility.HpBarDamageIndicator.DamageToUnit += hero => (float)Player.GetSpellDamage(hero, SpellSlot.R) * 3;
      }
      if (Config.Item("UseQKS").GetValue<bool>())
      {
        QKS();
      }
      if (Config.Item("UseWKS").GetValue<bool>())
      {
        WKS();
      }
      if (Config.Item("UseEKS").GetValue<bool>())
      {
        EKS();
      }
      if (Config.Item("UseRKS").GetValue<bool>())
      {
        RKS();
      }
    }
  }
}
