using Cairo;
using SleepNeed.Systems;
using SleepNeed.Util;
using System;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.Client.NoObf;

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

    public class GuiElementOverloadableBar : GuiElementTextBase
    {
        // Token: 0x1700000B RID: 11
        // (get) Token: 0x06000069 RID: 105 RVA: 0x000046DA File Offset: 0x000028DA
        // (set) Token: 0x0600006A RID: 106 RVA: 0x000046E2 File Offset: 0x000028E2
        public bool HideWhenFull { get; set; }

        // Token: 0x1700000C RID: 12
        // (get) Token: 0x0600006B RID: 107 RVA: 0x000046EB File Offset: 0x000028EB
        // (set) Token: 0x0600006C RID: 108 RVA: 0x000046F3 File Offset: 0x000028F3
        public float HideWhenLessThan { get; set; }

        // Token: 0x1700000D RID: 13
        // (get) Token: 0x0600006D RID: 109 RVA: 0x000046FC File Offset: 0x000028FC
        // (set) Token: 0x0600006E RID: 110 RVA: 0x00004704 File Offset: 0x00002904
        private bool ShowValueOnHover { get; set; } = true;

        // Token: 0x1700000E RID: 14
        // (get) Token: 0x0600006F RID: 111 RVA: 0x0000470D File Offset: 0x0000290D
        private bool IsOverloaded
        {
            get
            {
                return this._value > this._maxValue;
            }
        }

        // Token: 0x1700000F RID: 15
        // (get) Token: 0x06000070 RID: 112 RVA: 0x0000471D File Offset: 0x0000291D
        private int ValueHeight
        {
            get
            {
                return (int)this.Bounds.OuterHeight + 1;
            }
        }

        // Token: 0x17000010 RID: 16
        // (get) Token: 0x06000071 RID: 113 RVA: 0x0000472D File Offset: 0x0000292D
        private int ValueWidth
        {
            get
            {
                return this.Bounds.OuterWidthInt + 1;
            }
        }

        // Token: 0x06000072 RID: 114 RVA: 0x0000473C File Offset: 0x0000293C
        public GuiElementOverloadableBar(ICoreClientAPI capi, ElementBounds bounds, double[] color, double[] overloadColor, bool rightToLeft, bool hideable) : base(capi, "", CairoFont.WhiteDetailText(), bounds)
        {
            this._barTexture = new LoadedTexture(capi);
            this._flashTexture = new LoadedTexture(capi);
            this._valueTexture = new LoadedTexture(capi);
            if (hideable)
            {
                this._baseTexture = new LoadedTexture(capi);
            }
            this._hideable = hideable;
            this._color = color;
            this._overloadColor = overloadColor;
            this._rightToLeft = rightToLeft;
            this._onGetStatbarValue = (() => ((float)Math.Round((double)this._value, 1)).ToString() + " / " + ((int)this._maxValue).ToString());
        }

        // Token: 0x06000073 RID: 115 RVA: 0x00004808 File Offset: 0x00002A08
        public override void ComposeElements(Context ctx, ImageSurface surface)
        {
            this.Bounds.CalcWorldBounds();
            if (this._hideable)
            {
                surface = new ImageSurface(0, this.ValueWidth, this.ValueHeight);
                ctx = new Context(surface);
                GuiElement.RoundRectangle(ctx, 0.0, 0.0, this.Bounds.InnerWidth, this.Bounds.InnerHeight, 1.0);
                ctx.SetSourceRGBA(0.15, 0.15, 0.15, 1.0);
                ctx.Fill();
                base.EmbossRoundRectangleElement(ctx, 0.0, 0.0, this.Bounds.InnerWidth, this.Bounds.InnerHeight, false, 3, 1);
            }
            else
            {
                ctx.Operator = (Cairo.Operator)2;
                GuiElement.RoundRectangle(ctx, this.Bounds.drawX, this.Bounds.drawY, this.Bounds.InnerWidth, this.Bounds.InnerHeight, 1.0);
                ctx.SetSourceRGBA(0.15, 0.15, 0.15, 1.0);
                ctx.Fill();
                base.EmbossRoundRectangleElement(ctx, this.Bounds, false, 3, 1);
            }
            if (this._valuesSet)
            {
                this.RecomposeOverlays();
            }
            if (!this._hideable)
            {
                return;
            }
            base.generateTexture(surface, ref this._baseTexture, true);
            surface.Dispose();
            ctx.Dispose();
        }

        // Token: 0x06000074 RID: 116 RVA: 0x00004998 File Offset: 0x00002B98
        private void RecomposeOverlays()
        {
            TyronThreadPool.QueueTask(delegate ()
            {
                this.ComposeValueOverlay();
                this.ComposeFlashOverlay();
            });
            if (this.ShowValueOnHover)
            {
                this.api.Gui.TextTexture.GenOrUpdateTextTexture(this._onGetStatbarValue(), this._valueFont, ref this._valueTexture, new TextBackground
                {
                    FillColor = GuiStyle.DialogStrongBgColor,
                    Padding = 5,
                    BorderWidth = 2.0
                });
            }
        }

        // Token: 0x06000075 RID: 117 RVA: 0x00004A10 File Offset: 0x00002C10
        private void ComposeValueOverlay()
        {
            this.Bounds.CalcWorldBounds();
            double num = (double)this._value / (double)(this._maxValue - this._minValue);
            double num2 = (double)((this._value + 0.5) - this._maxValue) / (double)(this._maxValue * ConfigSystem.ConfigServer.SleepinessCapacityOverload);
            ImageSurface surface = new ImageSurface(0, this.ValueWidth, this.ValueHeight);
            Context ctx = new Context(surface);
            if (num > 0.01)
            {
                this.DrawColorBar(ctx, surface, num, this._color);
            }
            if (this.IsOverloaded && num2 > 0.01)
            {
                this.DrawColorBar(ctx, surface, num2, this._overloadColor);
            }
            ctx.SetSourceRGBA(0.0, 0.0, 0.0, 0.5);
            ctx.LineWidth = GuiElement.scaled(2.2);
            int num3 = Math.Min(50, (int)((this._maxValue - this._minValue) / this._lineInterval));
            for (int i = 1; i < num3; i++)
            {
                ctx.NewPath();
                ctx.SetSourceRGBA(0.0, 0.0, 0.0, 0.5);
                double num4 = this.Bounds.InnerWidth * (double)i / (double)num3;
                ctx.MoveTo(num4, 0.0);
                ctx.LineTo(num4, Math.Max(3.0, this.Bounds.InnerHeight - 1.0));
                ctx.ClosePath();
                ctx.Stroke();
            }
            this.api.Event.EnqueueMainThreadTask(delegate
            {
                this.generateTexture(surface, ref this._barTexture, true);
                ctx.Dispose();
                surface.Dispose();
            }, "recompstatbar");
        }

        // Token: 0x06000076 RID: 118 RVA: 0x00004C2C File Offset: 0x00002E2C
        private void DrawColorBar(Context ctx, ImageSurface surface, double widthRel, double[] color)
        {
            double num = this.Bounds.OuterWidth * widthRel;
            double x = this._rightToLeft ? (this.Bounds.OuterWidth - num) : 0.0;
            GuiElement.RoundRectangle(ctx, x, 0.0, num, this.Bounds.OuterHeight, 1.0);
            ctx.SetSourceRGB(color[0], color[1], color[2]);
            ctx.FillPreserve();
            ctx.SetSourceRGB(color[0] * 0.4, color[1] * 0.4, color[2] * 0.4);
            ctx.LineWidth = GuiElement.scaled(3.0);
            ctx.StrokePreserve();
            SurfaceTransformBlur.BlurFull(surface, 3.0);
            num = this.Bounds.InnerWidth * widthRel;
            x = (this._rightToLeft ? (this.Bounds.InnerWidth - num) : 0.0);
            base.EmbossRoundRectangleElement(ctx, x, 0.0, num, this.Bounds.InnerHeight, false, 2, 1);
        }

        // Token: 0x06000077 RID: 119 RVA: 0x00004D50 File Offset: 0x00002F50
        private void ComposeFlashOverlay()
        {
            ImageSurface surface = new ImageSurface(0, this.Bounds.OuterWidthInt + 28, this.Bounds.OuterHeightInt + 28);
            Context ctx = new Context(surface);
            ctx.SetSourceRGBA(0.0, 0.0, 0.0, 0.0);
            ctx.Paint();
            GuiElement.RoundRectangle(ctx, 12.0, 12.0, (double)(this.Bounds.OuterWidthInt + 4), (double)(this.Bounds.OuterHeightInt + 4), 1.0);
            ctx.SetSourceRGB(this._color[0], this._color[1], this._color[2]);
            ctx.FillPreserve();
            SurfaceTransformBlur.BlurFull(surface, 3.0);
            ctx.Fill();
            SurfaceTransformBlur.BlurFull(surface, 2.0);
            GuiElement.RoundRectangle(ctx, 15.0, 15.0, (double)(this.Bounds.OuterWidthInt - 2), (double)(this.Bounds.OuterHeightInt - 2), 1.0);
            ctx.Operator = 0;
            ctx.SetSourceRGBA(0.0, 0.0, 0.0, 0.0);
            ctx.Fill();
            this.api.Event.EnqueueMainThreadTask(delegate
            {
                this.generateTexture(surface, ref this._flashTexture, true);
                ctx.Dispose();
                surface.Dispose();
            }, "recompstatbar");
        }

        // Token: 0x06000078 RID: 120 RVA: 0x00004F2C File Offset: 0x0000312C
        public override void RenderInteractiveElements(float deltaTime)
        {
            double renderX = this.Bounds.renderX;
            double renderY = this.Bounds.renderY;
            if (this._value >= this._maxValue && this.HideWhenFull)
            {
                return;
            }
            if (this._value - this.HideWhenLessThan < 0f)
            {
                return;
            }
            if (this._hideable)
            {
                this.api.Render.RenderTexture(this._baseTexture.TextureId, renderX, renderY, (double)(this.Bounds.OuterWidthInt + 1), (double)(this.Bounds.OuterHeightInt + 1), 50f, null);
            }
            float num = 0f;
            if (this.ShouldFlash)
            {
                this._flashTime += 6f * deltaTime;
                num = GameMath.Sin(this._flashTime);
                if (num < 0f)
                {
                    this.ShouldFlash = false;
                    this._flashTime = 0f;
                }
                if (this._flashTime < 1.5707964f)
                {
                    num = Math.Min(1f, num * 3f);
                }
            }
            if (num > 0f)
            {
                this.api.Render.RenderTexture(this._flashTexture.TextureId, renderX - 14.0, renderY - 14.0, (double)(this.Bounds.OuterWidthInt + 28), (double)(this.Bounds.OuterHeightInt + 28), 50f, new Vec4f(1.5f, 1f, 1f, num));
            }
            if (this._barTexture.TextureId > 0)
            {
                this.api.Render.RenderTexture(this._barTexture.TextureId, renderX, renderY, (double)(this.Bounds.OuterWidthInt + 1), (double)this.ValueHeight, 50f, null);
            }
            if (!this.ShowValueOnHover || !this.Bounds.PointInside(this.api.Input.MouseX, this.api.Input.MouseY))
            {
                return;
            }
            double posX = (double)(this.api.Input.MouseX + 16);
            double posY = (double)(this.api.Input.MouseY + this._valueTexture.Height - 4);
            this.api.Render.RenderTexture(this._valueTexture.TextureId, posX, posY, (double)this._valueTexture.Width, (double)this._valueTexture.Height, 2000f, null);
        }

        // Token: 0x06000079 RID: 121 RVA: 0x00005194 File Offset: 0x00003394
        public void SetLineInterval(float value)
        {
            this._lineInterval = value;
        }

        // Token: 0x0600007A RID: 122 RVA: 0x0000519D File Offset: 0x0000339D
        public void SetValue(float value)
        {
            this._value = value;
            this._valuesSet = true;
            this.RecomposeOverlays();
        }

        // Token: 0x0600007B RID: 123 RVA: 0x000051B3 File Offset: 0x000033B3
        public float GetValue()
        {
            return this._value;
        }

        // Token: 0x0600007C RID: 124 RVA: 0x000051BB File Offset: 0x000033BB
        public void SetValues(float value, float min, float max)
        {
            this._valuesSet = true;
            this._value = value;
            this._minValue = min;
            this._maxValue = max;
            this.RecomposeOverlays();
        }

        // Token: 0x0600007D RID: 125 RVA: 0x000051DF File Offset: 0x000033DF
        public void SetMinMax(float min, float max)
        {
            this._minValue = min;
            this._maxValue = max;
            this.RecomposeOverlays();
        }

        // Token: 0x0600007E RID: 126 RVA: 0x000051F5 File Offset: 0x000033F5
        public override void Dispose()
        {
            base.Dispose();
            LoadedTexture baseTexture = this._baseTexture;
            if (baseTexture != null)
            {
                baseTexture.Dispose();
            }
            this._barTexture.Dispose();
            this._flashTexture.Dispose();
            this._valueTexture.Dispose();
        }

        // Token: 0x0400003C RID: 60
        private float _minValue;

        // Token: 0x0400003D RID: 61
        private float _maxValue = 100f;

        // Token: 0x0400003E RID: 62
        private float _value = 32f;

        // Token: 0x0400003F RID: 63
        private float _lineInterval = 10f;

        // Token: 0x04000040 RID: 64
        private readonly double[] _color;

        // Token: 0x04000041 RID: 65
        private readonly double[] _overloadColor;

        // Token: 0x04000042 RID: 66
        private readonly bool _rightToLeft;

        // Token: 0x04000046 RID: 70
        private LoadedTexture _baseTexture;

        // Token: 0x04000047 RID: 71
        private LoadedTexture _barTexture;

        // Token: 0x04000048 RID: 72
        private LoadedTexture _flashTexture;

        // Token: 0x04000049 RID: 73
        private LoadedTexture _valueTexture;

        // Token: 0x0400004A RID: 74
        public bool ShouldFlash;

        // Token: 0x0400004B RID: 75
        private float _flashTime;

        // Token: 0x0400004C RID: 76
        private bool _valuesSet;

        // Token: 0x0400004D RID: 77
        private readonly bool _hideable;

        // Token: 0x0400004E RID: 78
        private StatbarValueDelegate _onGetStatbarValue;

        // Token: 0x0400004F RID: 79
        private readonly CairoFont _valueFont = CairoFont.WhiteSmallText().WithStroke(ColorUtil.BlackArgbDouble, 0.75);
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
            0.0,
            0.709803951569,
            0.450980392157,
            1.0
        };

        public static readonly double[] SleepinessBarColor = new double[]
        {
            0.713725490196,
            0.470588235294,
            0.725490196078,
            1.0
        };

        public static readonly double[] SleepinessOverloadColor = new double[]
        {
            0.47,
            0.0,
            0.14,
            1.0
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

        public double[] SleepinessOverloadColor
        {
            get
            {
                return ModGuiStyle.FromHex(ConfigSystem.ConfigClient.SleepinessOverloadColor);
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
                ElementBounds sleepinessBarBounds = ElementStdBounds.Statbar((EnumDialogArea)3, (double)num * 0.41).WithFixedAlignmentOffset(0.0 + (double)ConfigSystem.ConfigClient.SleepinessBarX, (double)(5f + ConfigSystem.ConfigClient.SleepinessBarY));
                sleepinessBarBounds.WithFixedHeight(6.0);
                GuiComposer compo2 = this.capi.Gui.CreateCompo("sleepinessbar", parentBounds.FlatCopy().FixedGrow(0.0, 20.0));
                this._sleepinessBar = new GuiElementOverloadableBar(this.capi, sleepinessBarBounds, this.SleepinessBarColor, this.SleepinessOverloadColor, ConfigSystem.ConfigClient.SleepinessBarFillDirectionRightToLeft, true);
                compo2.BeginChildElements(parentBounds).AddInteractiveElement(this._sleepinessBar, "sleepinessbar").EndChildElements().Compose(true);
                this._sleepinessBar.HideWhenLessThan = ConfigSystem.ConfigClient.HideSleepinessBarAt;
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

        private GuiElementOverloadableBar _sleepinessBar;

        private float _lastEnergyLevel;

        private float _lastMaxEnergy;

        private float _lastSleepinessLevel;

        private float _lastSleepinessCapacity;
    }
}
