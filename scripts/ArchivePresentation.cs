using Godot;
using Atland;
using System;
using System.Collections.Generic;

public partial class Main
{
    private void SelectArchiveDecision(int choice)
    {
        if(_screen!=Screen.Archive||!_game.ChooseRoomArchive(choice))return;
        ChangeScreen(Screen.Game);foreach(var cue in _game.Events)HandleCue(cue);
    }
    private void DrawArchiveDocuments()
    {
        DrawTextureRect(_archiveDocuments!,new Rect2(0,0,1280,720),false);
        var ink=new Color("312a22");
        Text("MINNETS ARKIV · TVÅ HANDLINGAR",new Vector2(102,37),21,Gold,true);
        Wrapped(ArchiveRoom.Ledger,new Vector2(112,115),425,19,ink,30);
        Wrapped(ArchiveRoom.Witness,new Vector2(722,115),425,19,ink,30);
        if(_game.ArchiveChoice==0)
        {
            Text("Hela patrullen kommer: tre vakter.",new Vector2(112,490),16,ink);
            Text("Två går vidare; kontrollanten stannar.",new Vector2(722,490),16,ink);
            Button(new Rect2(90,592,500,48),"Bevara vittnesmålet","archive-preserve",true);
            Button(new Rect2(690,592,500,48),"Förfalska en passersedel","archive-forge");
            Centered("Beslutet sparas och följer expeditionen.",640,675,16,Pale);
        }
        else Centered(_game.ArchiveResult,640,618,18,Pale);
        Button(new Rect2(490,678,300,34),"Tillbaka · Esc","back");
    }
    private void AddArchiveLayers(List<(float Depth,Action Draw)> layers)
    {
        if(_game.Rooms!.Current!=PortRooms.Archive)return;
        layers.Add((687,()=>PaintForeground(_archiveArt!,new[]{new Vector2(575,550),new(659,453),new(960,531),new(952,569),new(884,681),new(864,689),new(579,620)})));
    }
}
