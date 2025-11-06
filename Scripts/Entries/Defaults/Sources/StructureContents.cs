using System.Collections.Generic;
using System.Linq;
using ItemBrowser.Utilities;
using PugMod;
using PugTilemap;
using UnityEngine;

namespace ItemBrowser.Entries.Defaults.Sources {
	public class StructureContents : ObjectEntry {
		public override ObjectEntryCategory Category => new(ObjectEntryType.Source, "ItemBrowser:ObjectEntry/StructureContents", ObjectID.RuinsCavelingPillar, 3200);
		
		public int Amount { get; protected set; }
		public string Scene { get; protected set; }
		public string Dungeon { get; protected set; }
		
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
							variation = objectData.variation,
							amount = 1
						});
					}

					// Add tiles
					for (var i = 0; i < customSceneBlob.tiles.Length; i++) {
						var tile = customSceneBlob.tiles[i];
						var objectInfo = PugDatabase.TryGetTileItemInfo(tile.tileType == TileType.ground ? TileType.wall : tile.tileType, tile.tileset);

						if (objectInfo != null) {
							allObjectDatas.Add(new ObjectDataCD {
								objectID = objectInfo.objectID,
								variation = objectInfo.variation,
								amount = 1
							});
						}
					}

					var combinedObjectDatas = allObjectDatas.GroupBy(objectData => objectData.objectID).Select(group => {
						var entry = group.First();
						return new ObjectDataCD {
							objectID = entry.objectID,
							variation = entry.variation,
							amount = group.Sum(objectData => objectData.amount)
						};
					});

					foreach (var objectData in combinedObjectDatas) {
						registry.Register(objectData.objectID, objectData.variation, new StructureContents {
							Amount = objectData.amount,
							Scene = StructureUtils.GetFixedSceneName(customSceneBlob.sceneName.ToString())
						});
					}
				}
			}
		}
	}
}