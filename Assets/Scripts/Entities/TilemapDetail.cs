// Import namespace của Unity.
// Ở đoạn code này THỰC RA CHƯA DÙNG UnityEngine,
// nhưng thường để sẵn vì class này sẽ được dùng trong Unity project.
using UnityEngine;


// Thư viện JSON.NET (Newtonsoft).
// Dùng để chuyển object C# -> chuỗi JSON (serialize) và ngược lại.
using Newtonsoft.Json;


// Enum định nghĩa các trạng thái logic của một ô tile.
// Enum giúp:
// - Code dễ đọc (Grass thay vì số 1, 2, 3)
// - Tránh lỗi nhập sai giá trị
// - Dễ mở rộng sau này (Water, Sand, Rock, ...)
//
// Đây là "trạng thái logic", KHÔNG phải TileBase của Unity.
public enum TilemapState
{
    Ground,
    Grass,
    Forest,
    Tilled,
}


// Class thuần C# (POCO / Data Model).
// Dùng để mô tả CHI TIẾT 1 ô tile trong tilemap:
// - Vị trí (x, y)
// - Trạng thái logic của tile
// - Thông tin crop (nếu có)
//
// Class này KHÔNG phụ thuộc vào Tilemap, TileBase, GameObject.
public class TilemapDetail 
{
    // Tọa độ của tile trong tilemap (theo grid logic).
    // x: cột
    // y: hàng
    //
    // Dùng property (get; set;) để:
    // - Dễ serialize sang JSON
    // - Dễ mở rộng validate sau này

    public int x { get; set; }
    public int y { get; set; }


    // Trạng thái hiện tại của tile (Ground / Grass / Forest).
    // Đây là dữ liệu LOGIC, không phải hình ảnh hay TileBase.
    //
    // Ví dụ:
    // tilemapState = Grass
    // => logic biết ô này là cỏ
    // => hệ thống render sẽ quyết định dùng TileBase nào
    public TilemapState tilemapState { get; set; }
    
    
    // ==================== PHASE 1: CROP PERSISTENCE FIELDS ====================
    // These fields enable saving/loading crop state to Firebase for persistence.
    
    /// <summary>
    /// Crop ID if a crop is planted on this tile. Null if no crop.
    /// Links to CropDefinition or ItemDefinition (for seeds that become crops).
    /// </summary>
    public string cropId { get; set; }
    
    /// <summary>
    /// Unix timestamp (milliseconds) when the crop was planted.
    /// Used to calculate growth progress over time.
    /// </summary>
    public long plantedAt { get; set; }
    
    /// <summary>
    /// Current growth stage: 0=Seed, 1=Sprout, 2=Growing, 3=Mature, 4=Ready to Harvest.
    /// </summary>
    public int growthStage { get; set; }
    
    /// <summary>
    /// Unix timestamp (milliseconds) when the crop was last watered.
    /// Used for water-based growth mechanics.
    /// </summary>
    public long lastWateredAt { get; set; }


    // Constructor rỗng (bắt buộc cho JSON).
    // Newtonsoft.Json cần constructor không tham số
    // để deserialize JSON -> object C#.
    public TilemapDetail()
    {
        cropId = null;
        plantedAt = 0;
        growthStage = 0;
        lastWateredAt = 0;
    }

    public TilemapDetail(int x, int y, TilemapState tilemapState)
    {
        this.x = x;
        this.y = y;
        this.tilemapState = tilemapState;
        this.cropId = null;
        this.plantedAt = 0;
        this.growthStage = 0;
        this.lastWateredAt = 0;
    }
    
    /// <summary>
    /// Checks if this tile has a crop planted.
    /// </summary>
    [JsonIgnore]
    public bool HasCrop => !string.IsNullOrEmpty(cropId);
    
    /// <summary>
    /// Clears the crop data from this tile (after harvest).
    /// </summary>
    public void ClearCrop()
    {
        cropId = null;
        plantedAt = 0;
        growthStage = 0;
        lastWateredAt = 0;
    }


    // Ghi đè ToString() mặc định của object.
    //
    // Khi gọi:
    // Debug.Log(tilemapDetail);
    // => sẽ in ra JSON thay vì tên class.
    //
    // Ví dụ output:
    // {"x":3,"y":5,"tilemapState":1}
    //
    // Rất tiện để:
    // - Debug
    // - Save game
    // - Gửi dữ liệu qua network
    public override string ToString()
    {
        return JsonConvert.SerializeObject(this);
    }
}
