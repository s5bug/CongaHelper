using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Party;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Client.UI.Info;

namespace CongaHelper;

public sealed class Conga
{
    private readonly IClientState clientState;
    private readonly IGameGui gameGui;
    private readonly IObjectTable objectTable;
    private readonly IPartyList partyList;
    private readonly IPlayerState playerState;
    
    public Conga(IClientState clientState, IGameGui gameGui, IObjectTable objectTable, IPartyList partyList, IPlayerState playerState)
    {
        this.clientState = clientState;
        this.gameGui = gameGui;
        this.objectTable = objectTable;
        this.partyList = partyList;
        this.playerState = playerState;
    }

    private IGameObject[]? Buddies()
    {
        unsafe
        {
            AgentHUD* agentHud = AgentHUD.Instance();
            
            List<HudPartyMember> hudPartyMembers = [];
            for (int i = 0; i < agentHud->PartyMemberCount; i++)
            {
                HudPartyMember partyMember = agentHud->PartyMembers[i];
                BattleChara* battleChara = partyMember.Object;
                if (battleChara != null)
                {
                    // == 4 means "is player"
                    if (battleChara->BattleNpcSubKind == BattleNpcSubKind.NpcPartyMember || (byte) battleChara->BattleNpcSubKind == 4)
                    {
                        hudPartyMembers.Add(partyMember);
                    }
                }
            }
            hudPartyMembers.Sort((a, b) => (int)a.Index - (int)b.Index);
            
            IGameObject[] gameObjects = new IGameObject[hudPartyMembers.Count];
            for (int i = 0; i < hudPartyMembers.Count; i++)
            {
                IGameObject? gameObject = objectTable.SearchByEntityId(hudPartyMembers[i].Object->EntityId);

                if (gameObject != null)
                {
                    gameObjects[i] = gameObject;
                }
                else
                {
                    return null;
                }
            }

            return gameObjects;
        }
    }

    private IGameObject[]? PartyMembers()
    {
        IGameObject[] gameObjects = new IGameObject[partyList.Length];
        for (int i = 0; i < partyList.Length; i++)
        {
            IGameObject? gameObject = partyList[i]?.GameObject;
            if (gameObject != null)
            {
                gameObjects[i] = gameObject;
            }
            else
            {
                return null;
            }
        }

        return gameObjects;
    }
    
    public void DoConga()
    {
        IGameObject[] byIndex;
        IGameObject[] byPosition;
        
        if (PartyMembers() is { Length: > 0 } partyMembers)
        {
            byIndex = partyMembers;
        }
        else if(Buddies() is { Length: > 0 } buddies)
        {
            byIndex = buddies;
        }
        else
        {
            Plugin.Log.Info("conga with no party");
            return;
        }
        
        byPosition = (IGameObject[])byIndex.Clone();
        Array.Sort(byPosition, (a, b) =>
        {
            Vector3 worldPositionA = a.Position;
            Vector3 worldPositionB = b.Position;
            gameGui.WorldToScreen(worldPositionA, out Vector2 screenPositionA);
            gameGui.WorldToScreen(worldPositionB, out Vector2 screenPositionB);

            return MathF.Sign(screenPositionA.X - screenPositionB.X);
        });
        
        // alg as follows:
        // - for index i to Length-2
        // - swap i with byIndex.indexOf(byPosition[i])
        // this should move the object that should be at idx 1 into idx 1,
        // the object that should be at idx 2 into idx 2, etc
        for (int i = 0; i < byIndex.Length - 1; i++)
        {
            // we move the actor at position i to the index i in the party list
            int j = Array.IndexOf(byIndex, byPosition[i]);
            Swap(i, j);
            (byIndex[i], byIndex[j]) = (byIndex[j], byIndex[i]);
        }
    }

    private void Swap(int i, int j)
    {
        if (i == j) return;
        
        if (i > j) Swap(j, i);
        else
        {
            unsafe
            {
                InfoProxyPartyMember.Instance()->ChangeOrder(i, j);
            }
        }
    }
}
