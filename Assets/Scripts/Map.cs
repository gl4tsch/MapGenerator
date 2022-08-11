using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map
{
    List<Cell> cells;
    public List<Cell> Cells => cells;

    public Map(int cellCount)
    {
        cells = new List<Cell>(cellCount);
    }
}
