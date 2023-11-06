using System;
using System.Threading.Tasks;
using PlasmaAPI;
using PlasmaAPI.Application;
/*
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
*/
using PlasmaAPI.Mods.RuntimeUnityEditor.Core;
using PlasmaAPI.Mods.RuntimeUnityEditor.Core.Utils;
using PlasmaAPI.Mods.RuntimeUnityEditor.Core.Utils.Abstractions;
using UnityEngine;

namespace PlasmaAPI.Mods.RuntimeUnityEditor
{
    public class Initialization
    {
        public Initialization()
        {
            /*
             * my vars here
             */
        }


        public void Start()
        {
            /*
            * start hooks here
            */
            PlasmaGame.OnGameInitialization += PlasmaGame_OnGameInitialization;
        }

        private void PlasmaGame_OnGameInitialization()
        {
            Debug.LogWarning("Creating editor...");
        }
    }
    
}
