﻿using System;
using System.Collections;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using System.Drawing;
using Color = System.Drawing.Color;
using System.Collections.Generic;
using System.Threading;

namespace Zac_The_Secret_Flubber
{
  class Program
  {

    private const string Champion = "Zac";

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

    private static float GetComboDamage(Obj_AI_Base Target)
    {
      var ComboDamage = 0d;

      if (Q.IsReady())
        ComboDamage += Player.GetSpellDamage(Target, SpellSlot.Q);


      if (E.IsReady())
        ComboDamage += Player.GetSpellDamage(Target, SpellSlot.E);

      if (R.IsReady())
        ComboDamage += Player.GetSpellDamage(Target, SpellSlot.R);


      return (float)ComboDamage;
    }

    static void Main(string[] args)
    {
      CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
    }

    static void Game_OnGameLoad(EventArgs args)
    {
          Notifications.AddNotification("Zac - The Secret Flubber by DanZ and Drunkenninja", 1000);
      if (ObjectManager.Player.BaseSkinName != Champion) return;

      Q = new Spell(SpellSlot.Q, 550);
      W = new Spell(SpellSlot.W, 350);
      E = new Spell(SpellSlot.E, 1550);
      R = new Spell(SpellSlot.R);

      Q.SetSkillshot(550, 120, int.MaxValue, false, SkillshotType.SkillshotLine);
      E.SetSkillshot(1550, 250, 1500, true, SkillshotType.SkillshotCone);
      E.SetCharged("ZacE", "ZacE", 1150, 1550, 1.5f);

      SpellList.Add(Q);
      SpellList.Add(W);
      SpellList.Add(E);
      SpellList.Add(R);

      Config = new Menu("Zac", "Zac_menu", true);

      var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
      TargetSelector.AddToMenu(targetSelectorMenu);
      Config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
      Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));
      Config.AddSubMenu(targetSelectorMenu);
      Config.AddSubMenu(new Menu("Combo", "Combo"));
      Config.SubMenu("Combo").AddItem(new MenuItem("UseQCombo", "Use Q")).SetValue(true);
      Config.SubMenu("Combo").AddItem(new MenuItem("UseWCombo", "Use W")).SetValue(true);
      Config.SubMenu("Combo").AddItem(new MenuItem("UseRCombo", "Use R")).SetValue(true);
      Config.AddSubMenu(new Menu("Jungle Clear", "JGClear"));
      Config.SubMenu("JGClear").AddItem(new MenuItem("QJGClear", "Use Q").SetValue(true));
      Config.SubMenu("JGClear").AddItem(new MenuItem("WJGClear", "Use W").SetValue(true));
      Config.AddSubMenu(new Menu("Harass", "Harass"));
      Config.SubMenu("Harass").AddItem(new MenuItem("UseQHarass", "Use Q")).SetValue(true);
      Config.SubMenu("Harass").AddItem(new MenuItem("UseWHarass", "Use W")).SetValue(true);
      Config.AddSubMenu(new Menu("Mis Settings", "Misc"));
      Config.SubMenu("Misc").AddItem(new MenuItem("KSQ", "KS with Q")).SetValue(true);
      Config.AddSubMenu(new Menu("Drawings", "Drawings"));
      Config.SubMenu("Drawings").AddItem(new MenuItem("drawEnable", "Enable Drawing")).SetValue(true);
      Config.SubMenu("Drawings").AddItem(new MenuItem("drawQ", "Draw Q")).SetValue(true);
      Config.SubMenu("Drawings").AddItem(new MenuItem("drawW", "Draw W")).SetValue(true);
      Config.SubMenu("Drawings").AddItem(new MenuItem("drawE", "Draw E")).SetValue(true);
      Config.SubMenu("Drawings").AddItem(new MenuItem("drawR", "Draw R")).SetValue(true);

