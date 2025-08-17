using System;
using SleepNeed.Hud;
using SleepNeed.Systems;
using HarmonyLib;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.GameContent;

namespace SleepNeed.HarmonyPatches.CharExtraDialogs
{
    
    public class CharacterExtraDialogs_Dlg_ComposeExtraGuis_Patch
    {
        
        public static bool ShouldSkipPatch()
        {
            return !ConfigSystem.SyncedConfigData.EnableEnergy;
        }

        
        public static void Postfix(CharacterExtraDialogs __instance)
        {
            if (CharacterExtraDialogs_Dlg_ComposeExtraGuis_Patch.ShouldSkipPatch())
            {
                return;
            }
            Traverse traverse = Traverse.Create(__instance);
            ICoreClientAPI capi = traverse.Field("capi").GetValue() as ICoreClientAPI;
            if (capi == null)
            {
                return;
            }
            GuiDialogCharacterBase dlg = traverse.Field("dlg").GetValue() as GuiDialogCharacterBase;
            if (dlg == null)
            {
                return;
            }
            GuiDialog.DlgComposers composers = dlg.Composers;
            EntityPlayer entity = capi.World.Player.Entity;
            float blended = entity.Stats.GetBlended(BtCore.Modid + ":energyrate");
            string text2 = ((int)Math.Round(100.0 * (double)blended)).ToString() + "%";
            ElementBounds bounds = composers["playercharacter"].Bounds;
            ElementBounds bounds4 = composers["environment"].Bounds;
            ElementBounds elementBounds = ElementBounds.Fixed(0.0, 25.0, 90.0, 20.0);
            ElementBounds elementBounds2 = ElementBounds.Fixed(120.0, 30.0, 120.0, 8.0);
            ElementBounds leftColumnBoundsW = ElementBounds.Fixed(0.0, 0.0, 140.0, 20.0);
            ElementBounds elementBounds3 = ElementBounds.Fixed(165.0, 0.0, 120.0, 20.0);
            double num = bounds4.InnerHeight / (double)RuntimeEnv.GUIScale + 10.0;
            double statsHeight = bounds.InnerHeight / (double)RuntimeEnv.GUIScale - GuiStyle.ElementToDialogPadding - 20.0 + num;
            ElementBounds bounds2 = ElementBounds.Fixed(0.0, 3.0, 235.0, 0.15 * statsHeight).WithFixedPadding(GuiStyle.ElementToDialogPadding);
            ElementBounds bounds3 = bounds2.ForkBoundingParent(0.0, 0.0, 0.0, 0.0).WithAlignment((EnumDialogArea)2).WithFixedAlignmentOffset((bounds.renderX + bounds.OuterWidth + 10.0) / (double)RuntimeEnv.GUIScale, num / 2.0).WithFixedOffset(0.0, 0.42 * statsHeight);
            float? currentenergylevel;
            float? maxEnergy;
            CharacterExtraDialogs_Dlg_ComposeExtraGuis_Patch.getCurrentEnergyLevel(entity, out currentenergylevel, out maxEnergy);
            composers["modstats"] = Vintagestory.API.Client.GuiComposerHelpers.AddDialogTitleBar(Vintagestory.API.Client.GuiComposerHelpers.AddShadedDialogBG(capi.Gui.CreateCompo("modstats", bounds3), bounds2, true, 5.0, 0.75f), Lang.Get("Energy Stats", Array.Empty<object>()), delegate ()
            {
                dlg.OnTitleBarClose();
            }, null, null).BeginChildElements(bounds2);
            if (currentenergylevel != null)
            {
                ElementBounds refBounds;
                Vintagestory.API.Client.GuiComposerHelpers.AddStatbar(Vintagestory.API.Client.GuiComposerHelpers.AddStaticText(composers["modstats"], Lang.Get(BtCore.Modid + ":playerinfo-energy-boost", Array.Empty<object>()), CairoFont.WhiteDetailText(), elementBounds.WithFixedWidth(90.0), null), refBounds = elementBounds2.WithFixedOffset(0.0, -5.0), ModGuiStyle.EnergyBarColor, "energyHealthBar");
                leftColumnBoundsW = leftColumnBoundsW.FixedUnder(refBounds, -5.0);
            }
            if (currentenergylevel != null && maxEnergy != null)
            {
                GuiElementDynamicTextHelper.AddDynamicText(Vintagestory.API.Client.GuiComposerHelpers.AddStaticText(composers["modstats"], Lang.Get(BtCore.Modid + ":playerinfo-energy", Array.Empty<object>()), CairoFont.WhiteDetailText(), leftColumnBoundsW = leftColumnBoundsW.BelowCopy(0.0, 0.0, 0.0, 0.0), null), ((int)currentenergylevel.Value).ToString() + " / " + ((int)maxEnergy.Value).ToString(), CairoFont.WhiteDetailText(), elementBounds3 = elementBounds3.FlatCopy().WithFixedPosition(elementBounds3.fixedX, leftColumnBoundsW.fixedY), "energy");
            }
            GuiElementDynamicTextHelper.AddDynamicText(Vintagestory.API.Client.GuiComposerHelpers.AddStaticText(composers["modstats"], Lang.Get("Energy rate", Array.Empty<object>()), CairoFont.WhiteDetailText(), leftColumnBoundsW = leftColumnBoundsW.BelowCopy(0.0, 0.0, 0.0, 0.0), null), text2, CairoFont.WhiteDetailText(), elementBounds3.FlatCopy().WithFixedPosition(elementBounds3.fixedX, leftColumnBoundsW.fixedY).WithFixedHeight(30.0), "energyrate").Compose(true);
        }

        
        private static void getCurrentEnergyLevel(EntityPlayer entity, out float? currentenergylevel, out float? maxEnergy)
        {
            currentenergylevel = null;
            maxEnergy = null;
            ITreeAttribute treeAttribute = entity.WatchedAttributes.GetTreeAttribute("sleepneed:energy");
            if (treeAttribute != null)
            {
                currentenergylevel = treeAttribute.TryGetFloat("currentenergylevel");
                maxEnergy = treeAttribute.TryGetFloat("maxenergy");
            }
            if (currentenergylevel != null)
            {
                float value = currentenergylevel.Value;
            }
        }
    }

    
    public class CharacterExtraDialogs_UpdateStatBars_Patch
    {
        
