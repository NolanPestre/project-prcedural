using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using VTools.Grid;
using VTools.ScriptableObjectDatabase;

namespace Components.ProceduralGeneration.SimpleRoomPlacement
{
    [CreateAssetMenu(menuName = "Procedural Generation Method/Simple Room Placement")]
    public class SimpleRoomPlacement : ProceduralGenerationMethod
    {
        [Header("Room Parameters")]
        [SerializeField] private int _maxRooms = 10;
        private List<RectInt> placedRooms = new List<RectInt>();
        protected override async UniTask ApplyGeneration(CancellationToken cancellationToken)
        {
            int placeroomcount = 0;
            for (int i = 0; i < _maxSteps; i++)
            {
                if(placeroomcount == _maxRooms )
                {
                    break;
                }
                int x = RandomService.Range(0, Grid.Width);
                int z = RandomService.Range(0, Grid.Lenght);

                RectInt room = new RectInt(x, z, 5, 8);
                if (CanPlaceRoom(room))
                {
                    PlaceRoom(room);
                    placedRooms.Add(room);

                    if (placedRooms.Count > 1)
                    {
                        CheminRoom(placedRooms[placedRooms.Count - 2], placedRooms[placedRooms.Count - 1]);
                    }

                    placeroomcount++;
                }


                await UniTask.Delay(GridGenerator.StepDelay, cancellationToken : cancellationToken);
            }
            
            
            // Final ground building.
            BuildGround();
        }

        /// Marks the grid cells of the room as occupied
        private void PlaceRoom(RectInt room)
        {

            for (int ix = room.xMin; ix < room.xMax; ix++)
            {
                for (int iy = room.yMin; iy < room.yMax; iy++)
                {
                    if (!Grid.TryGetCellByCoordinates(ix, iy, out var cell))
                        continue;
                    AddTileToCell(cell, ROOM_TILE_NAME, true);

                }
            }
        }

        private bool CanPlaceRoom(RectInt room)
        {

            for (int ix = room.xMin; ix < room.xMax; ix++)
            {
                for (int iy = room.yMin; iy < room.yMax; iy++)
                {
                    if (!Grid.TryGetCellByCoordinates(ix, iy, out var cell))
                        continue;
                    if (cell.ContainObject)
                    {
                        return false;
                    }

                }
            }
            return true;
        }

        private void CheminRoom(RectInt roomA, RectInt roomB)
        {
            Vector2Int start = new Vector2Int((int)roomA.center.x, (int)roomA.center.y);
            Vector2Int end = new Vector2Int((int)roomB.center.x, (int)roomB.center.y);

            // Chemin horizontal (X)
            int xStep = start.x < end.x ? 1 : -1;
            for (int x = start.x; x != end.x + xStep; x += xStep)
            {
                if (Grid.TryGetCellByCoordinates(x, start.y, out var cell))
                {
                    if (!cell.ContainObject) // Vérifie que la cellule n'est pas occupée par une salle
                    {
                        AddTileToCell(cell, ROOM_TILE_NAME, true);
                    }
                }
            }

            // Chemin vertical (Y)
            int yStep = start.y < end.y ? 1 : -1;
            for (int y = start.y; y != end.y + yStep; y += yStep)
            {
                if (Grid.TryGetCellByCoordinates(end.x, y, out var cell))
                {
                    if (!cell.ContainObject) // Vérifie que la cellule n'est pas occupée par une salle
                    {
                        AddTileToCell(cell, ROOM_TILE_NAME, true);
                    }
                }
            }
        }



        private void BuildGround()
        {
            var groundTemplate = ScriptableObjectDatabase.GetScriptableObject<GridObjectTemplate>("Grass");
            
            // Instantiate ground blocks
            for (int x = 0; x < Grid.Width; x++)
            {
                for (int z = 0; z < Grid.Lenght; z++)
                {
                    if (!Grid.TryGetCellByCoordinates(x, z, out var chosenCell))
                    {
                        Debug.LogError($"Unable to get cell on coordinates : ({x}, {z})");
                        continue;
                    }
                    
                    GridGenerator.AddGridObjectToCell(chosenCell, groundTemplate, false);
                }
            }
        }
    }
}