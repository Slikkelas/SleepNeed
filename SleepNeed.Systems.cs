using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using SleepNeed.Config;
using SleepNeed.HarmonyPatches.CharExtraDialogs;
using SleepNeed.HarmonyPatches.EnergyJumpFactorPatch;
using SleepNeed.HarmonyPatches.SaturationSlowTickPatch;
using SleepNeed.Hud;
using SleepNeed.Sleepiness;
using SleepNeed.Energy;
using SleepNeed.Util;
using HarmonyLib;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Server;
using Vintagestory.GameContent;
using Vintagestory.API.MathTools;
using Newtonsoft.Json.Linq;


namespace SleepNeed.Systems
{
    public static class BtCommands
    {
        public static void Register(ICoreServerAPI api)
        {
            api.ChatCommands.Create("resetEnergyStats").WithDescription("Resets the player's stat modifiers from energy and sleepiness.").RequiresPrivilege("controlserver").WithArgs(new ICommandArgumentParser[]
            {
                api.ChatCommands.Parsers.OptionalWord("playerName")
            }).HandleWith((TextCommandCallingArgs args) => BtCommands.OnResetStatsCommand(api, args));
            api.ChatCommands.Create("setEnergy").WithDescription("Sets the player's energy level.").RequiresPrivilege("controlserver").WithArgs(new ICommandArgumentParser[]
            {
                api.ChatCommands.Parsers.OptionalWord("playerName"),
                api.ChatCommands.Parsers.Float("energyValue")
            }).HandleWith((TextCommandCallingArgs args) => BtCommands.OnSetEnergyCommand(api, args));
            api.ChatCommands.Create("setSleepiness").WithDescription("Sets the player's sleepiness level.").RequiresPrivilege("controlserver").WithArgs(new ICommandArgumentParser[]
            {
                api.ChatCommands.Parsers.OptionalWord("playerName"),
                api.ChatCommands.Parsers.Float("sleepinessValue")
            }).HandleWith((TextCommandCallingArgs args) => BtCommands.OnSetSleepinessCommand(api, args));
            // Added Invigoration command
            api.ChatCommands.Create("setInvigoration").WithDescription("Sets the player's invigoration level.").RequiresPrivilege("controlserver").WithArgs(new ICommandArgumentParser[]
            {
                api.ChatCommands.Parsers.OptionalWord("playerName"),
                api.ChatCommands.Parsers.Float("invigorationValue")
            }).HandleWith((TextCommandCallingArgs args) => BtCommands.OnSetInvigorationCommand(api, args));
        }

        private static TextCommandResult OnResetStatsCommand(ICoreServerAPI api, TextCommandCallingArgs args)
        {
            string playerName = args[0] as string;
            IServerPlayer targetPlayer;
            if (string.IsNullOrEmpty(playerName))
            {
                targetPlayer = (args.Caller.Player as IServerPlayer);
            }
            else
            {
                targetPlayer = BtCommands.GetPlayerByName(api, playerName);
                if (targetPlayer == null)
                {
                    return TextCommandResult.Error("Player '" + playerName + "' not found.", "");
                }
            }
            ConfigSystem.ResetModBoosts((targetPlayer != null) ? targetPlayer.Entity : null);
            return TextCommandResult.Success("Energy stats reset for player '" + targetPlayer.PlayerName + "'.", null);
        }

        private static TextCommandResult OnSetEnergyCommand(ICoreServerAPI api, TextCommandCallingArgs args)
        {
            string playerName = args[0] as string;
            float currentenergylevel = (float)args[1];
            IServerPlayer targetPlayer;
            if (string.IsNullOrEmpty(playerName))
            {
                targetPlayer = (args.Caller.Player as IServerPlayer);
            }
            else
            {
                targetPlayer = BtCommands.GetPlayerByName(api, playerName);
                if (targetPlayer == null)
                {
                    return TextCommandResult.Error("Player '" + playerName + "' not found.", "");
                }
            }
            EntityBehaviorEnergy energyBehavior = (targetPlayer != null) ? targetPlayer.Entity.GetBehavior<EntityBehaviorEnergy>() : null;
            if (energyBehavior == null)
            {
                return TextCommandResult.Error("Energy behavior not found.", "");
            }
            if (energyBehavior != null)
            {
                energyBehavior.CurrentEnergy = currentenergylevel;
                energyBehavior.UpdateEnergyBoosts();
            }
            DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(32, 2);
            defaultInterpolatedStringHandler.AppendLiteral("Energylevel set to ");
            defaultInterpolatedStringHandler.AppendFormatted<float>(currentenergylevel);
            defaultInterpolatedStringHandler.AppendLiteral(" for player '");
            defaultInterpolatedStringHandler.AppendFormatted(targetPlayer.PlayerName);
            defaultInterpolatedStringHandler.AppendLiteral("'.");
            return TextCommandResult.Success(defaultInterpolatedStringHandler.ToStringAndClear(), null);
        }

