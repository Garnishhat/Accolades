using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
namespace Accolades.Plugin {
	static class AccoladePlugin {

		public static string[] Best = new string[4];

        public static void CalculateProfit() {
            PlayerStats[] stats = StartOfRound.Instance.gameStats.allPlayerStats;
            PlayerControllerB[] scripts = StartOfRound.Instance.allPlayerScripts;
            int[] Steps = new int[StartOfRound.Instance.allPlayerScripts.Length];
            int[] Profit = new int[StartOfRound.Instance.allPlayerScripts.Length];
            int[] Turns = new int[StartOfRound.Instance.allPlayerScripts.Length];
            int[] Damage = new int[StartOfRound.Instance.allPlayerScripts.Length];
            for (int i = 0; i < StartOfRound.Instance.allPlayerScripts.Length; i++) {
                Profit[i] = stats[i].profitable;
                Damage[i] = stats[i].damageTaken;
                Steps[i] = stats[i].stepsTaken;
                Turns[i] = stats[i].turnAmount;
            }
            for (int i = 0; i < StartOfRound.Instance.allPlayerScripts.Length; i++) {
				if (Steps.Min() == stats[i].stepsTaken) {
					Best[1] = scripts[i].playerUsername;
				}
				if (Steps.Max() == stats[i].stepsTaken && Profit.Max() == stats[i].profitable) {
					Best[2] = scripts[i].playerUsername;
				} 
				if (Damage.Max() == stats[i].damageTaken) {
                    Best[3] = scripts[i].playerUsername;
                }
                if (Turns.Max() == stats[i].turnAmount) {
                    Best[4] = scripts[i].playerUsername;
                }
            }
        }

        [HarmonyPatch(typeof(StartOfRound))]
		[HarmonyPatch("WritePlayerNotes")]
		[HarmonyPrefix]
		public static void WriteNotes() {
			int AllOnlinePlayers = StartOfRound.Instance.allPlayerScripts.Length;
			PlayerControllerB[] scripts = StartOfRound.Instance.allPlayerScripts;
			PlayerStats[] stats = StartOfRound.Instance.gameStats.allPlayerStats;

			if (StartOfRound.Instance.connectedPlayersAmount > 0) {
				CalculateProfit();

				for (int i = 0; i < AllOnlinePlayers; i++) {
					bool lifeCheck = !StartOfRound.Instance.allPlayerScripts[i].isPlayerDead && !StartOfRound.Instance.allPlayerScripts[i].disconnectedMidGame;
					
					bool BestCheck =
						Best[1] != "" &&
						Best[2] != "" &&
						Best[3] != "" &&
						Best[4] != "";

					StartOfRound.Instance.gameStats.allPlayerStats[i].isActivePlayer =
						StartOfRound.Instance.allPlayerScripts[i].disconnectedMidGame ||
						StartOfRound.Instance.allPlayerScripts[i].isPlayerDead ||
						StartOfRound.Instance.allPlayerScripts[i].isPlayerControlled;

					if (stats[i].isActivePlayer && BestCheck) {
						if (scripts[i].playerUsername == Best[1]) {
							if (lifeCheck) {
								StartOfRound.Instance.gameStats.allPlayerStats[i].playerNotes.Add("Laziest!");
							} else {
								StartOfRound.Instance.gameStats.allPlayerStats[i].playerNotes.Add("Put in too little effort.");
							}
						}

						if (scripts[i].playerUsername == Best[2]) {
							if (lifeCheck) {
								StartOfRound.Instance.gameStats.allPlayerStats[i].playerNotes.Add("Most Profitable!");
							} else {
								StartOfRound.Instance.gameStats.allPlayerStats[i].playerNotes.Add("Self sacrificed for the cause.");
							}
						}
						if (scripts[i].playerUsername == Best[3]) {
							if (lifeCheck) {
								StartOfRound.Instance.gameStats.allPlayerStats[i].playerNotes.Add("Took the most damage!");
							} else {
								StartOfRound.Instance.gameStats.allPlayerStats[i].playerNotes.Add("Gave in to their injuries.");
							}
						}

						if (scripts[i].playerUsername == Best[4]) {
							if (lifeCheck) {
								StartOfRound.Instance.gameStats.allPlayerStats[i].playerNotes.Add("Wariest!");
							} else {
								StartOfRound.Instance.gameStats.allPlayerStats[i].playerNotes.Add("Most clueless.");
							}
						}
					}
				}
				Best[1] = "";
				Best[2] = "";
				Best[3] = "";
				Best[4] = "";
			}
		}
	}
}
