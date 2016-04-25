/*
*    ______               _             __  __           _ 
*   |___  /              | |           |  \/  |         | |
*      / / ___  _ __ ___ | |__   ___   | \  / | ___   __| |
*     / / / _ \| '_ ` _ \| '_ \ / _ \  | |\/| |/ _ \ / _` |
*    / /_| (_) | | | | | | |_) | (_) | | |  | | (_) | (_| |
*   /_____\___/|_| |_| |_|_.__/ \___/  |_|  |_|\___/ \__,_|
*
*          This file is part of ZomboMod Project.
*             https://www.github.com/ZomboMod
*
*             Copyright (C) 2016 Leonardosnt
*          ZomboMod is licensed under CC BY-NC-SA.
*   
*/

using SDG.Unturned;
using UnityEngine;
using ZomboMod.Core;

namespace ZomboMod.Patcher.Patches
{
    [Inject( In = "SDG.Unturned.Provider" )]
    public sealed class ProviderPatch : Patch
    {
        [Inject( In = "Awake", At = "BEFORE(Call, 'System.Void SDG.Unturned.SteamAdminlist::load()')" )]
        public void Init()
        {
            ZomboCore.PreInit();
        }
        
        [Inject( In = "addPlayer", At = "START" )]
        public static void OnPlayerPreAdded( SteamPlayerID playerId , Vector3 point   , byte angle                        ,
                                             bool isPro             , bool isAdmin    , int channel     , byte face       ,
                                             byte hair              , byte beard      , Color skin      , Color color     ,
                                             bool hand              , int shirtItem   , int pantsItem   , int hatItem     ,
                                             int backpackItem       , int vestItem    , int maskiTEM                      , 
                                             int glassesItem        , int[] skinItems , EPlayerSkillset skillset          ,
                                             bool isAnonymous                                                             )
        {
            ZomboCore.OnPlayerPreAdded(playerId, ref point, ref angle, ref isPro, ref isAdmin, ref channel, ref face, ref hair,
                                       ref beard, ref skin, ref color, ref hand, ref shirtItem, ref pantsItem, ref hatItem, 
                                       ref backpackItem, ref vestItem, ref maskiTEM, ref glassesItem, ref skinItems, ref skillset,
                                       ref isAnonymous);
        }
    }
}