        private static TextCommandResult OnSetInvigorationCommand(ICoreServerAPI api, TextCommandCallingArgs args)
        {
            string playerName = args[0] as string;
            float invigorationlevel = (float)args[1];
            IServerPlayer targetPlayer;
            if (string.IsNullOrEmpty(playerName))
            {
                targetPlayer = (args.Caller.Player as IServerPlayer);
            }
            else
            {
                targetPlayer = BtCommands.GetPlayerByName(api, playerName);
                if (targetPlayer == null)
                {
                    return TextCommandResult.Error("Player '" + playerName + "' not found.", "");
                }
            }
            EntityBehaviorEnergy energyBehavior = (targetPlayer != null) ? targetPlayer.Entity.GetBehavior<EntityBehaviorEnergy>() : null;
            if (energyBehavior == null)
            {
                return TextCommandResult.Error("Energy behavior not found.", "");
            }
            if (energyBehavior != null)
            {
                energyBehavior.Invigorated = invigorationlevel;
                energyBehavior.UpdateEnergyBoosts();
            }
            DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(32, 2);
            defaultInterpolatedStringHandler.AppendLiteral("invigorated set to ");
            defaultInterpolatedStringHandler.AppendFormatted<float>(invigorationlevel);
            defaultInterpolatedStringHandler.AppendLiteral(" for player '");
            defaultInterpolatedStringHandler.AppendFormatted(targetPlayer.PlayerName);
            defaultInterpolatedStringHandler.AppendLiteral("'.");
            return TextCommandResult.Success(defaultInterpolatedStringHandler.ToStringAndClear(), null);
        }

        private static IServerPlayer GetPlayerByName(ICoreServerAPI api, string playerName)
        {
            foreach (IServerPlayer player in api.World.AllOnlinePlayers)
            {
                if (player.PlayerName.Equals(playerName, StringComparison.OrdinalIgnoreCase))
                {
                    return player;
                }
            }
            return null;
        }

        private static TextCommandResult OnSetSleepinessCommand(ICoreServerAPI api, TextCommandCallingArgs args)
        {
            string playerName = args[0] as string;
            float newLevel = (float)args[1];
            IServerPlayer targetPlayer;
            if (string.IsNullOrEmpty(playerName))
            {
                targetPlayer = (args.Caller.Player as IServerPlayer);
            }
            else
            {
                targetPlayer = BtCommands.GetPlayerByName(api, playerName);
                if (targetPlayer == null)
                {
                    return TextCommandResult.Error("Player '" + playerName + "' not found.", "");
                }
            }
            EntityBehaviorSleepiness sleepinessBehavior = (targetPlayer != null) ? targetPlayer.Entity.GetBehavior<EntityBehaviorSleepiness>() : null;
            if (sleepinessBehavior == null)
            {
                return TextCommandResult.Error("Sleepiness behavior not found.", "");
            }
            sleepinessBehavior.CurrentSleepinessLevel = newLevel;
            DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(31, 2);
            defaultInterpolatedStringHandler.AppendLiteral("Sleepiness set to ");
            defaultInterpolatedStringHandler.AppendFormatted<float>(newLevel);
            defaultInterpolatedStringHandler.AppendLiteral(" for player '");
            defaultInterpolatedStringHandler.AppendFormatted(targetPlayer.PlayerName);
            defaultInterpolatedStringHandler.AppendLiteral("'.");
            return TextCommandResult.Success(defaultInterpolatedStringHandler.ToStringAndClear(), null);
        }
    }

