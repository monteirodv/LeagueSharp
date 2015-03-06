using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;


namespace GetBuffs
{

  class Program
  {
            public static Obj_AI_Hero Player = ObjectManager.Player;

    static void Main(string[] args)
    {
      CustomEvents.Game.OnGameLoad += OnGameLoad;

        
    }
    private static void OnGameLoad(EventArgs args)
    {
      Game.OnGameUpdate += OnGameUpdate;

    }
    private static void OnGameUpdate(EventArgs args)
    {
      String temp = "";
      foreach (var buff in Player.Buffs)
      {
        temp += (buff.DisplayName + "(" + buff.Count + ")" + ", ");
      }
      Game.PrintChat(temp);

    }
  }


}
