using System.Collections.Generic;
using ItemBrowser.Utilities;
using UnityEngine;

namespace ItemBrowser.Entries.Defaults {
	public class ChallengeArenaReward : ObjectEntry {
		public override ObjectEntryCategory Category => new("ItemBrowser:ObjectEntry/ChallengeArenaReward", ObjectID.AlienChest, 3750);
		
		public float Chance { get; protected set; }
		public float ChanceForOne { get; protected set; }
		public (int Min, int Max) Amount { get; protected set; }
		public (int Min, int Max) Rolls { get; protected set; }
		public Biome OnlyDropsInBiome { get; protected set; }
		public bool IsFromGuaranteedPool { get; protected set; }
		public bool IsFromTableWithGuaranteedPool { get; protected set; }
		public float? ChanceWhenBraveMerchantAlive { get; protected set; }

		public class Provider : ObjectEntryProvider {
			private static readonly (ObjectID Id, Biome Biome)[] GemstoneTypes = {
				(ObjectID.NatureGemstone, Biome.Nature),
				(ObjectID.SeaGemstone, Biome.Sea),
				(ObjectID.DesertGemstone, Biome.Desert)
			};
			
			public override void Register(ObjectEntryRegistry registry, List<(ObjectData ObjectData, GameObject Authoring)> allObjects) {
				// All of these are hardcoded in EventTerminalSystem
				registry.Register(ObjectEntryType.Source, ObjectID.AlienChest, 0, new ChallengeArenaReward {
					Amount = (1, 1),
					Chance = 1f,
					ChanceForOne = 1f,
					Rolls = (1, 1)
				});
				registry.Register(ObjectEntryType.Source, ObjectID.CrystalMerchantSpawnItem, 0, new ChallengeArenaReward {
					Amount = (1, 1),
					Chance = 1f,
					ChanceForOne = 1f,
					Rolls = (1, 1),
					ChanceWhenBraveMerchantAlive = 0.2f
				});

				foreach (var gem in GemstoneTypes) {
					registry.Register(ObjectEntryType.Source, gem.Id, 0, new ChallengeArenaReward {
						Amount = (2, 3),
						Chance = 1f,
						ChanceForOne = 1f,
						Rolls = (1, 1),
						OnlyDropsInBiome = gem.Biome
					});
				}
				
				foreach (var drop in LootUtils.GetLootTableContents(LootTableID.AlienEventTerminal)) {
					registry.Register(ObjectEntryType.Source, drop.ObjectId, 0, new ChallengeArenaReward {
						Chance = drop.Chance,
						ChanceForOne = drop.CalculateChanceForOne(),
						Amount = drop.ObjectAmount,
						Rolls = drop.CalculateRolls(),
						OnlyDropsInBiome = drop.OnlyDropsInBiome,
						IsFromGuaranteedPool = drop.IsFromGuaranteedPool,
						IsFromTableWithGuaranteedPool = drop.TableHasGuaranteedPool
					});
				}
			}
		}
	}
}