            Config.AddToMainMenu();

Game.OnUpdate += OnGameUpdate;
      Drawing.OnDraw += OnDraw;

    }

    private static void Combo()
    {
      var target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);
      if (target == null) return;

      var comboDamage = GetComboDamage(target);
      var useQ = Config.Item("UseQCombo").GetValue<bool>();

      if (Q.IsReady() && Config.Item("UseQCombo").GetValue<bool>() && target.IsValidTarget(Q.Range))
      {
        Q.Cast(target);
      }
      if (W.IsReady() && Config.Item("UseWCombo").GetValue<bool>() && target.IsValidTarget(W.Range))
      {
        W.Cast();
      }
 if (R.IsReady() && Config.Item("UseRCombo").GetValue<bool>())
            if (Q.IsReady())
            {
              return;
            }
            else
            {
              R.Cast();
            }
        }

      
    



    private static void Harass()
    {
      var target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);
      if (target == null) return;

      var comboDamage = GetComboDamage(target);
      var useQ = Config.Item("UseQHarass").GetValue<bool>();

      if (Q.IsReady() && Config.Item("UseQHarass").GetValue<bool>() && target.IsValidTarget(Q.Range))
      {
        Q.Cast(target);
      }
      if (W.IsReady() && Config.Item("UseWHarass").GetValue<bool>() && target.IsValidTarget(W.Range))
      {
        W.Cast(target);
      }

      


    }
    private static void KSQ()
    {
      foreach (Obj_AI_Hero hero in ObjectManager.Get<Obj_AI_Hero>().Where(unit => unit.IsValidTarget(Q.Range)))
      {
        var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
        if (target == null) return;

        var prediction = Q.GetPrediction(target);

        if (Q.IsReady())
        {

          if (target.Health < GetQDamage(target))
          {
            
              Q.Cast(prediction.CastPosition);
            

          }
        }
      }
    }
    private static float GetQDamage(Obj_AI_Base enemy)
    {
      double damage = 0d;

      if (Q.IsReady()) damage += Player.GetSpellDamage(enemy, SpellSlot.Q);

      return (float)damage * 2; 
    }
    private static void JungleClear()
    {
      var mobs = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, E.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
      if (mobs.Count > 0)
      {
        var mob = mobs[0];
        if (Q.IsReady() && Config.Item("WJGClear").GetValue<bool>())
        {
          Q.Cast(mob);
        }

        if (W.IsReady() && Config.Item("WJGClear").GetValue<bool>())
        {
          W.Cast();
        }

        
      }
    }

   // private static void LaneClear()
    





    private static void OnDraw(EventArgs args)
    {
      if (Config.Item("drawEnable").GetValue<bool>())
      {
        if (Config.Item("drawQ").GetValue<bool>())
        {

          Render.Circle.DrawCircle(ObjectManager.Player.Position, Q.Range, Color.Aqua, 1);
        }

        if (Config.Item("drawW").GetValue<bool>())
        {

          Render.Circle.DrawCircle(ObjectManager.Player.Position, Q.Range, Color.Aqua, 1);
        }

        if (Config.Item("drawE").GetValue<bool>())
        {

          Render.Circle.DrawCircle(ObjectManager.Player.Position, E.Range, Color.Aqua, 1);
        }
        if (Config.Item("drawR").GetValue<bool>())
        {

          Render.Circle.DrawCircle(ObjectManager.Player.Position, R.Range, Color.Aqua, 1);
        }
      }
    }

private static void OnGameUpdate(EventArgs args)
    {

      if (E.IsCharging)
      {
        Orbwalker.SetMovement(false);
        
      }

      switch (Orbwalker.ActiveMode)
      {
        case Orbwalking.OrbwalkingMode.Combo:
          Combo();

          break;
        case Orbwalking.OrbwalkingMode.LaneClear:
          JungleClear();
          
          break;
        case Orbwalking.OrbwalkingMode.Mixed:
          Harass();

          break;


      }

      }
    }



    

     

  }

