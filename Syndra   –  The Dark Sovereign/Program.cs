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

namespace Syndra______The_Dark_Sovereign
{
  class Program
  {
    private static Orbwalking.Orbwalker Orbwalker;
    private static List<Spell> SpellList = new List<Spell>();
    private static int QET;
    public static int OrbTimer;
    private static Spell Q, W, QE, E, R; 
    private static SpellSlot FlashSlot = SpellSlot.Unknown;
    public static float FlashRange = 450f;
    private static Menu Config;
    public static Vector2 oWp;
    public static Vector2 nWp;
    public static Obj_AI_Hero Player = ObjectManager.Player;

    static void Main(string[] args)
    {
      CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
    }
    static void Game_OnGameLoad(EventArgs args)
    {
      if (Player.ChampionName != "Syndra") return;
      Notifications.AddNotification("Syndra - The Dark Sovereign by DanZ Loaded!", 1000);
      FlashSlot = Player.GetSpellSlot("SummonerFlash");

      Q = new Spell(SpellSlot.Q, 800);
      W = new Spell(SpellSlot.W, 950);
      E = new Spell(SpellSlot.E, 650);
      QE = new Spell(SpellSlot.Q, Q.Range + 500);
      R = new Spell(SpellSlot.R, 675);

      SpellList.Add(Q);
      SpellList.Add(W);
      SpellList.Add(E);
      SpellList.Add(R);
      Q.SetSkillshot(0.600f, 150f, int.MaxValue, false, SkillshotType.SkillshotCircle);
      W.SetSkillshot(0.500f, 70f, 1900f, false, SkillshotType.SkillshotCircle);
      E.SetSkillshot(0.300f, 90f, 1601f, false, SkillshotType.SkillshotCone);
      QE.SetSkillshot(float.MaxValue, 55f, 2000f, false, SkillshotType.SkillshotCircle);
      Config = new Menu("Syndra", "syndra_menu", true);
      var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
      TargetSelector.AddToMenu(targetSelectorMenu);
      Config.AddSubMenu(targetSelectorMenu);
      Config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
      Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));
      Config.SubMenu("Combo").AddItem(new MenuItem("UseQCombo", "Use Q")).SetValue(true);
      Config.SubMenu("Combo").AddItem(new MenuItem("UseWCombo", "Use W")).SetValue(true);
      Config.SubMenu("Combo").AddItem(new MenuItem("UseECombo", "Use W")).SetValue(true);
      Config.SubMenu("Combo").AddItem(new MenuItem("UseQECombo", "Use QE")).SetValue(true);
      Config.SubMenu("Combo").AddItem(new MenuItem("UseRCombo", "Use R if Killable")).SetValue(true);
      
      Config.AddSubMenu(new Menu("Mass Orb Ultimate", "SpecialUlt"));
      Config.SubMenu("SpecialUlt").AddItem(new MenuItem("SpheresNumber", "Gather * spheres and Ult")).SetValue(new Slider(4, 4, 6));
      Config.SubMenu("SpecialUlt").AddItem(new MenuItem("SpecialUltGO", "Go!").SetValue(new KeyBind('T', KeyBindType.Press)));


      Config.AddSubMenu(new Menu("Harass", "Harass"));
      Config.SubMenu("Harass").AddItem(new MenuItem("UseQHarass", "Use Q")).SetValue(true);
      Config.SubMenu("Harass").AddItem(new MenuItem("UseWHarass", "Use W")).SetValue(true);
      Config.SubMenu("Harass").AddItem(new MenuItem("UseQEHarass", "Use Q-E")).SetValue(true);
      Config.SubMenu("Harass").AddItem(new MenuItem("UseQAA", "Use Q while target is attacking")).SetValue(true);
       Config.SubMenu("Harass").AddItem(new MenuItem("HarassMana", "Min Mana % to harass:")).SetValue(new Slider(50, 0, 100));
     