        public static bool ShouldSkipPatch()
        {
            return !ConfigSystem.SyncedConfigData.EnableEnergy;
        }

        
        public static void Postfix(CharacterExtraDialogs __instance)
        {
            if (CharacterExtraDialogs_UpdateStatBars_Patch.ShouldSkipPatch())
            {
                return;
            }
            Traverse traverse = Traverse.Create(__instance);
            ICoreClientAPI capi = traverse.Field("capi").GetValue() as ICoreClientAPI;
            if (capi == null)
            {
                return;
            }
            GuiDialogCharacterBase dlg = traverse.Field("dlg").GetValue() as GuiDialogCharacterBase;
            if (dlg == null)
            {
                return;
            }
            GuiDialog.DlgComposers composers = dlg.Composers;
            EntityPlayer entity = capi.World.Player.Entity;
            GuiComposer composer = composers["modstats"];
            if (composer == null || !traverse.Method("IsOpened", Array.Empty<object>()).GetValue<bool>())
            {
                return;
            }
            ITreeAttribute treeAttribute = entity.WatchedAttributes.GetTreeAttribute("sleepneed:energy");
            if (treeAttribute == null)
            {
                return;
            }
            float currentenergylevel = treeAttribute.GetFloat("currentenergylevel", 0f);
            float max = treeAttribute.GetFloat("maxenergy", 0f);
            float maxInvigorated = ConfigSystem.ConfigServer.MaxEnergy;
            float invigorated = treeAttribute.GetFloat("invigorated", 0f);
            GuiElementDynamicTextHelper.GetDynamicText(composer, "energy").SetNewText(((int)currentenergylevel).ToString() + " / " + max.ToString(), false, false, false);
            Vintagestory.API.Client.GuiComposerHelpers.GetStatbar(composer, "energyHealthBar").SetLineInterval(100f);
            Vintagestory.API.Client.GuiComposerHelpers.GetStatbar(composer, "energyHealthBar").SetValues(invigorated, 0f, maxInvigorated);
        }
    }

    
    public class CharacterExtraDialogs_UpdateStats_Patch
    {
        
        public static bool ShouldSkipPatch()
        {
            return !ConfigSystem.SyncedConfigData.EnableEnergy;
        }

        
        public static void Postfix(CharacterExtraDialogs __instance)
        {
            if (CharacterExtraDialogs_UpdateStats_Patch.ShouldSkipPatch())
            {
                return;
            }
            Traverse traverse = Traverse.Create(__instance);
            ICoreClientAPI capi = traverse.Field("capi").GetValue() as ICoreClientAPI;
            if (capi == null)
            {
                return;
            }
            GuiDialogCharacterBase dlg = traverse.Field("dlg").GetValue() as GuiDialogCharacterBase;
            if (dlg == null)
            {
                return;
            }
            GuiDialog.DlgComposers composers = dlg.Composers;
            EntityPlayer entity = capi.World.Player.Entity;
            GuiComposer composer = composers["modstats"];
            if (composer == null || !traverse.Method("IsOpened", Array.Empty<object>()).GetValue<bool>())
            {
                return;
            }
            float? currentenergylevel;
            float? maxEnergy;
            CharacterExtraDialogs_UpdateStats_Patch.getCurrentEnergyLevel(entity, out currentenergylevel, out maxEnergy);
            float blended = entity.Stats.GetBlended(BtCore.Modid + ":energyrate");
            if (currentenergylevel != null && maxEnergy != null)
            {
                GuiElementDynamicTextHelper.GetDynamicText(composer, "energy").SetNewText(((int)currentenergylevel.Value).ToString() + " / " + ((int)maxEnergy.Value).ToString(), false, false, false);
            }
            GuiElementDynamicText dynamicText = GuiElementDynamicTextHelper.GetDynamicText(composer, "energyrate");
            if (dynamicText != null)
            {
                dynamicText.SetNewText(((int)Math.Round(100.0 * (double)blended)).ToString() + "%", false, false, false);
            }
        }

        
        private static void getCurrentEnergyLevel(EntityPlayer entity, out float? currentenergylevel, out float? maxEnergy)
        {
            currentenergylevel = null;
            maxEnergy = null;
            ITreeAttribute treeAttribute = entity.WatchedAttributes.GetTreeAttribute("sleepneed:energy");
            if (treeAttribute != null)
            {
                currentenergylevel = treeAttribute.TryGetFloat("currentenergylevel");
                maxEnergy = treeAttribute.TryGetFloat("maxenergy");
            }
            if (currentenergylevel != null)
            {
                float value = currentenergylevel.Value;
            }
        }
    }
}
