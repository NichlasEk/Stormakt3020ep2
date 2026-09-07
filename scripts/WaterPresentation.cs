using Godot;
using Atland;
using System;
using System.Collections.Generic;

public partial class Main
{
    private Texture2D? _pumpArt,_pumpLowArt,_cisternArt,_galleryArt,_chamberArt,_chamberOpenArt,_archiveArt,_archiveDocuments;
    private void LoadWaterArt()
    {
        if(_pumpArt!=null)return;
        _pumpArt=GD.Load<Texture2D>("res://assets/art/room-pump-v1.png");
        _pumpLowArt=GD.Load<Texture2D>("res://assets/art/room-pump-low-v2.png");
        _archiveArt=GD.Load<Texture2D>("res://assets/art/room-archive-v1.png");
        _archiveDocuments=GD.Load<Texture2D>("res://assets/art/archive-documents-v1.png");
        _galleryArt=GD.Load<Texture2D>("res://assets/art/room-gallery-v1.png");
        _chamberOpenArt=GD.Load<Texture2D>("res://assets/art/room-chamber-open-v1.png");
        _chamberArt=GD.Load<Texture2D>("res://assets/art/room-chamber-v1.png");
        _oathCast=new OathCast();
        _cisternArt=GD.Load<Texture2D>("res://assets/art/room-cistern-v1.png");
    }
    private Texture2D RoomBackground=>_game.Rooms!.Current switch
    {PortRooms.Archive=>_archiveArt!,PortRooms.Gallery=>_galleryArt!,PortRooms.Chamber=>_game.Rooms.Completed?_chamberOpenArt!:_chamberArt!,PortRooms.Lodge=>_warehouse,PortRooms.Pump=>_game.Rooms.WaterLowered?_pumpLowArt!:_pumpArt!,PortRooms.Cistern=>_cisternArt!,_=>_campaignWorlds[0]};
    private void AddWaterLayers(List<(float Depth,Action Draw)> layers)
    {
        bool pump=_game.Rooms!.Current==PortRooms.Pump;
        var rim=pump?new Vector2[]{new(596,467),new(760,385),new(926,467),new(922,489),new(766,589),new(599,489)}
            :new Vector2[]{new(532,511),new(803,378),new(1019,513),new(1015,545),new(757,689),new(535,540)};
        layers.Add((pump?585:685,()=>PaintForeground(RoomBackground,rim)));
    }
}
