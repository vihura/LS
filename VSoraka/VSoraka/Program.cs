﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;
namespace Vsoraka
{
    class Program
    {
        public static string ChampName = "Soraka";
        public static Orbwalking.Orbwalker Orbwalker;
        public static Obj_AI_Base Player = ObjectManager.Player; // Instead of typing ObjectManager.Player you can just type Player
        public static Spell Q, W, E;

        public static Menu V;
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        static void Game_OnGameLoad(EventArgs args)
        {
            if (Player.BaseSkinName != ChampName) return;

            Q = new Spell(SpellSlot.Q, 925);
            Q.SetSkillshot(0.5f, 70f, 1750f, false, SkillshotType.SkillshotCircle);

            W = new Spell(SpellSlot.W, 450);

            E = new Spell(SpellSlot.E, 925);
            E.SetSkillshot(0.5f, 70f, 1750, false, SkillshotType.SkillshotCircle);

            //Base menu
            V = new Menu("V" + ChampName, ChampName, true);
            //Orbwalker and menu
            V.AddSubMenu(new Menu("Orbwalker", "Orbwalker"));
            Orbwalker = new Orbwalking.Orbwalker(V.SubMenu("Orbwalker"));
            //Target selector and menu
            var ts = new Menu("Target Selector", "Target Selector");
            SimpleTs.AddToMenu(ts);
            V.AddSubMenu(ts);
            //Combo menu
            V.AddSubMenu(new Menu("Combo", "Combo"));
            V.SubMenu("Combo").AddItem(new MenuItem("comboQ", "Use Q?").SetValue(true));
            V.SubMenu("Combo").AddItem(new MenuItem("comboE", "Use E?").SetValue(true));
            V.SubMenu("Combo").AddItem(new MenuItem("ComboActive", "Combo").SetValue(new KeyBind(32, KeyBindType.Press)));
            //Farming
            V.AddSubMenu(new Menu("Farming", "Farming"));
            V.SubMenu("Farming").AddItem(new MenuItem("farmQ", "Harras with Q").SetValue(true));
            V.SubMenu("Farming").AddItem(new MenuItem("HarrasActive", "Harras?").SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));
            //AutoW
            V.AddSubMenu(new Menu("Heal", "Heal"));
            V.SubMenu("Heal").AddItem(new MenuItem("healW", "Auto heal?").SetValue(true));
            V.SubMenu("Heal").AddItem(new MenuItem("PlayerHP", "Your minimum HP").SetValue(new Slider(40, 1, 100)));
            V.SubMenu("Heal").AddItem(new MenuItem("AllyHP", "Ally maximum HP").SetValue(new Slider(80, 1, 100)));
            //Drawlings
            V.AddSubMenu(new Menu("Drawings", "Drawings"));
            V.SubMenu("Drawings").AddItem(new MenuItem("DrawQE", "Draw Q/E").SetValue(true));
            V.SubMenu("Drawings").AddItem(new MenuItem("DrawW", "Draw W").SetValue(true));
            //EInterrupt
            V.AddSubMenu(new Menu("Interrupt", "Interrupt"));
            V.SubMenu("Interrupt").AddItem(new MenuItem("InterruptE", "Use E to interrupt").SetValue(true));
            //Make the menu visible
            V.AddToMainMenu();

            Drawing.OnDraw += Drawing_OnDraw; // Add onDraw
            Game.OnGameUpdate += Game_OnGameUpdate; // adds OnGameUpdate (Same as onTick in bol)
            Interrupter.OnPossibleToInterrupt += OnInterruptCreate; //add interputs on E

            Game.PrintChat("V" + ChampName + " loaded! By ViHuRa. Enjoy Free Wins");
            Game.PrintChat("Remember to use Ultimate alone I recommend BIND Ultimate under Z");
        }

        static void Game_OnGameUpdate(EventArgs args)
        {
            if (Player.IsDead) return;
            Heal();
            if (V.Item("ComboActive").GetValue<KeyBind>().Active)
            {
                Combo();
            }
            if (V.Item("HarrasActive").GetValue<KeyBind>().Active)
            {
                Harras();
            }
        }

        static void Drawing_OnDraw(EventArgs args)
        {
            if (V.Item("DrawQE").GetValue<bool>())
            {
                if (Q.IsReady())
                {
                    Utility.DrawCircle(Player.Position, 925f, Color.Green);
                }
                else
                {
                    Utility.DrawCircle(Player.Position, 925f, Color.Red);
                }
            }
            if (V.Item("DrawW").GetValue<bool>())
            {
                if (W.IsReady())
                {
                    Utility.DrawCircle(Player.Position, 450f, Color.Green);
                }
                else
                {
                    Utility.DrawCircle(Player.Position, 450f, Color.Red);
                }
            }
        }
        private static void OnInterruptCreate(Obj_AI_Base unit, InterruptableSpell spell)
        {
            if (spell.DangerLevel < InterruptableDangerLevel.High || unit.IsAlly)
                return;
            if (V.Item("InterruptE").GetValue<bool>())
            {
                E.CastIfHitchanceEquals(unit, HitChance.Medium);
                return;
            }
        }

        public static void Combo()
        {
            var target = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Magical);
            if (target == null) return;

            if (target.IsValidTarget(Q.Range) && Q.IsReady() && V.Item("comboQ").GetValue<bool>())
            {
                Q.Cast(target);

            }
            if (target.IsValidTarget(E.Range) && E.IsReady() && V.Item("comboE").GetValue<bool>())
            {
                E.Cast(target);
            }
        }
        public static void Harras()
        {
            var target = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Magical);

            if (target == null) return;
            if (target.IsValidTarget(Q.Range) && Q.IsReady() && V.Item("farmQ").GetValue<bool>())
            {
                Q.Cast(target);

            }

        }
        private static void Heal()
        {
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsAlly))
            {
                if ((hero.Health / hero.MaxHealth) * 100 <= V.SubMenu("Heal").Item("AllyHP").GetValue<Slider>().Value &&
                    (Player.Health / Player.MaxHealth) * 100 >= V.SubMenu("Heal").Item("PlayerHP").GetValue<Slider>().Value &&
                    V.SubMenu("Heal").Item("healW").GetValue<bool>() &&
                    W.IsReady() &&
                    hero.Distance(Player.ServerPosition) <= W.Range)
                {
                    W.Cast(hero);
                }
            }
        }
    }
}