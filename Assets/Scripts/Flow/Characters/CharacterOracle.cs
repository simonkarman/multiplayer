﻿using KarmanProtocol;
using System;
using System.Collections.Generic;
using UnityEngine;

public class CharacterOracle : MonoBehaviour {
    [SerializeField]
    private ServerFlow serverFlow = default;
    [SerializeField]
    private GameObject characterPrefab = default;

    private KarmanServer karmanServer;
    private int totalNumberOfCharactersJoined = 0;
    private readonly Dictionary<Guid, CharacterData> characters = new Dictionary<Guid, CharacterData>();

    protected void Start() {
        karmanServer = serverFlow.GetKarmanServer();
        karmanServer.OnClientJoinedCallback += OnClientJoined;
        karmanServer.OnClientLeftCallback += OnClientLeft;
        karmanServer.OnClientPackedReceivedCallback += OnClientPacketReceived;

        enabled = false;
        karmanServer.OnRunningCallback += () => enabled = true;
        karmanServer.OnShutdownCallback += () => enabled = false;
    }

    private void OnClientJoined(Guid clientId) {
        CharacterData character = new CharacterData(
                Guid.NewGuid(),
                clientId,
                UnityEngine.Random.insideUnitCircle * 4f,
                Color.HSVToRGB((totalNumberOfCharactersJoined++ % 7) / 7f, 1f, 1f),
                Instantiate(characterPrefab, transform)
            );
        Debug.Log(string.Format("Spawning a new character {0} because client {1} joined the server", character.GetId(), clientId));
        foreach (var otherCharacter in characters.Values) {
            karmanServer.Send(clientId, otherCharacter.GetSpawnPacket());
        }
        karmanServer.Broadcast(character.GetSpawnPacket());
        characters.Add(clientId, character);
    }

    private void OnClientLeft(Guid clientId) {
        CharacterData character = characters[clientId];
        Debug.Log(string.Format("Destroying character {0} because its client {1} left the server", character.GetId(), clientId));
        character.Destroy();
        karmanServer.Broadcast(character.GetDestroyPacket());
        characters.Remove(clientId);
    }

    private void OnClientPacketReceived(Guid clientId, Networking.Packet packet) {
        if (packet is CharacterUpdatePositionPacket characterUpdatePositionPacket) {
            CharacterData character = characters[clientId];
            if (character.GetId().Equals(characterUpdatePositionPacket.GetId())) {
                characters[clientId].SetPosition(characterUpdatePositionPacket.GetPosition());
                karmanServer.Broadcast(character.GetUpdatePositionPacket(), clientId);
            } else {
                Debug.LogWarning(string.Format("Client {0} is trying to move character {1} while that characters is not under control by that client", clientId, characterUpdatePositionPacket.GetId()));
            }
        }
    }

    protected void FixedUpdate() {
        foreach (var character in characters.Values) {
            if (character.RequestPositionSyncCheck()) {
                Debug.Log("Updating out of sync position of character:" + character.GetId());
                CharacterUpdatePositionPacket characterUpdatePositionPacket = character.GetUpdatePositionPacket();
                karmanServer.Broadcast(characterUpdatePositionPacket);
            }
        }
    }
}
