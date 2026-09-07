using Godot;
using Atland;
using System;
using System.Collections.Generic;

public partial class Main
{
    private void AddRootwayLayers(List<(float Depth,Action Draw)> layers)
    {
        if(_game.Rooms!.Current==PortRooms.Grove)
            layers.Add((649,()=>PaintForeground(_groveArt!,new Vector2[]{new(645,547),new(798,475),new(928,540),new(928,576),new(778,644),new(643,599)})));
    }
    private void DrawRootwayMarkers()
    {
        var r=_game.Rooms!;
        if(r.Current==PortRooms.Roots&&_game.CanSeeRoomPoint(Rootway.Winch))
            Text(r.RootGateOpen?"SPÄRREN ÄR LOSSAD":"MOTVIKTSPORTENS SPÄRR",G(Rootway.Winch)+new Vector2(-95,33),12,Gold);
        if(r.Current==PortRooms.Grove&&_game.CanSeeRoomPoint(Rootway.Memorial))
            Text(r.GroveSecured?"NAMNEN ÄR ÅTERFUNNA":"DE BORTHUGGNA NAMNEN",G(Rootway.Memorial)+new Vector2(-90,33),12,Gold);
    }
}