    public class BtCore : ModSystem
    {
        public override void StartPre(ICoreAPI api)
        {
            BtCore._api = api;
            BtCore.Modid = base.Mod.Info.ModID;
            BtCore.Logger = base.Mod.Logger;
            ConfigSystem.StartPre(api);
        }

        public override void Start(ICoreAPI api)
        {

            api.RegisterEntityBehaviorClass(BtCore.Modid + ":energy", typeof(EntityBehaviorEnergy));
            api.RegisterEntityBehaviorClass(BtCore.Modid + ":sleepiness", typeof(EntityBehaviorSleepiness));
            
        }

        public override void StartServerSide(ICoreServerAPI sapi)
        {
            sapi.Event.OnEntitySpawn += new EntityDelegate(this.AddEntityBehaviors);
            sapi.Event.OnEntityLoaded += new EntityDelegate(this.AddEntityBehaviors);
            sapi.Event.PlayerJoin += delegate (IServerPlayer player)
            {
                this.OnPlayerJoin(player.Entity);
            };
            sapi.Event.RegisterEventBusListener(new EventBusListenerDelegate(this.OnConfigReloaded), 0.5, EventIds.ConfigReloaded);
            BtCommands.Register(sapi);
            ConfigSystem.StartServerSide(sapi);
        }

        public override void StartClientSide(ICoreClientAPI capi)
        {
            capi.Gui.RegisterDialog(new GuiDialog[]
            {
                new EnergyBarHudElement(capi)
            });
            ConfigSystem.StartClientSide(capi);
        }
        
        private void OnPlayerJoin(EntityPlayer player)
        {
            ConfigSystem.ResetModBoosts(player);
        }

        private void AddEntityBehaviors(Entity entity)
        {
            if (!(entity is EntityPlayer))
            {
                return;
            }
            this.RemoveEntityBehaviors(entity);
            if (ConfigSystem.ConfigServer.EnableEnergy)
            {
                entity.AddBehavior(new EntityBehaviorEnergy(entity));
            }
            if (ConfigSystem.ConfigServer.EnableSleepiness)
            {
                entity.AddBehavior(new EntityBehaviorSleepiness(entity));
            }
        }

        private void RemoveEntityBehaviors(Entity entity)
        {
            if (!(entity is EntityPlayer))
            {
                return;
            }
            if (!ConfigSystem.ConfigServer.EnableEnergy && entity.HasBehavior<EntityBehaviorEnergy>())
            {
                entity.RemoveBehavior(entity.GetBehavior<EntityBehaviorEnergy>());
            }
            if (!ConfigSystem.ConfigServer.EnableSleepiness && entity.HasBehavior<EntityBehaviorSleepiness>())
            {
                entity.RemoveBehavior(entity.GetBehavior<EntityBehaviorSleepiness>());
            }
        }

        private void OnConfigReloaded(string eventname, ref EnumHandling handling, IAttribute data)
        {
            foreach (IPlayer player in BtCore._api.World.AllPlayers)
            {
                if (player.Entity != null)
                {
                    this.RemoveEntityBehaviors(player.Entity);
                    this.AddEntityBehaviors(player.Entity);
                }
            }
        }

        public override void AssetsFinalize(ICoreAPI api)
        {
            if (!EnumAppSideExtensions.IsServer(api.Side))
            {
                return;
            }
            if (!ConfigSystem.ConfigServer.EnableEnergy)
            {
                return;
            }
            
        }

        public static ILogger Logger;
        
        public static string Modid;

        private static ICoreAPI _api;

        
    }

    public static class ConfigSystem
    {
        public static ConfigServer ConfigServer { get; set; }

        public static ConfigClient ConfigClient { get; set; }

        public static SyncedConfig SyncedConfig { get; set; } = new SyncedConfig();

        public static SyncedConfig SyncedConfigData
        {
            get
            {
                return ConfigSystem.ConfigServer ?? ConfigSystem.SyncedConfig;
            }
        }

