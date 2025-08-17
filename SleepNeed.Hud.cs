using System;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.Client.NoObf;
using SleepNeed.Systems;
using SleepNeed.Util;

namespace SleepNeed.Hud
{
    
    public class BetterGuiElementStatbar : GuiElementStatbar
    {
        public float HideWhenLessThan { get; set; }

        public float MinValue { get; set; }

        public float MaxValue { get; set; }

        public bool Hide { get; set; }

        public void SetValues(float value, float min, float max)
        {
            this.MinValue = min;
            this.MaxValue = max;
            base.SetValues(value, min, max);
        }

        public BetterGuiElementStatbar(ICoreClientAPI capi, ElementBounds bounds, double[] color, bool rightToLeft, bool hideable) : base(capi, bounds, color, rightToLeft, hideable)
        {
        }

        public override void RenderInteractiveElements(float deltaTime)
        {
            if (this.Hide)
            {
                return;
            }
            if (base.GetValue() < this.HideWhenLessThan * this.MaxValue)
            {
                return;
            }
            base.RenderInteractiveElements(deltaTime);
        }
    }

    public static class ModGuiStyle
    {
        public static string ToHex(this double[] rgba)
        {
            return ColorUtil.Doubles2Hex(rgba);
        }

        public static double[] FromHex(string hex)
        {
            return ColorUtil.Hex2Doubles(hex);
        }

        public static readonly double[] EnergyBarColor = new double[]
        {
            0.341176471,
            1.0,
            0.423529412,
            0.55
        };

        public static readonly double[] SleepinessBarColor = new double[]
        {
            0.61568627451,
            0.164705882353,
            0.4,
            0.8
        };

        public static readonly double[] EnergyBarColor2 = new double[]
        {
            0.14901960784313725,
            0.27450980392156865,
            0.3254901960784314,
            1.0
        };

        public static readonly double[] EnergyBarColor3 = new double[]
        {
            0.3843137254901961,
            0.7450980392156863,
            0.7568627450980392,
            1.0
        };
    }

    public class EnergyBarHudElement : HudElement
    {
        private bool ShouldShowSleepinessBar
        {
            get
            {
                return ConfigSystem.ConfigClient.SleepinessBarVisible && ConfigSystem.SyncedConfigData.EnableSleepiness;
            }
        }

        private bool ShouldShowEnergyBar
        {
            get
            {
                return ConfigSystem.SyncedConfigData.EnableEnergy;
            }
        }

        public double[] EnergyBarColor
        {
            get
            {
                return ModGuiStyle.FromHex(ConfigSystem.ConfigClient.EnergyBarColor);
            }
        }

        public double[] SleepinessBarColor
        {
            get
            {
                return ModGuiStyle.FromHex(ConfigSystem.ConfigClient.SleepinessBarColor);
            }
        }

        public bool FirstComposed { get; private set; }

        public EnergyBarHudElement(ICoreClientAPI capi) : base(capi)
        {
            capi.Event.RegisterGameTickListener(new Action<float>(this.OnGameTick), 100, 0);
            capi.Event.RegisterGameTickListener(new Action<float>(this.OnFlashStatbars), 2500, 0);
            capi.Event.RegisterEventBusListener(new EventBusListenerDelegate(this.ReloadBars), 0.5, EventIds.ConfigReloaded);
        }

        private void ReloadBars(string eventname, ref EnumHandling handling, IAttribute data)
        {
            if (!this.FirstComposed)
            {
                return;
            }
            base.ClearComposers();
            this.Dispose();
            this.ComposeGuis();
            if (this.ShouldShowEnergyBar)
            {
                this.UpdateEnergyBar(true);
            }
            if (this.ShouldShowSleepinessBar)
            {
                this.UpdateSleepinessBar(true);
            }
        }

        private void OnGameTick(float dt)
        {
            if (this.ShouldShowEnergyBar)
            {
                this.UpdateEnergyBar(false);
            }
            if (this.ShouldShowSleepinessBar)
            {
                this.UpdateSleepinessBar(false);
            }
        }

        public override void OnOwnPlayerDataReceived()
        {
            this.ComposeGuis();
            this.OnGameTick(1f);
        }

