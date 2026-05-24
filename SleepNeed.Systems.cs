using HarmonyLib;
using Newtonsoft.Json.Linq;
using ProtoBuf;
using SleepNeed.Config;
using SleepNeed.Energy;
using SleepNeed.HarmonyPatches.CharExtraDialogs;
using SleepNeed.HarmonyPatches.EnergyJumpFactorPatch;
using SleepNeed.HarmonyPatches.SaturationSlowTickPatch;
using SleepNeed.HarmonyPatches.MiningWhenSittingPatch;
using SleepNeed.HarmonyPatches.BreathePatches;
using SleepNeed.Hud;
using SleepNeed.Sleepiness;
using SleepNeed.Util;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.GameContent;


namespace SleepNeed.Systems
{
    public static class BtCommands
    {
        public static void Register(ICoreServerAPI api)
        {
            api.ChatCommands.Create("resetSleepNeedStats").WithDescription("Resets the player's stat modifiers from energy and sleepiness.").RequiresPrivilege("controlserver").WithArgs(new ICommandArgumentParser[]
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
            // Added Tiredness command
            api.ChatCommands.Create("setTiredness").WithDescription("Sets the player's tiredness level.").RequiresPrivilege("controlserver").WithArgs(new ICommandArgumentParser[]
            {
                api.ChatCommands.Parsers.OptionalWord("playerName"),
                api.ChatCommands.Parsers.Float("tirednessValue")
            }).HandleWith((TextCommandCallingArgs args) => BtCommands.OnSetTirednessCommand(api, args));
            // Added Config Reload command
            api.ChatCommands.Create("sleepneedreload")
                .WithDescription("Reloads SleepNeed config from sleepneed.json and syncs to players.")
                .RequiresPrivilege("controlserver")
                .HandleWith((TextCommandCallingArgs args) => OnReloadCommand(api, args));
        }

        // This contains the logic to reload and sync, accessible from anywhere.
        public static void ForceReloadConfig(ICoreServerAPI api)
        {
            try
            {
                var diffConfig = ModConfig.ReadConfig<ConfigDifficulty>(api, "sleepneed_difficulty.json");
                var easyConfig = ModConfig.ReadConfig<ConfigServerEasy>(api, "sleepneed_easy.json");
                var normalConfig = ModConfig.ReadConfig<ConfigServerNormal>(api, "sleepneed_normal.json");
                var hardConfig = ModConfig.ReadConfig<ConfigServerHard>(api, "sleepneed_hard.json");
                if (diffConfig.DifficultyMode.ToLower() == "easy")
                {
                    ConfigSystem.ConfigServer = easyConfig;
                    BtCore.Logger.Notification("SleepNeed: Reloaded Easy Mode Config.");
                }
                else if (diffConfig.DifficultyMode.ToLower() == "hard")
                {
                    ConfigSystem.ConfigServer = hardConfig;
                    BtCore.Logger.Notification("SleepNeed: Reloaded Hard Mode Config.");
                }
                else
                {
                    ConfigSystem.ConfigServer = normalConfig;
                    BtCore.Logger.Notification("SleepNeed: Reloaded Normal Mode Config.");
                }
                ConfigSystem.SyncedConfig = ConfigSystem.ConfigServer.ToSyncedConfig();
                ConfigSystem.ConfigLoaded = true;

                // This triggers the event bus, which updates behaviors AND sends the network packet to clients
                api.Event.PushEvent(EventIds.ConfigReloaded, null);
            }
            catch (Exception e)
            {
                BtCore.Logger.Error($"SleepNeed: Failed to auto-reload config on player joining. {e.Message}");
            }
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
            ConfigSystem.DisableStatChanges((targetPlayer != null) ? targetPlayer.Entity : null);
            return TextCommandResult.Success("SleepNeed: Energy stats reset for player '" + targetPlayer.PlayerName + "'.", null);
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
                return TextCommandResult.Error("SleepNeed: Energy behavior not found.", "");
            }
            if (energyBehavior != null)
            {
                energyBehavior.CurrentEnergy = currentenergylevel;
                energyBehavior.UpdateEnergyBoosts();
            }
            DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(32, 2);
            defaultInterpolatedStringHandler.AppendLiteral("SleepNeed: Energylevel set to ");
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
                return TextCommandResult.Error("SleepNeed: Energy behavior not found.", "");
            }
            if (energyBehavior != null)
            {
                energyBehavior.Invigorated = invigorationlevel;
                energyBehavior.UpdateEnergyBoosts();
            }
            DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(32, 2);
            defaultInterpolatedStringHandler.AppendLiteral("SleepNeed: invigorated set to ");
            defaultInterpolatedStringHandler.AppendFormatted<float>(invigorationlevel);
            defaultInterpolatedStringHandler.AppendLiteral(" for player '");
            defaultInterpolatedStringHandler.AppendFormatted(targetPlayer.PlayerName);
            defaultInterpolatedStringHandler.AppendLiteral("'.");
            return TextCommandResult.Success(defaultInterpolatedStringHandler.ToStringAndClear(), null);
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
                return TextCommandResult.Error("SleepNeed: Sleepiness behavior not found.", "");
            }
            sleepinessBehavior.CurrentSleepinessLevel = newLevel;
            DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(31, 2);
            defaultInterpolatedStringHandler.AppendLiteral("SleepNeed: Sleepiness set to ");
            defaultInterpolatedStringHandler.AppendFormatted<float>(newLevel);
            defaultInterpolatedStringHandler.AppendLiteral(" for player '");
            defaultInterpolatedStringHandler.AppendFormatted(targetPlayer.PlayerName);
            defaultInterpolatedStringHandler.AppendLiteral("'.");
            return TextCommandResult.Success(defaultInterpolatedStringHandler.ToStringAndClear(), null);
        }

        private static TextCommandResult OnSetTirednessCommand(ICoreServerAPI api, TextCommandCallingArgs args)
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
            EntityBehaviorTiredness tirednessBehavior = (targetPlayer != null) ? targetPlayer.Entity.GetBehavior<EntityBehaviorTiredness>() : null;
            if (tirednessBehavior == null)
            {
                return TextCommandResult.Error("SleepNeed: Tiredness behavior not found.", "");
            }
            tirednessBehavior.Tiredness = newLevel;
            DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(31, 2);
            defaultInterpolatedStringHandler.AppendLiteral("SleepNeed: Tiredness set to ");
            defaultInterpolatedStringHandler.AppendFormatted<float>(newLevel);
            defaultInterpolatedStringHandler.AppendLiteral(" for player '");
            defaultInterpolatedStringHandler.AppendFormatted(targetPlayer.PlayerName);
            defaultInterpolatedStringHandler.AppendLiteral("'.");
            return TextCommandResult.Success(defaultInterpolatedStringHandler.ToStringAndClear(), null);
        }

        private static TextCommandResult OnReloadCommand(ICoreServerAPI api, TextCommandCallingArgs args)
        {
            // Force read from disk
            try
            {
                BtCommands.ForceReloadConfig(api);
                return TextCommandResult.Success("SleepNeed: Configuration reloaded and synchronized.");
            }
            catch (Exception e)
            {
                return TextCommandResult.Error($"SleepNeed: Failed to reload config. {e.Message}");
            }
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
            this.sapi = sapi;
            sapi.Network.RegisterChannel("SleepNeedAdrenaline").RegisterMessageType<AdrenalineMessagePacket>();
            
            _groundBedChannelServer = sapi.Network.RegisterChannel("SleepNeedGroundBed").RegisterMessageType<PlaceGroundBedPacket>().SetMessageHandler<PlaceGroundBedPacket>(OnReceivePlaceGroundBedPacket);
            _groundBedBuildStateChannelServer = sapi.Network.RegisterChannel("SleepNeedGroundBedBuildState").RegisterMessageType<BuildingGroundBedPacket>().SetMessageHandler<BuildingGroundBedPacket>(OnReceiveGroundBedBuildStatePacket);
            
            _serverSleepCheckListenerId = sapi.Event.RegisterGameTickListener(OnServerGameTick, 1000);
            sapi.Event.PlayerLeave += OnPlayerLeave;
            
            sapi.Event.OnEntitySpawn += new EntityDelegate(this.AddEntityBehaviors);
            sapi.Event.OnEntityLoaded += new EntityDelegate(this.AddEntityBehaviors);
            sapi.Event.PlayerJoin += delegate (IServerPlayer player)
            {
                this.OnPlayerJoin(player.Entity);
                BtCommands.ForceReloadConfig(sapi);
            };
            sapi.Event.RegisterEventBusListener(new EventBusListenerDelegate(this.OnConfigReloaded), 0.5, EventIds.ConfigReloaded);
            BtCommands.Register(sapi);
            ConfigSystem.StartServerSide(sapi);
        }

        public override void StartClientSide(ICoreClientAPI capi)
        {
            this.capi = capi;
            capi.Network.RegisterChannel("SleepNeedAdrenaline").RegisterMessageType<AdrenalineMessagePacket>().SetMessageHandler<AdrenalineMessagePacket>(OnClientReceivesAdrenalineMessage);
            
            _groundBedChannelClient = capi.Network.RegisterChannel("SleepNeedGroundBed").RegisterMessageType<PlaceGroundBedPacket>();
            
            capi.Gui.RegisterDialog(new GuiDialog[]
            {
                new EnergyBarHudElement(capi)
            });

            capi.Event.RegisterEventBusListener(new EventBusListenerDelegate(this.OnConfigReloaded), 0.5, EventIds.ConfigReloaded);
            ConfigSystem.StartClientSide(capi);
            _groundBedTickListenerId = capi.Event.RegisterGameTickListener(OnClientGameTick, 20); // 20ms = 50x i sekundet
            _groundBedBuildStateChannelClient = capi.Network.RegisterChannel("SleepNeedGroundBedBuildState").RegisterMessageType<BuildingGroundBedPacket>();

            capi.ChatCommands.Create("sleepneedclientreload")
                .WithDescription("Reloads local client settings (HUD positions, colors).")
                .HandleWith((TextCommandCallingArgs args) =>
            {
                try
                {
            
                    var newConfig = ModConfig.ReadConfig<ConfigClient>(capi, BtConstants.ConfigClientName);

                    if (newConfig != null)
                    {
                        ConfigSystem.ConfigClient = newConfig;
                        capi.Event.PushEvent(EventIds.ConfigReloaded, null);

                        return TextCommandResult.Success("Client config reloaded and HUD updated.");
                    }
                    else
                    {
                        return TextCommandResult.Error("Failed to read client config file.");
                    }
                }
                catch (Exception e)
                {
                        return TextCommandResult.Error($"Error reloading client config: {e.Message}");
                }
            });
        }

        private void OnClientGameTick(float dt)
        {
            if (capi?.World?.Player == null) return;

            IClientPlayer player = capi.World.Player;
            EntityPlayer playerEntity = player.Entity;

            if (playerEntity == null) return;

            
            bool sneakHeld = capi.Input.IsHotKeyPressed("sneak");

            bool leftClickHeld = capi.Input.InWorldMouseButton.Left;

            bool handEmpty = player.InventoryManager.ActiveHotbarSlot.Empty;

            bool isSitting = player.Entity.Controls.FloorSitting;

            bool isCurrentlyBuilding = sneakHeld && leftClickHeld && handEmpty && isSitting;

            var energyBehavior = playerEntity.GetBehavior<EntityBehaviorEnergy>();
            var sleepinessBehavior = playerEntity.GetBehavior<EntityBehaviorSleepiness>();
            if (energyBehavior != null)
            {
                energyBehavior.PlayerIsBuildingGroundBed = isCurrentlyBuilding;
            }
            else if (energyBehavior == null && sleepinessBehavior != null)
            {
                sleepinessBehavior.PlayerIsBuildingGroundBed = isCurrentlyBuilding;
            }

            if (isCurrentlyBuilding != _wasBuildingGroundBedLastTick)
            {
                _groundBedBuildStateChannelClient?.SendPacket(new BuildingGroundBedPacket { IsBuildingGroundBed = isCurrentlyBuilding });
                _wasBuildingGroundBedLastTick = isCurrentlyBuilding;
            }
           
            if (isCurrentlyBuilding)
            {
                BlockSelection currentTarget = player.CurrentBlockSelection;

                if (currentTarget != null)
                {
                    if (_lastTargetedBlock != null &&
                        _lastTargetedBlock.Position.Equals(currentTarget.Position) &&
                        _lastTargetedBlock.Face.Equals(currentTarget.Face))
                    {
                        _groundBedPlacementTimer += dt;

                        if (_groundBedPlacementTimer >= 5.0f)
                        {
                            SendPlaceGroundBedPacket(currentTarget);
                            _groundBedPlacementTimer = 0f;
                            _lastTargetedBlock = null;
                        }
                    }
                    else
                    {
                        _groundBedPlacementTimer = 0f;
                        _lastTargetedBlock = currentTarget.Clone(); 
                    }
                }
                else
                {
                    _groundBedPlacementTimer = 0f;
                    _lastTargetedBlock = null;
                }
            }
            else
            {
                _groundBedPlacementTimer = 0f;
                _lastTargetedBlock = null;
            }
        }

        
        private void OnServerGameTick(float dt)
        {
            if (sapi == null) return;

            foreach (IServerPlayer player in sapi.World.AllOnlinePlayers)
            {
                if (player.ConnectionState != EnumClientState.Playing || player.Entity == null) continue;
                string uid = player.PlayerUID;
                
                if (_temporaryBedsByPlayerUID.TryGetValue(uid, out BlockPos bedPos))
                {
                    double maxDistance = 20.0;

                    double distSq = player.Entity.Pos.SquareDistanceTo(bedPos.X + 0.5, bedPos.Y, bedPos.Z + 0.5);

                    if (distSq > (maxDistance * maxDistance))
                    {
                        DestroyTemporaryBed(player.Entity.World, bedPos, uid);
                        
                    }
                }
                
                var tirednessBehavior = player.Entity.GetBehavior<EntityBehaviorTiredness>();

                if (tirednessBehavior == null) continue;

                bool isCurrentlySleeping = tirednessBehavior.IsSleeping;
                bool wasSleeping = _playerWasSleeping.ContainsKey(uid) && _playerWasSleeping[uid];

                if (wasSleeping && !isCurrentlySleeping)
                {
                    if (_temporaryBedsByPlayerUID.TryGetValue(uid, out BlockPos existingBedPos))
                    {
                        DestroyTemporaryBed(player.Entity.World, bedPos, uid);
                    }
                }

                _playerWasSleeping[uid] = isCurrentlySleeping;
            }
        }

        
        private void DestroyTemporaryBed(IWorldAccessor world, BlockPos bedPos, string playerUid)
        {
            Block block = world.BlockAccessor.GetBlock(bedPos);
            if (block.Code.Domain == BtCore.Modid && block.Code.Path.StartsWith("groundbed-head"))
            {
                world.BlockAccessor.SetBlock(0, bedPos);
                sapi.Logger.Notification($"[SleepNeed] Removed temporary groundbed for player {playerUid} at {bedPos}.");
            }

            if (_temporaryBedsByPlayerUID.ContainsKey(playerUid))
            {
                _temporaryBedsByPlayerUID.Remove(playerUid);
            }
        }

        private void OnReceiveGroundBedBuildStatePacket(IServerPlayer player, BuildingGroundBedPacket packet)
        {
            if (player.Entity == null) return;
            var energyBehavior = player.Entity?.GetBehavior<EntityBehaviorEnergy>();
            if (energyBehavior != null)
            {
                energyBehavior.PlayerIsBuildingGroundBed = packet.IsBuildingGroundBed;
            }
            var sleepinessBehavior = player.Entity.GetBehavior<EntityBehaviorSleepiness>();
            if (sleepinessBehavior != null)
            {
                sleepinessBehavior.PlayerIsBuildingGroundBed = packet.IsBuildingGroundBed;
            }
        }

        private void SendPlaceGroundBedPacket(BlockSelection selection)
        {
            var packet = new PlaceGroundBedPacket
            {
                Position = selection.Position,
                FaceIndex = selection.Face.Index
            };
            _groundBedChannelClient?.SendPacket(packet);
        }

        private void OnReceivePlaceGroundBedPacket(IServerPlayer player, PlaceGroundBedPacket packet)
        {
            try
            {
                BlockFacing face = BlockFacing.ALLFACES[packet.FaceIndex];
                if (face == null)
                {
                    sapi.Logger.Warning($"[SleepNeed] Player {player.PlayerName} sent invalid FaceIndex {packet.FaceIndex}.");
                    return;
                }

                if (player.Entity == null)
                {
                    sapi.Logger.Error($"[SleepNeed] GroundBed place failed: player.Entity is null for player {player.PlayerName}.");
                    return;
                }
                
                if (player.Entity.Pos == null)
                {
                    sapi.Logger.Error($"[SleepNeed] GroundBed place failed: player.Entity.Pos is null for player {player.PlayerName}.");
                    return;
                }
                
                if (!player.InventoryManager.ActiveHotbarSlot.Empty)
                {
                    sapi.Logger.Warning($"[SleepNeed] Player {player.PlayerName} tried to place groundbed with item in hand.");
                    return;
                }

                BlockPos placePos = packet.Position.AddCopy(face);
                IWorldAccessor world = player.Entity.World;

                if (player.Entity.Pos.DistanceTo(placePos.ToVec3d()) > player.WorldData.PickingRange + 1)
                {
                    sapi.Logger.Warning($"[SleepNeed] Player {player.PlayerName} tried to place groundbed too far away.");
                    return;
                }

                string orientation = BlockFacing.HorizontalFromYaw(player.Entity.Pos.Yaw).Code;

                string headBlockCode = "groundbed-head-" + orientation;
                AssetLocation blockCodeToPlace = new AssetLocation(BtCore.Modid, headBlockCode);
                Block headBlockToPlace = world.GetBlock(blockCodeToPlace);

                if (headBlockToPlace == null)
                {
                    sapi.Logger.Error($"[SleepNeed] GroundBed place failed: Could not find the specific block '{blockCodeToPlace}'.");
                    return;
                }

                Block blockAtPlacePos = world.BlockAccessor.GetBlock(placePos);
                if (blockAtPlacePos == null)
                {
                    sapi.Logger.Error($"[SleepNeed] GroundBed place failed: world.BlockAccessor.GetBlock() returned null at position {placePos}.");
                    return;
                }

                BlockSelection bSel = new BlockSelection
                {
                    Position = placePos,
                    Face = face.Opposite,
                    DidOffset = true,
                    Block = blockAtPlacePos
                };

                string failureCode = "";
                bool canPlace = headBlockToPlace.CanPlaceBlock(world, player, bSel, ref failureCode);
                if (canPlace || failureCode == "entityobstructing")
                {
                    ItemStack groundBedStack = new ItemStack(headBlockToPlace);
                    headBlockToPlace.DoPlaceBlock(world, player, bSel, groundBedStack);
                    _temporaryBedsByPlayerUID[player.PlayerUID] = placePos;
                }
                else
                {
                    _groundBedChannelServer?.SendPacket(new AdrenalineMessagePacket { Code = "sn_groundbed_fail", Message = Lang.Get(failureCode) }, player);
                }
            }
            catch (Exception e)
            {
                sapi.Logger.Error($"[SleepNeed] Exception in OnReceivePlaceGroundBedPacket: {e}");
            }
        }


        public override void Dispose()
        {
            if (capi != null)
            {
                capi.Event.UnregisterGameTickListener(_groundBedTickListenerId);
            }
            
            if (sapi != null)
            {
                sapi.Event.UnregisterGameTickListener(_serverSleepCheckListenerId);
                sapi.Event.PlayerLeave -= OnPlayerLeave;
                sapi.Event.OnEntitySpawn -= this.AddEntityBehaviors;
                sapi.Event.OnEntityLoaded -= this.AddEntityBehaviors;
            }
            
            ConfigSystem.ConfigLoaded = false;
            ConfigSystem.SyncedConfig = null; 
            
            _groundBedChannelClient = null;
            _groundBedChannelServer = null;
            _lastTargetedBlock = null;
            base.Dispose();
        }

        private void OnPlayerJoin(EntityPlayer player)
        {
            ConfigSystem.DisableStatChanges(player);
            BtCommands.ForceReloadConfig(sapi);
        }

        private void OnPlayerLeave(IServerPlayer player)
        {
            string uid = player.PlayerUID;

            if (_playerWasSleeping.ContainsKey(uid))
            {
                _playerWasSleeping.Remove(uid);
            }

            if (_temporaryBedsByPlayerUID.TryGetValue(uid, out BlockPos bedPos))
            {
                DestroyTemporaryBed(player.Entity.World, bedPos, uid);
            }
        }
        private void AddEntityBehaviors(Entity entity)
        {
            if (!(entity is EntityPlayer))
            {
                return;
            }
            var energyBehaviorExists = entity.GetBehavior<EntityBehaviorEnergy>();
            if (energyBehaviorExists == null)
            {
                EntityBehaviorEnergy energyBehavior = new EntityBehaviorEnergy(entity);
                entity.AddBehavior(energyBehavior);
                energyBehavior.Initialize(entity.Properties, new JsonObject(new JObject())); 
            }
            var sleepinessBehaviorExists = entity.GetBehavior<EntityBehaviorSleepiness>();
            if (sleepinessBehaviorExists == null)
            {
                EntityBehaviorSleepiness sleepinessBehavior = new EntityBehaviorSleepiness(entity);
                entity.AddBehavior(sleepinessBehavior);
                sleepinessBehavior.Initialize(entity.Properties, new JsonObject(new JObject()));
            }

        }


        private void OnConfigReloaded(string eventname, ref EnumHandling handling, IAttribute data)
        {
            if (BtCore._api == null) return;

            foreach (IPlayer player in BtCore._api.World.AllPlayers)
            {
                EntityPlayer entity = player.Entity;
                if (entity != null)
                {
                    var energyBehavior = entity.GetBehavior<EntityBehaviorEnergy>();
                    if (energyBehavior != null)
                    {
                        energyBehavior.Initialize(entity.Properties, null);
                    }

                    var sleepinessBehavior = entity.GetBehavior<EntityBehaviorSleepiness>();
                    if (sleepinessBehavior != null)
                    {
                        sleepinessBehavior.Initialize(entity.Properties, null);
                    }
                }
            }
        }



        private void OnClientReceivesAdrenalineMessage(AdrenalineMessagePacket packet)
        {
            if (this.capi != null)
            {
                this.capi.TriggerIngameError(this, packet.Code, packet.Message);

                this.capi.Logger.Notification($"[SleepNeed: Adrenaline-SUCCESS] HUD message displayed: {packet.Code}");
            }
        }

        public override void AssetsFinalize(ICoreAPI api)
        {
            if (!EnumAppSideExtensions.IsServer(api.Side))
            {
                return;
            }
            

        }

        public static ILogger Logger;

        public static string Modid;

        private static ICoreAPI _api;

        public ICoreClientAPI capi { get; private set; }

        public ICoreServerAPI sapi { get; private set; }

        private IClientNetworkChannel _groundBedChannelClient;
        private IServerNetworkChannel _groundBedChannelServer;
        private float _groundBedPlacementTimer = 0f;
        private BlockSelection _lastTargetedBlock = null;
        private long _groundBedTickListenerId;
        private long _serverSleepCheckListenerId;
        private Dictionary<string, BlockPos> _temporaryBedsByPlayerUID = new Dictionary<string, BlockPos>();
        private Dictionary<string, bool> _playerWasSleeping = new Dictionary<string, bool>();
        private IClientNetworkChannel _groundBedBuildStateChannelClient;
        private IServerNetworkChannel _groundBedBuildStateChannelServer;
        private bool _wasBuildingGroundBedLastTick = false;
    }

    [ProtoContract]
    public class AdrenalineMessagePacket
    {
        [ProtoMember(1)]
        public string Code { get; set; }

        [ProtoMember(2)]
        public string Message { get; set; }
    }

    [ProtoContract]
    public class PlaceGroundBedPacket
    {
        [ProtoMember(1)]
        public BlockPos Position { get; set; } 

        [ProtoMember(2)]
        public int FaceIndex { get; set; } 
    }

    [ProtoContract]
    public class BuildingGroundBedPacket 
    {
        [ProtoMember(1)]
        public bool IsBuildingGroundBed { get; set; }
    }

    public static class ConfigSystem
    {
        public static bool ConfigLoaded = false;

        public static ConfigServer ConfigServer { get; set; }

        public static ConfigClient ConfigClient { get; set; }

        public static SyncedConfig SyncedConfig { get; set; }

        public static SyncedConfig SyncedConfigData
        {
            get
            {
                return ConfigSystem.SyncedConfig;
            }
        }

        public static void StartPre(ICoreAPI api)
        {
            ConfigSystem._api = api;
            if (api.Side == EnumAppSide.Server)
            {
                var diffConfig = ModConfig.ReadConfig<ConfigDifficulty>(api, "sleepneed_difficulty.json");
                var easyConfig = ModConfig.ReadConfig<ConfigServerEasy>(api, "sleepneed_easy.json");
                var normalConfig = ModConfig.ReadConfig<ConfigServerNormal>(api, "sleepneed_normal.json");
                var hardConfig = ModConfig.ReadConfig<ConfigServerHard>(api, "sleepneed_hard.json");
                if (diffConfig.DifficultyMode.ToLower() == "easy")
                {
                    ConfigSystem.ConfigServer = easyConfig;
                    BtCore.Logger.Notification("SleepNeed: Loaded Easy Mode Config.");
                }
                else if (diffConfig.DifficultyMode.ToLower() == "hard")
                {
                    ConfigSystem.ConfigServer = hardConfig;
                    BtCore.Logger.Notification("SleepNeed: Loaded Hard Mode Config.");
                }
                else
                {
                    ConfigSystem.ConfigServer = normalConfig;
                    BtCore.Logger.Notification("SleepNeed: Loaded Normal Mode Config.");
                }
                
                ConfigSystem.SyncedConfig = ConfigSystem.ConfigServer.ToSyncedConfig();
                ConfigSystem.ConfigLoaded = true;
                return;
            }
            if (api.Side == EnumAppSide.Client)
            {
                ConfigSystem.ConfigClient = ModConfig.ReadConfig<ConfigClient>(api, BtConstants.ConfigClientName);
                if (ConfigSystem.SyncedConfig != null)
                {
                    ConfigSystem.ConfigLoaded = true;
                    return;
                }
                var capi = api as ICoreClientAPI;
                if (capi != null && capi.IsSinglePlayer)
                {
                    try
                    {
                        var diffConfig = ModConfig.ReadConfig<ConfigDifficulty>(api, "sleepneed_difficulty.json");
                        var easyConfig = ModConfig.ReadConfig<ConfigServerEasy>(api, "sleepneed_easy.json");
                        var normalConfig = ModConfig.ReadConfig<ConfigServerNormal>(api, "sleepneed_normal.json");
                        var hardConfig = ModConfig.ReadConfig<ConfigServerHard>(api, "sleepneed_hard.json");
                        ConfigServer tempServerConfig;
                        switch (diffConfig.DifficultyMode.ToLower().Trim())
                        {
                            case "easy":
                                tempServerConfig = easyConfig;
                                break;
                            case "hard":
                                tempServerConfig = hardConfig;
                                break;
                            case "normal":
                            default:
                                // Default to normal if the text is missing, misspelled, or exactly "normal"
                                tempServerConfig = normalConfig;
                                break;
                        }

                        if (tempServerConfig != null)
                        {
                            ConfigSystem.SyncedConfig = tempServerConfig.ToSyncedConfig();
                            ConfigSystem.ConfigLoaded = true;
                            BtCore.Logger.Notification($"SleepNeed: Singleplayer client loaded '{diffConfig.DifficultyMode}' server config from disk.");
                            return;
                        }
                    }
                    catch (Exception)
                    {
                        BtCore.Logger.Warning("SleepNeed: Singleplayer client failed to read server config from disk. Using defaults temporarily.");
                    }
                }

                ConfigSystem.ConfigLoaded = false;
                ConfigSystem.SyncedConfig = new SyncedConfig();
            }
            
            
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
            clientChannel.SendPacket<SyncedConfig>(ConfigSystem.SyncedConfig);
        }

        public static void DisableStatChanges(EntityPlayer player)
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
                    treeAttribute.SetFloat("relaxingspeed", 0f);
                }
            }
            
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
            BtCore.Logger.Warning("SleepNeed: Reloading synced config from server memory");
            ConfigSystem.SyncedConfig = packet;
            ConfigSystem.ConfigLoaded = true;
            if (ConfigSystem.SyncedConfig.DisableStatChanges)
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
                ConfigSystem.DisableStatChanges(player);

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
                BtCore.Logger.Warning("SleepNeed: Forcing config from admin");

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
            BtCore.Logger.Warning("SleepNeed: Config reloaded, sending to all players");
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
            IServerNetworkChannel serverChannel = ConfigSystem._serverChannel;
            if (serverChannel == null)
            {
                return;
            }
            serverChannel.SendPacket<SyncedConfig>(ConfigSystem.SyncedConfig, new IServerPlayer[]
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

            // SleepNeed.HarmonyPatches.MiningWhenSittingPatch: Patch to prevent block breaking when sitting
            MethodInfo targetOnGettingBroken = AccessTools.Method(typeof(Block), "OnGettingBroken");
            if (targetOnGettingBroken == null)
            {
                api.Logger.Error("SleepNeed: Harmony Patch Error: Could not find target method 'Block.OnGettingBroken'. DisableBlockBreakeWhenSitting patch failed!");
            }
            else
            {
                api.Logger.Notification("SleepNeed: Applying patch to Block.OnGettingBroken for DisableBlockBreakeWhenSitting.");
                HarmonyPatches.HarmonyInstance.Patch(
                    original: targetOnGettingBroken,
                    prefix: new HarmonyMethod(typeof(SleepNeed.HarmonyPatches.MiningWhenSittingPatch.StopMiningWhileSittingPatch), "Prefix")
                );
            }

            // SleepNeed.HarmonyPatches.EnergyJumpFactorPatch: Patch to ensure that player can jump lower than a block.

            MethodInfo targetDoApplyMethod = AccessTools.Method(typeof(PModuleOnGround), "DoApply", new Type[] { typeof(float), typeof(Entity), typeof(EntityPos), typeof(EntityControls) });
            if (targetDoApplyMethod == null)
            {
                api.Logger.Error("SleepNeed: Harmony Patch Error: Could not find target method 'PModuleOnGround.DoApply'. EnergyJumpFactor patch failed!");
                return;
            }
            HarmonyPatches.HarmonyInstance.Patch
            (
                  original: targetDoApplyMethod,
                  transpiler: new HarmonyMethod(typeof(EnergyJumpFactorPatch), nameof(EnergyJumpFactorPatch.JumpFactorTranspilerMethod))
            );

            // BreathePatches (Abyssal Depths Compatibility)
            api.Logger.Notification("SleepNeed: Applying BreathePatches for MaxOxygen compatibility with other mods.");
            HarmonyPatches.HarmonyInstance.CreateClassProcessor(typeof(BreathePatches)).Patch();

            // SleepNeed.HarmonyPatches.SaturationSlowTickPatch: Patch to avoid damage from hunger
            HarmonyPatches.HarmonyInstance.Patch
            (
                  original: AccessTools.Method(typeof(EntityBehaviorHunger), "SlowTick"),
                  prefix: new HarmonyMethod(typeof(SaturationSlowTickPatch), nameof(SaturationSlowTickPatch.Prefix)),
                  postfix: new HarmonyMethod(typeof(SaturationSlowTickPatch), nameof(SaturationSlowTickPatch.Postfix))
            );
            api.Logger.Notification("SleepNeed: All Harmony patches applied successfully!");
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
