using System.Collections.Generic;
using ItemBrowser.Utilities;
using UnityEngine;

namespace ItemBrowser.Entries.Defaults {
	public record ChallengeArenaReward : ObjectEntry {
		public override ObjectEntryCategory Category => new("ItemBrowser:ObjectEntry/ChallengeArenaReward", ObjectID.AlienChest, 3750);
		
		public ObjectID Result { get; set; }
		public float Chance { get; set; }
		public float ChanceForOne { get; set; }
		public (int Min, int Max) Amount { get; set; }
		public (int Min, int Max) Rolls { get; set; }
		public Biome OnlyDropsInBiome { get; set; }
		public bool IsFromGuaranteedPool { get; set; }
		public bool IsFromTableWithGuaranteedPool { get; set; }
		public float? ChanceWhenBraveMerchantAlive { get; set; }

		public class Provider : ObjectEntryProvider {
			private static readonly (ObjectID Id, Biome Biome)[] GemstoneTypes = {
				(ObjectID.NatureGemstone, Biome.Nature),
				(ObjectID.SeaGemstone, Biome.Sea),
				(ObjectID.DesertGemstone, Biome.Desert)
			};
			
			public override void Register(ObjectEntryRegistry registry, List<(ObjectData ObjectData, GameObject Authoring)> allObjects) {
				// All of these are hardcoded in EventTerminalSystem
				registry.Register(ObjectEntryType.Source, ObjectID.AlienChest, 0, new ChallengeArenaReward {
					Result = ObjectID.AlienChest,
					Amount = (1, 1),
					Chance = 1f,
					ChanceForOne = 1f,
					Rolls = (1, 1)
				});
				registry.Register(ObjectEntryType.Source, ObjectID.CrystalMerchantSpawnItem, 0, new ChallengeArenaReward {
					Result = ObjectID.CrystalMerchantSpawnItem,
					Amount = (1, 1),
					Chance = 1f,
					ChanceForOne = 1f,
					Rolls = (1, 1),
					ChanceWhenBraveMerchantAlive = 0.2f
				});

				foreach (var gem in GemstoneTypes) {
					registry.Register(ObjectEntryType.Source, gem.Id, 0, new ChallengeArenaReward {
						Result = gem.Id,
						Amount = (2, 3),
						Chance = 1f,
						ChanceForOne = 1f,
						Rolls = (1, 1),
						OnlyDropsInBiome = gem.Biome
					});
				}
				
				foreach (var drop in LootUtils.GetLootTableContents(LootTableID.AlienEventTerminal)) {
					registry.Register(ObjectEntryType.Source, drop.ObjectId, 0, new ChallengeArenaReward {
						Result = drop.ObjectId,
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