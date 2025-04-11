using System;
using System.Collections.Generic;

public class ItemShape
{
    // 싱글톤 패턴
    private static ItemShape instance;

    public static ItemShape Instance
    {
        get
        {
            if (instance == null) instance = new ItemShape();
            return instance;
        }
    }

    public bool[][][][] itemShapes;

    private ItemShape()
    {
        InitializeShape();
    }

    private void InitializeShape()
    {
        itemShapes = new bool[14][][][];

        for (int i = 0; i < itemShapes.Length; i++)
        {
            itemShapes[i] = new bool[4][][];

            for (int r = 0; r < 4; r++)
            {
                itemShapes[i][r] = new bool[5][];

                for (int y = 0; y < 5; y++) itemShapes[i][r][y] = new bool[5];
            }
        }

        SetItemShape(0, 0,
            new bool[][]
            {
                new[] { false, false, false, false, false }, new[] { false, false, false, false, false },
                new[] { false, false, true, false, false }, new[] { false, false, false, false, false },
                new[] { false, false, false, false, false }
            });

        SetItemShape(0, 1,
            new bool[][]
            {
                new[] { false, false, false, false, false }, new[] { false, false, false, false, false },
                new[] { false, false, true, false, false }, new[] { false, false, false, false, false },
                new[] { false, false, false, false, false }
            });

        SetItemShape(0, 2,
            new bool[][]
            {
                new[] { false, false, false, false, false }, new[] { false, false, false, false, false },
                new[] { false, false, true, false, false }, new[] { false, false, false, false, false },
                new[] { false, false, false, false, false }
            });

        SetItemShape(0, 3,
            new bool[][]
            {
                new[] { false, false, false, false, false }, new[] { false, false, false, false, false },
                new[] { false, false, true, false, false }, new[] { false, false, false, false, false },
                new[] { false, false, false, false, false }
            });

        SetItemShape(1, 0,
            new bool[][]
            {
                new[] { false, false, false, false, false }, new[] { false, false, false, false, false },
                new[] { false, true, true, false, false }, new[] { false, false, false, false, false },
                new[] { false, false, false, false, false }
            });

        SetItemShape(1, 1,
            new bool[][]
            {
                new[] { false, false, false, false, false }, new[] { false, false, true, false, false },
                new[] { false, false, true, false, false }, new[] { false, false, false, false, false },
                new[] { false, false, false, false, false }
            });

        SetItemShape(1, 2,
            new bool[][]
            {
                new[] { false, false, false, false, false }, new[] { false, false, false, false, false },
                new[] { false, false, true, true, false }, new[] { false, false, false, false, false },
                new[] { false, false, false, false, false }
            });

        SetItemShape(1, 3,
            new bool[][]
            {
                new[] { false, false, false, false, false }, new[] { false, false, false, false, false },
                new[] { false, false, true, false, false }, new[] { false, false, true, false, false },
                new[] { false, false, false, false, false }
            });

        SetItemShape(2, 0,
            new bool[][]
            {
                new[] { false, false, false, false, false }, new[] { false, false, false, false, false },
                new[] { false, true, true, true, false }, new[] { false, false, false, false, false },
                new[] { false, false, false, false, false }
            });

        SetItemShape(2, 1,
            new bool[][]
            {
                new[] { false, false, false, false, false }, new[] { false, false, true, false, false },
                new[] { false, false, true, false, false }, new[] { false, false, true, false, false },
                new[] { false, false, false, false, false }
            });

        SetItemShape(2, 2,
            new bool[][]
            {
                new[] { false, false, false, false, false }, new[] { false, false, false, false, false },
                new[] { false, true, true, true, false }, new[] { false, false, false, false, false },
                new[] { false, false, false, false, false }
            });

        SetItemShape(2, 3,
            new bool[][]
            {
                new[] { false, false, false, false, false }, new[] { false, false, true, false, false },
                new[] { false, false, true, false, false }, new[] { false, false, true, false, false },
                new[] { false, false, false, false, false }
            });

        SetItemShape(3, 0,
            new bool[][]
            {
                new[] { false, false, false, false, false }, new[] { false, false, false, false, false },
                new[] { true, true, true, false, false }, new[] { false, false, true, false, false },
                new[] { false, false, false, false, false }
            });

        SetItemShape(3, 1,
            new bool[][]
            {
                new[] { false, false, true, false, false }, new[] { false, false, true, false, false },
                new[] { false, true, true, false, false }, new[] { false, false, false, false, false },
                new[] { false, false, false, false, false }
            });

        SetItemShape(3, 2,
            new bool[][]
            {
                new[] { false, false, false, false, false }, new[] { false, false, true, false, false },
                new[] { false, false, true, true, true }, new[] { false, false, false, false, false },
                new[] { false, false, false, false, false }
            });

        SetItemShape(3, 3,
            new bool[][]
            {
                new[] { false, false, false, false, false }, new[] { false, false, false, false, false },
                new[] { false, false, true, true, false }, new[] { false, false, true, false, false },
                new[] { false, false, true, false, false }
            });

        SetItemShape(4, 0,
            new bool[][]
            {
                new[] { false, false, false, false, false }, new[] { false, false, true, false, false },
                new[] { true, true, true, false, false }, new[] { false, false, false, false, false },
                new[] { false, false, false, false, false }
            });

        SetItemShape(4, 1,
            new bool[][]
            {
                new[] { false, false, true, false, false }, new[] { false, false, true, false, false },
                new[] { false, false, true, true, false }, new[] { false, false, false, false, false },
                new[] { false, false, false, false, false }
            });

        SetItemShape(4, 2,
            new bool[][]
            {
                new[] { false, false, false, false, false }, new[] { false, false, false, false, false },
                new[] { false, false, true, true, true }, new[] { false, false, true, false, false },
                new[] { false, false, false, false, false }
            });

        SetItemShape(4, 3,
            new bool[][]
            {
                new[] { false, false, false, false, false }, new[] { false, false, false, false, false },
                new[] { false, true, true, false, false }, new[] { false, false, true, false, false },
                new[] { false, false, true, false, false }
            });

        SetItemShape(5, 0,
            new bool[][]
            {
                new[] { false, false, false, false, false }, new[] { false, false, true, false, false },
                new[] { false, true, true, true, false }, new[] { false, false, false, false, false },
                new[] { false, false, false, false, false }
            });

        SetItemShape(5, 1,
            new bool[][]
            {
                new[] { false, false, false, false, false }, new[] { false, false, true, false, false },
                new[] { false, false, true, true, false }, new[] { false, false, true, false, false },
                new[] { false, false, false, false, false }
            });

        SetItemShape(5, 2,
            new bool[][]
            {
                new[] { false, false, false, false, false }, new[] { false, false, false, false, false },
                new[] { false, true, true, true, false }, new[] { false, false, true, false, false },
                new[] { false, false, false, false, false }
            });

        SetItemShape(5, 3,
            new bool[][]
            {
                new[] { false, false, false, false, false }, new[] { false, false, true, false, false },
                new[] { false, true, true, false, false }, new[] { false, false, true, false, false },
                new[] { false, false, false, false, false }
            });

        SetItemShape(6, 0,
            new bool[][]
            {
                new[] { false, false, false, false, false }, new[] { false, false, false, false, false },
                new[] { true, true, true, true, false }, new[] { false, false, false, false, false },
                new[] { false, false, false, false, false }
            });

        SetItemShape(6, 1,
            new bool[][]
            {
                new[] { false, false, true, false, false }, new[] { false, false, true, false, false },
                new[] { false, false, true, false, false }, new[] { false, false, true, false, false },
                new[] { false, false, false, false, false }
            });

        SetItemShape(6, 2,
            new bool[][]
            {
                new[] { false, false, false, false, false }, new[] { false, false, false, false, false },
                new[] { false, true, true, true, true }, new[] { false, false, false, false, false },
                new[] { false, false, false, false, false }
            });

        SetItemShape(6, 3,
            new bool[][]
            {
                new[] { false, false, false, false, false }, new[] { false, false, true, false, false },
                new[] { false, false, true, false, false }, new[] { false, false, true, false, false },
                new[] { false, false, true, false, false }
            });

        SetItemShape(7, 0,
            new bool[][]
            {
                new[] { false, false, false, false, false }, new[] { false, true, true, false, false },
                new[] { false, false, true, true, false }, new[] { false, false, false, false, false },
                new[] { false, false, false, false, false }
            });

        SetItemShape(7, 1,
            new bool[][]
            {
                new[] { false, false, false, false, false }, new[] { false, false, false, true, false },
                new[] { false, false, true, true, false }, new[] { false, false, true, false, false },
                new[] { false, false, false, false, false }
            });

        SetItemShape(7, 2,
            new bool[][]
            {
                new[] { false, false, false, false, false }, new[] { false, false, false, false, false },
                new[] { false, true, true, false, false }, new[] { false, false, true, true, false },
                new[] { false, false, false, false, false }
            });

        SetItemShape(7, 3,
            new bool[][]
            {
                new[] { false, false, false, false, false }, new[] { false, false, true, false, false },
                new[] { false, true, true, false, false }, new[] { false, true, false, false, false },
                new[] { false, false, false, false, false }
            });

        SetItemShape(8, 0,
            new bool[][]
            {
                new[] { false, false, false, false, false }, new[] { false, false, true, true, false },
                new[] { false, true, true, false, false }, new[] { false, false, false, false, false },
                new[] { false, false, false, false, false }
            });

        SetItemShape(8, 1,
            new bool[][]
            {
                new[] { false, false, false, false, false }, new[] { false, false, true, false, false },
                new[] { false, false, true, true, false }, new[] { false, false, false, true, false },
                new[] { false, false, false, false, false }
            });

        SetItemShape(8, 2,
            new bool[][]
            {
                new[] { false, false, false, false, false }, new[] { false, false, false, false, false },
                new[] { false, false, true, true, false }, new[] { false, true, true, false, false },
                new[] { false, false, false, false, false }
            });

        SetItemShape(8, 3,
            new bool[][]
            {
                new[] { false, false, false, false, false }, new[] { false, true, false, false, false },
                new[] { false, true, true, false, false }, new[] { false, false, true, false, false },
                new[] { false, false, false, false, false }
            });

        SetItemShape(9, 0,
            new bool[][]
            {
                new[] { false, false, false, false, false }, new[] { false, true, true, false, false },
                new[] { false, true, true, false, false }, new[] { false, false, false, false, false },
                new[] { false, false, false, false, false }
            });

        SetItemShape(9, 1,
            new bool[][]
            {
                new[] { false, false, false, false, false }, new[] { false, false, true, true, false },
                new[] { false, false, true, true, false }, new[] { false, false, false, false, false },
                new[] { false, false, false, false, false }
            });

        SetItemShape(9, 2,
            new bool[][]
            {
                new[] { false, false, false, false, false }, new[] { false, false, false, false, false },
                new[] { false, false, true, true, false }, new[] { false, false, true, true, false },
                new[] { false, false, false, false, false }
            });

        SetItemShape(9, 3,
            new bool[][]
            {
                new[] { false, false, false, false, false }, new[] { false, false, true, false, false },
                new[] { false, true, true, false, false }, new[] { false, true, true, false, false },
                new[] { false, false, false, false, false }
            });

        SetItemShape(10, 0,
            new bool[][]
            {
                new[] { false, false, false, false, false }, new[] { false, false, false, false, false },
                new[] { false, true, true, false, false }, new[] { false, true, true, false, false },
                new[] { false, false, false, false, false }
            });

        SetItemShape(10, 1,
            new bool[][]
            {
                new[] { false, false, false, false, false }, new[] { false, true, true, false, false },
                new[] { false, true, true, true, false }, new[] { false, false, false, false, false },
                new[] { false, false, false, false, false }
            });

        SetItemShape(10, 2,
            new bool[][]
            {
                new[] { false, false, false, false, false }, new[] { false, false, true, true, false },
                new[] { false, false, true, true, false }, new[] { false, false, true, false, false },
                new[] { false, false, false, false, false }
            });

        SetItemShape(10, 3,
            new bool[][]
            {
                new[] { false, false, false, false, false }, new[] { false, false, false, false, false },
                new[] { false, true, true, true, false }, new[] { false, false, true, true, false },
                new[] { false, false, false, false, false }
            });

        SetItemShape(11, 0,
            new bool[][]
            {
                new[] { false, false, false, false, false }, new[] { false, true, true, true, false },
                new[] { false, true, true, true, false }, new[] { false, false, false, false, false },
                new[] { false, false, false, false, false }
            });

        SetItemShape(11, 1,
            new bool[][]
            {
                new[] { false, false, false, false, false }, new[] { false, false, true, true, false },
                new[] { false, false, true, true, false }, new[] { false, false, true, true, false },
                new[] { false, false, false, false, false }
            });

        SetItemShape(11, 2,
            new bool[][]
            {
                new[] { false, false, false, false, false }, new[] { false, false, false, false, false },
                new[] { false, true, true, true, false }, new[] { false, true, true, true, false },
                new[] { false, false, false, false, false }
            });

        SetItemShape(11, 3,
            new bool[][]
            {
                new[] { false, false, false, false, false }, new[] { false, true, true, false, false },
                new[] { false, true, true, false, false }, new[] { false, true, true, false, false },
                new[] { false, false, false, false, false }
            });

        SetItemShape(12, 0,
            new bool[][]
            {
                new[] { false, false, false, false, false }, new[] { false, false, false, false, false },
                new[] { false, false, true, true, true }, new[] { false, false, true, false, true },
                new[] { false, false, false, false, false }
            });

        SetItemShape(12, 1,
            new bool[][]
            {
                new[] { false, false, false, false, false }, new[] { false, false, false, false, false },
                new[] { false, true, true, false, false }, new[] { false, false, true, false, false },
                new[] { false, true, true, false, false }
            });

        SetItemShape(12, 2,
            new bool[][]
            {
                new[] { false, false, false, false, false }, new[] { true, false, true, false, false },
                new[] { true, true, true, false, false }, new[] { false, false, false, false, false },
                new[] { false, false, false, false, false }
            });

        SetItemShape(12, 3,
            new bool[][]
            {
                new[] { false, false, true, true, false }, new[] { false, false, true, false, false },
                new[] { false, false, true, true, false }, new[] { false, false, false, false, false },
                new[] { false, false, false, false, false }
            });

        SetItemShape(13, 0,
            new bool[][]
            {
                new[] { false, false, false, false, false }, new[] { false, true, true, true, false },
                new[] { false, true, true, true, false }, new[] { false, true, true, true, false },
                new[] { false, false, false, false, false }
            });

        SetItemShape(13, 1,
            new bool[][]
            {
                new[] { false, false, false, false, false }, new[] { false, true, true, true, false },
                new[] { false, true, true, true, false }, new[] { false, true, true, true, false },
                new[] { false, false, false, false, false }
            });

        SetItemShape(13, 2,
            new bool[][]
            {
                new[] { false, false, false, false, false }, new[] { false, true, true, true, false },
                new[] { false, true, true, true, false }, new[] { false, true, true, true, false },
                new[] { false, false, false, false, false }
            });

        SetItemShape(13, 3,
            new bool[][]
            {
                new[] { false, false, false, false, false }, new[] { false, true, true, true, false },
                new[] { false, true, true, true, false }, new[] { false, true, true, true, false },
                new[] { false, false, false, false, false }
            });
    }

    // 블록 모양 설정 헬퍼 메서드
    private void SetItemShape(int itemId, int rotation, bool[][] visualShape)
    {
        for (int y = 0; y < 5; y++)
        for (int x = 0; x < 5; x++)
            // 시각적으로 위에서 아래로 정의된 배열을
            // 유니티 좌표계(좌하단 원점)에 맞게 y축 반전
            itemShapes[itemId][rotation][4 - y][x] = visualShape[y][x];
    }

    // 블록 모양 가져오기
    public bool[][] GetItemShape(int itemId, int rotation)
    {
        if (itemId < 0 || itemId >= itemShapes.Length || rotation < 0 || rotation > 3) return null;

        return itemShapes[itemId][rotation];
    }
}
