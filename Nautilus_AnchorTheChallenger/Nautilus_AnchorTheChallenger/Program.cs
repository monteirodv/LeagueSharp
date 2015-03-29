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


    private static SpellSlot FlashSlot = SpellSlot.Unknown;

    public static float FlashRange = 450f;
    private static SpellSlot smiteSlot;

    private static bool checkSmite = false;

    public static Obj_AI_Base minion;

    private static readonly string[] epics =
        {
            "SRU_Baron", "SRU_Dragon"
        };
    private static readonly string[] buffs =
        {
            "SRU_Red", "SRU_Blue"
        };
    private static readonly string[] buffandepics =
        {
            "SRU_Red", "SRU_Blue", "SRU_Dragon", "SRU_Baron"
        };

     

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
Notifications.AddNotification("Nautilus- Anchor the Challenger by Danz - Loaded", 1000);

      if (ObjectManager.Player.BaseSkinName != Champion) return;


      FlashSlot = Player.GetSpellSlot("SummonerFlash");
      Q = new Spell(SpellSlot.Q, 1100);
      W = new Spell(SpellSlot.W);
      E = new Spell(SpellSlot.E, 300);
      R = new Spell(SpellSlot.R, 1500);

      
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
      Config.SubMenu("Combo").AddItem(new MenuItem("UseECombo", "Use E")).SetValue(true); //Adding an item to the submenu (toggle)
      Config.SubMenu("Combo").AddItem(new MenuItem("UseRCombo", "Use R")).SetValue(true); //Adding an item to the submenu (toggle)
      Config.SubMenu("Combo").AddItem(new MenuItem("UseItems", "Use Items")).SetValue(true); //Adding an item to the submenu (toggle)
      Config.SubMenu("Combo").AddItem(new MenuItem("KSQ", "KS with Q")).SetValue(true);
      Config.SubMenu("Combo").AddItem(new MenuItem("ActiveCombo", "Combo!").SetValue(new KeyBind(32, KeyBindType.Press))); //Adding an item to the submenu (on key down (hotkey))
      Config.SubMenu("Combo").AddItem(new MenuItem("FlashQCombo", "Flash + Q Combo").SetValue(new KeyBind("G".ToCharArray()[0], KeyBindType.Press)));
      Config.AddSubMenu(new Menu("Jungle Clear", "JGClear"));
      Config.SubMenu("JGClear").AddItem(new MenuItem("WJGClear", "Use W").SetValue(true));
      Config.SubMenu("JGClear").AddItem(new MenuItem("EJGClear", "Use W").SetValue(true));

      Config.AddSubMenu(new Menu("Mis Settings", "Misc"));
      Config.SubMenu("Misc").AddItem(new MenuItem("InterruptSpells", "Interrupt Spells with Q").SetValue(true));
      Config.SubMenu("Misc").AddItem(new MenuItem("WGapCloser", "Auto use W on Gap Closers").SetValue(true));
      Config.SubMenu("Misc").AddItem(new MenuItem("EGapCloser", "Auto use E on Gap Closers").SetValue(true));
      //ElRengar.SmiteSettinsg
            Config.SubMenu("Smite Settings").AddItem(new MenuItem("smiteEnabled", "Auto smite enabled").SetValue(new KeyBind("M".ToCharArray()[0], KeyBindType.Toggle)));
            Config.SubMenu("Smite Settings").AddItem(new MenuItem("422442fsaafsf", ""));
            Config.SubMenu("Smite Settings").AddItem(new MenuItem("Selected Smite Targets", "Selected Smite Targets:"));

            Config.SubMenu("Smite Settings").AddItem(new MenuItem("SRU_Red", "Red Buff").SetValue(true));
            Config.SubMenu("Smite Settings").AddItem(new MenuItem("SRU_Blue", "Blue Buff").SetValue(true));
            Config.SubMenu("Smite Settings").AddItem(new MenuItem("SRU_Dragon", "Dragon").SetValue(true));
            Config.SubMenu("Smite Settings").AddItem(new MenuItem("SRU_Baron", "Baron").SetValue(true));
            Config.SubMenu("Smite Settings").AddItem(new MenuItem("normalSmite", "Normal Smite").SetValue(true));


      Config.AddSubMenu(new Menu("Drawings", "Drawings"));
      Config.SubMenu("Drawings").AddItem(new MenuItem("drawEnable", "Enable Drawing")).SetValue(true);
      Config.SubMenu("Drawings").AddItem(new MenuItem("drawQ", "Draw Q")).SetValue(true);
      Config.SubMenu("Drawings").AddItem(new MenuItem("drawE", "Draw E")).SetValue(true);
      Config.SubMenu("Drawings").AddItem(new MenuItem("drawR", "Draw R")).SetValue(true);
      Config.SubMenu("Drawings").AddItem(new MenuItem("drawCombodmg", "Draw Combo Damage on HPBar")).SetValue(true);


      Config.AddToMainMenu();

      Game.OnUpdate += OnGameUpdate;
      Drawing.OnDraw += OnDraw;
      AntiGapcloser.OnEnemyGapcloser += WEOnEnemyGapcloser;
      Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;

      

      


    }



    private static void FlashQCombo()
    {

      ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
      var target = TargetSelector.GetTarget(Q.Range + FlashRange - 25, TargetSelector.DamageType.Magical);

        if (Player.Distance3D(target) > Q.Range)
      {
        if (FlashSlot != SpellSlot.Unknown && Player.Spellbook.CanUseSpell(FlashSlot) == SpellState.Ready)
        {
          if (Q.IsReady())
          {
            Player.Spellbook.CastSpell(FlashSlot, target.ServerPosition);
            Q.Cast(target.ServerPosition);
          }
          else
          {
            Q.StartCharging();
          }
        }
      }
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





    //    private static void Interrupter2_OnInterruptableTarget(Obj_AI_Hero unit, InterruptableSpell spell)

    private static void Interrupter2_OnInterruptableTarget(Obj_AI_Hero unit, Interrupter2.InterruptableTargetEventArgs spell)
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

    private static double SmiteDmg()
    {
      int[] dmg =
            {
                20*Player.Level + 370, 30*Player.Level + 330, 40*+Player.Level + 240, 50*Player.Level + 100
            };
      return Player.Spellbook.CanUseSpell(smiteSlot) == SpellState.Ready ? dmg.Max() : 0;
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


        //Credits to Kurisu

        private static readonly int[] SmitePurple = { 3713, 3726, 3725, 3726, 3723 };
        private static readonly int[] SmiteGrey = { 3711, 3722, 3721, 3720, 3719 };
        private static readonly int[] SmiteRed = { 3715, 3718, 3717, 3716, 3714 };
        private static readonly int[] SmiteBlue = { 3706, 3710, 3709, 3708, 3707 };
        private static string smitetype()
        {
          if (SmiteBlue.Any(a => Items.HasItem(a)))
          {
            return "s5_summonersmiteplayerganker";
          }
          if (SmiteRed.Any(a => Items.HasItem(a)))
          {
            return "s5_summonersmiteduel";
          }
          if (SmiteGrey.Any(a => Items.HasItem(a)))
          {
            return "s5_summonersmitequick";
          }
          if (SmitePurple.Any(a => Items.HasItem(a)))
          {
            return "itemsmiteaoe";
          }
          return "summonersmite";
        }

    private static void OnGameUpdate(EventArgs args)
    {
      smiteSlot = Player.GetSpellSlot(smitetype());

      switch (Orbwalker.ActiveMode)
      {
        case Orbwalking.OrbwalkingMode.Combo:
          Combo();
          break;
        case Orbwalking.OrbwalkingMode.LaneClear:
          JungleClear();
          break;
          
      }



          if (Config.Item("FlashQCombo").GetValue<KeyBind>().Active)
          {
            FlashQCombo();
          }


          if (Config.Item("KSQ").GetValue<bool>())
          {
            KSQ();
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

        if (target.IsValidTarget(R.Range) && R.IsReady() && (Config.Item("UseRCombo").GetValue<bool>()))
        {
          R.Cast(target, true, true);
        }


      



			if (Config.Item("UseItems").GetValue < bool > ()) {
				if (Player.Distance3D(target) <= RDO.Range) {
					RDO.Cast(target);
				}

			}



		}


    private static void KSQ() {
      foreach(Obj_AI_Hero hero in ObjectManager.Get<Obj_AI_Hero>().Where(unit => unit.IsValidTarget(Q.Range)))
         {
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
