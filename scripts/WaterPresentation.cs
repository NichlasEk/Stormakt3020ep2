using Godot;
using Atland;
using System;
using System.Collections.Generic;

public partial class Main
{
    private Texture2D? _courtOpenArt,_lodgePassageArt,_cisternOpenArt,_grovePassageArt;
    private Texture2D? _pumpArt,_pumpLowArt,_cisternArt,_galleryArt,_chamberArt,_chamberOpenArt,_archiveArt,_archiveOpenArt,_archiveDocuments,_rootwayArt,_rootwayOpenArt,_groveArt;
    private void LoadWaterArt()
    {
        if(_pumpArt!=null)return;
        LoadRegimentArt();LoadMineArt();
        _grovePassageArt=GD.Load<Texture2D>("res://assets/art/room-grove-passage-v1.png");
        _courtOpenArt=GD.Load<Texture2D>("res://assets/art/world-atland-open-v1.png");
        _lodgePassageArt=GD.Load<Texture2D>("res://assets/art/warehouse-passage-v1.png");
        _cisternOpenArt=GD.Load<Texture2D>("res://assets/art/room-cistern-open-v1.png");
        _pumpArt=GD.Load<Texture2D>("res://assets/art/room-pump-v1.png");
        _pumpLowArt=GD.Load<Texture2D>("res://assets/art/room-pump-low-v2.png");
        _archiveOpenArt=GD.Load<Texture2D>("res://assets/art/room-archive-open-v1.png");
        _rootwayOpenArt=GD.Load<Texture2D>("res://assets/art/room-rootway-open-v1.png");
        _rootwayArt=GD.Load<Texture2D>("res://assets/art/room-rootway-v1.png");
        _groveArt=GD.Load<Texture2D>("res://assets/art/room-grove-v1.png");
        _archiveArt=GD.Load<Texture2D>("res://assets/art/room-archive-v1.png");
        _archiveDocuments=GD.Load<Texture2D>("res://assets/art/archive-documents-v1.png");
        _galleryArt=GD.Load<Texture2D>("res://assets/art/room-gallery-v1.png");
        _chamberOpenArt=GD.Load<Texture2D>("res://assets/art/room-chamber-open-v1.png");
        _chamberArt=GD.Load<Texture2D>("res://assets/art/room-chamber-v1.png");
        _oathCast=new OathCast();
        _cisternArt=GD.Load<Texture2D>("res://assets/art/room-cistern-v1.png");
    }
    private string? _paintRoom;
    private string PaintRoom=>_paintRoom??_game.Rooms!.Current;
    private Texture2D RoomBackground=>_game.InDoorTrial?_doorGround!:PaintRoom==Mine.Coolway&&_game.FoundryState.GateOpen?_coolwayOpen!:Mine.Known(PaintRoom)?_mineArt[PaintRoom]:PaintRoom==Regiment.Farled&&_game.MineState.EntranceOpen?_mineFarledOpen!:Regiment.Known(PaintRoom)?_regimentArt[PaintRoom]:PaintRoom switch
    {PortRooms.Roots=>_game.InConnectedWorld?_rootwayOpenArt!:_rootwayArt!,PortRooms.Grove=>_game.InConnectedWorld?_grovePassageArt!:_groveArt!,PortRooms.Archive=>_game.InConnectedWorld?_archiveOpenArt!:_archiveArt!,PortRooms.Gallery=>_galleryArt!,PortRooms.Chamber=>_game.Rooms!.Completed?_chamberOpenArt!:_chamberArt!,PortRooms.Lodge=>_game.InConnectedWorld?_lodgePassageArt!:_warehouse,PortRooms.Pump=>_game.Rooms!.WaterLowered?_pumpLowArt!:_pumpArt!,PortRooms.Cistern=>_game.InConnectedWorld?_cisternOpenArt!:_cisternArt!,_=>_game.InConnectedWorld?_courtOpenArt!:_campaignWorlds[0]};
    private void AddWaterLayers(List<(float Depth,Action Draw)> layers)
    {
        bool pump=PaintRoom==PortRooms.Pump;
        var rim=pump?new Vector2[]{new(596,467),new(760,385),new(926,467),new(922,489),new(766,589),new(599,489)}
            :new Vector2[]{new(532,511),new(803,378),new(1019,513),new(1015,545),new(757,689),new(535,540)};
        layers.Add((pump?585:685,()=>PaintForeground(RoomBackground,rim)));
    }
}
