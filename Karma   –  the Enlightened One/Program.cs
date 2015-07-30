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

namespace Karma______the_Enlightened_One
{
    class Program
    {
        private static Orbwalking.Orbwalker Orbwalker;
        private static List<Spell> SpellList = new List<Spell>();
        private static Spell Q, Q2, W, W2, E, E2, R;
        private static SpellSlot FlashSlot = SpellSlot.Unknown;
        public static float FlashRange = 450f;
        private static Menu Config;
        public static Obj_AI_Hero Player = ObjectManager.Player;
        private static bool FocusQCombo = false;
        private static bool FocusWCombo = false;
        private static bool FocusECombo = false;
        private static bool FocusQHarass = false;
        private static bool FocusWHarass = false;
        private static bool UseRHarass = false;

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }
        static void Game_OnGameLoad(EventArgs args)
        {
            if (Player.ChampionName != "Karma") return;
            Notifications.AddNotification("Karma - The Enlightened One by DanZ Loaded!", 1000);

            Q = new Spell(SpellSlot.Q, 1050);
            Q2 = new Spell(SpellSlot.Q, 1050);
            W = new Spell(SpellSlot.W, 700);
            W2 = new Spell(SpellSlot.W, 700);
            E = new Spell(SpellSlot.E, 800);
            E2 = new Spell(SpellSlot.E, 800);
            R = new Spell(SpellSlot.R);

            Q.SetSkillshot(0.250f, 90f, 1700f, true, SkillshotType.SkillshotLine);
            Q2.SetSkillshot(0.250f, 90f, 1700f, true, SkillshotType.SkillshotLine);

            SpellList.Add(Q);
            SpellList.Add(Q2);

            SpellList.Add(W);
            SpellList.Add(W2);

            SpellList.Add(E);
            SpellList.Add(E2);

            SpellList.Add(R);

            Config = new Menu("Karma", "_menu", true);
            var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(targetSelectorMenu);
            Config.AddSubMenu(targetSelectorMenu);
            Config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseQCombo", "Use Q")).SetValue(true);
            Config.SubMenu("Combo").AddItem(new MenuItem("UseWCombo", "Use W")).SetValue(true);
            Config.SubMenu("Combo").AddItem(new MenuItem("UseECombo", "Use E")).SetValue(true);
            Config.SubMenu("Combo").AddItem(new MenuItem("EComboHP  ", "Use E if HP% lower than")).SetValue(new Slider(45));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseRCombo", "Use R")).SetValue(true);

            Config.AddSubMenu(new Menu("Harass", "Harass"));
            Config.SubMenu("Harass").AddItem(new MenuItem("UseQHarass", "Use Q")).SetValue(true);
            Config.SubMenu("Harass").AddItem(new MenuItem("UseWHarass", "Use W")).SetValue(true);
            Config.SubMenu("Harass").AddItem(new MenuItem("UseRHarass", "Use R")).SetValue(false);

            Config.AddSubMenu(new Menu("LaneClear", "LaneClear"));
            Config.SubMenu("LaneClear").AddItem(new MenuItem("UseQFarm", "Use Q")).SetValue(true);
            Config.SubMenu("LaneClear").AddItem(new MenuItem("QClearMana", "Use Q if MP% above")).SetValue(new Slider(70));
            Config.SubMenu("LaneClear").AddItem(new MenuItem("UseRFarm", "Use R")).SetValue(true);

            Config.AddSubMenu(new Menu("Shielding", "Shielding"));
            Config.SubMenu("Shielding").AddItem(new MenuItem("ShieldGap", "Use E on gapcloser")).SetValue(true);

            Config.AddSubMenu(new Menu("Flee", "Flee"));
            Config.SubMenu("Flee").AddItem(new MenuItem("Flee", "Flee Mode")).SetValue(new KeyBind("X".ToArray()[0], KeyBindType.Press));

            Config.AddSubMenu(new Menu("KS", "KS"));
            Config.SubMenu("KS").AddItem(new MenuItem("QKS", "Use Q")).SetValue(true);
            Config.SubMenu("KS").AddItem(new MenuItem("WKS", "Use W")).SetValue(true);


            Config.AddSubMenu(new Menu("Drawings", "Drawings"));
            Config.SubMenu("Drawings").AddItem(new MenuItem("drawEnable", "Enable Drawing")).SetValue(true);
            Config.SubMenu("Drawings").AddItem(new MenuItem("drawQ", "Draw Q")).SetValue(true);
            Config.SubMenu("Drawings").AddItem(new MenuItem("drawW", "Draw W")).SetValue(true);
            Config.SubMenu("Drawings").AddItem(new MenuItem("drawE", "Draw E")).SetValue(true);
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
            AntiGapcloser.OnEnemyGapcloser += OnEnemyGapcloser;
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
        private static void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (gapcloser.Sender.IsAlly)
            {
                return;
            }

            if (Config.Item("ShieldGap").GetValue<bool>() && E.IsReady())
            {
                E.Cast(Player);
            }

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

            }
        }
        private static void Flee()
        {
            Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
            var target = TargetSelector.GetTarget(1300, TargetSelector.DamageType.Magical);
            if (E.IsReady())
            {
                R.Cast();
                E.Cast(Player);
            }
            Q.Cast(target);
            W.Cast(target);

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
                            Q.CastIfHitchanceEquals(target, HitChance.Dashing, true);
                            Q.CastIfHitchanceEquals(target, HitChance.Immobile, true);
                            var Qprediction = Q.GetPrediction(target);
                            if (Qprediction.Hitchance >= HitChance.Medium && Qprediction.CollisionObjects.Count(h => h.IsEnemy && !h.IsDead && h is Obj_AI_Minion) < 2)
                            {
                                Q.Cast(Qprediction.CastPosition);
                            }
                        }
                    }
                }
            }
            if (Config.Item("WKS").GetValue<bool>())
            {
                foreach (Obj_AI_Hero hero in ObjectManager.Get<Obj_AI_Hero>().Where(unit => unit.IsValidTarget(W.Range)))
                {
                    var target = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Magical);
                    if (target == null) return;
                    if (W.IsReady())
                    {
                        if (target.Health < W.GetDamage(target))
                        {
                            if (W.IsInRange(target))
                            {
                                W.Cast(target);
                            }
                        }
                    }
                }
            }
        }
        private static void Combo()
        {
            var target = TargetSelector.GetTarget(1300, TargetSelector.DamageType.Magical);
            if (Q.IsReady() && (Config.Item("UseRCombo").GetValue<bool>()) && (Config.Item("UseQCombo").GetValue<bool>()))
            {
                var Qprediction = Q.GetPrediction(target);

                if (Qprediction.Hitchance >= HitChance.High)
                {
                    R.Cast();
                    Q.Cast(Qprediction.CastPosition);
                }
                
            }
            if (Q.IsReady() && (Config.Item("UseQCombo").GetValue<bool>() && !Config.Item("UseRCombo").GetValue<bool>()))
            {
                var Qprediction = Q.GetPrediction(target);

                if (Qprediction.Hitchance >= HitChance.High)
                {
                    Q.Cast(Qprediction.CastPosition);
                }
            }
            if (W.IsReady() && (Config.Item("UseWCombo").GetValue<bool>()))
            {
                W.Cast(target);
            }

            if (E.IsReady() && (Config.Item("UseECombo").GetValue<bool>()))
            {
                if (Player.Health / Player.MaxHealth * 100 >= Config.Item("EComboHP").GetValue<Slider>().Value)
                {
                    E.Cast(Player);
                }
            }

        }
        private static void Harass()
        {
            var target = TargetSelector.GetTarget(1300, TargetSelector.DamageType.Magical);
            if (Q.IsReady() && (Config.Item("UseRHarass").GetValue<bool>()) && (Config.Item("UseQHarass").GetValue<bool>()))
            {
                var Qprediction = Q.GetPrediction(target);

                if (Qprediction.Hitchance >= HitChance.High)
                {
                    R.Cast();
                    Q.Cast(Qprediction.CastPosition);
                }
                
            }
            if (Q.IsReady() && (Config.Item("UseQHarass").GetValue<bool>() && !Config.Item("UseRHarass").GetValue<bool>()))
            {
                var Qprediction = Q.GetPrediction(target);

                if (Qprediction.Hitchance >= HitChance.High)
                {
                    Q.Cast(Qprediction.CastPosition);
                }
            }
            if (W.IsReady() && (Config.Item("UseWHarass").GetValue<bool>()))
            {
                W.Cast(target);
            }
          
        }
        private static void LaneClear()
        {
            if (Player.Mana / Player.MaxMana * 100 >= Config.Item("QClearMana").GetValue<Slider>().Value)
            {
                var minions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.NotAlly);
                bool jungleMobs = minions.Any(x => x.Team == GameObjectTeam.Neutral);
                if ((Config.Item("UseQFarm").GetValue<bool>() && Config.Item("UseRFarm").GetValue<bool>() || jungleMobs))
                {
                    MinionManager.FarmLocation farmLocation = Q.GetLineFarmLocation(minions);
                    if (farmLocation.Position.IsValid())
                        if (farmLocation.MinionsHit >= 1 || jungleMobs)
                            R.Cast();
                            Q.Cast(farmLocation.Position);
                }
                if ((Config.Item("UseQFarm").GetValue<bool>() && !Config.Item("UseRFarm").GetValue<bool>() || jungleMobs))
                {
                    MinionManager.FarmLocation farmLocation = Q.GetLineFarmLocation(minions);
                    if (farmLocation.Position.IsValid())
                        if (farmLocation.MinionsHit >= 1 || jungleMobs)
                    Q.Cast(farmLocation.Position);
                }
            }
        }
        private static void OnGameUpdate(EventArgs args)
        {
            KS();
            if (Config.Item("Flee").GetValue<KeyBind>().Active)
            {
                Flee();
            }
            if (!Config.Item("UseRHarass").GetValue<bool>())
            {
                UseRHarass = false;
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
