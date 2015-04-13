﻿﻿using System;
using System.Collections;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using System.Drawing;
using Color = System.Drawing.Color;
using System.Collections.Generic;
using System.Threading;

namespace GangPlank___Yarr_
{
  class Program
  {
     private const string Champion = "GangPlank";

    private static Orbwalking.Orbwalker Orbwalker;

    private static Spell Q, W, E, R;

    private static List<Spell> SpellList = new List<Spell>();

    private static Menu Config;

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
      Notifications.AddNotification("GangPlank - Yarr! by DanZ- Loaded", 1000);
      //if (ObjectManager.Player.BaseSkinName != Champion) return;
      Q = new Spell(SpellSlot.Q, 625);
      W = new Spell(SpellSlot.W);
      E = new Spell(SpellSlot.E, 1250);
      R = new Spell(SpellSlot.R);

      R.SetSkillshot(0.7f, 200, float.MaxValue, false, SkillshotType.SkillshotCircle);

      SpellList.Add(Q);
      SpellList.Add(W);
      SpellList.Add(E);
      SpellList.Add(R);

      Config = new Menu("Gangplank", "gp_menu", true);
      var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
      TargetSelector.AddToMenu(targetSelectorMenu);
      Config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
      Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));
      Config.AddSubMenu(targetSelectorMenu);
      Config.AddSubMenu(new Menu("Combo", "Combo"));
      Config.SubMenu("Combo").AddItem(new MenuItem("UseQCombo", "Use Q")).SetValue(true);
      Config.SubMenu("Combo").AddItem(new MenuItem("UseECombo", "Use E")).SetValue(true);
      Config.SubMenu("Combo").AddItem(new MenuItem("UseRCombo", "Use R on target")).SetValue(false);
      Config.AddSubMenu(new Menu("Farming", "Farming"));
      Config.SubMenu("Farming").AddItem(new MenuItem("QFarm", "Last hit with Q").SetValue(false));
      Config.AddSubMenu(new Menu("Jungle Clear", "JGClear"));
      Config.SubMenu("JGClear").AddItem(new MenuItem("QJGClear", "Use Q").SetValue(true));
      Config.SubMenu("JGClear").AddItem(new MenuItem("EJGClear", "Use E").SetValue(true));

      Config.AddSubMenu(new Menu("Harass", "Harass"));
      Config.SubMenu("Harass").AddItem(new MenuItem("UseQHarass", "Use Q")).SetValue(true);
      Config.AddSubMenu(new Menu("Misc Settings", "Misc"));
      Config.SubMenu("Misc").AddItem(new MenuItem("KSQ", "KS with Q")).SetValue(true);
      Config.SubMenu("Misc").AddItem(new MenuItem("KSR", "KS with R")).SetValue(true);
      Config.SubMenu("Misc").AddItem(new MenuItem("autoW", "Auto W if stunned")).SetValue(true);
      Config.SubMenu("Misc").AddItem(new MenuItem("wLow", "Auto W if low life")).SetValue(new Slider(45));



      Config.AddSubMenu(new Menu("Drawings", "Drawings"));
      Config.SubMenu("Drawings").AddItem(new MenuItem("drawEnable", "Enable Drawing")).SetValue(true);
      Config.SubMenu("Drawings").AddItem(new MenuItem("drawQ", "Draw Q")).SetValue(true);
      Config.SubMenu("Drawings").AddItem(new MenuItem("drawE", "Draw E")).SetValue(true);

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

          Render.Circle.DrawCircle(ObjectManager.Player.Position, Q.Range, Color.Aqua, 1);
        }



        if (Config.Item("drawE").GetValue<bool>())
        {

          Render.Circle.DrawCircle(ObjectManager.Player.Position, E.Range, Color.Aqua, 1);
        }

      }
    }


    private static void OnGameUpdate(EventArgs args)
    {
      if (Config.Item("QFarm").GetValue<bool>())
      {
        farmQ();
      }
      if (Config.Item("KSQ").GetValue<bool>())
      {
        KSQ();
      }
      if (Config.Item("KSR").GetValue<bool>())
      {
        KSR();
      }
      if (Config.Item("autoW").GetValue<bool>())
      {
        if (!W.IsReady()) return;
        if (Player.HasBuffOfType(BuffType.Stun) || Player.HasBuffOfType(BuffType.Snare) || Player.HasBuffOfType(BuffType.Slow)) W.Cast();

        if (Config.Item("wLow").GetValue<Slider>().Value >= Player.Health / Player.MaxHealth * 100 && W.IsReady()) W.Cast();
      }
      switch (Orbwalker.ActiveMode)
      {
        case Orbwalking.OrbwalkingMode.Combo:
          Combo();

          break;
      }
    }

    //Credits to DanThePman
    private static void farmQ()
    {
      foreach (var minion in MinionManager.GetMinions(625f, MinionTypes.All, MinionTeam.Enemy).
                           Where(x => Q.GetDamage(x) >= x.Health))
      {
        if (Q.CanCast(minion))
          Q.CastOnUnit(minion, true);
      }
    }

    private static void Combo()
    {
      var target = (TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical) ??
                     TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical)) ?? TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.True);                    

      if (target == null)
      {
        return;
        }

      if (Q.IsReady() && Config.Item("UseQCombo").GetValue<bool>())
      {

          Q.CastOnUnit(target);

        }
      
      if (E.IsReady() && Config.Item("UseECombo").GetValue<bool>())
      {
        E.Cast();
      }
      if (R.IsReady() && Config.Item("UseRCombo").GetValue<bool>())
      {
        R.Cast(target);
      }
    }
    private static void Harass()
    {
      var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
      if (target == null) return;

      if (Q.IsReady() && Config.Item("UseQCombo").GetValue<bool>() && target.IsValidTarget(Q.Range))
      {
        Q.CastOnUnit(target);
      }
    }
    private static void jgClear()
    {
      var mobs = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, E.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
      if (mobs.Count > 0)
      {
        var mob = mobs[0];
        if (Q.IsReady() && Config.Item("QJGClear").GetValue<bool>())
        {
          Q.CastOnUnit(mob);
        }

        if (W.IsReady() && Config.Item("EJGClear").GetValue<bool>())
        {
          W.Cast();
        }
      }
    }

    private static float GetQDamage(Obj_AI_Base enemy)
    {
      double damage = 0d;

      if (Q.IsReady()) damage += Player.GetSpellDamage(enemy, SpellSlot.Q);

      return (float)damage * 2;
    }
    private static float GetRDamage(Obj_AI_Base enemy)
    {
      double damage = 0d;

      if (R.IsReady()) damage += Player.GetSpellDamage(enemy, SpellSlot.R);

      return (float)damage * 2;
    }

    private static void KSQ() {
			foreach(Obj_AI_Hero hero in ObjectManager.Get < Obj_AI_Hero > ().Where(unit => unit.IsValidTarget(Q.Range))) {
				var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
				if (target == null) return;
				if (Q.IsReady()) {

					if (target.Health < GetQDamage(target)) {

						Q.CastOnUnit(target);


					}
				}
			}
		}
    private static void KSR()
    {
      var target = TargetSelector.GetTarget(9000, TargetSelector.DamageType.Magical);
      if (target == null) return;
      if (R.IsReady()) if (target.Health < GetRDamage(target))
        {
          R.Cast(target);
        }




    }

  }
}