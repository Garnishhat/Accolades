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
            
            PlayerStats[] stats = StartOfRound.Instance.gameStats.allPlayerStats;
			PlayerControllerB[] scripts = StartOfRound.Instance.allPlayerScripts;
                                   
			int GarnishScrapCount = TimeOfDay.Instance.profitQuota / 65; // (Minimum of 2)
			int GarnishHighScrapCount = GarnishScrapCount * GarnishScrapCount;
			bool profitable = StartOfRound.Instance.scrapCollectedLastRound >= GarnishScrapCount;
            bool highlyProfitable = StartOfRound.Instance.scrapCollectedLastRound >= GarnishHighScrapCount ||
            StartOfRound.Instance.scrapCollectedLastRound >= 30;
			// Number count also doesn't check to see if it's the actual amount you know, since there's a ton of different combinations and stuff

			if (StartOfRound.Instance.connectedPlayersAmount == 0) {
                
				if (profitable) {
					if (highlyProfitable) {
							StartOfRound.Instance.gameStats.allPlayerStats[0].playerNotes.Add("REALLY Profitable!");
					} else {
							StartOfRound.Instance.gameStats.allPlayerStats[0].playerNotes.Add("Profitable!");
					}
				} else {
						StartOfRound.Instance.gameStats.allPlayerStats[0].playerNotes.Add("Failed, but no penalty.");
						StartOfRound.Instance.gameStats.allPlayerStats[0].playerNotes.Add("Base quota is " + GarnishScrapCount + " items.");
				}
			}
				
			if (StartOfRound.Instance.connectedPlayersAmount > 0) {

				CalculateProfit();

				for (int i = 0; i < AllOnlinePlayers ; i++) {

					bool lifeCheck = !StartOfRound.Instance.allPlayerScripts[i].isPlayerDead && !StartOfRound.Instance.allPlayerScripts[i].disconnectedMidGame;

					StartOfRound.Instance.gameStats.allPlayerStats[i].isActivePlayer =
						StartOfRound.Instance.allPlayerScripts[i].disconnectedMidGame ||
						StartOfRound.Instance.allPlayerScripts[i].isPlayerDead ||
						StartOfRound.Instance.allPlayerScripts[i].isPlayerControlled;

					if (stats[i].isActivePlayer) {
						if (scripts[i].playerUsername == Best[1]) {
							if (lifeCheck) {
								StartOfRound.Instance.gameStats.allPlayerStats[i].playerNotes.Add("Laziest!");
								Best[1] = "";
							} else {
								StartOfRound.Instance.gameStats.allPlayerStats[i].playerNotes.Add("Wasn't too careful.");
								Best[1] = "";
							}
						}

						if (scripts[i].playerUsername == Best[2]) {
							if (lifeCheck) {
								StartOfRound.Instance.gameStats.allPlayerStats[i].playerNotes.Add("Most Profitable!");
                                Best[2] = "";
							} else {
								StartOfRound.Instance.gameStats.allPlayerStats[i].playerNotes.Add("Gave their life for the cause.");
                                Best[2] = "";
							}
						}

						if (scripts[i].playerUsername == Best[3]) {
							if (lifeCheck) {
								StartOfRound.Instance.gameStats.allPlayerStats[i].playerNotes.Add("Most injured!");
                                Best[3] = "";
							} else {
								StartOfRound.Instance.gameStats.allPlayerStats[i].playerNotes.Add("Gave in to their injuries.");
                                Best[3] = "";
							}
						}

                        if (scripts[i].playerUsername == Best[4]) {
                            if (lifeCheck) {
                                StartOfRound.Instance.gameStats.allPlayerStats[i].playerNotes.Add("Wariest!");
                                Best[4] = "";
                            } else {
                                StartOfRound.Instance.gameStats.allPlayerStats[i].playerNotes.Add("Most clueless.");
                                Best[4] = "";
                            }
                        }
                    }
				}
			}
		}
	}
}