      Config.AddSubMenu(new Menu("LaneClear", "LaneClear"));
      Config.SubMenu("LaneClear").AddItem(new MenuItem("UseQClear", "Use Q")).SetValue(true);
      Config.SubMenu("LaneClear").AddItem(new MenuItem("UseWClear", "Use W")).SetValue(true);
      Config.SubMenu("LaneClear").AddItem(new MenuItem("UseEClear", "Use E")).SetValue(true);
      Config.SubMenu("LaneClear").AddItem(new MenuItem("UseQEClear", "Use Q-E")).SetValue(true);
      Config.SubMenu("LaneClear").AddItem(new MenuItem("ClearMana", "Min Mana % to Clear:")).SetValue(new Slider(50, 0, 100));
      
      Config.AddSubMenu(new Menu("KS", "KS"));
      Config.SubMenu("KS").AddItem(new MenuItem("UseQKS", "Use Q")).SetValue(true);
      Config.SubMenu("KS").AddItem(new MenuItem("UseWKS", "Use W")).SetValue(true);
      Config.SubMenu("KS").AddItem(new MenuItem("UseEKS", "Use E")).SetValue(true);
      Config.SubMenu("KS").AddItem(new MenuItem("UseQEKS", "Use Q-E")).SetValue(true);
      Config.SubMenu("KS").AddItem(new MenuItem("UseRKS", "Use R")).SetValue(false);

      Config.AddSubMenu(new Menu("Interrupts", "Interrupts"));
      Config.SubMenu("Interrupts").AddItem(new MenuItem("EInterrupt", "Interrupt Spells with E").SetValue(true));

      Config.AddSubMenu(new Menu("Gap Closers", "GapClosers"));
      Config.SubMenu("GapClosers").AddItem(new MenuItem("EGapCloser", "Auto use E away on Gap Closers").SetValue(true));

      Config.AddSubMenu(new Menu("Drawings", "Drawings"));
      Config.SubMenu("Drawings").AddItem(new MenuItem("drawEnable", "Enable Drawing")).SetValue(true);
      Config.SubMenu("Drawings").AddItem(new MenuItem("drawEpred", "Draw Q-E line prediction")).SetValue(true);
      Config.SubMenu("Drawings").AddItem(new MenuItem("drawQ", "Draw Q")).SetValue(true);
      Config.SubMenu("Drawings").AddItem(new MenuItem("drawW", "Draw W")).SetValue(true);
      Config.SubMenu("Drawings").AddItem(new MenuItem("drawE", "Draw E")).SetValue(true);
      Config.SubMenu("Drawings").AddItem(new MenuItem("drawR", "Draw R")).SetValue(true);
      Config.SubMenu("Drawings").AddItem(new MenuItem("drawtimer", "Draw Timer on Orbs(Not implemented yet)")).SetValue(true);
      var dmg = new MenuItem("combodamage", "Damage Indicator").SetValue(true);
      var drawFill = new MenuItem("color", "Fill colour", true).SetValue(new Circle(true, Color.Orange));
      Config.SubMenu("Draw").AddItem(drawFill);
      Config.SubMenu("Draw").AddItem(dmg);
  
      DrawDamage.DamageToUnit = GetComboDamage;
      DrawDamage.Enabled = dmg.GetValue<bool>();
      DrawDamage.Fill = drawFill.GetValue<Circle>().Active;
      DrawDamage.FillColor = drawFill.GetValue<Circle>().Color;

      dmg.ValueChanged += delegate(object sender, OnValueChangeEventArgs eventArgs)
      {
        DrawDamage.Enabled = eventArgs.GetNewValue<bool>();
      };

      drawFill.ValueChanged += delegate(object sender, OnValueChangeEventArgs eventArgs)
      {
        DrawDamage.Fill = eventArgs.GetNewValue<Circle>().Active;
        DrawDamage.FillColor = eventArgs.GetNewValue<Circle>().Color;
      };