        public static void StartPre(ICoreAPI api)
        {
            ConfigSystem._api = api;
            ConfigSystem.SyncedConfig = ModConfig.ReadConfig<SyncedConfig>(api, BtConstants.SyncedConfigName);
            if (EnumAppSideExtensions.IsClient(api.Side))
            {
                ConfigSystem.ConfigClient = ModConfig.ReadConfig<ConfigClient>(api, BtConstants.ConfigClientName);
                return;
            }
            ConfigSystem.ConfigServer = ModConfig.ReadConfig<ConfigServer>(api, BtConstants.ConfigServerName);
        }

        public static void StartClientSide(ICoreClientAPI api)
        {
            IClientNetworkChannel clientNetworkChannel = api.Network.RegisterChannel("sleepneed:config").RegisterMessageType<SyncedConfig>();
            NetworkServerMessageHandler<SyncedConfig> messageHandler;
            if ((messageHandler = ConfigSystem.__ReloadSyncedConfig) == null)
            {
                messageHandler = (ConfigSystem.__ReloadSyncedConfig) = new NetworkServerMessageHandler<SyncedConfig>(ConfigSystem.ReloadSyncedConfig);
            }
            ConfigSystem._clientChannel = clientNetworkChannel.SetMessageHandler<SyncedConfig>(messageHandler);
            IEventAPI @event = api.Event;
            EventBusListenerDelegate eventBusListenerDelegate;
            if ((eventBusListenerDelegate = ConfigSystem.__AdminSendSyncedConfig) == null)
            {
                eventBusListenerDelegate = (ConfigSystem.__AdminSendSyncedConfig) = new EventBusListenerDelegate(ConfigSystem.AdminSendSyncedConfig);
            }
            @event.RegisterEventBusListener(eventBusListenerDelegate, 0.5, EventIds.AdminSetConfig);
        }

        private static void AdminSendSyncedConfig(string eventname, ref EnumHandling handling, IAttribute data)
        {
            IClientNetworkChannel clientChannel = ConfigSystem._clientChannel;
            if (clientChannel == null)
            {
                return;
            }
            clientChannel.SendPacket<SyncedConfig>(ModConfig.ReadConfig<SyncedConfig>(ConfigSystem._api, BtConstants.SyncedConfigName));
        }

        public static void ResetModBoosts(EntityPlayer player)
        {
            if (player == null)
            {
                return;
            }
            SyncedTreeAttribute attributes = player.Attributes;
            if (attributes != null)
            {
                ITreeAttribute treeAttribute = attributes.GetTreeAttribute(BtCore.Modid + ":energy");
                if (treeAttribute != null)
                {
                    treeAttribute.SetFloat("energyloss", 0f);
                }
            }
            player.GetBehavior<EntityBehaviorEnergy>().Energyloss = 0f;
            player.Stats.Remove(BtCore.Modid + ":energyrate", "energyloss");
            player.Stats.Remove(BtCore.Modid + ":energyrate", "resistheat");
            player.Stats.Remove(BtCore.Modid + ":energyrate", "hungryrate");
            player.Stats.Remove(BtCore.Modid + ":energyrate", "sleepinessfull");
            player.Stats.Remove("rangedWeaponsAcc", "sleepinessfull");
            player.Stats.Remove("rangedWeaponsSpeed", "sleepinessfull");
            player.Stats.Remove("walkspeed", "sleepinessfull");
            player.Stats.Remove("hungerrate", "fatigue");
            player.Stats.Remove("miningSpeedMul", "fatigue");
            player.Stats.Remove("jumpHeightMul", "fatigue");
            player.Stats.Remove("walkspeed", "fatigue");
            player.Stats.Remove("rangedWeaponsSpeed", "fatigue");
            player.Stats.Remove("rangedWeaponsDamage", "fatigue");
            player.Stats.Remove("rangedWeaponsDamage", "fatigue");
            player.Stats.Remove("meleeWeaponsDamage", "fatigue");
            player.Stats.Remove("armorWalkSpeedAffectedness", "fatigue");
            player.Stats.Remove("bowDrawingStrength", "fatigue");
            player.Stats.Remove("animalHarvestingTime", "fatigue");

        }

