﻿using System;
using System.Collections.Generic;
using ModCommon.Util;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MultiplayerServer
{
    public class ServerHandle
    {
        public static void WelcomeReceived(byte fromClient, Packet packet)
        {
            byte clientIdCheck = packet.ReadByte();
            string username = packet.ReadString();
            string currentClip = packet.ReadString();
            string activeScene = packet.ReadString();
            Vector3 position = packet.ReadVector3();
            Vector3 scale = packet.ReadVector3();
            int health = packet.ReadInt();
            int maxHealth = packet.ReadInt();
            int healthBlue = packet.ReadInt();

            List<bool> charmsData = new List<bool>();
            for (int charmNum = 1; charmNum <= 40; charmNum++)
            {
                charmsData.Add(packet.ReadBool());
            }
            
            Server.clients[fromClient].SendIntoGame(username, position, scale, currentClip, health, maxHealth, healthBlue, charmsData);
            SceneChanged(fromClient, activeScene);

            Log($"{username} connected successfully and is now player {fromClient}.");
            if (fromClient != clientIdCheck)
            {
                Log($"Player \"{username}\" (ID: {fromClient}) has assumed the wrong client ID ({clientIdCheck}.");
            }
        }

        #region CustomKnight Integration

        private static void HandleTexture(byte fromClient, Packet packet, int serverPacketId)
        {
            short order = packet.ReadShort();
            byte[] texBytes = packet.ReadBytes(16378);
            
            ServerSend.SendTexture(fromClient, order, texBytes, serverPacketId);
        }
        
        public static void FinishedSendingTexBytes(byte fromClient, Packet packet)
        {
            string texName = packet.ReadString();
            bool finishedSending = packet.ReadBool();
            Log("Received Finished Sending Bool: " + finishedSending);

            ServerSend.FinishedSendingTexBytes(fromClient, texName, finishedSending);
        }
        
        public static void BaldurTexture(byte fromClient, Packet packet)
        {
            HandleTexture(fromClient, packet, (int) ServerPackets.BaldurTexture);
        }
        
        public static void FlukeTexture(byte fromClient, Packet packet)
        {
            HandleTexture(fromClient, packet, (int) ServerPackets.FlukeTexture);
        }
        
        public static void GrimmTexture(byte fromClient, Packet packet)
        {
            HandleTexture(fromClient, packet, (int) ServerPackets.GrimmTexture);
        }
        
        public static void HatchlingTexture(byte fromClient, Packet packet)
        {
            HandleTexture(fromClient, packet, (int) ServerPackets.HatchlingTexture);
        }

        public static void KnightTexture(byte fromClient, Packet packet)
        {
            HandleTexture(fromClient, packet, (int) ServerPackets.KnightTexture);
        }

        public static void ShieldTexture(byte fromClient, Packet packet)
        {
            HandleTexture(fromClient, packet, (int) ServerPackets.ShieldTexture);
        }
        
        public static void SprintTexture(byte fromClient, Packet packet)
        {
            HandleTexture(fromClient, packet, (int) ServerPackets.SprintTexture);
        }
        
        public static void UnnTexture(byte fromClient, Packet packet)
        {
            HandleTexture(fromClient, packet, (int) ServerPackets.UnnTexture);
        }
        
        public static void VoidTexture(byte fromClient, Packet packet)
        {
            HandleTexture(fromClient, packet, (int) ServerPackets.VoidTexture);
        }
        
        public static void VSTexture(byte fromClient, Packet packet)
        {
            HandleTexture(fromClient, packet, (int) ServerPackets.VSTexture);
        }
        
        public static void WeaverTexture(byte fromClient, Packet packet)
        { 
            HandleTexture(fromClient, packet, (int) ServerPackets.WeaverTexture);
        }
        
        public static void WraithsTexture(byte fromClient, Packet packet)
        {
            HandleTexture(fromClient, packet, (int) ServerPackets.WraithsTexture);
        }

        public static void ServerHash(byte fromClient, Packet packet)
        {
            string texName = packet.ReadString();
            string hash = packet.ReadString();

            Player player = Server.clients[fromClient].player;
            
            switch (texName)
            {
                case "Baldur":
                    player.baldurHash = hash;
                    break;
                case "Fluke":
                    player.flukeHash = hash;
                    break;
                case "Grimm":
                    player.grimmHash = hash;
                    break;
                case "Hatchling":
                    player.hatchlingHash = hash;
                    break;
                case "Knight":
                    player.knightHash = hash;
                    break;
                case "Shield":
                    player.shieldHash = hash;
                    break;
                case "Sprint":
                    player.sprintHash = hash;
                    break;
                case "Unn":
                    player.unnHash = hash;
                    break;
                case "Void":
                    player.voidHash = hash;
                    break;
                case "VS":
                    player.vsHash = hash;
                    break;
                case "Weaver":
                    player.weaverHash = hash;
                    break;
                case "Wraiths":
                    player.wraithsHash = hash;
                    break;
                default:
                    Log("Invalid texture name!");
                    break;
            }
        }
        
        #endregion CustomKnight Integration
        
        public static void PlayerPosition(byte fromClient, Packet packet)
        {
            Vector3 position = packet.ReadVector3();
            
            Server.clients[fromClient].player.SetPosition(position);
        }

        public static void PlayerScale(byte fromClient, Packet packet)
        {
            Vector3 scale = packet.ReadVector3();

            Server.clients[fromClient].player.SetScale(scale);
        }
        
        public static void PlayerAnimation(byte fromClient, Packet packet)
        {
            string animation = packet.ReadString();
            
            Server.clients[fromClient].player.SetAnimation(animation);
        }

        public static void SceneChanged(byte fromClient, Packet packet)
        {
            string sceneName = packet.ReadString();
            
            Server.clients[fromClient].player.activeScene = sceneName;

            for (byte i = 1; i <= Server.MaxPlayers; i++)    
            {    
                if (Server.clients[i].player != null && i != fromClient)
                {
                    if (Server.clients[i].player.activeScene == sceneName)
                    {
                        Log("Same Scene, Spawning Players Subsequent Pass");
                        Player iPlayer = Server.clients[i].player;
                        Player fromPlayer = Server.clients[fromClient].player;
                        ServerSend.SpawnPlayer(fromClient, iPlayer);
                        ServerSend.SpawnPlayer(i, fromPlayer);
                        // CustomKnight integration
                        if (ServerSettings.CustomKnightIntegration)
                        {
                            Log("Requesting Textures");
                            ServerSend.RequestTextures(
                                i,
                                iPlayer.baldurHash,
                                iPlayer.flukeHash,
                                iPlayer.grimmHash,
                                iPlayer.hatchlingHash,
                                iPlayer.knightHash,
                                iPlayer.shieldHash,
                                iPlayer.sprintHash,
                                iPlayer.unnHash,
                                iPlayer.voidHash,
                                iPlayer.vsHash,
                                iPlayer.weaverHash,
                                iPlayer.wraithsHash
                            );
                            ServerSend.RequestTextures(
                                fromClient,
                                fromPlayer.baldurHash,
                                fromPlayer.flukeHash,
                                fromPlayer.grimmHash,
                                fromPlayer.hatchlingHash,
                                fromPlayer.knightHash,
                                fromPlayer.shieldHash,
                                fromPlayer.sprintHash,
                                fromPlayer.unnHash,
                                fromPlayer.voidHash,
                                fromPlayer.vsHash,
                                fromPlayer.weaverHash,
                                fromPlayer.wraithsHash
                            );
                        }
                    }
                    else
                    {
                        Log("Different Scene, Destroying Players");
                        ServerSend.DestroyPlayer(i, fromClient);
                        //ServerSend.DestroyPlayer(fromClient, i);
                    }
                }
            }
        }

        /// <summary>Initial scene load when joining the server for the first time.</summary>
        /// <param name="fromClient">The ID of the client who joined the server</param>
        /// <param name="sceneName">The name of the client's active scene when joining the server</param>
        public static void SceneChanged(byte fromClient, string sceneName)
        {
            Server.clients[fromClient].player.activeScene = sceneName;

            for (byte i = 1; i <= Server.MaxPlayers; i++)
            {
                if (Server.clients[i].player != null && i != fromClient)
                {
                    if (Server.clients[i].player.activeScene == sceneName)
                    {
                        Log("Same Scene, Spawning Players First Pass");
                        ServerSend.SpawnPlayer(fromClient, Server.clients[i].player);
                        ServerSend.SpawnPlayer(Server.clients[i].player.id, Server.clients[fromClient].player);
                        Player iPlayer = Server.clients[i].player;
                        Player fromPlayer = Server.clients[fromClient].player;
                        ServerSend.SpawnPlayer(fromClient, iPlayer);
                        ServerSend.SpawnPlayer(i, fromPlayer);
                        // CustomKnight integration
                        if (ServerSettings.CustomKnightIntegration)
                        {
                            Log("Requesting Textures");
                            ServerSend.RequestTextures(
                                i,
                                iPlayer.baldurHash,
                                iPlayer.flukeHash,
                                iPlayer.grimmHash,
                                iPlayer.hatchlingHash,
                                iPlayer.knightHash,
                                iPlayer.shieldHash,
                                iPlayer.sprintHash,
                                iPlayer.unnHash,
                                iPlayer.voidHash,
                                iPlayer.vsHash,
                                iPlayer.weaverHash,
                                iPlayer.wraithsHash
                            );
                            ServerSend.RequestTextures(
                                fromClient,
                                fromPlayer.baldurHash,
                                fromPlayer.flukeHash,
                                fromPlayer.grimmHash,
                                fromPlayer.hatchlingHash,
                                fromPlayer.knightHash,
                                fromPlayer.shieldHash,
                                fromPlayer.sprintHash,
                                fromPlayer.unnHash,
                                fromPlayer.voidHash,
                                fromPlayer.vsHash,
                                fromPlayer.weaverHash,
                                fromPlayer.wraithsHash
                            );
                        }
                    }
                }
            }
        }

        public static void HealthUpdated(byte fromClient, Packet packet)
        {
            int currentHealth = packet.ReadInt();
            int currentMaxHealth = packet.ReadInt();
            int currentHealthBlue = packet.ReadInt();

            Log("From Client: " + currentHealth + " " + currentMaxHealth + " " + currentHealthBlue);
            
            Server.clients[fromClient].player.health = currentHealth;
            Server.clients[fromClient].player.maxHealth = currentMaxHealth;
            Server.clients[fromClient].player.healthBlue = currentHealthBlue;

            ServerSend.HealthUpdated(fromClient, currentHealth, currentMaxHealth, currentHealthBlue);
        }
        
        public static void CharmsUpdated(byte fromClient, Packet packet)
        {
            for (int charmNum = 1; charmNum <= 40; charmNum++)
            {
                bool equippedCharm = packet.ReadBool();
                Server.clients[fromClient].player.SetAttr("equippedCharm_" + charmNum, equippedCharm);
            }
            
            ServerSend.CharmsUpdated(fromClient, Server.clients[fromClient].player);
        }
        
        public static void PlayerDisconnected(byte fromClient, Packet packet)
        {
            int id = packet.ReadInt();

            Object.Destroy(Server.clients[id].player.gameObject);
            Server.clients[id].player = null;
            Server.clients[id].Disconnect();
        }

        private static void Log(object message) => Modding.Logger.Log("[Server Handle] " + message);
    }
}