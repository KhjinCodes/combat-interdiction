﻿using ProtoBuf;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using VRage.Collections;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.Utils;

namespace Khjin.CombatInterdiction
{
    public class CombatInterdictionMessaging
    {
        private const ushort channelId = 30541; // Unique ID for this mod
        private Networking networking = null;
        private bool isWelcomeDone = false;
        public const string SYNC_BOOST_KEY = "#SB#";
        private readonly MyConcurrentQueue<Message> receivedQueue;

        private struct Message
        {
            public ulong SenderId;
            public bool IsFromServer;
            public MessagePacket Data;
        }

        public CombatInterdictionMessaging() 
        {
            receivedQueue = new MyConcurrentQueue<Message>();
        }

        public void LoadData()
        {
            networking = new Networking(channelId, new Action<ulong, MessagePacket, bool>(OnMessageReceived));
            networking.Register();
        }

        public void UnloadData()
        {
            networking.Unregister();
            networking = null;
        }

        public void WelcomePlayer()
        {
            if (!isWelcomeDone)
                isWelcomeDone = true;
            else
                return;

            string message = "Combat Interdiction Mod by Khjin. To view the list of available commands, enter /ccommands in chat.";
            ChatPlayer(message);
        }

        public void MessageServer(string message)
        {
            networking.SendToServer(new MessagePacket(message));
        }

        public void MessagePlayer(string message, ulong recipientId)
        {
            MessagePacket messagePacket = new MessagePacket(message);
            networking.SendToPlayer(messagePacket, recipientId);
        }

        public void NotifyPlayer(string message, string fontColor = "White")
        {
            Utilities.NotifyMessage(message, fontColor);
        }

        public void ProcessMessages()
        {
            for (int i = 0; (i <= 10 && receivedQueue.Count > 0); i++)
            {
                Message msg;
                if (receivedQueue.TryDequeue(out msg))
                {
                    if (msg.IsFromServer)
                    {
                        ProcessAsClient(msg.SenderId, msg.Data);
                    }
                    else
                    {
                        if (Utilities.IsServer())
                        {
                            ProcessAsServer(msg.SenderId, msg.Data);
                        }
                    }
                }
            }
        }

        private void OnMessageReceived(ulong senderId, MessagePacket packet, bool isArrivedFromServer)
        {
            receivedQueue.Enqueue(new Message()
            {
                SenderId = senderId,
                IsFromServer = isArrivedFromServer,
                Data = packet
            });
        }
    
        private void ProcessAsServer(ulong senderId, MessagePacket packet)
        {
            if (packet.Message.StartsWith(SYNC_BOOST_KEY))
            {
                if (CombatInterdictionSession.Instance.Logic != null)
                {
                    string[] values = packet.Message.Split(new char[] { '|' },
                        StringSplitOptions.RemoveEmptyEntries);
                    long entityId = long.Parse(values[1]);
                    long blockId = long.Parse(values[2]);
                    bool value = bool.Parse(values[3]);
                    CombatInterdictionSession.Instance.Logic.SyncBoostRequest(entityId, blockId, value);
                }
            }
            else if (packet.Message.StartsWith("/r"))
            {
                // Set fromLocal as false as this always comes from clients
                CombatInterdictionSession.Instance.Commands.HandleCommand(packet.Message, senderId, false);
            }
        }

        private void ProcessAsClient(ulong senderId, MessagePacket packet)
        {
            ChatPlayer(packet.Message);
        }

        public void ChatPlayer(string message)
        {
            networking.ChatLocalPlayer(CombatInterdictionSession.MOD_NAME, message);
        }
    }

    public class Networking
    {
        public readonly ushort channelId;
        private Action<ushort, byte[], ulong, bool> messageHandler;
        public Action<ulong, MessagePacket, bool> PacketHandler { get; private set; }
        
        private List<IMyPlayer> currentPlayers = null;

        public Networking(ushort channelId, Action<ulong, MessagePacket, bool> packetHandler)
        {
            this.channelId = channelId;
            PacketHandler = packetHandler;
        }

        public void Register()
        {
            messageHandler = new Action<ushort, byte[], ulong, bool>(HandleMessage);
            MyAPIGateway.Multiplayer.RegisterSecureMessageHandler(channelId, messageHandler);
        }

        public void Unregister()
        {
            MyAPIGateway.Multiplayer.UnregisterSecureMessageHandler(channelId, messageHandler);
        }

        public void HandleMessage(ushort channelId, byte[] messageBytes, ulong senderId, bool isArrivedFromServer)
        {
            try
            {
                // Only recognize messages from this mod
                if (channelId == this.channelId)
                {
                    var packet = MyAPIGateway.Utilities.SerializeFromBinary<PacketBase>(messageBytes);

                    if (packet != null && packet is MessagePacket)
                    {
                        MessagePacket messagePacket = packet as MessagePacket;
                        PacketHandler(senderId, messagePacket, isArrivedFromServer);
                    }
                }
            }
            catch (Exception ex)
            {
                MyLog.Default.WriteLineAndConsole($"{ex.Message}\n{ex.StackTrace}");
                Utilities.NotifyMessage($"[ERROR: {GetType().FullName}: {ex.Message} |" +
                                      " Send SpaceEngineers.Log to mod author]",
                                      MyFontEnum.Red);
            }
        }

        public void SendToServer(MessagePacket packet)
        {
            if (!Utilities.IsServer())
            {
                var bytes = MyAPIGateway.Utilities.SerializeToBinary(packet);
                MyAPIGateway.Multiplayer.SendMessageToServer(channelId, bytes);
            }
        }

        public void SendToPlayer(MessagePacket packet, ulong recipientId)
        {
            if (Utilities.IsServer())
            {
                var bytes = MyAPIGateway.Utilities.SerializeToBinary(packet);
                MyAPIGateway.Multiplayer.SendMessageTo(channelId, bytes, recipientId);
            }
        }

        public void ChatLocalPlayer(string sender, string message)
        {
            if (MyAPIGateway.Session?.Player != null)
            {
                MyAPIGateway.Utilities.ShowMessage(sender, message);
            }
        }

        public void BroadCastToPlayers(MessagePacket packet)
        {
            if (!Utilities.IsServer()) { return; }

            if (currentPlayers == null)
                currentPlayers = new List<IMyPlayer>(MyAPIGateway.Session.SessionSettings.MaxPlayers);
            else
                currentPlayers.Clear();

            MyAPIGateway.Players.GetPlayers(currentPlayers);
            foreach (var p in currentPlayers)
            {
                if (p.IsBot)
                    continue;

                if (p.SteamUserId == MyAPIGateway.Multiplayer.ServerId)
                    continue;

                if (p.SteamUserId == packet.SenderId)
                    continue;

                byte[] rawData = MyAPIGateway.Utilities.SerializeToBinary(packet);
                MyAPIGateway.Multiplayer.SendMessageTo(channelId, rawData, p.SteamUserId);
            }

            currentPlayers.Clear();
        }
    }

    [ProtoContract]
    public class MessagePacket : PacketBase
    {
        [ProtoMember(1)]
        public readonly string Message;

        // Required for deserialization (Digi)
        public MessagePacket() { }

        public MessagePacket(string message)
        {
            Message = message;
        }

        public override bool Received()
        {
            return false;
        }
    }

    [ProtoInclude(1000, typeof(MessagePacket))]
    [ProtoContract]
    public abstract class PacketBase
    {
        [ProtoMember(1)]
        public readonly ulong SenderId;

        public PacketBase()
        {
            SenderId = MyAPIGateway.Multiplayer.MyId;
        }

        public abstract bool Received();
    }
}