        private static void ReloadSyncedConfig(SyncedConfig packet)
        {
            BtCore.Logger.Warning("Reloading synced config");
            ModConfig.WriteConfig<SyncedConfig>(ConfigSystem._api, BtConstants.SyncedConfigName, packet);
            ConfigSystem.SyncedConfig = packet.Clone();
            if (ConfigSystem.SyncedConfig.ResetModBoosts)
            {
                ICoreClientAPI coreClientAPI = ConfigSystem._api as ICoreClientAPI;
                EntityPlayer player;
                if (coreClientAPI == null)
                {
                    player = null;
                }
                else
                {
                    IClientWorldAccessor world = coreClientAPI.World;
                    if (world == null)
                    {
                        player = null;
                    }
                    else
                    {
                        IClientPlayer player2 = world.Player;
                        player = ((player2 != null) ? player2.Entity : null);
                    }
                }
                ConfigSystem.ResetModBoosts(player);
                ConfigSystem.SyncedConfig.ResetModBoosts = false;
                ModConfig.WriteConfig<SyncedConfig>(ConfigSystem._api, BtConstants.SyncedConfigName, ConfigSystem.SyncedConfig);
            }
            ICoreAPI api = ConfigSystem._api;
            if (api == null)
            {
                return;
            }
            api.Event.PushEvent(EventIds.ConfigReloaded, null);
        }

        public static void StartServerSide(ICoreServerAPI api)
        {
            IServerNetworkChannel serverNetworkChannel = api.Network.RegisterChannel("sleepneed:config").RegisterMessageType<SyncedConfig>();
            NetworkClientMessageHandler<SyncedConfig> messageHandler;
            if ((messageHandler = ConfigSystem.__ForceConfigFromAdmin) == null)
            {
                messageHandler = (ConfigSystem.__ForceConfigFromAdmin = new NetworkClientMessageHandler<SyncedConfig>(ConfigSystem.ForceConfigFromAdmin));
            }
            ConfigSystem._serverChannel = serverNetworkChannel.SetMessageHandler<SyncedConfig>(messageHandler);
            IServerEventAPI @event = api.Event;
            PlayerDelegate playerDelegate;
            if ((playerDelegate = ConfigSystem.__SendSyncedConfig) == null)
            {
                playerDelegate = ConfigSystem.__SendSyncedConfig = new PlayerDelegate(ConfigSystem.SendSyncedConfig);
            }
            @event.PlayerJoin += playerDelegate;
            IEventAPI event2 = api.Event;
            EventBusListenerDelegate eventBusListenerDelegate;
            if ((eventBusListenerDelegate = ConfigSystem.___SendSyncedConfig) == null)
            {
                eventBusListenerDelegate = ConfigSystem.___SendSyncedConfig = new EventBusListenerDelegate(ConfigSystem.SendSyncedConfig);
            }
            event2.RegisterEventBusListener(eventBusListenerDelegate, 0.5, EventIds.ConfigReloaded);
        }

        private static void ForceConfigFromAdmin(IServerPlayer fromplayer, SyncedConfig packet)
        {
            if (fromplayer.HasPrivilege("controlserver"))
            {
                BtCore.Logger.Warning("Forcing config from admin");
                ModConfig.WriteConfig<SyncedConfig>(ConfigSystem._api, BtConstants.SyncedConfigName, packet.Clone());
                ConfigSystem.SyncedConfig = packet;
                ICoreAPI api = ConfigSystem._api;
                if (api == null)
                {
                    return;
                }
                api.Event.PushEvent(EventIds.ConfigReloaded, null);
            }
        }

        private static void SendSyncedConfig(string eventname, ref EnumHandling handling, IAttribute data)
        {
            BtCore.Logger.Warning("Config reloaded, sending to all players");
            ICoreAPI api = ConfigSystem._api;
            if (((api != null) ? api.World : null) == null)
            {
                return;
            }
            IPlayer[] allPlayers = ConfigSystem._api.World.AllPlayers;
            for (int i = 0; i < allPlayers.Length; i++)
            {
                IServerPlayer serverPlayer = allPlayers[i] as IServerPlayer;
                if (serverPlayer != null)
                {
                    ConfigSystem.SendSyncedConfig(serverPlayer);
                }
            }
        }

        private static void SendSyncedConfig(IServerPlayer byplayer)
        {
            BtCore.Logger.Warning("Sending config to player: {0}", new object[]
            {
                byplayer.PlayerName
            });
            IServerNetworkChannel serverChannel = ConfigSystem._serverChannel;
            if (serverChannel == null)
            {
                return;
            }
            serverChannel.SendPacket<SyncedConfig>(ModConfig.ReadConfig<SyncedConfig>(ConfigSystem._api, BtConstants.SyncedConfigName), new IServerPlayer[]
            {
                byplayer
            });
        }

