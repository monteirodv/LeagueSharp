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

namespace Katarina___the_Sinister_Blade
{
  class Program
  {
    private static Orbwalking.Orbwalker Orbwalker;
    private static List<Spell> SpellList = new List<Spell>();
    private static Spell Q, W, E, R;
    private static SpellSlot FlashSlot = SpellSlot.Unknown;
    public static float FlashRange = 450f;
    private static Menu Config;
    public static Obj_AI_Hero Player = ObjectManager.Player;
    private static int lastPlaced;
    private static Vector3 lastWardPos;

    static void Main(string[] args)
    {
      CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
    }
    static void Game_OnGameLoad(EventArgs args)
    {
      if (Player.ChampionName != "Katarina") return;
      Notifications.AddNotification("Katarina - The Sinister Blade by DanZ Loaded!", 1000);
      FlashSlot = Player.GetSpellSlot("SummonerFlash");

      Q = new Spell(SpellSlot.Q, 675);
      W = new Spell(SpellSlot.W, 400);
      E = new Spell(SpellSlot.E, 700);
      R = new Spell(SpellSlot.R, 550);
      SpellList.Add(Q);
      SpellList.Add(W);
      SpellList.Add(E);
      SpellList.Add(R);

      Config = new Menu("Katarina", "_menu", true);
      var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
      TargetSelector.AddToMenu(targetSelectorMenu);
      Config.AddSubMenu(targetSelectorMenu);
      Config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
      Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));
      Config.SubMenu("Combo").AddItem(new MenuItem("ComboMode", "Combo Mode").SetValue(new StringList(new[] { "EWQR(Recommended)", "QEWR", "EQWR" }, 0)));
      Config.SubMenu("Combo").AddItem(new MenuItem("UseQCombo", "Use Q")).SetValue(true);
      Config.SubMenu("Combo").AddItem(new MenuItem("UseWCombo", "Use W")).SetValue(true);
      Config.SubMenu("Combo").AddItem(new MenuItem("UseECombo", "Use E")).SetValue(true);
      Config.SubMenu("Combo").AddItem(new MenuItem("UseRCombo", "Use R if Killable")).SetValue(true);

      Config.AddSubMenu(new Menu("Harass", "Harass"));
      Config.SubMenu("Harass").AddItem(new MenuItem("HarassMode", "Harass Mode").SetValue(new StringList(new[] { "QW", "QEW", "Q" }, 1)));
      Config.SubMenu("Harass").AddItem(new MenuItem("AutoW", "Auto use W when enemy in range")).SetValue(true);

      Config.AddSubMenu(new Menu("LaneClear", "LaneClear"));
      Config.SubMenu("LaneClear").AddItem(new MenuItem("ClearMode", "LaneClear Mode").SetValue(new StringList(new[] { "QW", "QEW", "Q", "W" }, 1)));

      Config.AddSubMenu(new Menu("LastHit", "LastHit"));
      Config.SubMenu("LastHit").AddItem(new MenuItem("LastQ", "Last hit with Q")).SetValue(true);

      Config.AddSubMenu(new Menu("Wardjump", "Wardjump"));
      Config.SubMenu("Wardjump").AddItem(new MenuItem("Wardjump", "Ward Jump").SetValue(new KeyBind("G".ToArray()[0], KeyBindType.Press)));

      Config.AddSubMenu(new Menu("Legit Mode (NOT ENABLED)", "Legit"));
      Config.SubMenu("Legit").AddItem(new MenuItem("LegitON", "On").SetValue(false));
      Config.SubMenu("Legit") .AddItem(new MenuItem("QDelay", "Q Delay").SetValue(new Slider(2100, 0, 6000)));
      Config.SubMenu("Legit").AddItem(new MenuItem("EDelay", "E Delay").SetValue(new Slider(2100, 0, 6000)));
      
      Config.AddSubMenu(new Menu("KS", "KS"));
      Config.SubMenu("KS").AddItem(new MenuItem("KSSpells", "Use Single KS Spells ---v---")).SetValue(true);
      Config.SubMenu("KS").AddItem(new MenuItem("QKS", "Use Q")).SetValue(true);
      Config.SubMenu("KS").AddItem(new MenuItem("WKS", "Use W")).SetValue(true);
      Config.SubMenu("KS").AddItem(new MenuItem("EKS", "Use E")).SetValue(true);
      Config.SubMenu("KS").AddItem(new MenuItem("RKS", "Use R")).SetValue(false);

