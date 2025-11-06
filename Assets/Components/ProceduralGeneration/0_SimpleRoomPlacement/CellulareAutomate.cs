using Components.ProceduralGeneration;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using VTools.Grid;
using VTools.RandomService;
using VTools.ScriptableObjectDatabase;

[CreateAssetMenu(menuName = "Procedural Generation Method/Celular otomata")]
public class CellulareAutomate : ProceduralGenerationMethod
{
    [SerializeField] private int _noiseDensity;
    [SerializeField] private int _grassCount = 4;

    List<Cell> GetCellList(int x, int y)
    {
        List<Cell> neighbors = new List<Cell>();

        int[,] offsets = new int[,]
        {
        { -1, -1 }, { -1, 0 }, { -1, 1 },
        {  0, -1 },{  0, 1 },
        {  1, -1 }, {  1, 0 }, {  1, 1 }
        };

        for (int i = 0; i < offsets.GetLength(0); i++)
        {
            int nx = x + offsets[i, 0];
            int ny = y + offsets[i, 1];

            if (Grid.TryGetCellByCoordinates(nx, ny, out Cell neighborCell))
            {
                neighbors.Add(neighborCell);
            }
            else
            {
                neighbors.Add(null);
            }
        }

        return neighbors; 
    }

    protected override async UniTask ApplyGeneration(CancellationToken cancellationToken)
    {
        for (int x = 0; x < Grid.Width; x++)
        {
            for (int z = 0; z < Grid.Lenght; z++)
            {
                if (!Grid.TryGetCellByCoordinates(x, z, out Cell chosenCell))
                    continue;

                int typ = RandomService.Range(0, 101);

                if (typ < _noiseDensity)
                    AddTileToCell(chosenCell, GRASS_TILE_NAME, true);
                else
                    AddTileToCell(chosenCell, WATER_TILE_NAME, true);
            }
        }

        for (int i = 0; i < _maxSteps; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            bool[,] isgroundsave = new bool[Grid.Width, Grid.Lenght];
            for (int x = 1; x < Grid.Width - 1; x++)
            {
                for (int y = 1; y < Grid.Lenght - 1; y++)
                {
                    if (!Grid.TryGetCellByCoordinates(x, y, out Cell cell))
                        continue;

                    isgroundsave[x,y] = IsSurroundedByGrass(cell,x,y);
                }
            }

            for (int x = 0; x < Grid.Width - 1; x++)
            {
                for (int y = 0; y < Grid.Lenght - 1; y++)
                {
                    if (!Grid.TryGetCellByCoordinates(x, y, out Cell cell))
                        continue;

                    if (isgroundsave[x, y])
                    {

                        ChangeGrass(cell);
                    }
                    else
                    {
                        ChangeWater(cell);
                    }
                }
            }

            await UniTask.Delay(GridGenerator.StepDelay, cancellationToken: cancellationToken);
        }
    }
    bool IsSurroundedByGrass(Cell cell, int x, int y)
    {
        var neighbors = GetCellList(x, y);
        int GrassCount = 0;

        foreach (var n in neighbors)
        {
            if (n.GridObject.Template.Name == GRASS_TILE_NAME)
                GrassCount++;
            if(n == null)
            {
                GrassCount++;
            }
        }

        return GrassCount >= _grassCount;
    }

    void ChangeGrass(Cell cell)
    {
        if (cell.GridObject.Template.Name == GRASS_TILE_NAME)
        {
            return;
        }

        AddTileToCell(cell, GRASS_TILE_NAME, true);
    }

    void ChangeWater(Cell cell)
    {
        AddTileToCell(cell, WATER_TILE_NAME, true);
    }
}