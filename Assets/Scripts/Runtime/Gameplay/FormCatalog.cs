using System;

namespace AlienCrusher.Gameplay
{
    public static class FormCatalog
    {
        public readonly struct Entry
        {
            public readonly FormType Type;
            public readonly string DisplayName;
            public readonly string ButtonName;
            public readonly FormSmashMethod SmashMethod;
            public readonly string StrategyHint;
            public readonly string SkillReadyLabel;
            public readonly string MethodShortLabel;

            public Entry(
                FormType type,
                string displayName,
                string buttonName,
                FormSmashMethod smashMethod,
                string strategyHint,
                string skillReadyLabel,
                string methodShortLabel)
            {
                Type = type;
                DisplayName = displayName;
                ButtonName = buttonName;
                SmashMethod = smashMethod;
                StrategyHint = strategyHint;
                SkillReadyLabel = skillReadyLabel;
                MethodShortLabel = methodShortLabel;
            }
        }

        public static readonly Entry[] All =
        {
            new Entry(FormType.Sphere, "SPHERE", "Form_Sphere", FormSmashMethod.BodyRam, "Ram the lane. Body crush.", "PULSE", "RAM"),
            new Entry(FormType.Spike, "DRILL", "Form_Spike", FormSmashMethod.DrillBurrow, "Burrow a drill line through props.", "BURROW", "DRILL"),
            new Entry(FormType.Ram, "TANK", "Form_Ram", FormSmashMethod.ChargeBurst, "Hold to charge, burst the lane.", "CHARGE", "BURST"),
            new Entry(FormType.Saucer, "UFO", "Form_Saucer", FormSmashMethod.UfoRay, "Hold or tap to fire smash rays.", "RAY", "RAY"),
            new Entry(FormType.Crusher, "MAGNET", "Form_Crusher", FormSmashMethod.MagnetGrab, "Pull breakables, then detonate.", "PULL", "MAGNET")
        };

        public static FormType[] AllTypes
        {
            get
            {
                var types = new FormType[All.Length];
                for (var i = 0; i < All.Length; i++)
                {
                    types[i] = All[i].Type;
                }

                return types;
            }
        }

        public static Entry Get(FormType form)
        {
            for (var i = 0; i < All.Length; i++)
            {
                if (All[i].Type == form)
                {
                    return All[i];
                }
            }

            return All[0];
        }

        public static string GetDisplayName(FormType form)
        {
            return Get(form).DisplayName;
        }

        public static string GetButtonName(FormType form)
        {
            return Get(form).ButtonName;
        }

        public static FormSmashMethod GetSmashMethod(FormType form)
        {
            return Get(form).SmashMethod;
        }

        public static string GetStrategyHint(FormType form)
        {
            return Get(form).StrategyHint;
        }

        public static bool HasDistinctSmashMethods()
        {
            var seen = 0;
            for (var i = 0; i < All.Length; i++)
            {
                var bit = 1 << (int)All[i].SmashMethod;
                if ((seen & bit) != 0)
                {
                    return false;
                }

                seen |= bit;
            }

            return All.Length >= 4 && Enum.IsDefined(typeof(FormSmashMethod), FormSmashMethod.UfoRay);
        }

        public static bool TryGetBySmashMethod(FormSmashMethod method, out Entry entry)
        {
            for (var i = 0; i < All.Length; i++)
            {
                if (All[i].SmashMethod == method)
                {
                    entry = All[i];
                    return true;
                }
            }

            entry = All[0];
            return false;
        }
    }
}
