using System;
using System.Collections.Generic;
using SleepNeed.Config;
using SleepNeed.Sleepiness;
using System.Linq;
using SleepNeed.Systems;
using SleepNeed.Energy;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Newtonsoft.Json.Linq;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.GameContent;


namespace SleepNeed.Util
{
    public static class BtConstants
    {

        public static readonly string ConfigServerName = "SleepNeed/sleepneed.json";

        public static readonly string ConfigClientName = "SleepNeed/sleepneed_client.json";

        public static readonly string SyncedConfigName = "SleepNeed/sleepneed_sync.json";

        public static string FatiguedEffectId = BtCore.Modid + ":fatigued";

    }

    



        public enum EnumBuffCurve
    {
        None,
        Linear,
        Sin,
        Asin,
        Cubic,
        InverseCubic,
        Quintic,
        InverseQuintic,
        Flat0,
        Flat1
    }

    



    public enum EnumUpOrDown
    {
        Centered,
        Up,
        Down
    }

    public class EventIds
    {
        public static readonly string Interaction = BtCore.Modid + ":interaction";

        public static readonly string ConfigReloaded = "sleepneed:configreloaded";

        public static readonly string AdminSetConfig = "sleepneed:adminsetconfig";
    }

    public static class Extensions
    {
        public static string Localize(this string input, params object[] args)
        {
            return Lang.Get(input, args);
        }
        
        public static void IngameError(this IPlayer byPlayer, object sender, string errorCode, string text)
        {
            ICoreClientAPI coreClientAPI = byPlayer.Entity.World.Api as ICoreClientAPI;
            if (coreClientAPI == null)
            {
                return;
            }
            coreClientAPI.TriggerIngameError(sender, errorCode, text);
        }

        public static bool IsSleepinessOverloaded(this IPlayer player)
        {
            ITreeAttribute sleepinessTree = player.Entity.WatchedAttributes.GetTreeAttribute(BtCore.Modid + ":sleepiness");
            if (sleepinessTree == null)
            {
                return false;
            }
            float? currentsleepinessLevel = sleepinessTree.TryGetFloat("currentsleepinesslevel");
            float? sleepinessCapacity = sleepinessTree.TryGetFloat("sleepinesscapacity");
            if (currentsleepinessLevel == null || sleepinessCapacity == null)
            {
                return false;
            }
            float? num = currentsleepinessLevel;
            float? num2 = sleepinessCapacity;
            return num.GetValueOrDefault() > num2.GetValueOrDefault() & (num != null & num2 != null);
        }

        public static bool IsEnergyMaxed(this Entity entity)
        {
            ITreeAttribute energyTree = entity.WatchedAttributes.GetTreeAttribute(BtCore.Modid + ":energy");
            if (energyTree == null)
            {
                return false;
            }
            float? currentEnergyLevel = energyTree.TryGetFloat("currentenergylevel");
            float? maxEnergy = energyTree.TryGetFloat("maxenergy");
            if (currentEnergyLevel == null || maxEnergy == null)
            {
                return false;
            }
            float? num = currentEnergyLevel;
            float? num2 = maxEnergy;
            return num.GetValueOrDefault() >= num2.GetValueOrDefault() & (num != null & num2 != null);
        }

    }

    public static class Func
    {
        public static float StatModifierSin(float ratio, float param)
        {
            return param * (float)Math.Sin(1.5707963267948966 * (double)(1f - 2f * ratio));
        }

        public static float StatModifierLinear(float ratio, float param)
        {
            return param * (1f - 2f * ratio);
        }

        public static float StatModifierArcSin(float ratio, float param)
        {
            return param * (float)(6.283185307179586 * Math.Asin((double)(1f - 2f * ratio)));
        }

        public static float StatModifierCubic(float ratio, float param)
        {
            return param * (float)Math.Pow((double)(1f - 2f * ratio), 3.0);
        }

        public static float StatModifierICubic(float ratio, float param)
        {
            return -param * (float)Math.Pow((double)Math.Abs(1f - 2f * ratio), 0.3333333333333333);
        }

        public static float StatModifierQuintic(float ratio, float param)
        {
            return param * (float)Math.Pow((double)(1f - 2f * ratio), 5.0);
        }

        public static float StatModifierIQuintic(float ratio, float param)
        {
            return -param * (float)Math.Pow((double)Math.Abs(1f - 2f * ratio), 0.2);
        }

        public static float CalcStatModifier(float ratio, float param, EnumBuffCurve curveType, EnumUpOrDown centering = EnumUpOrDown.Centered)
        {
            float num;
            switch (curveType)
            {
                case EnumBuffCurve.Linear:
                    num = Func.StatModifierLinear(ratio, param);
                    break;
                case EnumBuffCurve.Sin:
                    num = Func.StatModifierSin(ratio, param);
                    break;
                case EnumBuffCurve.Asin:
                    num = Func.StatModifierArcSin(ratio, param);
                    break;
                case EnumBuffCurve.Cubic:
                    num = Func.StatModifierCubic(ratio, param);
                    break;
                case EnumBuffCurve.InverseCubic:
                    num = Func.StatModifierICubic(ratio, param);
                    break;
                case EnumBuffCurve.Quintic:
                    num = Func.StatModifierQuintic(ratio, param);
                    break;
                case EnumBuffCurve.InverseQuintic:
                    num = Func.StatModifierIQuintic(ratio, param);
                    break;
                case EnumBuffCurve.Flat0:
                    num = 0f;
                    break;
                case EnumBuffCurve.Flat1:
                    num = param * (float)Math.Sign(0.5f - ratio);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("curveType", curveType, null);
            }
            float res = num;
            if (centering == EnumUpOrDown.Centered)
            {
                return res;
            }
            if (centering != EnumUpOrDown.Up)
            {
                return 0.5f * (res - param);
            }
            return 0.5f * (res + param);
        }
    }

    public class StatMultiplier
    {
        public float Multiplier { get; set; }

        public EnumUpOrDown Centering { get; set; }

        public EnumBuffCurve Curve { get; set; }

        public EnumBuffCurve LowishCurve { get; set; } = EnumBuffCurve.Linear;

        public EnumBuffCurve LowestCurve { get; set; } = EnumBuffCurve.Asin;

        public EnumBuffCurve UpperHalfCurve { get; set; }

        public bool Inverted { get; set; }

        public float CalcModifier(float ratio)
        {
            if (this.Inverted)
            {
                ratio = 1f - ratio;
            }
            if (this.UpperHalfCurve == EnumBuffCurve.None)
            {
                return Func.CalcStatModifier(ratio, this.Multiplier, this.Curve, this.Centering);
            }
            if (ratio < 0.5 && ratio > 0.3)
            {
                return Func.CalcStatModifier(ratio, this.Multiplier, this.LowishCurve, this.Centering);
            }
            if ((double)ratio < 0.3)
            {
                return Func.CalcStatModifier(ratio, this.Multiplier, this.LowestCurve, this.Centering);
            }
            return Func.CalcStatModifier(ratio, this.Multiplier, this.UpperHalfCurve, this.Centering);
        }
    }


}
