using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

public class Map
{

    public List<TilemapDetail> lstTilemapDetail { get; set; }

    public Map()
    {
    }

    public Map(List<TilemapDetail> lstTilemapDetail)
    {
        this.lstTilemapDetail = lstTilemapDetail;
    }

    public override string ToString()
    {
        return JsonConvert.SerializeObject(this);
    }

    public int GetLength()
    {
        return lstTilemapDetail.Count;  
    }
}
