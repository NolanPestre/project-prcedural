using Components.ProceduralGeneration;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using VTools.Grid;
using VTools.RandomService;
using VTools.ScriptableObjectDatabase;

[CreateAssetMenu(menuName = "Procedural Generation Method/Test algo")]
public class TestAlgo : ProceduralGenerationMethod
{
    private List<RectInt> _placedRooms = new();

    protected override async UniTask ApplyGeneration(CancellationToken cancellationToken)
    {
        Debug.Log("Test algo");

        var allGrid = new RectInt(0, 0, Grid.Width, Grid.Lenght);


        var root = new TestNode(
            allGrid,
            RandomService,
            _placedRooms,
            PlaceRoom
        );

        BuildGround();
    }

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

    private void BuildGround()
    {
        var groundTemplate = ScriptableObjectDatabase.GetScriptableObject<GridObjectTemplate>("Grass");


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


public class TestNode
{
    private readonly RectInt _bounds;
    private readonly RandomService _randomService;
    private readonly List<RectInt> _placedRooms;
    private readonly System.Action<RectInt> _placeRoom;

    private TestNode _child1, _child2;

    private Vector2Int _roomMinSize = new(5, 5);

    public TestNode(
        RectInt bounds,
        RandomService randomService,
        List<RectInt> placedRooms,
        System.Action<RectInt> placeRoomCallback)
    {
        _bounds = bounds;
        _randomService = randomService;
        _placedRooms = placedRooms;
        _placeRoom = placeRoomCallback;

        RectInt splitBoundsLeft = new RectInt(_bounds.xMin, _bounds.yMin, _bounds.width / 2, _bounds.height);
        RectInt splitBoundsRight = new RectInt(_bounds.xMin + _bounds.width / 2, _bounds.yMax, _bounds.width / 2, _bounds.height);

        if (splitBoundsLeft.width < _roomMinSize.x || splitBoundsLeft.height < _roomMinSize.y)
        {
            RectInt room = GenerateRoom(_bounds);

            _placeRoom(room);
            _placedRooms.Add(room);
            return;
        }

        _child1 = new TestNode(splitBoundsLeft, _randomService, _placedRooms, _placeRoom);
        _child2 = new TestNode(splitBoundsRight, _randomService, _placedRooms, _placeRoom);
    }

    private RectInt GenerateRoom(RectInt parent)
    {
        int width = _randomService.Range(_roomMinSize.x, Mathf.Max(_roomMinSize.x + 1, parent.width));
        int height = _randomService.Range(_roomMinSize.y, Mathf.Max(_roomMinSize.y + 1, parent.height));

        int x = _randomService.Range(parent.xMin, parent.xMax - width);
        int y = _randomService.Range(parent.yMin, parent.yMax - height);

        return new RectInt(x, y, width, height);
    }
}