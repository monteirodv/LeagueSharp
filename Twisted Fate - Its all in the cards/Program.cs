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

namespace Twisted_Fate___Its_all_in_the_cards
{
  class Program
  {
    private const string Champion = "TwistedFate";
    private static Spell Q, W, R;
    public static int CardTick;
    private static Menu Config;
    public static Obj_AI_Hero Player

    {
      get
      {
        return ObjectManager.Player;
      }
    }
    private static Orbwalking.Orbwalker Orbwalker;


    static void Main(string[] args)
    {
      CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
    }

    static void Game_OnGameLoad(EventArgs args)
    {
      //if (ObjectManager.Player.BaseSkinName != Champion) return;
      Notifications.AddNotification("Twisted Fate by DanZ - Loaded", 1000);
      Q = new Spell(SpellSlot.Q, 1410);
      W = new Spell(SpellSlot.W, Player.AttackRange);
      R = new Spell(SpellSlot.R, 5500);

			Q.SetSkillshot(0.25f, 40f, 1000f, false, SkillshotType.SkillshotLine);
      W.SetSkillshot(0.3f, 80f, 1600, true, SkillshotType.SkillshotLine);
      Config = new Menu("Twisted Fate", "tf_menu", true);
      var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
      TargetSelector.AddToMenu(targetSelectorMenu);
      Config.AddSubMenu(targetSelectorMenu);
      Config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
      Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));
      Config.AddSubMenu(new Menu("Combo", "Combo"));
      Config.SubMenu("Combo").AddItem(new MenuItem("UseQCombo", "Use Q")).SetValue(true);
      Config.SubMenu("Combo").AddItem(new MenuItem("UseWComboGold", "Use W (Gold Card)")).SetValue(true);
      Config.SubMenu("Combo").AddItem(new MenuItem("UseWComboBlue", "Use W (Blue Card)")).SetValue(false);
      Config.SubMenu("Combo").AddItem(new MenuItem("UseWComboRed", "Use W (Red Card)")).SetValue(false);
      Config.AddSubMenu(new Menu("Lane Clear", "LaneClear"));
      Config.SubMenu("LaneClear").AddItem(new MenuItem("QLaneClear", "Use Q").SetValue(true));
      Config.SubMenu("LaneClear").AddItem(new MenuItem("WLaneClear", "Use W(Smart Choose(Red / Blue))").SetValue(true));    
      Config.AddSubMenu(new Menu("Card Picking", "pickCard"));
      Config.SubMenu("pickCard").AddItem(new MenuItem("redcard", "Red Card").SetValue(new KeyBind("U".ToCharArray()[0], KeyBindType.Press)));
      Config.SubMenu("pickCard").AddItem(new MenuItem("yellowcard", "Yellow Card").SetValue(new KeyBind("I".ToCharArray()[0], KeyBindType.Press)));
      Config.SubMenu("pickCard").AddItem(new MenuItem("bluecard", "Blue Card").SetValue(new KeyBind("O".ToCharArray()[0], KeyBindType.Press))); 
      Config.AddSubMenu(new Menu("Harass", "Harass"));
      Config.SubMenu("Harass").AddItem(new MenuItem("UseQHarass", "Use Q")).SetValue(true);
      Config.SubMenu("Harass").AddItem(new MenuItem("UseWHarass", "Use Blue Card")).SetValue(false);
      Config.AddSubMenu(new Menu("Interrupt Spells", "InterruptSpells"));
      Config.SubMenu("InterruptSpells").AddItem(new MenuItem("YellowInterrupt", "Use Gold Card")).SetValue(true);
      Config.AddSubMenu(new Menu("Anti GapCloser", "AntiGapCloser"));
      Config.SubMenu("AntiGapCloser").AddItem(new MenuItem("YellowGapCloser", "Use Gold Card ")).SetValue(true);
      Config.AddSubMenu(new Menu("Misc Settings", "Misc"));
      Config.SubMenu("Misc").AddItem(new MenuItem("KSQ", "KS with Q")).SetValue(true);
      Config.AddSubMenu(new Menu("Drawings", "Drawings"));
      Config.SubMenu("Drawings").AddItem(new MenuItem("drawEnable", "Enable Drawing")).SetValue(true);
      Config.SubMenu("Drawings").AddItem(new MenuItem("drawQ", "Draw Q")).SetValue(true);
      Config.SubMenu("Drawings").AddItem(new MenuItem("drawR", "Draw R on minimap(Not implemented)")).SetValue(false);

      Config.AddToMainMenu();

