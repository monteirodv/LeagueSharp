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
using Collision = LeagueSharp.Common.Collision;


namespace Nautilus_AnchorTheChallengers
{

  class program
  {

     private const string Champion = "Nautilus";

    private static Orbwalking.Orbwalker Orbwalker;

    private static Spell Q;

    private static Spell W;

    private static Spell E;

    private static Spell R;

    public static SpellSlot smiteSlot = SpellSlot.Unknown;
    public static Spell smite;

    // Kurisu
    private static readonly int[] SmitePurple = { 3713, 3726, 3725, 3726, 3723 };
    private static readonly int[] SmiteGrey = { 3711, 3722, 3721, 3720, 3719 };
    private static readonly int[] SmiteRed = { 3715, 3718, 3717, 3716, 3714 };
    private static readonly int[] SmiteBlue = { 3706, 3710, 3709, 3708, 3707 };

    private static List<Spell> SpellList = new List<Spell>();

    private static Menu Config;

    private static Items.Item RDO;

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

      Game.PrintChat("<font color=\"#FF0000\">Nautilus - Anchor the Challenger by DanZ</font> - <font color=\"#0000FF\">Loaded</font>");
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

      var SmiteSlot =  ObjectManager.Player.GetSpellSlot("summonerdot");


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
      Config.SubMenu("Combo").AddItem(new MenuItem("UseECombo", "Use E")).SetValue(true); //Adding an item to the submenu (toggle)
      Config.SubMenu("Combo").AddItem(new MenuItem("UseRCombo", "Use R")).SetValue(true); //Adding an item to the submenu (toggle)
      Config.SubMenu("Combo").AddItem(new MenuItem("UseItems", "Use Items")).SetValue(true); //Adding an item to the submenu (toggle)
      Config.SubMenu("Combo").AddItem(new MenuItem("KSQ", "KS with Q")).SetValue(true);
      Config.SubMenu("Combo").AddItem(new MenuItem("ActiveCombo", "Combo!").SetValue(new KeyBind(32, KeyBindType.Press))); //Adding an item to the submenu (on key down (hotkey))
      
      Config.AddSubMenu(new Menu("Jungle Clear", "JGClear"));
      Config.SubMenu("JGClear").AddItem(new MenuItem("WJGClear", "Use W").SetValue(true));
      Config.SubMenu("JGClear").AddItem(new MenuItem("EJGClear", "Use W").SetValue(true));
    //  Config.SubMenu("JGClear").AddItem(new MenuItem("AutoSmite", "AutoSmite").SetValue<KeyBind>(new KeyBind('G', KeyBindType.Toggle)));                

      Config.AddSubMenu(new Menu("Mis Settings", "Misc"));
      Config.SubMenu("Misc").AddItem(new MenuItem("InterruptSpells", "Interrupt Spells with Q").SetValue(true));
      Config.SubMenu("Misc").AddItem(new MenuItem("WGapCloser", "Auto use W on Gap Closers").SetValue(true));
      Config.SubMenu("Misc").AddItem(new MenuItem("EGapCloser", "Auto use E on Gap Closers").SetValue(true));



      Config.AddSubMenu(new Menu("Drawings", "Drawings"));
      Config.SubMenu("Drawings").AddItem(new MenuItem("drawEnable", "Enable Drawing")).SetValue(true);
      Config.SubMenu("Drawings").AddItem(new MenuItem("drawQ", "Draw Q")).SetValue(true);
      Config.SubMenu("Drawings").AddItem(new MenuItem("drawE", "Draw E")).SetValue(true);
      Config.SubMenu("Drawings").AddItem(new MenuItem("drawR", "Draw R")).SetValue(true);
      Config.SubMenu("Drawings").AddItem(new MenuItem("drawCombodmg", "Draw Combo Damage on HPBar")).SetValue(true);


      Config.AddToMainMenu();

