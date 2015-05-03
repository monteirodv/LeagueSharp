using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using System.Drawing;
using Color = System.Drawing.Color;



namespace Dev_Essentials
{
  class Program
  {
    private static Spell Q, W, E, R;


   public static Menu Config;
   public static Obj_AI_Hero Player = ObjectManager.Player;

  
    static void Main(string[] args)
    {
      CustomEvents.Game.OnGameLoad += OnGameLoad;
      Drawing.OnDraw += OnDraw;

    }



    private static void OnGameLoad(EventArgs args)
    {
      Q = new Spell(SpellSlot.Q);
      W = new Spell(SpellSlot.W);
      E = new Spell(SpellSlot.E);
      R = new Spell(SpellSlot.R);

      Notifications.AddNotification("Dev Essentials by DanZ and DrunkenNinja loaded", 3000);
      Config = new Menu("DevEssentials", "Dev Essentials", true);
      Config.SubMenu("DevEssentials").AddItem(new MenuItem("ActiveConsole", "Write to Console").SetValue(new KeyBind("G".ToCharArray()[0], KeyBindType.Press)));






      Game.OnUpdate += OnGameUpdate;


    }



    private static void OnGameUpdate(EventArgs args)
    {
      var PlayerCrit = Player.Crit.ToString();
      var PlayerCrit1 = PlayerCrit.Replace("0,", "") + "%";

      String temp = "";
      foreach (var buff in Player.Buffs)
      {
        temp += (buff.DisplayName + "(" + buff.Count + ")" + ", ");
      }


      SpellDataInst spellQ = Player.Spellbook.GetSpell(SpellSlot.Q);
      SpellData dataQ = Player.Spellbook.GetSpell(SpellSlot.Q).SData;
      SpellDataInst spellW = Player.Spellbook.GetSpell(SpellSlot.W);
      SpellData dataW = Player.Spellbook.GetSpell(SpellSlot.W).SData;
      SpellDataInst spellE = Player.Spellbook.GetSpell(SpellSlot.E);
      SpellData dataE = Player.Spellbook.GetSpell(SpellSlot.E).SData;
      SpellDataInst spellR = Player.Spellbook.GetSpell(SpellSlot.R);
      SpellData dataR = Player.Spellbook.GetSpell(SpellSlot.R).SData;

      if (Config.Item("ActiveConsole").GetValue<KeyBind>().Active)
      {
        Console.WriteLine("Coordinates:" + Player.Position.ToString());
        Console.Write("Gold Earned: " + Player.GoldTotal.ToString());
        Console.Write("Attack Delay: " + Player.AttackDelay.ToString());
        Console.Write("Chance of Critical: " + PlayerCrit);
        Console.Write("Wards Destroyed: ", Player.WardsKilled.ToString());
        Console.Write("Wards Placed: ", Player.WardsPlaced.ToString());
        Console.Write("Wards Bought: ", Player.SightWardsBought + Player.VisionWardsBought.ToString());
        Console.Write("Last SpellCasted" + Player.LastCastedSpellName());
        Console.Write("Player Direction:" + Player.Direction.ToString());
        Console.Write("Base AD: " + Player.BaseAttackDamage.ToString());
        Console.Write("Base AP: " + Player.BaseAbilityDamage.ToString());
        Console.Write("Experience: " + Player.Experience.ToString());
        Console.Write("Cursor Position: " + Game.CursorPos.ToString());
        Console.Write("Buffs: " + temp.ToString());
        Console.Write("Q Name:" + spellQ.Name.ToString());
        Console.Write("Q Level:" + spellQ.Level.ToString());
        Console.Write("Q Range:" + spellQ.SData.CastRange.ToString());
        Console.Write("W Name:" + spellW.Name.ToString());
        Console.Write("W Level:" + spellW.Level.ToString());
        Console.Write("W Range:" + spellW.SData.CastRange.ToString());
        Console.Write("E Name:" + spellE.Name.ToString());
        Console.Write("E Level:" + spellE.Level.ToString());
        Console.Write("E Range:" + spellE.SData.CastRange.ToString());
        Console.Write("R Name:" + spellR.Name.ToString());
        Console.Write("R Level:" + spellR.Level.ToString());
        Console.Write("R Range:" + spellR.SData.CastRange.ToString());


      }
      Drawing.DrawText(10, 0, Color.Red, "Dev Essentials v0.1 by DanZ and DrunkenNinja ");
      Drawing.DrawText(10, 10, Color.White, "Coordinates:");
      Drawing.DrawText(10, 25, Color.White, Player.Position.ToString());
      Drawing.DrawText(10, 55, Color.White, "General Info:");
      Drawing.DrawText(10, 70, Color.White, "Gold Earned: " + Player.GoldTotal.ToString());
      Drawing.DrawText(10, 85, Color.White, "Attack Delay: " + Player.AttackDelay.ToString());
      Drawing.DrawText(10, 100, Color.White, "Chance of Critical: " + PlayerCrit);
      Drawing.DrawText(10, 130, Color.White, "Wards:");
      Drawing.DrawText(10, 145, Color.White, "Wards Destroyed: ", Player.WardsKilled.ToString());
      Drawing.DrawText(10, 160, Color.White, "Wards Placed: ", Player.WardsPlaced.ToString());
      Drawing.DrawText(10, 175, Color.White, "Wards Bought: ", Player.SightWardsBought + Player.VisionWardsBought.ToString());
      Drawing.DrawText(10, 195, Color.White, "Last Spell Casted:");
      Drawing.DrawText(10, 210, Color.White, Player.LastCastedSpellName());
      Drawing.DrawText(10, 225, Color.White, "Player Direction:");
      Drawing.DrawText(10, 240, Color.White, Player.Direction.ToString());
      Drawing.DrawText(10, 265, Color.White, "Base AD: " + Player.BaseAttackDamage.ToString());
      Drawing.DrawText(10, 280, Color.White, "Base AP: " + Player.BaseAbilityDamage.ToString());
      Drawing.DrawText(10, 305, Color.White, "Experience: " + Player.Experience.ToString());
      Drawing.DrawText(10, 325, Color.White, "Cursor Position: " + Game.CursorPos.ToString());
      Drawing.DrawText(10, 355, Color.White, "Buffs: ");
      Drawing.DrawText(10, 370, Color.White, temp.ToString());

      Drawing.DrawText(400, 0, Color.White, "Skill Info:");
      Drawing.DrawText(400, 25, Color.White, "Q: ");
      Drawing.DrawText(400, 40, Color.White, "--------");
      Drawing.DrawText(400, 50, Color.White, "Name: " + spellQ.Name.ToString());
      Drawing.DrawText(400, 65, Color.White, "Level: " + spellQ.Level.ToString());
      Drawing.DrawText(400, 80, Color.White, "Range: " + spellQ.SData.CastRange.ToString());
      Drawing.DrawText(400, 100, Color.White, "W: ");
      Drawing.DrawText(400, 115, Color.White, "--------");
      Drawing.DrawText(400, 130, Color.White, "Name: " + spellW.Name.ToString());
      Drawing.DrawText(400, 145, Color.White, "Level: " + spellW.Level.ToString());
      Drawing.DrawText(400, 160, Color.White, "Range: " + spellW.SData.CastRange.ToString());
      Drawing.DrawText(400, 180, Color.White, "E: ");
      Drawing.DrawText(400, 195, Color.White, "--------");
      Drawing.DrawText(400, 210, Color.White, "Name: " + spellE.Name.ToString());
      Drawing.DrawText(400, 225, Color.White, "Level: " + spellE.Level.ToString());
      Drawing.DrawText(400, 240, Color.White, "Range: " + spellE.SData.CastRange.ToString());
      Drawing.DrawText(400, 280, Color.White, "R: ");
      Drawing.DrawText(400, 295, Color.White, "--------");
      Drawing.DrawText(400, 310, Color.White, "Name: " + spellR.Name.ToString());
      Drawing.DrawText(400, 325, Color.White, "Level: " + spellR.Level.ToString());
      Drawing.DrawText(400, 340, Color.White, "Range: " + spellR.SData.CastRange.ToString());
      Drawing.DrawText(0, 10, Color.Red, "|");
      Drawing.DrawText(0, 20, Color.Red, "|");
      Drawing.DrawText(0, 30, Color.Red, "|");
      Drawing.DrawText(0, 40, Color.Red, "|");
      Drawing.DrawText(0, 50, Color.Red, "|");
      Drawing.DrawText(0, 60, Color.Red, "|");
      Drawing.DrawText(0, 70, Color.Red, "|");
      Drawing.DrawText(0, 80, Color.Red, "|");
      Drawing.DrawText(0, 90, Color.Red, "|");
      Drawing.DrawText(0, 100, Color.Red, "|");
      Drawing.DrawText(0, 110, Color.Red, "|");
      Drawing.DrawText(0, 120, Color.Red, "|");
      Drawing.DrawText(0, 130, Color.Red, "|");
      Drawing.DrawText(0, 140, Color.Red, "|");
      Drawing.DrawText(0, 150, Color.Red, "|");
      Drawing.DrawText(0, 160, Color.Red, "|");
      Drawing.DrawText(0, 170, Color.Red, "|");
      Drawing.DrawText(0, 180, Color.Red, "|");
      Drawing.DrawText(0, 190, Color.Red, "|");
      Drawing.DrawText(0, 200, Color.Red, "|");
      Drawing.DrawText(0, 210, Color.Red, "|");
      Drawing.DrawText(0, 220, Color.Red, "|");
      Drawing.DrawText(0, 230, Color.Red, "|");
      Drawing.DrawText(0, 240, Color.Red, "|");
      Drawing.DrawText(0, 250, Color.Red, "|");
      Drawing.DrawText(0, 260, Color.Red, "|");
      Drawing.DrawText(0, 270, Color.Red, "|");
      Drawing.DrawText(0, 280, Color.Red, "|");
      Drawing.DrawText(0, 290, Color.Red, "|");
      Drawing.DrawText(0, 300, Color.Red, "|");
      Drawing.DrawText(0, 310, Color.Red, "|");
      Drawing.DrawText(0, 320, Color.Red, "|");
      Drawing.DrawText(0, 330, Color.Red, "|");
      Drawing.DrawText(0, 340, Color.Red, "|");
      Drawing.DrawText(0, 350, Color.Red, "|");
      Drawing.DrawText(0, 360, Color.Red, "|");
      Drawing.DrawText(0, 370, Color.Red, "|");

      //
      Drawing.DrawText(390, 0, Color.Red, "|");
      Drawing.DrawText(390, 10, Color.Red, "|");
      Drawing.DrawText(390, 20, Color.Red, "|");
      Drawing.DrawText(390, 30, Color.Red, "|");
      Drawing.DrawText(390, 40, Color.Red, "|");
      Drawing.DrawText(390, 50, Color.Red, "|");
      Drawing.DrawText(390, 60, Color.Red, "|");
      Drawing.DrawText(390, 70, Color.Red, "|");
      Drawing.DrawText(390, 80, Color.Red, "|");
      Drawing.DrawText(390, 90, Color.Red, "|");
      Drawing.DrawText(390, 100, Color.Red, "|");
      Drawing.DrawText(390, 110, Color.Red, "|");
      Drawing.DrawText(390, 120, Color.Red, "|");
      Drawing.DrawText(390, 130, Color.Red, "|");
      Drawing.DrawText(390, 140, Color.Red, "|");
      Drawing.DrawText(390, 150, Color.Red, "|");
      Drawing.DrawText(390, 160, Color.Red, "|");
      Drawing.DrawText(390, 170, Color.Red, "|");
      Drawing.DrawText(390, 180, Color.Red, "|");
      Drawing.DrawText(390, 190, Color.Red, "|");
      Drawing.DrawText(390, 200, Color.Red, "|");
      Drawing.DrawText(390, 210, Color.Red, "|");
      Drawing.DrawText(390, 220, Color.Red, "|");
      Drawing.DrawText(390, 230, Color.Red, "|");
      Drawing.DrawText(390, 240, Color.Red, "|");
      Drawing.DrawText(390, 250, Color.Red, "|");
      Drawing.DrawText(390, 260, Color.Red, "|");
      Drawing.DrawText(390, 270, Color.Red, "|");
      Drawing.DrawText(390, 280, Color.Red, "|");
      Drawing.DrawText(390, 290, Color.Red, "|");
      Drawing.DrawText(390, 300, Color.Red, "|");
      Drawing.DrawText(390, 310, Color.Red, "|");
      Drawing.DrawText(390, 320, Color.Red, "|");
      Drawing.DrawText(390, 330, Color.Red, "|");
      Drawing.DrawText(390, 335, Color.Red, "|");
      Drawing.DrawText(390, 340, Color.Red, "|");




    }







      








    
    private static void OnWndProc(WndEventArgs args)
    {

    }
    private static void OnDraw(EventArgs args)
    {
    }
  }
}
