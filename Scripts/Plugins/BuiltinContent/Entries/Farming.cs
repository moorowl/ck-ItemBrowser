using System.Collections.Generic;
using System.Linq;
using ItemBrowser.Api.Entries;
using UnityEngine;

namespace ItemBrowser.Plugins.BuiltinContent.Entries {
	public record Farming : ObjectEntry {
		public override ObjectEntryCategory Category => new("ItemBrowser:ObjectEntry/Farming", ObjectID.HeartBerrySeed, Priorities.Farming);
		
		public ObjectID Result { get; set; }
		public ObjectID Seed { get; set; }
		public bool HasGoldSeed  { get; set; }
		public bool RequiresGoldSeed { get; set; }
		public float GrowthTime { get; set; }
		
		public class Provider : ObjectEntryProvider {
			public override void Register(ObjectEntryRegistry registry, List<(ObjectData ObjectData, GameObject Authoring)> allObjects) {
				foreach (var (objectData, authoring) in allObjects) {
					if (objectData.variation != 0 || !authoring.TryGetComponent<SeedAuthoring>(out var seedAuthoring))
						continue;

					var turnsIntoPlant = seedAuthoring.turnsIntoPlantID;
					var turnsIntoPlantVariationRare = seedAuthoring.rarePlantVariation;

					var seedGrowthSettings = seedAuthoring.growingSettings;
					var growthTime = seedGrowthSettings.timeBetweenStages * seedGrowthSettings.highestStage;

					var plantAuthoring = allObjects.First(x => x.ObjectData.objectID == turnsIntoPlant && x.ObjectData.variation == 0).Authoring.GetComponent<PlantAuthoring>();
					var plantGrowthSettings = plantAuthoring.growingSettings;
					growthTime += plantGrowthSettings.timeBetweenStages * plantGrowthSettings.highestStage;

					var normalEntry = new Farming {
						Result = plantAuthoring.objectToDropWhenHarvested,
						Seed = objectData.objectID,
						RequiresGoldSeed = false,
						HasGoldSeed = turnsIntoPlantVariationRare > 0,
						GrowthTime = growthTime
					};
					registry.Register(ObjectEntryType.Source, normalEntry.Result, 0, normalEntry);
					registry.Register(ObjectEntryType.Usage, normalEntry.Seed, 0, normalEntry);

					if (turnsIntoPlantVariationRare > 0) {
						var rarePlantAuthoring = allObjects.First(x => x.ObjectData.objectID == turnsIntoPlant && x.ObjectData.variation == turnsIntoPlantVariationRare).Authoring.GetComponent<PlantAuthoring>();
						var goldEntry = new Farming {
							Result = rarePlantAuthoring.objectToDropWhenHarvested,
							Seed = objectData.objectID,
							RequiresGoldSeed = true,
							HasGoldSeed = true,
							GrowthTime = growthTime
						};
						registry.Register(ObjectEntryType.Source, goldEntry.Result, 0, goldEntry);
						registry.Register(ObjectEntryType.Usage, goldEntry.Seed, 0, goldEntry);
					}
				}
			}
		}
	}
}