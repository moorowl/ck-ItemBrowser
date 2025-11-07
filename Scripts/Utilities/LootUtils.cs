using System;
using System.Collections.Generic;
using PugMod;
using Unity.Mathematics;
using UnityEngine;

namespace ItemBrowser.Utilities {
	public static class LootUtils {
		public readonly struct LootTableDrop {
			public readonly ObjectID ObjectId;
			public readonly (int Min, int Max) ObjectAmount;
			public readonly float Chance;
			public readonly Biome OnlyDropsInBiome;
			public readonly bool IsFromGuaranteedPool;
			public readonly bool TableHasGuaranteedPool;
			
			private readonly (int Min, int Max) _baseRolls;
			
			public LootTableDrop(ObjectID objectId, (int, int) objectAmount, (int, int) baseRolls, float chance, Biome onlyDropsInBiome, bool isFromGuaranteedPool, bool tableHasGuaranteedPool) {
				ObjectId = objectId;
				ObjectAmount = objectAmount;
				Chance = chance;
				OnlyDropsInBiome = onlyDropsInBiome;
				IsFromGuaranteedPool = isFromGuaranteedPool;
				TableHasGuaranteedPool = tableHasGuaranteedPool;
				
				_baseRolls = baseRolls;
			}

			public float CalculateChanceForOneForBosses(int? playerCountOverride = null) {
				var (minRolls, maxRolls) = CalculateRollsForBosses(playerCountOverride);
				var averageRolls = (minRolls + maxRolls) / 2f;

				return 1f - Mathf.Pow(1f - Chance, averageRolls);
			}
			
			public float CalculateChanceForOne() {
				return CalculateChanceForOneForBosses(1);
			}

			public (int Min, int Max) CalculateRollsForBosses(int? playerCountOverride = null) {
				var playerCount = Math.Max(playerCountOverride ?? WorldUtils.ClientPlayerCount, 1);

				var rollsMultiplier = 1f + Math.Max(0, playerCount - 1) * 0.5f;
				if (WorldUtils.ClientWorldInfo.IsWorldModeEnabled(WorldMode.Hard))
					rollsMultiplier *= Constants.hardModeBossLootAmountMultiplier;
				
				return (
					(int) Math.Round(_baseRolls.Min * rollsMultiplier),
					(int) Math.Round(_baseRolls.Max * rollsMultiplier)
				);
			}
			
			public (int Min, int Max) CalculateRolls() {
				return CalculateRollsForBosses(1);
			}
		}

		public static IEnumerable<LootTableDrop> GetLootTableContents(LootTableID id) {
			var drops = new List<LootTableDrop>();

			ref var lootTable = ref TryGetLootTable(id, out var exists);
			if (!exists)
				return drops;

			ref var guaranteedPool = ref lootTable.guaranteedDropsLootTable;
			var guaranteedPoolTotalWeight = 0f;
			ref var normalPool = ref lootTable.lootTable;
			var normalPoolTotalWeight = 0f;

			var minRolls = lootTable.minUniqueDrops;
			var maxRolls = lootTable.maxUniqueDrops;
			if (guaranteedPool.Length > 0) {
				minRolls--;
				maxRolls--;
			}

			for (var i = 0; i < guaranteedPool.Length; i++) {
				var entry = guaranteedPool[i];
				guaranteedPoolTotalWeight += entry.weight;
			}
			for (var i = 0; i < normalPool.Length; i++) {
				var entry = normalPool[i];
				normalPoolTotalWeight += entry.weight;
			}
			
			for (var i = 0; i < guaranteedPool.Length; i++) {
				var entry = guaranteedPool[i];
				drops.Add(new LootTableDrop(
					entry.objectID,
					(entry.amount.min, entry.amount.max),
					(1, 1),
					entry.weight / guaranteedPoolTotalWeight,
					entry.onlyDropsInBiome,
					true,
					true
				));
			}
			
			for (var i = 0; i < normalPool.Length; i++) {
				var entry = normalPool[i];
				drops.Add(new LootTableDrop(
					entry.objectID,
					(entry.amount.min, entry.amount.max),
					(minRolls, maxRolls),
					entry.weight / normalPoolTotalWeight,
					entry.onlyDropsInBiome,
					false,
					guaranteedPool.Length > 0
				));
			}
			
			return drops;
		}
		
		private static EntityLootTable _defaultLootTable;

		private static ref EntityLootTable TryGetLootTable(LootTableID id, out bool exists) {
			ref var lootTables = ref API.Client.GetEntityQuery(typeof(LootTableBankCD)).GetSingleton<LootTableBankCD>().Value.Value.lootTables;

			for (var i = 0; i < lootTables.Length; i++) {
				if (lootTables[i].lootTableID == id) {
					exists = true;
					return ref lootTables[i];
				}
			}

			exists = false;
			return ref _defaultLootTable;
		}

		public static float GetChanceForActiveWorld(EnvironmentalSpawnChance chance) {
			var worldGenSettings = Manager.saves.GetWorldInfo().worldGenerationSettings;
			
			return chance.source switch {
				EnvironmentalSpawnChance.Source.Constant => chance.constantValue.GetValueForCurrentPlatform(),
				EnvironmentalSpawnChance.Source.WorldGenSetting => worldGenSettings.Count == 0
					? chance.worldGenDependentValue.GetValue(WorldGenerationSettingLevel.Normal)
					: chance.worldGenDependentValue.GetValue(worldGenSettings[(int) chance.worldGenDependentValue.worldGenSetting].level),
				_ => throw new ArgumentOutOfRangeException()
			};
		}
		
		public static int GetMultiplayerScaledAmount(int baseAmount, float multiplayerAmountAdditionScaling, int? playerCountOverride = null) {
			var playerCount = playerCountOverride ?? WorldUtils.ClientPlayerCount;
			return (int) math.round(baseAmount * (1f + math.max(0, playerCount - 1) * multiplayerAmountAdditionScaling));
		}
	}
}