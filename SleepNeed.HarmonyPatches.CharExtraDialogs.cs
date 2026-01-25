using System;
using SleepNeed.Hud;
using SleepNeed.Systems;
using SleepNeed.Energy;
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
            if (ConfigSystem.SyncedConfig == null)
            {
                return false;
            }
            return !ConfigSystem.SyncedConfig.EnableEnergy;
        }

        // __instance er en reference til den instans af CharacterExtraDialogs, der kører.
        public static void Postfix(CharacterExtraDialogs __instance)
        {
            if (CharacterExtraDialogs_Dlg_ComposeExtraGuis_Patch.ShouldSkipPatch())
            {
                return;
            }
            Traverse traverse = Traverse.Create(__instance); // Opretter en Harmony 'Traverse' instans. Dette er nødvendigt for at tilgå 'private' felter i spillets kode, som man normalt ikke kan nå udefra.
            ICoreClientAPI capi = traverse.Field("capi").GetValue() as ICoreClientAPI; // Henter værdien af det private felt 'capi' (CoreClientAPI) fra instansen via Traverse.
            if (capi == null)
            {
                return;
            }
            GuiDialogCharacterBase dlg = traverse.Field("dlg").GetValue() as GuiDialogCharacterBase; // Henter det private felt 'dlg', som er selve dialog-objektet (GuiDialogCharacterBase).
            if (dlg == null)
            {
                return;
            }
            GuiDialog.DlgComposers composers = dlg.Composers; // Henter listen over 'composers' (de dele der udgør GUI'en).
            IClientPlayer player = capi.World.Player;
            if (player == null || player.Entity == null)
            {
                return; // Stopper hvis spilleren ikke eksistere.
            }
            EntityPlayer entity = player.Entity;
            float energyrate = 1.0f; // Standardværdi på 1.0f (100%)
            float? energyGain; // Standardværdi på 1.0f (100%)
            float? currentenergylevel;
            float? maxEnergy;
            CharacterExtraDialogs_Dlg_ComposeExtraGuis_Patch.getCurrentEnergyLevel(entity, out currentenergylevel, out maxEnergy, out energyGain);
            float safeEnergyGain = energyGain ?? 0f;
            float safeCurrentEnergyLevel = currentenergylevel ?? 0f;
            float safeMaxEnergy = maxEnergy ?? 900f;
            if (entity.Stats != null)
            {
                energyrate = entity.Stats.GetBlended(BtCore.Modid + ":energyrate");
                
            }
            // ------------------------
            string energyrateText = ((int)Math.Round(100.0 * (double)energyrate)).ToString() + "%"; // Formaterer energy-rate til en procent-streng (f.eks. "100%").
            string energygainText = ((int)Math.Round((double)safeEnergyGain)).ToString() + Lang.Get(BtCore.Modid + ":playerinfo-energy-gain-unit");
            ElementBounds characterBounds = composers["playercharacter"].Bounds; // Henter positionen (bounds) for karakter-vinduet for at kunne placere vores nye ting i forhold til det.
            ElementBounds environmentBounds = composers["environment"].Bounds; // Henter positionen for 'environment' fanen (bruges til at beregne højde).
            
            // Definerer layout-bokse (Bounds) for de forskellige elementer, vi vil indsætte.
            // ElementBounds.Fixed(x, y, width, height).
            // INVIGORATED:
            ElementBounds labelBoundsInvigorated = ElementBounds.Fixed(0.0, 25.0, 90.0, 20.0); // Tekst label for invigoration   default:(0.0, 25.0, 90.0, 20.0);
            ElementBounds statbarBoundsInvigorated = ElementBounds.Fixed(120.0, 30.0, 120.0, 8.0); // Statbar for invigoration    default:(120.0, 30.0, 120.0, 8.0)
            // ------------
            // OVERALL HEALTH:
            // Bruger Fixed position for at sikre alignment (25.0 + 15.0 = 40.0 i start Y-koordinat)
            ElementBounds labelBoundsOverallHealth = ElementBounds.Fixed(0.0, 45.0, 90.0, 20.0); // NYT: Label til Overall Health (Y=40.0) (default:(0.0, 40.0, 90.0, 20.0);)
            ElementBounds statbarBoundsOverallHealth = ElementBounds.Fixed(120.0, 50.0, 120.0, 8.0); // NYT: Bar til Overall Health (Y=45.0, med offset -5.0 -> Y=40.0) default:(120.0, 45.0, 120.0, 8.0);
            // ---------------
            ElementBounds leftColumnBoundsW = ElementBounds.Fixed(0.0, 0.0, 140.0, 20.0); // Venstre kolonne
            ElementBounds rightColumnBoundsW = ElementBounds.Fixed(165.0, 0.0, 120.0, 20.0); // Højre kolonne
            double windowHeight = environmentBounds.InnerHeight / (double)RuntimeEnv.GUIScale + 10.0; // Beregner højden dynamisk baseret på GUI-skalaen og det eksisterende 'environment' vindue.
            double statsHeight = characterBounds.InnerHeight / (double)RuntimeEnv.GUIScale - GuiStyle.ElementToDialogPadding - 20.0 + windowHeight; // Beregner hvor meget plads vi har til stats.
            ElementBounds windowSize = ElementBounds.Fixed(0.0, 3.0, 235.0, 0.28 * statsHeight).WithFixedPadding(GuiStyle.ElementToDialogPadding); // Opretter hoved-boksen til vores nye sektion. 0.15 * statsHeight betyder den fylder 15% af den beregnede højde.
            ElementBounds windowPlacement = windowSize.ForkBoundingParent(0.0, 0.0, 0.0, 0.0).WithAlignment((EnumDialogArea)2).WithFixedAlignmentOffset((characterBounds.renderX + characterBounds.OuterWidth + 10.0) / (double)RuntimeEnv.GUIScale, windowHeight / 2.0).WithFixedOffset(0.0, 0.42 * statsHeight); // Placerer vores boks (bounds3) i forhold til karakter-vinduet (til højre for det med .WithAlignment((EnumDialogArea)2)).
            
            
            // HER BYGGES GUI'EN:
            // Opretter en ny GuiComposer med navnet "modstats".
            // Tilføjer en skygge-baggrund (ShadedDialogBG).
            // Tilføjer en titel-bar "Energy Stats".
            composers["modstats"] = Vintagestory.API.Client.GuiComposerHelpers.AddDialogTitleBar(Vintagestory.API.Client.GuiComposerHelpers.AddShadedDialogBG(capi.Gui.CreateCompo("modstats", windowPlacement), windowSize, true, 5.0, 0.75f), Lang.Get(BtCore.Modid + ":playerinfo-energy-stats", Array.Empty<object>()), delegate ()
            {
                dlg.OnTitleBarClose(); // Luk-knap funktionalitet
            }, null, null).BeginChildElements(windowSize); // Starter med at tilføje elementer inde i boksen.
            if (currentenergylevel != null)
            {
                // INVIGORATION
                ElementBounds refBoundsInvigorated; // Klargøre variablen, sættes i linjen Vintagestory.API.Client......
                // Tilføjer en stat-bar (den visuelle bar).
                // "energyHealthBar" er nøglen til at opdatere den senere.
                // ModGuiStyle.EnergyBarColor henter farven fra config.
                Vintagestory.API.Client.GuiComposerHelpers.AddStatbar(
                    Vintagestory.API.Client.GuiComposerHelpers.AddStaticText(composers["modstats"], Lang.Get(BtCore.Modid + ":playerinfo-energy-invigoration", Array.Empty<object>()), CairoFont.WhiteDetailText(), labelBoundsInvigorated.WithFixedWidth(90.0), null), 
                    refBoundsInvigorated = statbarBoundsInvigorated.WithFixedOffset(0.0, 0.0),  // default(0.0, -5.0)
                    ModGuiStyle.InvigorationBarColor, 
                    "energyHealthBar"
                );
                

                // OVERALL HEALTH
                ElementBounds refBoundsHealth;
                Vintagestory.API.Client.GuiComposerHelpers.AddStatbar(
                    Vintagestory.API.Client.GuiComposerHelpers.AddStaticText(composers["modstats"], Lang.Get(BtCore.Modid + ":playerinfo-overall-health", Array.Empty<object>()), CairoFont.WhiteDetailText(), labelBoundsOverallHealth.WithFixedWidth(120.0), null),
                    refBoundsHealth = statbarBoundsOverallHealth.WithFixedOffset(0.0, 0.0), // default (0.0, -5.0)
                    GuiStyle.HealthBarColor,
                    "overallHealthBar"
                );
                
                // Opdaterer layout-ankeret til at være under den SIDSTE bar (Health bar)
                leftColumnBoundsW = leftColumnBoundsW.FixedUnder(refBoundsHealth, -5.0); // Justerer layoutet til næste element.
            }
            if (currentenergylevel != null && maxEnergy != null)
            {
                // Tilføjer tekst der viser tallene (f.eks. "450 / 900").
                GuiElementDynamicTextHelper.AddDynamicText(Vintagestory.API.Client.GuiComposerHelpers.AddStaticText(composers["modstats"], Lang.Get(BtCore.Modid + ":playerinfo-energy", Array.Empty<object>()), CairoFont.WhiteDetailText(), leftColumnBoundsW = leftColumnBoundsW.BelowCopy(0.0, 0.0, 0.0, 0.0), null), ((int)safeCurrentEnergyLevel).ToString() + " / " + ((int)safeMaxEnergy).ToString(), CairoFont.WhiteDetailText(), rightColumnBoundsW = rightColumnBoundsW.FlatCopy().WithFixedPosition(rightColumnBoundsW.fixedX, leftColumnBoundsW.fixedY), "energy");
                // "energy" er nøglenavnet til at opdatere teksten senere
            }
            // Tilføjer tekst for Energy Rate %.
            GuiElementDynamicTextHelper.AddDynamicText(
                Vintagestory.API.Client.GuiComposerHelpers.AddStaticText(composers["modstats"], 
                Lang.Get(BtCore.Modid + ":playerinfo-energy-rate", Array.Empty<object>()), CairoFont.WhiteDetailText(), leftColumnBoundsW = leftColumnBoundsW.BelowCopy(0.0, 0.0, 0.0, 0.0), null), 
                energyrateText, 
                CairoFont.WhiteDetailText(), 
                rightColumnBoundsW.FlatCopy().WithFixedPosition(rightColumnBoundsW.fixedX, leftColumnBoundsW.fixedY).WithFixedHeight(30.0), 
                "energyrate"
            );
            
            // Tilføjer tekst for Relaxing Speed /hour
            GuiElementDynamicTextHelper.AddDynamicText(
                Vintagestory.API.Client.GuiComposerHelpers.AddStaticText(composers["modstats"],
                Lang.Get(BtCore.Modid + ":playerinfo-energy-gain", Array.Empty<object>()), CairoFont.WhiteDetailText(), leftColumnBoundsW = leftColumnBoundsW.BelowCopy(0.0, 0.0, 0.0, 0.0), null),
                energygainText, // Standard tekst indtil den opdateres
                CairoFont.WhiteDetailText(),
                rightColumnBoundsW.FlatCopy().WithFixedPosition(rightColumnBoundsW.fixedX, leftColumnBoundsW.fixedY).WithFixedHeight(20.0),
                "relaxingspeed" // Nøgle til at finde elementet i UpdateStats
            );

            // Tilføjer tekst for Relaxing Type
            // Her sætter vi .Compose(true) til sidst, da dette er det sidste element.
            GuiElementDynamicTextHelper.AddDynamicText(
                Vintagestory.API.Client.GuiComposerHelpers.AddStaticText(composers["modstats"],
                Lang.Get(BtCore.Modid + ":playerinfo-energy-relaxing", Array.Empty<object>()), CairoFont.WhiteDetailText(), leftColumnBoundsW = leftColumnBoundsW.BelowCopy(0.0, 0.0, 0.0, 0.0), null),
                "-", // Standard tekst
                CairoFont.WhiteDetailText(),
                rightColumnBoundsW.FlatCopy().WithFixedPosition(rightColumnBoundsW.fixedX, leftColumnBoundsW.fixedY).WithFixedHeight(20.0),
                "relaxingtype" // Nøgle til at finde elementet i UpdateStats
            ).Compose(true);
            // .Compose(true); Afslutter compositionen og bygger GUI'en.
        }

        // Hjælpemetode til at udtrække data fra entity'ens attributes træ.
        private static void getCurrentEnergyLevel(EntityPlayer entity, out float? currentenergylevel, out float? maxEnergy, out float? energyGain)
        {
            currentenergylevel = null;
            maxEnergy = null;
            energyGain = null;
            ITreeAttribute treeAttribute = entity.WatchedAttributes.GetTreeAttribute("sleepneed:energy");
            if (treeAttribute != null)
            {
                currentenergylevel = treeAttribute.TryGetFloat("currentenergylevel");
                maxEnergy = treeAttribute.TryGetFloat("maxenergy");
                energyGain = treeAttribute.TryGetFloat("relaxingspeed");
            }
        }
    }

    // Denne patch sørger for, at de grafiske barer (som vi oprettede ovenfor med nøglen "energyHealthBar") bevæger sig og viser de rigtige værdier, når menuen er åben.
    public class CharacterExtraDialogs_UpdateStatBars_Patch
    {
        
        public static bool ShouldSkipPatch()
        {
            if (ConfigSystem.SyncedConfig == null)
            {
                return false;
            }
            return !ConfigSystem.SyncedConfig.EnableEnergy;
        }

        // Postfix på 'UpdateStats' metoden i spillet.
        public static void Postfix(CharacterExtraDialogs __instance)
        {
            if (CharacterExtraDialogs_UpdateStatBars_Patch.ShouldSkipPatch())
            {
                return;
            }
            Traverse traverse = Traverse.Create(__instance); // Igen, Traverse for at få fat i private felter.
            ICoreClientAPI capi = traverse.Field("capi").GetValue() as ICoreClientAPI;
            if (capi == null || capi.World.Player == null)
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
            GuiComposer composer = composers["modstats"]; // Henter vores 'modstats' composer som vi lavede i den forrige patch.
            // Tjekker om composer findes og om dialogen faktisk er åben (IsOpened er en privat metode/property, der kaldes via Traverse).
            if (composer == null || !traverse.Method("IsOpened", Array.Empty<object>()).GetValue<bool>())
            {
                return;
            }
            // Henter data.
            ITreeAttribute treeAttribute = entity.WatchedAttributes.GetTreeAttribute("sleepneed:energy");
            if (treeAttribute == null)
            {
                return;
            }
            // Tjek for om composers er null.
            var InvigorationBarComposer = Vintagestory.API.Client.GuiComposerHelpers.GetStatbar(composer, "energyHealthBar");
            var healthBarComposer = Vintagestory.API.Client.GuiComposerHelpers.GetStatbar(composer, "overallHealthBar");
            if (InvigorationBarComposer == null || healthBarComposer == null)
            {
                return;
            }

            // INVIGORATED: Henter de rå værdier. 
            float? maxInvigorated;
            float? invigorated;
            float? overallHealthRatio;

            CharacterExtraDialogs_UpdateStatBars_Patch.getCurrentBoostLevel(entity, out invigorated, out maxInvigorated, out overallHealthRatio);
            // Bruger '??' operatoren som betyder "hvis venstre side er null, så brug højre side".
            float safeInvigorated = invigorated ?? 0f;
            float safeMaxInvigorated = maxInvigorated ?? 900f; // Default til 100 hvis config fejler
            float safeOverallHealth = overallHealthRatio ?? 0f;
            // Opdaterer stat-baren "energyHealthBar".
            Vintagestory.API.Client.GuiComposerHelpers.GetStatbar(composer, "energyHealthBar").SetLineInterval(100f);
            Vintagestory.API.Client.GuiComposerHelpers.GetStatbar(composer, "energyHealthBar").SetValues(safeInvigorated, 0f, safeMaxInvigorated);
            

            // Opdater Overall Health Bar
            // Sætter værdierne for Overall Health (Min: 0, Max: 1)
            // Bruger interval 0.1 (10%), da værdien er mellem 0 og 1
            Vintagestory.API.Client.GuiComposerHelpers.GetStatbar(composer, "overallHealthBar").SetLineInterval(0.1f);
            Vintagestory.API.Client.GuiComposerHelpers.GetStatbar(composer, "overallHealthBar").SetValues(safeOverallHealth, 0f, 1f);
            
        }
        private static void getCurrentBoostLevel(EntityPlayer entity, out float? invigorated, out float? maxInvigorated, out float? overallHealthRatio)
        {
            invigorated = null;
            overallHealthRatio = null;
            maxInvigorated = null;
            ITreeAttribute treeAttribute = entity.WatchedAttributes.GetTreeAttribute("sleepneed:energy");
            if (treeAttribute != null && ConfigSystem.SyncedConfig != null)
            {
                invigorated = treeAttribute.TryGetFloat("invigorated");
                overallHealthRatio = treeAttribute.TryGetFloat("overallhealth");
                maxInvigorated = ConfigSystem.SyncedConfig.MaxEnergy;
            }
        }
    }

    
    public class CharacterExtraDialogs_UpdateStats_Patch
    {
        
        public static bool ShouldSkipPatch()
        {
            if (ConfigSystem.SyncedConfig == null)
            {
                return false;
            }
            return !ConfigSystem.SyncedConfig.EnableEnergy;
        }

        
        public static void Postfix(CharacterExtraDialogs __instance)
        {
            if (CharacterExtraDialogs_UpdateStats_Patch.ShouldSkipPatch())
            {
                return;
            }
            // Standard Traverse setup (samme som før).
            Traverse traverse = Traverse.Create(__instance);
            ICoreClientAPI capi = traverse.Field("capi").GetValue() as ICoreClientAPI;
            if (capi == null || capi.World.Player == null)
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
            // Tjekker om dialogen er åben.
            if (composer == null || !traverse.Method("IsOpened", Array.Empty<object>()).GetValue<bool>())
            {
                return;
            }
            float? currentenergylevel;
            float? maxEnergy;
            float? currentRelaxingSpeed;
            bool? currentRelaxingType;
            
            CharacterExtraDialogs_UpdateStats_Patch.getCurrentEnergyLevel(entity, out currentenergylevel, out maxEnergy, out currentRelaxingSpeed, out currentRelaxingType);
            
            float energyrate = 1.0f; 
            float safeCurrentEnergyLevel = currentenergylevel ?? 0f;
            float safeMaxEnergy = maxEnergy ?? 900f;
            float safeRelaxingSpeed = currentRelaxingSpeed ?? 0f;
            bool safeRelaxingType = currentRelaxingType ?? false;

            if (entity.Stats != null)
            {
                energyrate = entity.Stats.GetBlended(BtCore.Modid + ":energyrate");
            }
            GuiElementDynamicTextHelper.GetDynamicText(composer, "energy").SetNewText(((int)safeCurrentEnergyLevel).ToString() + " / " + ((int)safeMaxEnergy).ToString(), false, false, false);

            GuiElementDynamicText energyrateText = GuiElementDynamicTextHelper.GetDynamicText(composer, "energyrate");
            if (energyrateText != null)
            {
                energyrateText.SetNewText(((int)Math.Round(100.0 * (double)energyrate)).ToString() + "%", false, false, false);
            }
            
            GuiElementDynamicText relaxingspeedText = GuiElementDynamicTextHelper.GetDynamicText(composer, "relaxingspeed");
            if (relaxingspeedText != null)
            {
                string energygainText = ((int)Math.Round((double)safeRelaxingSpeed)).ToString() + Lang.Get(BtCore.Modid + ":playerinfo-energy-gain-unit");
                relaxingspeedText.SetNewText(energygainText, false, false, false);
            }

            // Opdater Relaxing Type tekst
            GuiElementDynamicText relaxingtypeText = GuiElementDynamicTextHelper.GetDynamicText(composer, "relaxingtype");
            if (relaxingtypeText != null)
            {
                string relaxingtypeString;
                double[] colorToUse;
                if (safeRelaxingSpeed > 0f)
                {
                    if (safeRelaxingType == true) // Wellness
                    {
                        relaxingtypeString = "Wellness";
                        // En lyseblå farve (minder om "Wet" status i vanilla)
                        // Hex: #99DDFF
                        colorToUse = Vintagestory.API.MathTools.ColorUtil.Hex2Doubles("#99DDFF");
                    }
                    else // Relaxing
                    {
                        relaxingtypeString = "Relaxing";
                        // En frisk grøn farve
                        // Hex: #77CC55
                        colorToUse = Vintagestory.API.MathTools.ColorUtil.Hex2Doubles("#77CC55");
                    }
                }
                else
                {
                    // Hvis vi ikke slapper af
                    relaxingtypeString = "-";
                    // Brug standard hvid farve (henter farven fra WhiteDetailText)
                    colorToUse = Vintagestory.API.Client.CairoFont.WhiteDetailText().Color;
                }

                // Tager standard fonten (WhiteDetailText) og påfører vores valgte farve med .WithColor()
                // Så sættes elementets font til denne nye, farvede version.
                relaxingtypeText.Font = Vintagestory.API.Client.CairoFont.WhiteDetailText().WithColor(colorToUse);

                // Til sidst opdateres teksten som normalt
                relaxingtypeText.SetNewText(relaxingtypeString, false, false, false);
            }
        }
                

        // Kopi af hjælpemetoden fra den første klasse.
        private static void getCurrentEnergyLevel(EntityPlayer entity, out float? currentenergylevel, out float? maxEnergy, out float? currentRelaxingSpeed, out bool? currentRelaxingType)
        {
            currentenergylevel = null;
            maxEnergy = null;
            currentRelaxingSpeed = null;
            currentRelaxingType = null;
            ITreeAttribute treeAttribute = entity.WatchedAttributes.GetTreeAttribute("sleepneed:energy");
            if (treeAttribute != null)
            {
                currentenergylevel = treeAttribute.TryGetFloat("currentenergylevel");
                maxEnergy = treeAttribute.TryGetFloat("maxenergy");
                currentRelaxingSpeed = treeAttribute.TryGetFloat("relaxingspeed");
                currentRelaxingType = treeAttribute.TryGetBool("relaxingtype");
            }
        }
    }
}