      Game.OnUpdate += OnGameUpdate;
      Drawing.OnDraw += OnDraw;
    }
    private static void OnDraw(EventArgs args)
    {
      if (Config.Item("drawEnable").GetValue<bool>())
      {
        if (Config.Item("drawQ").GetValue<bool>())
        {

          Render.Circle.DrawCircle(Player.Position, Q.Range, Color.Aqua, 1);
        }


      }
    }
    public static void PickBlue()
    {
      if (!W.IsReady())
      {
        return;
      }
      if (Environment.TickCount - CardTick < 250)
      {
        return;
      }
      CardTick = Environment.TickCount;
            if (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).Name == "PickACard")
            {
                W.Cast();
            }
            if (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).Name == "bluecardlock")
            {
                W.Cast();
            }
        }        
    
    private static void PickRed()
    {
      if (!W.IsReady())
      {
        return;
      }
      if (Environment.TickCount - CardTick < 250)
      {
        return;
      }
      CardTick = Environment.TickCount;
      if (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).Name == "PickACard")
      {
        W.Cast();
      }
      if (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).Name == "redcardlock")
      {
        W.Cast();
      }
    }
    private static void PickYellow()
    {
      if (!W.IsReady())
      {
        return;
      }
      if (Environment.TickCount - CardTick < 250)
      {
        return;
      }
      CardTick = Environment.TickCount;
      if (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).Name == "PickACard")
      {
        W.Cast();
      }
      if (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).Name == "goldcardlock")
      {
        W.Cast();
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
      if (W.IsReady() && (Config.Item("UseWComboGold").GetValue<bool>()))
      {
        PickYellow();
        Orbwalker.ForceTarget(target);
        Orbwalker.SetAttack(true);
      }
      if (W.IsReady() && (Config.Item("UseWComboBlue").GetValue<bool>()))
      {
        PickBlue();
        Orbwalker.ForceTarget(target);
        Orbwalker.SetAttack(true);
      }
      if (W.IsReady() && (Config.Item("UseWComboRed").GetValue<bool>()))
      {
        PickRed();
        Orbwalker.ForceTarget(target);
        Orbwalker.SetAttack(true);
      }
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
        if (Player.Distance(target.ServerPosition) < Player.AttackRange - 25)
        {
          PickBlue();
        }

      }
    }
    private static void LaneClear()
    {
      var minions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.NotAlly);
      bool jungleMobs = minions.Any(x => x.Team == GameObjectTeam.Neutral);
      if ((Config.Item("QLaneClear").GetValue<bool>() || jungleMobs))
      {
        MinionManager.FarmLocation farmLocation = Q.GetLineFarmLocation(minions);
        if (farmLocation.Position.IsValid())
          if (farmLocation.MinionsHit >= 2 || jungleMobs)
            Q.Cast(farmLocation.Position);
      }
      if (Config.Item("WLaneClear").GetValue<bool>())
      {
        if (Player.Mana / Player.MaxMana * 100 <= 35)
        {
          PickBlue();
        }
        else
        {
          PickRed();
        }
      }
      
    }

    private static void KS()
    {
      foreach (Obj_AI_Hero hero in ObjectManager.Get<Obj_AI_Hero>().Where(unit => unit.IsValidTarget(Q.Range)))
      {
        var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
        if (target == null) return;
        if (Q.IsReady())
        {

          if (target.Health < Q.GetDamage(target))
          {
            var Qprediction = Q.GetPrediction(target);

            if (Qprediction.Hitchance >= HitChance.VeryHigh)
            {
              Q.Cast(Qprediction.CastPosition);
            }
          }
        }
      }
    }
    private static void OnStunedQ()
    {
      var NearEnemies = Player.GetEnemiesInRange(Q.Range)
                         .Where(x => !x.IsAlly)
                         .Where(x => !x.IsDead)
                         .Where(x => x.Distance(Player.Position) <= Q.Range)
                         .Where(x => x.IsStunned)
                         .FirstOrDefault();

      Q.Cast(NearEnemies.ServerPosition);
    }
    private static void OnStunedW()
    {
      var NearEnemies = Player.GetEnemiesInRange(Q.Range)
                         .Where(x => !x.IsAlly)
                         .Where(x => !x.IsDead)
                         .Where(x => x.Distance(Player.Position) <= Q.Range)
                         .Where(x => x.IsStunned)
                         .FirstOrDefault();

        PickYellow();
      }
 
    private static void OnPossibleToInterrupt(Obj_AI_Hero target, Interrupter2.InterruptableTargetEventArgs args)
    {
      if (Config.Item("YellowInterrupt").GetValue<bool>() && W.IsReady() && W.IsInRange(target))
      {
        PickYellow();
      }
    }
    private static void OnEnemyGapcloser(ActiveGapcloser gapcloser)
    {
      if (gapcloser.Sender.IsAlly)
      {
        return;
      }
      if (Config.Item("YellowGapCloser").GetValue<bool>() && W.IsReady() && W.IsInRange(gapcloser.End))
      {
        PickYellow();
      }
    }

    private static void OnGameUpdate(EventArgs args)
    {

      if (Config.Item("KSQ").GetValue<bool>())
      {
        KS();
      }
      if (Config.Item("redcard").GetValue<KeyBind>().Active)
      {
        PickRed();
      }
      if (Config.Item("yellowcard").GetValue<KeyBind>().Active)
      {
        PickYellow();
      }
      if (Config.Item("bluecard").GetValue<KeyBind>().Active)
      {
        PickBlue();
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
     
      }
    }

  }