        private void UpdateEnergyBar(bool forceReload = false)
        {
            ITreeAttribute energyTree = this.capi.World.Player.Entity.WatchedAttributes.GetTreeAttribute(BtCore.Modid + ":energy");
            if (energyTree == null || this._energyBar == null)
            {
                return;
            }
            float? currentEnergyLevel = energyTree.TryGetFloat("currentenergylevel");
            float? maxEnergy = energyTree.TryGetFloat("maxenergy");
            if (currentEnergyLevel == null || maxEnergy == null)
            {
                return;
            }
            bool flag = (double)Math.Abs(this._lastEnergyLevel - currentEnergyLevel.Value) >= 0.1;
            bool isMaxEnergyChanged = (double)Math.Abs(this._lastMaxEnergy - maxEnergy.Value) >= 0.1;
            if (!flag && !isMaxEnergyChanged && !forceReload)
            {
                return;
            }
            this._energyBar.SetLineInterval(100f);
            this._energyBar.SetValues(currentEnergyLevel.Value, 0f, maxEnergy.Value);
            this._lastEnergyLevel = currentEnergyLevel.Value;
            this._lastMaxEnergy = maxEnergy.Value;
        }

        private void UpdateSleepinessBar(bool forceReload = false)
        {
            ITreeAttribute sleepinessTree = this.capi.World.Player.Entity.WatchedAttributes.GetTreeAttribute(BtCore.Modid + ":sleepiness");
            if (sleepinessTree == null || this._sleepinessBar == null)
            {
                return;
            }
            float? currentsleepinessLevel = sleepinessTree.TryGetFloat("currentsleepinesslevel");
            float? sleepinessCapacity = sleepinessTree.TryGetFloat("sleepinesscapacity");
            if (currentsleepinessLevel == null || sleepinessCapacity == null)
            {
                return;
            }
            bool flag = (double)Math.Abs(this._lastSleepinessLevel - currentsleepinessLevel.Value) >= 0.1;
            bool isSleepinessCapacityChanged = (double)Math.Abs(this._lastSleepinessCapacity - sleepinessCapacity.Value) >= 0.1;
            if (!flag && !isSleepinessCapacityChanged && !forceReload)
            {
                return;
            }
            this._sleepinessBar.SetLineInterval(1f); 
            this._sleepinessBar.SetValues(currentsleepinessLevel.Value, 0f, sleepinessCapacity.Value);
            this._lastSleepinessLevel = currentsleepinessLevel.Value;
            this._lastSleepinessCapacity = sleepinessCapacity.Value;
        }

        private void OnFlashStatbars(float dt)
        {
            ITreeAttribute energyTree = this.capi.World.Player.Entity.WatchedAttributes.GetTreeAttribute(BtCore.Modid + ":energy");
            ITreeAttribute sleepinessTree = this.capi.World.Player.Entity.WatchedAttributes.GetTreeAttribute(BtCore.Modid + ":sleepiness");
            if (energyTree != null && this._energyBar != null)
            {
                float? nullable2 = energyTree.TryGetFloat("currentenergylevel");
                float? nullable3 = energyTree.TryGetFloat("maxenergy");
                double? nullable4 = (nullable2 != null & nullable3 != null) ? new double?((double)nullable2.GetValueOrDefault() / (double)nullable3.GetValueOrDefault()) : null;
                double num = 0.2;
                if (nullable4.GetValueOrDefault() < num & nullable4 != null)
                {
                    this._energyBar.ShouldFlash = true;
                }
                if (sleepinessTree != null && this.ShouldShowSleepinessBar)
                {
                    float? currentsleepinesslevel = sleepinessTree.TryGetFloat("currentsleepinesslevel");
                    float? sleepinesscapacity = sleepinessTree.TryGetFloat("sleepinesscapacity");
                    double? ratio = (currentsleepinesslevel != null & sleepinesscapacity != null) ? new double?((double)currentsleepinesslevel.GetValueOrDefault() / (double)sleepinesscapacity.GetValueOrDefault()) : null;
                    if (ratio.GetValueOrDefault() > 1.0 & ratio != null)
                    {
                        this._sleepinessBar.ShouldFlash = true;
                    }
                }
            }
            if (sleepinessTree != null && !this.ShouldShowSleepinessBar && this._sleepinessBar != null)
            {
                float? currentsleepinesslevel2 = sleepinessTree.TryGetFloat("currentsleepinesslevel");
                float? sleepinesscapacity2 = sleepinessTree.TryGetFloat("sleepinesscapacity");
                double? ratio2 = (currentsleepinesslevel2 != null & sleepinesscapacity2 != null) ? new double?((double)currentsleepinesslevel2.GetValueOrDefault() / (double)sleepinesscapacity2.GetValueOrDefault()) : null;
                if (ratio2.GetValueOrDefault() > 1.0 & ratio2 != null)
                {
                    this._sleepinessBar.ShouldFlash = true;
                }
            }
        }