      Config.AddSubMenu(new Menu("R Options", "ROptions"));
      Config.SubMenu("ROptions").AddItem(new MenuItem("BlockR", "Block movement while casting R").SetValue(true));

      Config.AddSubMenu(new Menu("Drawings", "Drawings"));
      Config.SubMenu("Drawings").AddItem(new MenuItem("drawEnable", "Enable Drawing")).SetValue(true);
      Config.SubMenu("Drawings").AddItem(new MenuItem("drawQ", "Draw Q")).SetValue(true);
      Config.SubMenu("Drawings").AddItem(new MenuItem("drawW", "Draw W")).SetValue(true);
      Config.SubMenu("Drawings").AddItem(new MenuItem("drawE", "Draw E")).SetValue(true);
      Config.SubMenu("Drawings").AddItem(new MenuItem("drawR", "Draw R")).SetValue(true);
      var dmg = new MenuItem("combodamage", "Damage Indicator").SetValue(true);
      var drawFill = new MenuItem("color", "Fill colour", true).SetValue(new Circle(true, Color.Orange));
      Config.SubMenu("Drawings").AddItem(drawFill);
      Config.SubMenu("Drawings").AddItem(dmg);
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
      }
    }
    private static void KS()
    {
      if (Config.Item("QKS").GetValue<bool>())
      {
        foreach (Obj_AI_Hero hero in ObjectManager.Get<Obj_AI_Hero>().Where(unit => unit.IsValidTarget(Q.Range)))
        {
          var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
          if (target == null) return;
          if (Q.IsReady())
          {
            if (target.Health < Q.GetDamage(target))
            {
              Q.Cast(target);
            }
          }
        }
      }
      if (Config.Item("WKS").GetValue<bool>())
      {
        foreach (Obj_AI_Hero hero in ObjectManager.Get<Obj_AI_Hero>().Where(unit => unit.IsValidTarget(E.Range)))
        {
          var target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);
          if (target == null) return;
          if (W.IsReady() && E.IsReady())
          {
            if (target.Health < E.GetDamage(target) + W.GetDamage(target))
            {
              E.Cast(target);
              if (W.IsInRange(target))
              {
                W.Cast();
              }
            }
          }
        }
      }
      if (Config.Item("EKS").GetValue<bool>())
      {
        foreach (Obj_AI_Hero hero in ObjectManager.Get<Obj_AI_Hero>().Where(unit => unit.IsValidTarget(E.Range)))
        {
          var target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);
          if (target == null) return;
          if (E.IsReady())
          {
            if (target.Health < E.GetDamage(target))
            {
              E.Cast(target);
            }
          }
        }
        if (Config.Item("RKS").GetValue<bool>())
        {
          foreach (Obj_AI_Hero hero in ObjectManager.Get<Obj_AI_Hero>().Where(unit => unit.IsValidTarget(E.Range)))
          {
            var target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);
            if (target == null) return;
            if (E.IsReady() && R.IsReady())
            {
              if (target.Health < E.GetDamage(target) + R.GetDamage(target))
              {
                E.Cast(target);
                if (R.IsInRange(target))
                {
                  R.Cast();
                }
              }
            }
          }
        }
      }
    }
    private static void Combo()
    {
      var target = TargetSelector.GetTarget(1300, TargetSelector.DamageType.Magical);
      var ComboMode = Config.Item("ComboMode").GetValue<StringList>().SelectedIndex;

        if (ComboMode == 0)
      {
        E.Cast(target);
        if (Player.CountEnemiesInRange(W.Range) >=1 )
        {
          W.Cast();
        }
        Q.Cast(target);
        if (Player.CountEnemiesInRange(W.Range) >= 1)
        {
          R.Cast();
        }
      }
      if (ComboMode == 1)
      {
        Q.Cast(target);
        E.Cast(target);
        if (Player.CountEnemiesInRange(W.Range) >= 1)
        {
          W.Cast();
        }
        if (Player.CountEnemiesInRange(W.Range) >= 1)
        {
          R.Cast();
        }
      }
      if (ComboMode == 2)
      {
        E.Cast(target);
        Q.Cast(target);
        if (Player.CountEnemiesInRange(W.Range) >= 1)
        {
          W.Cast();
        }
        if (Player.CountEnemiesInRange(W.Range) >= 1)
        {
          R.Cast();
        }
      }
    }
    private static void Harass()
    {
      var target = TargetSelector.GetTarget(1300, TargetSelector.DamageType.Magical);
      var HarassMode = Config.Item("HarassMode").GetValue<StringList>().SelectedIndex;

      if (HarassMode == 0)
      {
        Q.Cast(target);
        if (W.IsInRange(target))
        {
          W.Cast();
        }
      }
      if (HarassMode == 1)
      {
        E.Cast(target);
        Q.Cast(target);
        if (W.IsInRange(target))
        {
          W.Cast();
        }
      }
      if (HarassMode == 2)
      {
        Q.Cast(target);
      }
      if (HarassMode == 3)
      {
        if (W.IsInRange(target))
        {
          W.Cast();
        }
      }
    }
    private static void LastHit()
    {
      if (Config.Item("LastQ").GetValue<bool>())
      {
        foreach (var minion in MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.Enemy).
                             Where(x => Q.GetDamage(x) >= x.Health))
        {
          if (Q.CanCast(minion))
            Q.CastOnUnit(minion, true);
        }
      }
    }
    private static void LaneClear()
    {
      var ClearMode = Config.Item("ClearMode").GetValue<StringList>().SelectedIndex;
      if (ClearMode == 0)
      {
        foreach (var minion in MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.Enemy))
        {
          Q.Cast(minion);
          if (W.IsInRange(minion))
          {
            W.Cast();
          }
        }
        if (ClearMode == 1)
        {
          foreach (var minion in MinionManager.GetMinions(E.Range, MinionTypes.All, MinionTeam.Enemy))
          {
            Q.Cast(minion);
            E.Cast(minion);
            if (W.IsInRange(minion))
            {
              W.Cast();
            }
          }
        }
        if (ClearMode == 2)
        {
          foreach (var minion in MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.Enemy))
          {
            Q.Cast(minion);
          }
        }
      }
    }
    private static InventorySlot FindBestWardItem()
    {
      InventorySlot slot = Items.GetWardSlot();
      if (slot == default(InventorySlot)) return null;
      return slot;
    }
    public static void WardJump()
    {
      //CREDITS TO XSALICE
      //wardWalk(Game.CursorPos);

      foreach (Obj_AI_Minion ward in ObjectManager.Get<Obj_AI_Minion>().Where(ward =>
          ward.Name.ToLower().Contains("ward") && ward.Distance(Game.CursorPos) < 250))
      {
        if (E.IsReady())
        {
          E.Cast(ward);
          return;
        }
      }

      foreach (
          Obj_AI_Hero hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.Distance(Game.CursorPos) < 250 && !hero.IsDead))
      {
        if (E.IsReady())
        {
          E.Cast(hero);
          return;
        }
      }

      foreach (Obj_AI_Minion minion in ObjectManager.Get<Obj_AI_Minion>().Where(minion =>
          minion.Distance(Game.CursorPos) < 250))
      {
        if (E.IsReady())
        {
          E.Cast(minion);
          return;
        }
      }

      if (Utils.TickCount <= lastPlaced + 3000 || !E.IsReady()) return;

      Vector3 cursorPos = Game.CursorPos;
      Vector3 myPos = Player.ServerPosition;

      Vector3 delta = cursorPos - myPos;
      delta.Normalize();

      Vector3 wardPosition = myPos + delta * (600 - 5);

      InventorySlot invSlot = FindBestWardItem();
      if (invSlot == null) return;

      Items.UseItem((int)invSlot.Id, wardPosition);
      lastWardPos = wardPosition;
      lastPlaced = Utils.TickCount;
    }
    private static void OnGameUpdate(EventArgs args)
    {
      KS();
      if (Player.IsChannelingImportantSpell()
                || Player.HasBuff("katarinarsound", true)
                || Player.HasBuff("KatarinaR"))
      {
        Orbwalker.SetAttack(false);
        Orbwalker.SetMovement(false);

        return;
      }
      Orbwalker.SetMovement(true);
      Orbwalker.SetAttack(true);
      if (Config.SubMenu("Wardjump").Item("Wardjump").GetValue<KeyBind>().Active)
      {
       WardJump();
      }
      switch (Orbwalker.ActiveMode)
      {
        case Orbwalking.OrbwalkingMode.Combo:
          Combo();
          break;
        case Orbwalking.OrbwalkingMode.LastHit:
          LastHit();
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
