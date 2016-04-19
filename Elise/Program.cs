namespace Elise
{
    using System;
    using System.Linq;

    using EloBuddy;
    using EloBuddy.SDK;
    using EloBuddy.SDK.Enumerations;
    using EloBuddy.SDK.Rendering;
    using EloBuddy.SDK.Events;
    using EloBuddy.SDK.Menu;
    using EloBuddy.SDK.Menu.Values;

    using SharpDX;

    internal class Elise
    {
        public static Spell.Skillshot E { get; set; }

        public static Spell.Skillshot W { get; set; }

        public static Spell.Targeted Q { get; set; }

        public static Spell.Active R { get; set; }

        public static Spell.Targeted IgniteSpell { get; set; }

        public static Spell.Targeted SmiteSpell { get; set; }

        public static Spell.Targeted SpiderQ { get; set; }

        public static Spell.Targeted SpiderE { get; set; }

        public static Spell.Active SpiderW { get; set; }

        public static AIHeroClient Player { get; set; }

        public static Menu ComboMenu { get; private set; }

        public static Menu HarassMenu { get; private set; }

        public static Menu LaneMenu { get; private set; }

        public static Menu MiscMenu { get; private set; }

        public static Menu JungleMenu { get; private set; }

        public static Menu DrawMenu { get; private set; }


        private static bool SpiderForm
        {
            get
            {
                return Player.Spellbook.GetSpell(SpellSlot.Q).Name == "EliseSpiderQCast"
                       || Player.Spellbook.GetSpell(SpellSlot.W).Name == "EliseSpiderW"
                       || Player.Spellbook.GetSpell(SpellSlot.E).Name == "EliseSpiderEInitial";
            }
        }

        private static bool HumanForm
        {
            get
            {
                return Player.Spellbook.GetSpell(SpellSlot.Q).Name == "EliseHumanQ"
                       || Player.Spellbook.GetSpell(SpellSlot.W).Name == "EliseHumanW"
                       || Player.Spellbook.GetSpell(SpellSlot.E).Name == "EliseHumanE";
            }
        }

        private static Menu Menu;

        private static void Main(string[] args)
        {
            Loading.OnLoadingComplete += OnLoad;
        }

        private static void OnLoad(EventArgs args)
        {
            if (ObjectManager.Player.BaseSkinName != "Elise")
            {
                return;
            }

            Menu = MainMenu.AddMenu("Elise", "Elise");
            Menu.AddGroupLabel("Elise Addon!");
            ComboMenu = Menu.AddSubMenu("Combo");
            ComboMenu.AddGroupLabel("Combo Settings");
            ComboMenu.Add("UseQ", new CheckBox("Use Q Human"));
            ComboMenu.Add("UseW", new CheckBox("Use W Human"));
            ComboMenu.Add("UseE", new CheckBox("Use E Human"));
            ComboMenu.Add("UseQSpider", new CheckBox("Use Q Spider"));
            ComboMenu.Add("UseWSpider", new CheckBox("Use W Spider"));
            ComboMenu.Add("UseESpider", new CheckBox("Use E Spider"));
            ComboMenu.Add("UseR", new CheckBox("Switch forms"));
            ComboMenu.Add("forcee", new KeyBind("Force E key", false, KeyBind.BindTypes.HoldActive, 'T'));
            ComboMenu.Add("cmana", new Slider("Dont switch to human if mana under", 10, 0, 100));

            HarassMenu = Menu.AddSubMenu("Harass");
            HarassMenu.AddGroupLabel("Harass Settings");
            HarassMenu.Add("UseQ", new CheckBox("Use Q Human"));
            HarassMenu.Add("UseW", new CheckBox("Use W Human"));

            LaneMenu = Menu.AddSubMenu("LaneClear");
            LaneMenu.AddGroupLabel("LaneClear Settings");
            LaneMenu.Add("UseQ", new CheckBox("Use Q Human"));
            LaneMenu.Add("UseW", new CheckBox("Use W Human"));
            LaneMenu.Add("UseQSpider", new CheckBox("Use Spider Q"));
            LaneMenu.Add("UseWSpider", new CheckBox("Use Spider W"));
            LaneMenu.Add("UseR", new CheckBox("Switch R"));
            LaneMenu.Add("lmana", new Slider("Minimum mana", 20, 0, 100));

            JungleMenu = Menu.AddSubMenu("JungleClear");
            JungleMenu.AddGroupLabel("JungleClear Settings");
            JungleMenu.Add("UseQ", new CheckBox("Use Q"));
            JungleMenu.Add("UseW", new CheckBox("Use W"));
            JungleMenu.Add("UseQSpider", new CheckBox("Use Spider Q"));
            JungleMenu.Add("UseWSpider", new CheckBox("Use Spider W"));
            JungleMenu.Add("UseR", new CheckBox("Switch R"));
            JungleMenu.Add("jmana", new Slider("Minimum mana", 20, 0, 100));

            MiscMenu = Menu.AddSubMenu("Misc");
            MiscMenu.AddGroupLabel("Misc Settings");
            MiscMenu.Add("autoe", new CheckBox("Auto E immobile"));
            MiscMenu.Add("Antigapcloser", new CheckBox("Use E - Antigapcloser"));
            MiscMenu.Add("Interrupter", new CheckBox("Use E - interrupter"));

            DrawMenu = Menu.AddSubMenu("Draw");
            DrawMenu.AddGroupLabel("Draw Settings");
            DrawMenu.Add("drawoff", new CheckBox("Disable drawings", false));
            DrawMenu.Add("drawq", new CheckBox("Draw Q"));
            DrawMenu.Add("draww", new CheckBox("Draw W"));
            DrawMenu.Add("drawe", new CheckBox("Draw E"));


            Q = new Spell.Targeted(SpellSlot.Q, 625);
            W = new Spell.Skillshot(SpellSlot.W, 950, SkillShotType.Circular);
            E = new Spell.Skillshot(SpellSlot.E, 1075, SkillShotType.Linear, 250, 1600, 80) { AllowedCollisionCount = 0 };
            R = new Spell.Active(SpellSlot.R);
            SpiderQ = new Spell.Targeted(SpellSlot.Q, 475);
            SpiderW = new Spell.Active(SpellSlot.W);
            SpiderE = new Spell.Targeted(SpellSlot.E, 750);

            Game.OnUpdate += OnUpdate;
            Drawing.OnDraw += OnDraw;
            Gapcloser.OnGapcloser += Gapcloser_OnGapcloser;
            Interrupter.OnInterruptableSpell += OnInterruptableSpell;
        }

        private static void Gapcloser_OnGapcloser(AIHeroClient sender, Gapcloser.GapcloserEventArgs e)
        {
            if (!SpiderForm)
            {
                if (MiscMenu.Get<CheckBox>("Antigapcloser").CurrentValue && E.IsReady())
                {
                    if (sender.IsValidTarget(E.Range))
                    {
                        E.Cast(sender);
                    }
                }
            }
        }

        private static void OnInterruptableSpell(Obj_AI_Base Sender, Interrupter.InterruptableSpellEventArgs args)
        {
            if (!SpiderForm)
            {
                if (MiscMenu.Get<CheckBox>("Interrupter").CurrentValue && E.IsReady())
                {
                    if (Sender.IsValidTarget(E.Range))
                    {
                        E.Cast(Sender);
                    }
                }
            }
        }

        private static void OnDraw(EventArgs args)
        {
            if (Player.IsDead)
            {
                return;
            }


            if (DrawMenu.Get<CheckBox>("drawoff").CurrentValue)
            {
                return;
            }

            if (DrawMenu.Get<CheckBox>("drawq").CurrentValue)
            {
                if (Q.Level > 0)
                {
                    Circle.Draw(Color.Blue, Q.Range, ObjectManager.Player.Position);
                }
            }

            if (DrawMenu.Get<CheckBox>("draww").CurrentValue)
            {
                if (W.Level > 0)
                {
                    Circle.Draw(Color.Blue, W.Range, ObjectManager.Player.Position);
                }
            }

            if (DrawMenu.Get<CheckBox>("drawe").CurrentValue)
            {
                if (E.Level > 0)
                {
                    Circle.Draw(Color.Blue, E.Range, ObjectManager.Player.Position);
                }
            }
        }



        private static void OnUpdate(EventArgs args)
        {
            if (Player.IsDead)
            {
                return;
            }

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
            {
                Combo();
            }

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass))
            {
                Harass();
            }

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear) || Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear))
            {
                Jungleclear();
                Laneclear();
            }

            if (ComboMenu.Get<CheckBox>("forcee").CurrentValue)
            {
                ForceE();
            }

            if (MiscMenu.Get<CheckBox>("autoe").CurrentValue)
            {
                AutoE();
            }

        }

        private static void AutoE()
        {
            if (HumanForm)
            {
                if (E.IsReady() && (MiscMenu.Get<CheckBox>("autoe").CurrentValue))
                {
                    foreach (var enemy in ObjectManager.Get<AIHeroClient>())
                    {
                        if (enemy.IsValidTarget(E.Range))
                        {
                            var pred = E.GetPrediction(enemy);
                            if (pred.HitChance >= HitChance.Immobile)
                            {
                                E.Cast(pred.CastPosition);
                            }
                        }
                    }
                }
            }
        }

        private static void ForceE()
        {
            Orbwalker.OrbwalkTo(Game.CursorPos);

            var target = TargetSelector.GetTarget(E.Range, DamageType.Magical);
            if (target == null || !target.IsValidTarget())
            {
                return;
            }

            Orbwalker.OrbwalkTo(Game.CursorPos);

            if (!E.IsReady() || !target.IsValidTarget(E.Range))
            {
                return;
            }

            var prediction = E.GetPrediction(target);
            E.Cast(prediction.CastPosition);
        }

        private static float GetComboDamage(Obj_AI_Base enemy)
        {
            float damage = 0;

            if (Q.IsReady())
            {
                damage += Player.GetSpellDamage(enemy, SpellSlot.Q);
            }
            if (E.IsReady())
            {
                damage += Player.GetSpellDamage(enemy, SpellSlot.E);
            }
            if (R.IsReady())
            {
                damage += Player.GetSpellDamage(enemy, SpellSlot.R);
            }

            var ignitedmg = Player.GetSummonerSpellDamage(enemy, DamageLibrary.SummonerSpells.Ignite);
            if (Player.Spellbook.CanUseSpell(Player.GetSpellSlotFromName("summonerdot")) == SpellState.Ready &&
                enemy.Health < damage + ignitedmg)
            {
                damage += ignitedmg;
            }

            return (float)damage;
        }


        private static void Combo()
        {
            var target = TargetSelector.GetTarget(E.Range, DamageType.Magical);
            if (target == null)
            {
                return;
            }

            if (HumanForm)
            {
                if (target.HasBuff("buffelisecocoon") && SpiderQ.IsReady() && target.IsValidTarget(SpiderQ.Range))
                {
                    R.Cast();
                }

                if (ComboMenu.Get<CheckBox>("useE").CurrentValue && target.Distance(Player.Position) <= E.Range && E.IsReady())
                {
                    var prediction = E.GetPrediction(target);
                    if (prediction.HitChance >= HitChance.High)
                    {
                        E.Cast(prediction.CastPosition);
                    }
                }

                if (ComboMenu.Get<CheckBox>("useQ").CurrentValue && Q.IsReady() && target.IsValidTarget(Q.Range))
                {
                    Q.Cast(target);
                }

                if (ComboMenu.Get<CheckBox>("useW").CurrentValue && W.IsReady() && target.IsValidTarget(W.Range))
                {
                    var prediction = W.GetPrediction(target);
                    if (prediction.CollisionObjects.Count() == 0)
                    {
                        W.Cast(target.ServerPosition);
                    }
                }

                if (ComboMenu.Get<CheckBox>("useR").CurrentValue)
                {
                    if (Player.ManaPercent < ComboMenu.Get<Slider>("Cmana").CurrentValue)
                    {
                        R.Cast();
                    }

                    if (Player.Distance(target) <= 750 && R.IsReady()
                        && (!Q.IsReady() && !W.IsReady() && !E.IsReady()
                            || !Q.IsReady() && !W.IsReady() && !E.IsReady()))
                    {
                        R.Cast();
                    }

                    if (SpiderQ.IsReady() && target.IsValidTarget(SpiderQ.Range)
                        && target.IsValidTarget(SpiderQ.Range))
                    {
                        R.Cast();
                    }
                }
            }

            if (SpiderForm)
            {
                if (ComboMenu.Get<CheckBox>("useQSpider").CurrentValue && SpiderQ.IsReady())
                {
                    if (target.IsValidTarget(SpiderQ.Range))
                    {
                        SpiderQ.Cast(target);
                    }
                }

                if (ComboMenu.Get<CheckBox>("useWSpider").CurrentValue && Player.Distance(target) <= 140 && SpiderW.IsReady())
                {
                    if (target.IsValidTarget(SpiderW.Range))
                    {
                        SpiderW.Cast();
                    }
                }

                if (ComboMenu.Get<CheckBox>("useESpider").CurrentValue && Player.Distance(target) <= SpiderE.Range && Player.Distance(target) > SpiderQ.Range && SpiderE.IsReady())
                {
                    if (target.IsValidTarget(SpiderQ.Range)) return;
                    SpiderE.Cast(target);
                }

                if (ComboMenu.Get<CheckBox>("useR").CurrentValue)
                {
                    if (Player.ManaPercent < ComboMenu.Get<Slider>("Cmana").CurrentValue)
                    {
                        return;
                    }

                    if (R.IsReady() && !target.IsValidTarget(SpiderQ.Range) && !SpiderE.IsReady())
                    {
                        R.Cast();
                    }

                    if (!SpiderQ.IsReady() && !SpiderW.IsReady() && R.IsReady())
                    {
                        R.Cast();
                    }

                    if (!SpiderQ.IsReady() && !SpiderE.IsReady() && !SpiderW.IsReady()
                        || !SpiderQ.IsReady() && Q.IsReady() && GetComboDamage(target) > target.Health)
                    {
                        R.Cast();
                    }
                }

                if (ComboMenu.Get<CheckBox>("useESpider").CurrentValue && Player.Distance(target) > SpiderQ.Range && SpiderE.IsReady())
                {
                    SpiderE.Cast(target);
                }
            }

            if (ComboMenu.Get<CheckBox>("useR").CurrentValue)
            {
                if (!Q.IsReady() && !W.IsReady() && !R.IsReady()
                    || (Q.IsReady() && GetComboDamage(target) >= target.Health)
                    || W.IsReady() && GetComboDamage(target) >= target.Health)
                {
                    if (SpiderQ.IsReady() && target.IsValidTarget(SpiderQ.Range))
                    {
                        return;
                    }

                    R.Cast();
                }
            }
        }

        private static void Harass()
        {
            var target = TargetSelector.GetTarget(W.Range, DamageType.Magical);
            if (target == null)
            {
                return;
            }

            if (!SpiderForm)
            {
                if (HarassMenu.Get<CheckBox>("useQ").CurrentValue && Q.IsReady() && target.IsValidTarget(Q.Range))
                {
                    Q.Cast(target);
                }

                if (HarassMenu.Get<CheckBox>("useW").CurrentValue && W.IsReady() && target.IsValidTarget(W.Range))
                {
                    var prediction = W.GetPrediction(target);
                    if (prediction.CollisionObjects.Count() == 0)
                    {
                        W.Cast(target.ServerPosition);
                    }
                }
            }
        }

        private static void Laneclear()
        {
            var minion = EntityManager.MinionsAndMonsters.GetLaneMinions(EntityManager.UnitTeam.Enemy, Player.ServerPosition, Q.Range + W.Width).FirstOrDefault();
            if (minion == null)
            {
                return;
            }

            if (!SpiderForm)
            {
                if (Player.ManaPercent < LaneMenu.Get<Slider>("lmana").CurrentValue)
                {
                    if (LaneMenu.Get<CheckBox>("useR").CurrentValue && R.IsReady())
                    {
                        R.Cast();
                    }
                }

                if (LaneMenu.Get<CheckBox>("useQ").CurrentValue && Q.IsReady())
                {
                    Q.Cast(minion);
                }

                if (LaneMenu.Get<CheckBox>("useW").CurrentValue && W.IsReady() && minion.IsValidTarget(W.Range))
                {
                    W.Cast(minion.Position);
                }
                if (LaneMenu.Get<CheckBox>("useR").CurrentValue && (!Q.IsReady() && !W.IsReady()) || Player.ManaPercent < LaneMenu.Get<Slider>("lmana").CurrentValue)
                {
                    R.Cast();
                }
            }

            if (SpiderForm)
            {
                if (LaneMenu.Get<CheckBox>("useQSpider").CurrentValue && SpiderQ.IsReady())
                {
                    SpiderQ.Cast(minion);
                }

                if (LaneMenu.Get<CheckBox>("useWSpider").CurrentValue && W.IsReady() && minion.IsValidTarget(SpiderW.Range))
                {
                    SpiderW.Cast();
                }


                if (LaneMenu.Get<CheckBox>("useR").CurrentValue && R.IsReady() && Q.IsReady() && !SpiderQ.IsReady() && !SpiderW.IsReady())
                {
                    R.Cast();
                }
            }
        }

        private static void Jungleclear()
        {
                var minion =
                    EntityManager.MinionsAndMonsters.GetJungleMonsters()
                    .Where(x => x.IsValidTarget(W.Range))
                    .OrderByDescending(x => x.MaxHealth)
                    .FirstOrDefault(x => x != null);

                if (minion == null)
                {
                    return;
                }

                if (!SpiderForm)
                {
                    if (JungleMenu.Get<CheckBox>("useQ").CurrentValue && Q.IsReady())
                    {
                        Q.Cast(minion);
                    }

                    if (JungleMenu.Get<CheckBox>("useW").CurrentValue && W.IsReady() && minion.IsValidTarget(W.Range))
                    {
                        W.Cast(minion.Position);
                    }

                    if (JungleMenu.Get<CheckBox>("useR").CurrentValue && (!Q.IsReady() && !W.IsReady()) || Player.ManaPercent < JungleMenu.Get<Slider>("jmana").CurrentValue)
                    {
                        R.Cast();
                    }
                }

                if (SpiderForm)
                {
                    if (JungleMenu.Get<CheckBox>("useQSpider").CurrentValue && SpiderQ.IsReady())
                    {
                        SpiderQ.Cast(minion);
                    }

                    if (JungleMenu.Get<CheckBox>("useWSpider").CurrentValue && W.IsReady() && minion.IsValidTarget(SpiderW.Range))
                    {
                        SpiderW.Cast();
                    }

                    if (JungleMenu.Get<CheckBox>("useR").CurrentValue && R.IsReady() && Q.IsReady() && !SpiderQ.IsReady() && !SpiderW.IsReady())
                    {
                        R.Cast();
                    }
                }
            }
    }
}