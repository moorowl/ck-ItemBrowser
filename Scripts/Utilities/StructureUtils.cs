using System;
using System.Collections.Generic;
using System.Linq;
using ItemBrowser.Utilities.Extensions;
using PugMod;
using PugWorldGen;
using Unity.Collections;
using Unity.Entities;
using SceneReference = Pug.UnityExtensions.SceneReference;

namespace ItemBrowser.Utilities {
	public static class StructureUtils {
		private static readonly HashSet<string> ScenesThatSpawnInAnyWorld = new();
		private static readonly HashSet<string> ScenesThatSpawnInCurrentWorld = new();
		private static readonly HashSet<string> DungeonsThatSpawnInAnyWorld = new();
		private static readonly HashSet<string> DungeonsThatSpawnInCurrentWorld = new();

		private static readonly HashSet<Biome> BiomesAvailableInClassicWorlds = new() {
			Biome.None,
			Biome.Slime,
			Biome.Larva,
			Biome.Stone,
			Biome.Nature,
			Biome.Sea,
			Biome.Desert
		};
		
		internal static void InitOnWorldLoad() {
			ScenesThatSpawnInAnyWorld.Clear();
			ScenesThatSpawnInCurrentWorld.Clear();
			DungeonsThatSpawnInAnyWorld.Clear();
			DungeonsThatSpawnInCurrentWorld.Clear();

			var currentWorldGenType = API.Client.GetEntityQuery(typeof(WorldGenerationTypeCD)).GetSingleton<WorldGenerationTypeCD>().Value;

			ref var customScenesTable = ref API.Client.GetEntityQuery(typeof(CustomSceneTableCD)).GetSingleton<CustomSceneTableCD>().Value.Value;
			for (var i = 0; i < customScenesTable.scenes.Length; i++) {
				ref var customScene = ref customScenesTable.scenes[i];
				var name = GetPersistentSceneName(customScene.sceneName.ToString());

				if (customScene.maxOccurrences == 0 || customScene.replacedByContentBundle.hasValue)
					continue;

				var classicBiomes = customScene.biomesToSpawnIn.classic.ConvertToList();
				var fullReleaseBiomes = customScene.biomesToSpawnIn.fullRelease.ConvertToList();

				if (classicBiomes.Count == 0 || classicBiomes.Any(biome => CanBiomeGenerate(WorldGenerationType.Classic, biome))) {
					ScenesThatSpawnInAnyWorld.Add(name);
					
					if (currentWorldGenType == WorldGenerationType.Classic)
						ScenesThatSpawnInCurrentWorld.Add(name);
				}
				
				if (fullReleaseBiomes.Count == 0 || fullReleaseBiomes.Any(biome => CanBiomeGenerate(WorldGenerationType.FullRelease, biome))) {
					ScenesThatSpawnInAnyWorld.Add(name);
					
					if (currentWorldGenType == WorldGenerationType.FullRelease)
						ScenesThatSpawnInCurrentWorld.Add(name);
				}
			}
		}
		
		public static string GetPersistentSceneName(string sceneName) {
			// This is to turn SceneBuilder's runtime names (e.g. SB/318923147) into its identifier
			return new SceneReference {
				ScenePath = sceneName
			}.SceneName;
		}

		public static bool CanSceneGenerateInAnyWorld(string sceneName) {
			return ScenesThatSpawnInAnyWorld.Contains(sceneName);
		}

		public static bool CanContentBundleBeActive(WorldGenerationType worldGenerationType, ContentBundleID contentBundle) {
			return worldGenerationType switch {
				WorldGenerationType.Classic => contentBundle == ContentBundleID.Classic,
				WorldGenerationType.FullRelease => contentBundle != ContentBundleID.Classic,
				_ => false
			};
		}
		
		public static bool CanBiomeGenerate(WorldGenerationType worldGenerationType, Biome biome) {
			return worldGenerationType switch {
				WorldGenerationType.Classic => BiomesAvailableInClassicWorlds.Contains(biome),
				WorldGenerationType.FullRelease => true,
				_ => false
			};
		}
		
