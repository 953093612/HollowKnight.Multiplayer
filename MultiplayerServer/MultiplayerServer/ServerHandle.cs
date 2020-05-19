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
        
        public static void BaldurTexture(byte fromClient, Packet packet)
        {
            int byteLength = packet.ReadInt();
            byte[] texBytes = packet.ReadBytes(byteLength);
            ServerSend.BaldurTexture(fromClient, texBytes);
        }
        
        public static void FlukeTexture(byte fromClient, Packet packet)
        {
            int byteLength = packet.ReadInt();
            byte[] texBytes = packet.ReadBytes(byteLength);
            ServerSend.FlukeTexture(fromClient, texBytes);
        }
        
        public static void GrimmTexture(byte fromClient, Packet packet)
        {
            int byteLength = packet.ReadInt();
            byte[] texBytes = packet.ReadBytes(byteLength);
            ServerSend.GrimmTexture(fromClient, texBytes);
        }
        
        public static void HatchlingTexture(byte fromClient, Packet packet)
        {
            int byteLength = packet.ReadInt();
            byte[] texBytes = packet.ReadBytes(byteLength);
            ServerSend.HatchlingTexture(fromClient, texBytes);
        }

        public static void KnightTexture(byte fromClient, Packet packet)
        {
            int byteLength = packet.ReadInt();
            byte[] texBytes = packet.ReadBytes(byteLength);
            ServerSend.KnightTexture(fromClient, texBytes);
        }

        public static void ShieldTexture(byte fromClient, Packet packet)
        {
            int byteLength = packet.ReadInt();
            byte[] texBytes = packet.ReadBytes(byteLength);
            ServerSend.ShieldTexture(fromClient, texBytes);
        }
        
        public static void SprintTexture(byte fromClient, Packet packet)
        {
            int byteLength = packet.ReadInt();
            byte[] texBytes = packet.ReadBytes(byteLength);
            ServerSend.SprintTexture(fromClient, texBytes);
        }
        
        public static void UnnTexture(byte fromClient, Packet packet)
        {
            int byteLength = packet.ReadInt();
            byte[] texBytes = packet.ReadBytes(byteLength);
            ServerSend.UnnTexture(fromClient, texBytes);
        }
        
        public static void VoidTexture(byte fromClient, Packet packet)
        {
            int byteLength = packet.ReadInt();
            byte[] texBytes = packet.ReadBytes(byteLength);
            ServerSend.VoidTexture(fromClient, texBytes);
        }
        
        public static void VSTexture(byte fromClient, Packet packet)
        {
            int byteLength = packet.ReadInt();
            byte[] texBytes = packet.ReadBytes(byteLength);
            ServerSend.VSTexture(fromClient, texBytes);
        }
        
        public static void WeaverTexture(byte fromClient, Packet packet)
        {
            int byteLength = packet.ReadInt();
            byte[] texBytes = packet.ReadBytes(byteLength);
            ServerSend.WeaverTexture(fromClient, texBytes);
        }
        
        public static void WraithsTexture(byte fromClient, Packet packet)
        {
            int byteLength = packet.ReadInt();
            byte[] texBytes = packet.ReadBytes(byteLength);
            ServerSend.WraithsTexture(fromClient, texBytes);
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
                                iPlayer.knightHash,
                                iPlayer.sprintHash,
                                iPlayer.unnHash,
                                iPlayer.voidHash,
                                iPlayer.vsHash,
                                iPlayer.wraithsHash
                            );
                            ServerSend.RequestTextures(
                                fromClient,
                                fromPlayer.knightHash,
                                fromPlayer.sprintHash,
                                fromPlayer.unnHash,
                                fromPlayer.voidHash,
                                fromPlayer.vsHash,
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
                                iPlayer.knightHash,
                                iPlayer.sprintHash,
                                iPlayer.unnHash,
                                iPlayer.voidHash,
                                iPlayer.vsHash,
                                iPlayer.wraithsHash
                            );
                            ServerSend.RequestTextures(
                                fromClient,
                                fromPlayer.knightHash,
                                fromPlayer.sprintHash,
                                fromPlayer.unnHash,
                                fromPlayer.voidHash,
                                fromPlayer.vsHash,
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