        private void ComposeGuis()
        {
            this.FirstComposed = true;
            float num = 850f;
            ElementBounds parentBounds = this.GenParentBounds();
            if (this.ShouldShowEnergyBar)
            {
                ElementBounds energyBarBounds = ElementStdBounds.Statbar((EnumDialogArea)3, (double)num * 0.41).WithFixedAlignmentOffset(0.0 + (double)ConfigSystem.ConfigClient.EnergyBarX, (double)(22f + ConfigSystem.ConfigClient.EnergyBarY));
                energyBarBounds.WithFixedHeight(10.0);
                GuiComposer compo = this.capi.Gui.CreateCompo("energybar", parentBounds.FlatCopy().FixedGrow(0.0, 20.0));
                this._energyBar = new BetterGuiElementStatbar(this.capi, energyBarBounds, this.EnergyBarColor, ConfigSystem.ConfigClient.EnergyBarFillDirectionRightToLeft, false);
                compo.BeginChildElements(parentBounds).AddInteractiveElement(this._energyBar, "energybar").EndChildElements().Compose(true);
                this._energyBar.Hide = !this.ShouldShowEnergyBar;
                this.Composers["energybar"] = compo;
            }
            if (this.ShouldShowSleepinessBar)
            {
                ElementBounds sleepinessBarBounds = ElementStdBounds.Statbar((EnumDialogArea)3, (double)num * 0.41).WithFixedAlignmentOffset(0.0 + (double)ConfigSystem.ConfigClient.SleepinessBarX, (double)(7f + ConfigSystem.ConfigClient.SleepinessBarY));
                sleepinessBarBounds.WithFixedHeight(6.0);
                GuiComposer compo2 = this.capi.Gui.CreateCompo("sleepinessbar", parentBounds.FlatCopy().FixedGrow(0.0, 20.0));
                this._sleepinessBar = new BetterGuiElementStatbar(this.capi, sleepinessBarBounds, this.SleepinessBarColor, ConfigSystem.ConfigClient.SleepinessBarFillDirectionRightToLeft, true);
                compo2.BeginChildElements(parentBounds).AddInteractiveElement(this._sleepinessBar, "sleepinessbar").EndChildElements().Compose(true);
                this._sleepinessBar.HideWhenLessThan = ConfigSystem.ConfigClient.HideSleepinessBarAt;
                this._sleepinessBar.Hide = !this.ShouldShowSleepinessBar;
                this.Composers["sleepinessbar"] = compo2;
            }
            this.TryOpen();
        }

        private ElementBounds GenParentBounds()
        {
            return new ElementBounds
            {
                Alignment = (EnumDialogArea)7,
                BothSizing = 0,
                fixedWidth = 850.0,
                fixedHeight = 25.0
            }.WithFixedAlignmentOffset(0.0, -55.0);
        }

        public override bool TryClose()
        {
            return false;
        }

        public override bool ShouldReceiveKeyboardEvents()
        {
            return false;
        }

        public override bool Focusable
        {
            get
            {
                return false;
            }
        }

        private BetterGuiElementStatbar _energyBar;

        private BetterGuiElementStatbar _sleepinessBar;

        private float _lastEnergyLevel;

        private float _lastMaxEnergy;

        private float _lastSleepinessLevel;

        private float _lastSleepinessCapacity;
    }
}
