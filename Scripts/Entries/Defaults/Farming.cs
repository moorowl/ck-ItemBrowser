using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ItemBrowser.Entries.Defaults {
	public class Farming : ObjectEntry {
		public override ObjectEntryCategory Category => new("ItemBrowser:ObjectEntry/Farming", ObjectID.HeartBerrySeed, 4900);
		
		public ObjectID Seed { get; protected set; }
		public bool HasGoldSeed  { get; protected set; }
		public bool RequiresGoldSeed { get; protected set; }
		public float GrowthTime { get; protected set; }
		
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
					
					registry.Register(ObjectEntryType.Source, plantAuthoring.objectToDropWhenHarvested, 0, new Farming {
						Seed = objectData.objectID,
						RequiresGoldSeed = false,
						HasGoldSeed = turnsIntoPlantVariationRare > 0,
						GrowthTime = growthTime
					});

					if (turnsIntoPlantVariationRare > 0) {
						var rarePlantAuthoring = allObjects.First(x => x.ObjectData.objectID == turnsIntoPlant && x.ObjectData.variation == turnsIntoPlantVariationRare).Authoring.GetComponent<PlantAuthoring>();
						registry.Register(ObjectEntryType.Source, rarePlantAuthoring.objectToDropWhenHarvested, 0, new Farming {
							Seed = objectData.objectID,
							RequiresGoldSeed = true,
							HasGoldSeed = true,
							GrowthTime = growthTime
						});
					}
				}
			}
		}
	}
}