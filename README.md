Les fichiers de ce repertoire sont le rendu de cette semaine d'exercicie sur le projet procedural

En premier nous avons vu le placement de room de maniere procedural dans ce debut on a appris a toucher et comprendre comment fonctionne le code procedural a l'aide d'un premiere algorithme que nous devions faire, 
il nous a mis a disposition un dossier pour nous pre-hacher le travailler avec un procedural generation de grid deja fait, par la suite il nous a demander de faire un alghorithme permetant d'initialiser
et de generer des rooms relier par des couloirs

voici le code :


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


que fait ce code ?

Celui-ci génère plusieurs salles placées aléatoirement sur une grid, en essayant de ne pas les crees les unes sur les autres.
Chaque nouvelle salle ajoutée est ensuite reliée à l'aidede couloirs pour former un chemin.
Une fois les salles générées et connectées, il remplit le sol du niveau.

la fonction BuildGround() remplit toute la grille avec des blocs de sol :

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

la fonction PlaceRoom(RectInt room) il prend en parametre les rooms et marque les cellules de la grille correspondant à la room comme occupées grace a leurs coordonnées x et y

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

la fonction CanPlaceRoom(RectInt room) prend en parametre les rooms et regarde si l’espace où l’on veut placer une room est libre

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

la fonction CheminRoom(RectInt roomA, RectInt roomB) prend en parametre 2 rooms avec leur parametre et les relie grace a des chemins donc on change l'affichage de cellule pour creer des chemins

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



On a continuer avec une initialisation d'un cellular Automata permettant de creer un espace/ base de monde, grace a une grid
