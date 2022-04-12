using BepInEx;
using BepInEx.Configuration;
using RoR2;
using System;
using UnityEngine;

namespace EngiAutoFire
{
    [BepInPlugin("com.Moffein.EngiAutoFire", "EngiAutoFire", "1.1.5")]
    public class EngiAutoFire : BaseUnityPlugin
    {
        enum FireModes { Auto, Hold };
        bool allowFireSelect = true;
        FireModes selectedMode = FireModes.Auto;
        bool selectWithScrollWheel = true;
        KeyCode selectButton = KeyCode.None;
        KeyCode autoButton = KeyCode.None;
        KeyCode holdButton = KeyCode.None;

        public void Awake()
        {
            allowFireSelect = base.Config.Bind<bool>(new ConfigDefinition("Settings", "Enable Fire Mode Selection"), true, new ConfigDescription("Allows you to choose between holding and autofiring.")).Value;
            selectWithScrollWheel = base.Config.Bind<bool>(new ConfigDefinition("Settings", "Select with ScrollWheel"), true, new ConfigDescription("Scroll wheel swaps between firemodes.")).Value;
            selectButton = base.Config.Bind<KeyCode>(new ConfigDefinition("Settings", "Select Button"), KeyCode.None, new ConfigDescription("Button to swap between firemodes.")).Value;
            autoButton = base.Config.Bind<KeyCode>(new ConfigDefinition("Settings", "Select Button"), KeyCode.None, new ConfigDescription("Button to swap to Auto mode.")).Value;
            holdButton = base.Config.Bind<KeyCode>(new ConfigDefinition("Settings", "Select Button"), KeyCode.None, new ConfigDescription("Button to swap to Hold mode.")).Value;
            if (allowFireSelect)
            {
                On.RoR2.UI.SkillIcon.Update += (orig, self) =>
                {
                    orig(self);
                    if (self.targetSkill && self.targetSkillSlot == SkillSlot.Primary)
                    {
                        if (self.targetSkill.characterBody.baseNameToken == "ENGI_BODY_NAME")
                        {
                            self.stockText.gameObject.SetActive(true);
                            self.stockText.fontSize = 12f;
                            self.stockText.SetText(selectedMode.ToString());
                        }
                    }
                };

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
                if (selectWithScrollWheel && Input.GetAxis("Mouse ScrollWheel") != 0)
                {
                    ToggleFireMode();
                }
                if (Input.GetKeyDown(selectButton))
                {
                    ToggleFireMode();
                }
                if (Input.GetKeyDown(holdButton))
                {
                    selectedMode = FireModes.Hold;
                }
                if (Input.GetKeyDown(autoButton))
                {
                    selectedMode = FireModes.Auto;
                }
            }
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

namespace EnigmaticThunder
{
    [AttributeUsage(AttributeTargets.Assembly)]
    public class ManualNetworkRegistrationAttribute : Attribute
    {
    }
}