      Game.OnGameUpdate += OnGameUpdate;
      Drawing.OnDraw += OnDraw;
      AntiGapcloser.OnEnemyGapcloser += WEOnEnemyGapcloser;
      Interrupter.OnPossibleToInterrupt += Interrupter_OnPossibleToInterrupt;

      


    }



    private static void JungleClear()
    {
      var mobs = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, E.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
      if (mobs.Count > 0)
      {
        var mob = mobs[0];

        if (W.IsReady() && Config.Item("WJGClear").GetValue<bool>())
        {
          W.Cast();
        }

        if (E.IsReady() && Config.Item("EJGClear").GetValue<bool>())
        {
          E.Cast();
        }
      }
    }


            private static int GetSmiteDmg()
        {
            int level = Player.Level;
            int index = Player.Level/5;
            float[] dmgs = {370 + 20*level, 330 + 30*level, 240 + 40*level, 100 + 50*level};
            return (int) dmgs[index];
        }

      
    

    private static void Interrupter_OnPossibleToInterrupt(Obj_AI_Hero unit, InterruptableSpell spell)
    {
      if (Config.Item("InterruptSpells").GetValue<bool>())
      {
        if (Q.IsInRange(unit) && Q.IsReady() && unit.IsEnemy)
        {
          Q.CastOnUnit(unit);
        }
      }
    }

    private static void WEOnEnemyGapcloser(ActiveGapcloser gapcloser)
    {
      if (Config.Item("WGapCloser").GetValue<bool>())
      {
        W.Cast();
      }
      if (Config.Item("EGapCloser").GetValue<bool>())
      {
        E.Cast();
      }
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
        if (Config.Item("drawR").GetValue<bool>())
        {

          Render.Circle.DrawCircle(ObjectManager.Player.Position, R.Range, Color.Aqua, 1);
        }


      }
    }



    private static void OnGameUpdate(EventArgs args)
    {

      switch (Orbwalker.ActiveMode)
      {
        case Orbwalking.OrbwalkingMode.Combo:
          Combo();
          break;
        case Orbwalking.OrbwalkingMode.Mixed:
          JungleClear();
          break;
        case Orbwalking.OrbwalkingMode.LaneClear:
          JungleClear();
          break;

        default:
          break;






          if (Config.Item("KSQ").GetValue<bool>())
          {
            KSQ();
          }
      }
    }

    


    private static void Combo() {


			var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
			if (target == null) return;

			//Combo
      if (Q.IsReady() && (Config.Item("UseQCombo").GetValue<bool>()))
      {
        var Qprediction = Q.GetPrediction(target);



        {

          if (Qprediction.Hitchance >= HitChance.High && Qprediction.CollisionObjects.Count(h => h.IsEnemy && !h.IsDead && h is Obj_AI_Minion) < 2)
          {
            Q.Cast(Qprediction.CastPosition);

          }
        }
      }

        if (target.IsValidTarget(E.Range) && E.IsReady() && (Config.Item("UseECombo").GetValue<bool>()))
        {
          E.Cast(target, true, true);
        }


      



			if (Config.Item("UseItems").GetValue < bool > ()) {
				if (Player.Distance3D(target) <= RDO.Range) {
					RDO.Cast(target);
				}

			}



		}


    private static void KSQ() {
			var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
			if (target == null) return;

			var prediction = Q.GetPrediction(target);

			if (Q.IsReady()) {

				if (target.Health < GetQDamage(target)) {
					if (prediction.Hitchance >= HitChance.High && prediction.CollisionObjects.Count(h => h.IsEnemy && !h.IsDead && h is Obj_AI_Minion) < 1) {
						Q.Cast(prediction.CastPosition);
					}


				}
			}
		}


    private static float GetQDamage(Obj_AI_Base enemy)
    {
      double damage = 0d;

      if (Q.IsReady()) damage += Player.GetSpellDamage(enemy, SpellSlot.W);

      return (float)damage * 2; //return damage of W back to the function of KS W
    }


    public static bool lagcircle
    {
      get;
      set;
    }

    public static int i { get; set; }

    public static Vector3 botTurret { get; set; }
  }
}
