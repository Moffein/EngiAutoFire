using BepInEx;
using BepInEx.Configuration;
using RoR2;
using RoR2.Skills;
using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace EngiAutoFire
{
    [BepInDependency("com.rune580.riskofoptions", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInPlugin("com.Moffein.EngiAutoFire", "EngiAutoFire", "1.2.0")]
    public class EngiAutoFire : BaseUnityPlugin
    {
        enum FireModes { Auto, Hold };

        public static bool allowFireSelect = true;
        private static FireModes selectedMode = FireModes.Auto;

        public static ConfigEntry<bool> selectWithScrollWheel;
        public static ConfigEntry<KeyboardShortcut> selectButton;
        public static ConfigEntry<KeyboardShortcut> autoButton;
        public static ConfigEntry<KeyboardShortcut> holdButton;

        public void Awake()
        {
            allowFireSelect = base.Config.Bind<bool>(new ConfigDefinition("Settings", "Enable Fire Mode Selection"), true, new ConfigDescription("Allows you to choose between holding and autofiring.")).Value;

            selectWithScrollWheel = base.Config.Bind<bool>(new ConfigDefinition("Settings", "Select with ScrollWheel"), true, new ConfigDescription("Scroll wheel swaps between firemodes."));
            selectButton = base.Config.Bind<KeyboardShortcut>(new ConfigDefinition("Settings", "Select Button"), KeyboardShortcut.Empty, new ConfigDescription("Button to swap between firemodes."));
            autoButton = base.Config.Bind<KeyboardShortcut>(new ConfigDefinition("Settings", "Auto Button"), KeyboardShortcut.Empty, new ConfigDescription("Button to swap to Auto mode."));
            holdButton = base.Config.Bind<KeyboardShortcut>(new ConfigDefinition("Settings", "Hold Button"), KeyboardShortcut.Empty, new ConfigDescription("Button to swap to Hold mode."));

            if (allowFireSelect)
            {
                if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.rune580.riskofoptions"))
                {
                    RiskOfOptionsCompat();
                }

                SkillDef engiGrenadeDef = Addressables.LoadAssetAsync<SkillDef>("RoR2/Base/Engi/EngiBodyFireGrenade.asset").WaitForCompletion();
                On.RoR2.UI.SkillIcon.Update += (orig, self) =>
                {
                    orig(self);
                    if (self.targetSkill && self.targetSkill.skillDef == engiGrenadeDef)
                    {
                        self.stockText.gameObject.SetActive(true);
                        self.stockText.fontSize = 12f;
                        self.stockText.SetText(selectedMode.ToString());
                    }
                };
            }

            //Shouldn't be changing the static value like this.
            On.EntityStates.Engi.EngiWeapon.ChargeGrenades.OnEnter += (orig, self) =>
            {
                float oldBaseTotalDuration = EntityStates.Engi.EngiWeapon.ChargeGrenades.baseTotalDuration;
                if (selectedMode == FireModes.Auto || !allowFireSelect)
                {
                    EntityStates.Engi.EngiWeapon.ChargeGrenades.baseTotalDuration = EntityStates.Engi.EngiWeapon.ChargeGrenades.baseMaxChargeTime;
                }
                else
                {
                    EntityStates.Engi.EngiWeapon.ChargeGrenades.baseTotalDuration = 1000000f;
                }
                orig(self);
                EntityStates.Engi.EngiWeapon.ChargeGrenades.baseTotalDuration = oldBaseTotalDuration;
            };
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private void RiskOfOptionsCompat()
        {
            RiskOfOptions.ModSettingsManager.AddOption(new RiskOfOptions.Options.CheckBoxOption(selectWithScrollWheel));
            RiskOfOptions.ModSettingsManager.AddOption(new RiskOfOptions.Options.KeyBindOption(selectButton));
            RiskOfOptions.ModSettingsManager.AddOption(new RiskOfOptions.Options.KeyBindOption(autoButton));
            RiskOfOptions.ModSettingsManager.AddOption(new RiskOfOptions.Options.KeyBindOption(holdButton));
        }

        public void ToggleFireMode()
        {
            if (selectedMode == FireModes.Auto)
            {
                selectedMode = FireModes.Hold;
            }
            else
            {
                selectedMode = FireModes.Auto;
            }
        }

        public void Update()
        {
            if (allowFireSelect)
            {
                if (selectWithScrollWheel.Value && Input.GetAxis("Mouse ScrollWheel") != 0)
                {
                    ToggleFireMode();
                }

                if (GetKeyPressed(selectButton))
                {
                    ToggleFireMode();
                }
                else if (GetKeyPressed(holdButton))
                {
                    selectedMode = FireModes.Hold;
                }
                else if (GetKeyPressed(autoButton))
                {
                    selectedMode = FireModes.Auto;
                }
            }
        }

        //Taken from https://github.com/ToastedOven/CustomEmotesAPI/blob/main/CustomEmotesAPI/CustomEmotesAPI/CustomEmotesAPI.cs
        private static bool GetKeyPressed(ConfigEntry<KeyboardShortcut> entry)
        {
            foreach (var item in entry.Value.Modifiers)
            {
                if (!Input.GetKey(item))
                {
                    return false;
                }
            }
            return Input.GetKeyDown(entry.Value.MainKey);
        }
    }
}

namespace R2API.Utils
{
    [AttributeUsage(AttributeTargets.Assembly)]
    public class ManualNetworkRegistrationAttribute : Attribute
    {
    }
}