		public static HashSet<(Entity Entity, string Name, float SpawnValue, WorldGenerationTypeDependentValue<Biome> Biome)> GetAllRandomDungeons() {
			var dungeons = new HashSet<(Entity Entity, string Name, float SpawnValue, WorldGenerationTypeDependentValue<Biome> Biome)>();

			// Biome-specific dungeons
			var dungeonBiomeSpawnTableBuffer = API.Client.GetEntityQuery(typeof(DungeonBiomeSpawnTableBuffer)).GetSingletonBuffer<DungeonBiomeSpawnTableBuffer>(true);
			foreach (var biomeSpawnTable in dungeonBiomeSpawnTableBuffer) {
				foreach (var dungeon in EntityUtility.GetBuffer<DungeonSpawnTableBuffer>(biomeSpawnTable.tableEntity, API.Client.World))
					dungeons.Add((dungeon.prefabEntity, dungeon.name.ToString(), dungeon.spawnValue, biomeSpawnTable.biome));
			}
			
			return dungeons;
		}
		
		public static HashSet<(Entity Entity, string Name)> GetAllUniqueDungeons() {
			var dungeons = new HashSet<(Entity Entity, string Name)>();

			// Biome-specific dungeons
			using var dungeonSpawnTableBufferEntities = API.Client.GetEntityQuery(typeof(DungeonSpawnTableBuffer)).ToEntityArray(Allocator.Temp);
			foreach (var dungeonSpawnTableBufferEntity in dungeonSpawnTableBufferEntities) {
				foreach (var dungeonSpawnTable in EntityUtility.GetBuffer<DungeonSpawnTableBuffer>(dungeonSpawnTableBufferEntity, API.Client.World))
					dungeons.Add((dungeonSpawnTable.prefabEntity, dungeonSpawnTable.name.ToString()));
			}
			
			// Guaranteed dungeons
			using var pugWorldGenCDs = API.Client.GetEntityQuery(typeof(PugWorldGenCD)).ToComponentDataArray<PugWorldGenCD>(Allocator.Temp);
			foreach (var pugWorldGenCD in pugWorldGenCDs)
				dungeons.Add((pugWorldGenCD.entity, pugWorldGenCD.name.ToString()));

			return dungeons;
		}

		public static HashSet<(Entity Entity, string Name)> GetAllDungeons() {
			var dungeons = new HashSet<(Entity Entity, string Name)>();

			// Biome-specific dungeons
			using var dungeonSpawnTableBufferEntities = API.Client.GetEntityQuery(typeof(DungeonSpawnTableBuffer)).ToEntityArray(Allocator.Temp);
			foreach (var dungeonSpawnTableBufferEntity in dungeonSpawnTableBufferEntities) {
				foreach (var dungeonSpawnTable in EntityUtility.GetBuffer<DungeonSpawnTableBuffer>(dungeonSpawnTableBufferEntity, API.Client.World))
					dungeons.Add((dungeonSpawnTable.prefabEntity, dungeonSpawnTable.name.ToString()));
			}
			
			// Guaranteed dungeons
			using var pugWorldGenCDs = API.Client.GetEntityQuery(typeof(PugWorldGenCD)).ToComponentDataArray<PugWorldGenCD>(Allocator.Temp);
			foreach (var pugWorldGenCD in pugWorldGenCDs)
				dungeons.Add((pugWorldGenCD.entity, pugWorldGenCD.name.ToString()));

			return dungeons;
		}
		
		public static HashSet<RoomFlags> SeparateFlags(RoomFlags flagsToSeparate) {
			var separatedFlags = new HashSet<RoomFlags>();
				
			foreach (RoomFlags flag in Enum.GetValues(typeof(RoomFlags))) {
				if (flagsToSeparate.HasFlag(flag))
					separatedFlags.Add(flag);
			}

			return separatedFlags;
		}
	}
}