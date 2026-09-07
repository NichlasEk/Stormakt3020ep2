using Godot;
using Atland;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Main
{
    // Painted silhouettes are drawn with their original UVs. Their world-space
    // baselines participate in actor sorting; walls beyond the floor draw last.
    private void PaintForeground(Texture2D texture,Vector2[] polygon)
        =>DrawPolygon(polygon,new[]{Colors.White},polygon.Select(p=>p/new Vector2(1536,1024)).ToArray(),texture);
    private void DrawDepthSortedActors()
    {
        var layers=new List<(float Depth,Action Draw)>();
        foreach(var enemy in _game.Enemies.Where(e=>_game.CanSeeRoomPoint(e.Position))){var e=enemy;layers.Add((e.Position.Y,()=>Actor(e,e.Dead)));}
        layers.Add((_game.Player.Y,DrawPlayer));
        if((_game.Region==Region.Warehouse||_game.Rooms?.Current==PortRooms.Lodge))
        {
            layers.Add((623,()=>PaintForeground(_warehouse,new Vector2[]{new(677,513),new(690,499),new(720,490),new(722,466),new(753,450),new(812,454),new(850,470),new(874,477),new(882,572),new(791,621),new(677,566)})));
            layers.Add((644,()=>PaintForeground(_warehouse,new Vector2[]{new(493,580),new(517,556),new(568,574),new(597,592),new(594,626),new(548,647),new(494,617)})));
            layers.Add((699,()=>PaintForeground(_warehouse,new Vector2[]{new(1018,678),new(1037,618),new(1051,614),new(1081,630),new(1099,645),new(1105,679),new(1073,699)})));
        }
        if(_game.Region==Region.Quay)
        {
            layers.Add((666,()=>PaintForeground(_background,new Vector2[]{new(1169,647),new(1171,617),new(1180,603),new(1190,589),new(1198,607),new(1209,615),new(1209,650),new(1200,662),new(1180,666)})));
            layers.Add((404,()=>PaintForeground(_background,new Vector2[]{new(949,389),new(951,363),new(959,353),new(961,345),new(968,358),new(976,365),new(977,391),new(970,402),new(952,401)})));
        }
        if(_game.Rooms?.Current is PortRooms.Pump or PortRooms.Cistern)AddWaterLayers(layers);
        if(_game.InRooms)AddOathLayers(layers);
        if(_game.InRooms)AddRoomLayers(layers);else if(_game.CampaignStage==0)AddPortLayers(layers);
        foreach(var layer in layers.OrderBy(l=>l.Depth))layer.Draw();
        if((_game.Region==Region.Warehouse||_game.Rooms?.Current==PortRooms.Lodge))
        {
            PaintForeground(_warehouse,new Vector2[]{new(927,865),new(956,847),new(958,825),new(996,805),new(1019,778),new(1049,772),new(1056,747),new(1110,714),new(1144,690),new(1171,680),new(1207,651),new(1238,625),new(1260,597),new(1287,569),new(1320,542),new(1350,495),new(1400,490),new(1536,620),new(1536,1024),new(768,1024),new(768,960),new(921,871)});
            PaintForeground(_warehouse,new Vector2[]{new(0,517),new(115,525),new(190,574),new(242,614),new(306,655),new(366,694),new(430,735),new(493,780),new(550,817),new(599,852),new(646,882),new(648,910),new(689,935),new(737,955),new(766,968),new(768,1024),new(0,1024)});
        }
        if(_game.Region==Region.Shore)
            PaintForeground(_game.AtlandRevealed?_shoreRevealed:_shore,new Vector2[]{new(602,947),new(600,827),new(623,751),new(651,727),new(669,728),new(685,751),new(710,850),new(724,952),new(698,980),new(623,987)});
        if(_game.Region==Region.Quay)
            PaintForeground(_background,new Vector2[]{new(847,967),new(853,946),new(909,920),new(943,909),new(962,885),new(1014,859),new(1037,858),new(1062,825),new(1084,816),new(1088,796),new(1118,781),new(1145,757),new(1173,741),new(1208,713),new(1236,695),new(1270,667),new(1301,638),new(1340,610),new(1367,585),new(1403,563),new(1442,531),new(1485,497),new(1518,470),new(1536,473),new(1536,1024),new(847,1024)});
    }
}
