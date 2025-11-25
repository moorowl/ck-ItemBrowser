using System.Collections.Generic;
using ItemBrowser.Utilities;
using PugMod;
using PugTilemap;
using UnityEngine;

namespace ItemBrowser.Entries.Defaults {
	public record StructureContents : ObjectEntry {
		public override ObjectEntryCategory Category => new("ItemBrowser:ObjectEntry/StructureContents", "ItemBrowser:ObjectEntry/StructureContents_NonObtainable", ObjectID.RuinsCavelingPillar, 3200);
		
		public (ObjectID Id, int Variation, int Amount) Result { get; set; }
		public string Scene { get; set; }
		public string Dungeon { get; set; }
		
		public class Provider : ObjectEntryProvider {
			public override void Register(ObjectEntryRegistry registry, List<(ObjectData ObjectData, GameObject Authoring)> allObjects) {
				ref var customScenesBlobArray = ref API.Client.GetEntityQuery(typeof(CustomSceneTableCD)).GetSingleton<CustomSceneTableCD>().Value.Value.scenes;

				for (var sceneIdx = 0; sceneIdx < customScenesBlobArray.Length; sceneIdx++) {
					ref var customSceneBlob = ref customScenesBlobArray[sceneIdx];
					var allObjectDatas = new List<ObjectDataCD>();
					
					// Add normal objects
					for (var i = 0; i < customSceneBlob.prefabObjectDatas.Length; i++) {
						var objectData = customSceneBlob.prefabObjectDatas[i];
						var objectInfo = PugDatabase.GetObjectInfo(objectData.objectID, objectData.variation);
						
						// Some scenes contain objects that can't be placed for some reason
						if (objectInfo == null || objectInfo.prefabInfos[0].prefab == null)
							continue;
						
						allObjectDatas.Add(new ObjectDataCD {
							objectID = objectData.objectID,
							variation = ObjectUtils.GetPrimaryVariation(objectData.objectID, objectData.variation),
							amount = 1
						});
					}

					// Add tiles
					for (var i = 0; i < customSceneBlob.tiles.Length; i++) {
						var tile = customSceneBlob.tiles[i];
						
						var tileType = tile.tileType;
						if (tileType == TileType.ground)
							tileType = TileType.wall;
						var tileset = tile.tileset;
						if (tileType == TileType.bigRoot && tileset == (int) Tileset.Nature)
							tileset = (int) Tileset.Dirt;
						
						var objectInfo = PugDatabase.TryGetTileItemInfo(tileType, tileset);
						if (objectInfo != null) {
							allObjectDatas.Add(new ObjectDataCD {
								objectID = objectInfo.objectID,
								variation = ObjectUtils.GetPrimaryVariation(objectInfo.objectID, objectInfo.variation),
								amount = 1
							});
						}
					}

					var combinedObjectDatas = ObjectUtils.GroupAndSumObjects(allObjectDatas, false);

					foreach (var objectData in combinedObjectDatas) {
						var entry = new StructureContents {
							Result = (objectData.objectID, objectData.variation, objectData.amount),
							Scene = customSceneBlob.sceneName.ToString()
						};
						registry.Register(ObjectEntryType.Source, entry.Result.Id, entry.Result.Variation, entry);
					}
				}
			}
		}
	}
}