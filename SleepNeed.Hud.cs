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
        
        public bool HideWhenFull { get; set; }

        
        public float HideWhenLessThan { get; set; }

        
        private bool ShowValueOnHover { get; set; } = true;

        
        private bool IsOverloaded
        {
            get
            {
                return this._value > this._maxValue;
            }
        }

        
        private int ValueHeight
        {
            get
            {
                return (int)this.Bounds.OuterHeight + 1;
            }
        }

        
        private int ValueWidth
        {
            get
            {
                return this.Bounds.OuterWidthInt + 1;
            }
        }

        
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

        
        private void RecomposeOverlays()
        {
            this.Bounds.CalcWorldBounds();
            int surfWidth = this.ValueWidth;
            int surfHeight = this.ValueHeight;

            double innerW = this.Bounds.InnerWidth;
            double innerH = this.Bounds.InnerHeight;
            double outerW = this.Bounds.OuterWidth;
            double outerH = this.Bounds.OuterHeight;
            int outerWint = this.Bounds.OuterWidthInt;
            int outerHint = this.Bounds.OuterHeightInt;
            TyronThreadPool.QueueTask(delegate ()
            {
                this.ComposeValueOverlay(surfWidth, surfHeight, innerW, innerH, outerW, outerH);
                this.ComposeFlashOverlay(surfWidth, surfHeight, outerWint, outerHint);
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

        
        private void ComposeValueOverlay(int surfWidth, int surfHeight, double innerW, double innerH, double outerW, double outerH)
        {
            
            double num = (double)this._value / (double)(this._maxValue - this._minValue);
            double num2 = (double)((this._value + 0.5) - this._maxValue) / (double)this.CapacityOverloadFactor;
            ImageSurface surface = new ImageSurface(0, surfWidth, surfHeight);
            Context ctx = new Context(surface);
            if (num > 0.01)
            {
                this.DrawColorBar(ctx, surface, num, this._color, outerW, outerH, innerW, innerH);
            }
            if (this.IsOverloaded && num2 > 0.01)
            {
                this.DrawColorBar(ctx, surface, num2, this._overloadColor, outerW, outerH, innerW, innerH);
            }
            ctx.SetSourceRGBA(0.0, 0.0, 0.0, 0.5);
            ctx.LineWidth = GuiElement.scaled(2.2);
            int num3 = Math.Min(50, (int)((this._maxValue - this._minValue) / this._lineInterval));
            for (int i = 1; i < num3; i++)
            {
                ctx.NewPath();
                ctx.SetSourceRGBA(0.0, 0.0, 0.0, 0.5);
                double num4 = innerW * (double)i / (double)num3;
                ctx.MoveTo(num4, 0.0);
                ctx.LineTo(num4, Math.Max(3.0, innerH - 1.0));
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

        
        private void DrawColorBar(Context ctx, ImageSurface surface, double widthRel, double[] color, double outerW, double outerH, double innerW, double innerH)
        {
            double num = outerW * widthRel;
            double x = this._rightToLeft ? (outerW - num) : 0.0;
            GuiElement.RoundRectangle(ctx, x, 0.0, num, outerH, 1.0);
            ctx.SetSourceRGB(color[0], color[1], color[2]);
            ctx.FillPreserve();
            ctx.SetSourceRGB(color[0] * 0.4, color[1] * 0.4, color[2] * 0.4);
            ctx.LineWidth = GuiElement.scaled(3.0);
            ctx.StrokePreserve();
            SurfaceTransformBlur.BlurFull(surface, 3.0);
            num = innerW * widthRel;
            x = (this._rightToLeft ? (innerW - num) : 0.0);
            base.EmbossRoundRectangleElement(ctx, x, 0.0, num, innerH, false, 2, 1);
        }

        
        private void ComposeFlashOverlay(int surfWidth, int surfHeight, int outerWint, int outerHint)
        {
            ImageSurface surface = new ImageSurface(0, surfWidth + 28, surfHeight + 28);
            Context ctx = new Context(surface);
            ctx.SetSourceRGBA(0.0, 0.0, 0.0, 0.0);
            ctx.Paint();
            GuiElement.RoundRectangle(ctx, 12.0, 12.0, (double)(outerWint + 4), (double)(outerHint + 4), 1.0);
            ctx.SetSourceRGB(this._color[0], this._color[1], this._color[2]);
            ctx.FillPreserve();
            SurfaceTransformBlur.BlurFull(surface, 3.0);
            ctx.Fill();
            SurfaceTransformBlur.BlurFull(surface, 2.0);
            GuiElement.RoundRectangle(ctx, 15.0, 15.0, (double)(outerWint - 2), (double)(outerHint - 2), 1.0);
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

        
        public void SetLineInterval(float value)
        {
            this._lineInterval = value;
        }

        
        public void SetValue(float value)
        {
            this._value = value;
            this._valuesSet = true;
            this.RecomposeOverlays();
        }

        
        public float GetValue()
        {
            return this._value;
        }

        
        public void SetValues(float value, float min, float max)
        {
            this._valuesSet = true;
            this._value = value;
            this._minValue = min;
            this._maxValue = max;
            this.RecomposeOverlays();
        }

        
        public void SetMinMax(float min, float max)
        {
            this._minValue = min;
            this._maxValue = max;
            this.RecomposeOverlays();
        }

        
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

        
        private float _minValue;

        
        private float _maxValue = 100f;

        
        private float _value = 32f;

        
        private float _lineInterval = 10f;

        public float CapacityOverloadFactor { get; set; } = 11.0f;

        
        private readonly double[] _color;

        
        private readonly double[] _overloadColor;

        
        private readonly bool _rightToLeft;

        
        private LoadedTexture _baseTexture;

        
        private LoadedTexture _barTexture;

        
        private LoadedTexture _flashTexture;

        
        private LoadedTexture _valueTexture;

        
        public bool ShouldFlash;

        
        private float _flashTime;

        
        private bool _valuesSet;

        
        private readonly bool _hideable;

        
        private StatbarValueDelegate _onGetStatbarValue;

        
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

        public static readonly double[] InvigorationBarColor = new double[]
        {
            0.0039215686,
            0.7058823529,
            0.7019607843,
            1.0
        };
    }

    public class EnergyBarHudElement : HudElement
    {
        private bool ShouldShowSleepinessBar
        {
            get
            {
                return ConfigSystem.ConfigClient.SleepinessBarVisible && ConfigSystem.SyncedConfig.EnableSleepiness; 
            }
        }

        private bool ShouldShowEnergyBar
        {
            get
            {
                return ConfigSystem.SyncedConfig.EnableEnergy;
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
            this._energyBar = null;
            this._sleepinessBar = null;
            // this.Dispose();

            if (!this.ShouldShowEnergyBar && !this.ShouldShowSleepinessBar)
            {
                return;
            }
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
            if (this.Composers.ContainsKey("energybar") && this.ShouldShowEnergyBar)
            {
                this.UpdateEnergyBar(false);
            }
            if (this.Composers.ContainsKey("sleepinessbar") && this.ShouldShowSleepinessBar)
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
            float? sleepinessCapacityOverload = sleepinessTree.TryGetFloat("sleepinesscapacityoverload");
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
            this._sleepinessBar.CapacityOverloadFactor = sleepinessCapacityOverload.GetValueOrDefault(11.0f);
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
                bool? fighting = energyTree.TryGetBool("adrenalineindicator"); // added fighting
                double? nullable4 = (nullable2 != null & nullable3 != null) ? new double?((double)nullable2.GetValueOrDefault() / (double)nullable3.GetValueOrDefault()) : null;
                double num = 0.2;
                if ((nullable4.GetValueOrDefault() < num & nullable4 != null) || fighting == true) // added fighting
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
