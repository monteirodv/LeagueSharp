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

namespace Nautilus_AnchorTheChallenger
{

  class program
  {

    private const string Champion = "Nautilus";

    private static Orbwalking.Orbwalker Orbwalker;

    private static Spell Q;

    private static Spell W;

    private static Spell E;

    private static Spell R;

    private static List<Spell> SpellList = new List<Spell>();

    private static Menu Config;

    private static Items.Item RDO;

    public static Obj_AI_Hero Player { get { return ObjectManager.Player; } }


    static void Main(string[] args)
    {
      CustomEvents.Game.OnGameLoad += Game_OnGameLoad;

    }


    static void Game_OnGameLoad(EventArgs args)
    {
      Game.PrintChat("v3");

      if (ObjectManager.Player.BaseSkinName != Champion) return;

      Q = new Spell(SpellSlot.Q, 1100);
      W = new Spell(SpellSlot.W);
      E = new Spell(SpellSlot.E, 600);
      R = new Spell(SpellSlot.R, 825);

      Q.SetSkillshot(1100f, 90, 2000, true, SkillshotType.SkillshotLine);


      SpellList.Add(Q);
      SpellList.Add(W);
      SpellList.Add(E);
      SpellList.Add(R);

      RDO = new Items.Item(3143, 490f);


      Config = new Menu("Nautilus", "naut_menu", true);

      var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
      TargetSelector.AddToMenu(targetSelectorMenu);
      Config.AddSubMenu(targetSelectorMenu);

      Config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
      Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));

      Config.AddSubMenu(new Menu("Combo", "Combo")); 
      Config.SubMenu("Combo").AddItem(new MenuItem("UseQCombo", "Use Q")).SetValue(true); //Adding an item to the submenu (toggle)
      Config.SubMenu("Combo").AddItem(new MenuItem("UseWCombo", "Use W")).SetValue(true); //Adding an item to the submenu (toggle)
      Config.SubMenu("Combo").AddItem(new MenuItem("UseECombo", "Use E")).SetValue(true);  //Adding an item to the submenu (toggle)
      Config.SubMenu("Combo").AddItem(new MenuItem("UseRCombo", "Use R")).SetValue(true);  //Adding an item to the submenu (toggle)
      Config.SubMenu("Combo").AddItem(new MenuItem("UseItems", "Use Items")).SetValue(true);  //Adding an item to the submenu (toggle)
      Config.SubMenu("Combo").AddItem(new MenuItem("KSQ", "KS with Q")).SetValue(true);
      Config.SubMenu("Combo").AddItem(new MenuItem("ActiveCombo", "Combo!").SetValue(new KeyBind(32, KeyBindType.Press)));  //Adding an item to the submenu (on key down (hotkey))

      Config.AddSubMenu(new Menu("Drawings", "Drawings"));
      Config.SubMenu("Drawings").AddItem(new MenuItem("drawEnable", "Enable Drawing")).SetValue(true);
      Config.SubMenu("Drawings").AddItem(new MenuItem("drawQ", "Draw Q")).SetValue(true);
      Config.SubMenu("Drawings").AddItem(new MenuItem("drawE", "Draw E")).SetValue(true);
      

      Config.AddToMainMenu();

      Game.OnGameUpdate += OnGameUpdate;
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




      if (Config.Item("ActiveCombo").GetValue<KeyBind>().Active)
      {
        Combo();
      }

      if (Config.Item("KSQ").GetValue<bool>())
      {
        KSQ();
      }


    }

    private static void Combo()
    {


      var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
      if (target == null) return;

      //Combo
      if (Q.IsReady() && (Config.Item("UseQCombo").GetValue<bool>()))
      {
        var prediction = Q.GetPrediction(target);

        if (prediction.Hitchance >= HitChance.High && prediction.CollisionObjects.Count(h => h.IsEnemy && !h.IsDead && h is Obj_AI_Minion) < 2)
        {
          Q.Cast(prediction.CastPosition);

        }
      }
      if (target.IsValidTarget(E.Range) && E.IsReady() && (Config.Item("UseECombo").GetValue<bool>()))
      {
        E.Cast(target, true, true);
      }






      if (Config.Item("UseItems").GetValue<bool>())
      {
        if (Player.Distance3D(target) <= RDO.Range)
        {
          RDO.Cast(target);
        }

      }



    }


    private static void KSQ()
    {
      var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
      if (target == null) return;

      var prediction = Q.GetPrediction(target);

      if (Q.IsReady())
      {

        if (target.Health < GetQDamage(target))
        {
          if (prediction.Hitchance >= HitChance.High && prediction.CollisionObjects.Count(h => h.IsEnemy && !h.IsDead && h is Obj_AI_Minion) < 1)
          {
            Q.Cast(prediction.CastPosition);
          }


        }
      }
    }


    private static float GetQDamage(Obj_AI_Base enemy)
    {
      double damage = 0d;

      if (Q.IsReady())
        damage += Player.GetSpellDamage(enemy, SpellSlot.W);

      return (float)damage * 2; //return damage of W back to the function of KS W
    }


    public static bool lagcircle { get; set; }
  }
}