        private static IClientNetworkChannel _clientChannel;

        private static IServerNetworkChannel _serverChannel;

        private const string _channelName = "sleepneed:config";

        private static ICoreAPI _api;

        [CompilerGenerated]
        public static NetworkServerMessageHandler<SyncedConfig> __ReloadSyncedConfig;

        public static EventBusListenerDelegate __AdminSendSyncedConfig;

        public static NetworkClientMessageHandler<SyncedConfig> __ForceConfigFromAdmin;

        public static PlayerDelegate __SendSyncedConfig;

        public static EventBusListenerDelegate ___SendSyncedConfig;


    }


    public class HarmonyPatches : ModSystem
    {
        public override double ExecuteOrder()
        {
            return 1.00;
        }

        public override void Start(ICoreAPI api)
        {
            this._api = api;
            HarmonyPatches.Patch();
            SyncedConfig syncedConfigData = ConfigSystem.SyncedConfigData;
            HarmonyPatches.HarmonyInstance.Patch(typeof(CharacterExtraDialogs).GetMethod("Dlg_ComposeExtraGuis", BindingFlags.Instance | BindingFlags.NonPublic), null, typeof(CharacterExtraDialogs_Dlg_ComposeExtraGuis_Patch).GetMethod("Postfix"), null, null);
            HarmonyPatches.HarmonyInstance.Patch(typeof(CharacterExtraDialogs).GetMethod("UpdateStats", BindingFlags.Instance | BindingFlags.NonPublic), null, typeof(CharacterExtraDialogs_UpdateStats_Patch).GetMethod("Postfix"), null, null);
            HarmonyPatches.HarmonyInstance.Patch(typeof(CharacterExtraDialogs).GetMethod("UpdateStats", BindingFlags.Instance | BindingFlags.NonPublic), null, typeof(CharacterExtraDialogs_UpdateStatBars_Patch).GetMethod("Postfix"), null, null);
            // Patch to ensure that player can jump lower than a block.
            MethodInfo targetDoApplyMethod = AccessTools.Method(typeof(PModuleOnGround), "DoApply", new Type[] { typeof(float), typeof(Entity), typeof(EntityPos), typeof(EntityControls) } );
            if (targetDoApplyMethod == null)
            {
                api.Logger.Error("Harmony Patch Error: Could not find target method 'PModuleOnGround.DoApply'. EnergyJumpFactor patch failed!");
                return;
            }
            HarmonyPatches.HarmonyInstance.Patch
            (
                original: targetDoApplyMethod,
                transpiler: new HarmonyMethod(typeof(EnergyJumpFactorPatch), nameof(EnergyJumpFactorPatch.JumpFactorTranspilerMethod))
            );
            api.Logger.Notification("All Harmony patches applied successfully, including EnergyJumpFactorPatch!");

            // Patch to clamp the saturation to 0.1f
            if (ConfigSystem.ConfigServer.OnlyDieFromNoEnergy)
            {
                HarmonyInstance.Patch
                (
                    original: AccessTools.Method(typeof(Vintagestory.GameContent.EntityBehaviorHunger), "SlowTick"),
                    transpiler: new HarmonyMethod(typeof(SaturationSlowTickPatch), nameof(SaturationSlowTickPatch.HungerDamageTranspilerMethod))
                );
            }
        }

        public override void Dispose()
        {
            HarmonyPatches.Unpatch();
        }

        public static void Patch()
        {
            if (HarmonyPatches.HarmonyInstance != null)
            {
                return;
            }
            HarmonyPatches.HarmonyInstance = new Harmony(BtCore.Modid);
        }

        public static void Unpatch()
        {
            if (HarmonyPatches.HarmonyInstance == null)
            {
                return;
            }
            HarmonyPatches.HarmonyInstance.UnpatchAll(null);
            HarmonyPatches.HarmonyInstance = null;
        }

        private ICoreAPI _api;

        private static Harmony HarmonyInstance;
    }

    


    
    }