      Config.AddToMainMenu();
      Game.OnUpdate += OnGameUpdate;
      Drawing.OnDraw += OnDraw;
      AntiGapcloser.OnEnemyGapcloser += OnEnemyGapcloser;
      Interrupter2.OnInterruptableTarget += OnPossibleToInterrupt;
      Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;

      Config = new Menu("Syndra", "syndra_menu", true);

    }
    private static float GetComboDamage(Obj_AI_Hero enemy)
    {
      double damage = 0d;

      if (Q.IsReady())
        damage += Player.GetSpellDamage(enemy, SpellSlot.Q);

      if (W.IsReady())
        damage += Player.GetSpellDamage(enemy, SpellSlot.W);

      if (E.IsReady())
        damage += Player.GetSpellDamage(enemy, SpellSlot.E);

      if (R.IsReady())
        damage += Player.GetSpellDamage(enemy, SpellSlot.R);


      return (float)damage;
    }
    static Vector2 V2E(Vector3 from, Vector3 direction, float distance)
    {
      return from.To2D() + distance * Vector3.Normalize(direction - from).To2D();
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
        if (Config.Item("drawR").GetValue<bool>())
        {

          Render.Circle.DrawCircle(ObjectManager.Player.Position, R.Range, Color.Aqua, 1);
        }
        if (Config.Item("drawtimer").GetValue<bool>())
        {
          foreach (var orb in OrbManager.GetOrbs(false))
          {
            var orbPos = Drawing.WorldToScreen(orb);
            Drawing.DrawText(orbPos.X, orbPos.Y, Color.Lime, OrbTimer.ToString());
          }
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



        if (Config.Item("drawEpred").GetValue<bool>())
        {
          Drawing.DrawLine(myPos.X - 25, myPos.Y - 25, nWp[0] - 25, nWp[1] - 25, 1, Color.Red);
          Drawing.DrawLine(myPos.X + 25, myPos.Y + 25, nWp[0] + 25, nWp[1] + 25, 1, Color.Red);
        }

      
      }
    }
    private static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
    {
      if (Player.Mana / Player.MaxMana * 100 > Config.Item("HarassMana").GetValue<Slider>().Value)
      {
        if (Config.Item("UseQAA").GetValue<bool>() && sender.Type == Player.Type && sender.Team != Player.Team && args.SData.Name.ToLower().Contains("attack") && Player.Distance(sender, true) <= Math.Pow(Q.Range, 2))
        {
          Q.Cast(sender.ServerPosition);
        }
      }
    }
    private static void OnPossibleToInterrupt(Obj_AI_Hero target, Interrupter2.InterruptableTargetEventArgs args)
    {
      if (Config.Item("EInterrupt").GetValue<bool>() && E.IsReady() && E.IsInRange(target))
      {
        E.Cast(target.ServerPosition);
      }
    }
    private static void OnEnemyGapcloser(ActiveGapcloser gapcloser)
    {
      if (gapcloser.Sender.IsAlly)
      {
        return;
      }

      if (Config.Item("EGapCloser").GetValue<bool>() && E.IsReady() && E.IsInRange(gapcloser.Start))
      {
        E.Cast(gapcloser.End);
      }
    }
    private static Vector3 GetGrabableObjectPos(bool onlyOrbs)
    {
      if (!onlyOrbs)
        foreach (var minion in ObjectManager.Get<Obj_AI_Minion>().Where(minion => minion.IsValidTarget(W.Range))
            )
          return minion.ServerPosition;

      return OrbManager.GetOrbToGrab((int)W.Range);
    }
    private static int GetOrbCount()
    {
      OrbManager.GetOrbs().Count();
      return OrbManager.GetOrbs().Count();
    }
    private static void Harass()
    {
      var target = TargetSelector.GetTarget(1300, TargetSelector.DamageType.Magical);
      if (Q.IsReady() && (Config.Item("UseQHarass").GetValue<bool>()))
      {
        var Qprediction = Q.GetPrediction(target);

        if (Qprediction.Hitchance >= HitChance.VeryHigh)
        {
          Q.Cast(Qprediction.CastPosition);
        }
      }
      if (W.IsReady() && (Config.Item("UseWHarass").GetValue<bool>()))
      {
        if (Player.Spellbook.GetSpell(SpellSlot.W).ToggleState == 1 && W.IsReady())
        {
          var WGrabObject = GetGrabableObjectPos(target == null);

          if (WGrabObject.To2D().IsValid() && Utils.TickCount - W.LastCastAttemptT > Game.Ping + 300)
          {
            W.Cast(WGrabObject);
            W.LastCastAttemptT = Utils.TickCount;
          }
          else if (target != null && Player.Spellbook.GetSpell(SpellSlot.W).ToggleState != 1 && W.IsReady() &&
         Utils.TickCount - W.LastCastAttemptT > Game.Ping + 100)
          {
            if (OrbManager.WObject(false) != null)
            {
              W.From = OrbManager.WObject(false).ServerPosition;
              W.Cast(target, false, true);
            }
          }
        }
        else if (target != null && Player.Spellbook.GetSpell(SpellSlot.W).ToggleState != 1 && W.IsReady() &&
                 Utils.TickCount - W.LastCastAttemptT > Game.Ping + 100)
        {
          if (OrbManager.WObject(false) != null)
          {
            W.From = OrbManager.WObject(false).ServerPosition;
            W.Cast(target, false, true);
          }
        }
      }
      if (E.IsReady() && (Config.Item("UseEHarass").GetValue<bool>()))
      {
        foreach (var orb in OrbManager.GetOrbs(true))
          if (Player.Distance(orb) < E.Range + 100)
          {
            var startPoint = orb.To2D().Extend(Player.ServerPosition.To2D(), 100);
            var endPoint = Player.ServerPosition.To2D()
                .Extend(orb.To2D(), Player.Distance(orb) > 200 ? 1300 : 1000);
            QE.Delay = E.Delay + Player.Distance(orb) / E.Speed;
            QE.From = orb;
            var enemyPred = QE.GetPrediction(target);
            if (enemyPred.Hitchance >= HitChance.VeryHigh &&
                enemyPred.UnitPosition.To2D().Distance(startPoint, endPoint, false) <
                QE.Width + target.BoundingRadius)
            {
              E.Cast(orb, true);
              W.LastCastAttemptT = Utils.TickCount;
              return;
            }
          }
      }
      if (E.IsReady() && Q.IsReady() && (Config.Item("UseQEHarass").GetValue<bool>()))
      {
        QE.Delay = E.Delay + Q.Range / E.Speed;
        QE.From = Player.ServerPosition.To2D().Extend(target.ServerPosition.To2D(), Q.Range).To3D();

        var prediction = QE.GetPrediction(target);
        if (prediction.Hitchance >= HitChance.High)
        {
          Q.Cast(Player.ServerPosition.To2D().Extend(prediction.CastPosition.To2D(), Q.Range - 100));
          QET = Utils.TickCount;
          W.LastCastAttemptT = Utils.TickCount;
        }
      }
    }
    private static void Combo()
    {
      var target = TargetSelector.GetTarget(1300, TargetSelector.DamageType.Magical);
      if (Q.IsReady() && (Config.Item("UseQCombo").GetValue<bool>()))
      {
        var Qprediction = Q.GetPrediction(target);

        if (Qprediction.Hitchance >= HitChance.VeryHigh)
        {
          Q.Cast(Qprediction.CastPosition);
        }
      }
      if (Player.Spellbook.GetSpell(SpellSlot.W).ToggleState == 1 && W.IsReady())
      {
        var WGrabObject = GetGrabableObjectPos(target == null);

        if (WGrabObject.To2D().IsValid() && Utils.TickCount - W.LastCastAttemptT > Game.Ping + 300)
        {
          W.Cast(WGrabObject);
          W.LastCastAttemptT = Utils.TickCount;
        }
        else if (target != null && Player.Spellbook.GetSpell(SpellSlot.W).ToggleState != 1 && W.IsReady() &&
       Utils.TickCount - W.LastCastAttemptT > Game.Ping + 100)
        {
          if (OrbManager.WObject(false) != null)
          {
            W.From = OrbManager.WObject(false).ServerPosition;
            W.Cast(target, false, true);
          }
        }
      }
      else if (target != null && Player.Spellbook.GetSpell(SpellSlot.W).ToggleState != 1 && W.IsReady() &&
               Utils.TickCount - W.LastCastAttemptT > Game.Ping + 100)
      {
        if (OrbManager.WObject(false) != null)
        {
          W.From = OrbManager.WObject(false).ServerPosition;
          W.Cast(target, false, true);
        }
      }
      if (E.IsReady() && (Config.Item("UseECombo").GetValue<bool>()))
      {
        foreach (var orb in OrbManager.GetOrbs(true))
          if (Player.Distance(orb) < E.Range + 100)
          {
            var startPoint = orb.To2D().Extend(Player.ServerPosition.To2D(), 100);
            var endPoint = Player.ServerPosition.To2D()
                .Extend(orb.To2D(), Player.Distance(orb) > 200 ? 1300 : 1000);
            QE.Delay = E.Delay + Player.Distance(orb) / E.Speed;
            QE.From = orb;
            var enemyPred = QE.GetPrediction(target);
            if (enemyPred.Hitchance >= HitChance.VeryHigh &&
                enemyPred.UnitPosition.To2D().Distance(startPoint, endPoint, false) <
                QE.Width + target.BoundingRadius)
            {
              E.Cast(orb, true);
              W.LastCastAttemptT = Utils.TickCount;
              return;
            }
          }
      }
      if (E.IsReady() && Q.IsReady() && (Config.Item("UseQECombo").GetValue<bool>()))
      {
        QE.Delay = E.Delay + Q.Range / E.Speed;
        QE.From = Player.ServerPosition.To2D().Extend(target.ServerPosition.To2D(), Q.Range).To3D();

        var prediction = QE.GetPrediction(target);
        if (prediction.Hitchance >= HitChance.High)
        {
          Q.Cast(Player.ServerPosition.To2D().Extend(prediction.CastPosition.To2D(), Q.Range - 100));
          QET = Utils.TickCount;
          W.LastCastAttemptT = Utils.TickCount;
        }
        }
        if (R.IsReady() && (Config.Item("UseRCombo").GetValue<bool>()))
        {
          if (target.Health < R.GetDamage(target))
          {
            R.Cast(target);
          }
        }
      }
    
    private static void Clear()
    {
      //if (Player.Mana / Player.MaxMana * 100 >= Config.Item("ClearMana").GetValue<Slider>().Value)
     // {
        var minions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.NotAlly);

        if (minions.Count == 0)
          return; 
        bool jungleMobs = minions.Any(x => x.Team == GameObjectTeam.Neutral);
        if (Config.Item("UseQClear").GetValue<bool>())
        {
          MinionManager.FarmLocation QfarmLocation = Q.GetCircularFarmLocation(minions);
          if (QfarmLocation.MinionsHit >= 1 || jungleMobs)
          {
            Q.Cast(QfarmLocation.Position);
          }
        }
        if (Config.Item("UseWClear").GetValue<bool>())
        {
          MinionManager.FarmLocation WfarmLocation = W.GetCircularFarmLocation(minions);
          if (WfarmLocation.MinionsHit >= 1 || jungleMobs)
          {
            if (Player.Spellbook.GetSpell(SpellSlot.W).ToggleState == 1 && W.IsReady())
            {
            //  var WGrabObject = GetGrabableObjectPos(target == null);

              //if (WGrabObject.To2D().IsValid())
              //{
                W.Cast(WfarmLocation.Position);
              //}
            }
          }
        }
        if (Config.Item("UseEClear").GetValue<bool>())
        {
          MinionManager.FarmLocation EfarmLocation = E.GetLineFarmLocation(minions);
          if (EfarmLocation.MinionsHit >= 1 || jungleMobs)
          {
            Q.Cast(EfarmLocation.Position);
          }
        }
      }
   // }
    private static void SpecialUlt()
    {
      Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
      var target = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);
      if (GetOrbCount() == Config.Item("SpheresNumber").GetValue<Slider>().Value)
      {
        R.Cast(target);
      }
      if (Config.Item("SpheresNumber").GetValue<Slider>().Value == 4)
      {
        Q.Cast(Player.Position);
      }
      if (Config.Item("SpheresNumber").GetValue<Slider>().Value == 5)
      {
        if (Q.IsReady() && GetOrbCount() < 5)
        {
          Q.Cast(Player.Position);
        }
      }
      if (Config.Item("SpheresNumber").GetValue<Slider>().Value == 6)
      {
        if (Q.IsReady() && GetOrbCount() < 6)
        {
          Q.Cast(Player.Position);
          var sphere = GetGrabableObjectPos(true);
          if (sphere.To2D().IsValid() && Utils.TickCount - W.LastCastAttemptT > Game.Ping + 300)
          {
            W.Cast(sphere);
            W.LastCastAttemptT = Utils.TickCount;
          }
          else if (target != null && Player.Spellbook.GetSpell(SpellSlot.W).ToggleState != 1 && W.IsReady() &&
         Utils.TickCount - W.LastCastAttemptT > Game.Ping + 100)
          {
            if (OrbManager.WObject(false) != null)
            {
              W.From = OrbManager.WObject(false).ServerPosition;
              W.Cast(Player, false, true);
            }
          }
        }
      }
      if (Config.Item("SpheresNumber").GetValue<Slider>().Value == 7)
      {
        if (Q.IsReady() && GetOrbCount() < 5)
        {
          Q.Cast(Player.Position);
        }
        var sphere = GetGrabableObjectPos(true);
        if (sphere.To2D().IsValid() && Utils.TickCount - W.LastCastAttemptT > Game.Ping + 300)
        {
          W.Cast(sphere);
          W.LastCastAttemptT = Utils.TickCount;
        }
        else if (target != null && Player.Spellbook.GetSpell(SpellSlot.W).ToggleState != 1 && W.IsReady() &&
       Utils.TickCount - W.LastCastAttemptT > Game.Ping + 100)
        {
          if (OrbManager.WObject(false) != null)
          {
            W.From = OrbManager.WObject(false).ServerPosition;
            W.Cast(Player, false, true);
          }
        }
        if (Q.IsReady() && GetOrbCount() > 7)
        {
          Q.Cast(Player.Position);
        }
      }
    }
    private static void KS()
    {
      foreach (Obj_AI_Hero hero in ObjectManager.Get<Obj_AI_Hero>().Where(unit => unit.IsValidTarget(1300)))
      {
        var target = TargetSelector.GetTarget(1300, TargetSelector.DamageType.Magical);

        if (Config.Item("UseQKS").GetValue<bool>() && target.Health < Q.GetDamage(target))
        {
          Q.Cast(target);
        }
        if (Config.Item("UseWKS").GetValue<bool>() && target.Health < W.GetDamage(target))
        {
          W.Cast(target);
        }
        if (Config.Item("UseEKS").GetValue<bool>() && target.Health < E.GetDamage(target))
        {
          E.Cast(target);
        }
        if (Config.Item("UseQEKS").GetValue<bool>() && target.Health < E.GetDamage(target))
        {
          E.Cast(target);
        }

        if (Config.Item("UseRKS").GetValue<bool>() && target.Health < R.GetDamage(target) && target.Health > Q.GetDamage(target) || target.Health > W.GetDamage(target) || target.Health > E.GetDamage(target))
        {
          R.Cast(target);
        }
      }
    }

    private static void OnGameUpdate(EventArgs args)
    {
      KS();
      if (Config.Item("SpecialUltGO").GetValue<KeyBind>().Active)
      {
        SpecialUlt();
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
          Clear();
          break;
      }
    }
  }
}
