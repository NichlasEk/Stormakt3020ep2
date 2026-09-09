using Godot;
using Atland;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public partial class Main
{
    private bool _roomsSlot,_roomChecks;
    private void StartStandardExpedition()
    {
        _uppsalaSlot=false;_shipTime=0;_pendingStoryFilm="";_radioBreath=0;_foundrySlot=_mineSlot=_regimentSlot=false;
        // A shore continuation keeps its original save slot. Prefer whichever room expedition
        // was played most recently, without overwriting the other independent run.
        var quay=ProjectSettings.GlobalizePath("user://quay-save.json");
        var rooms=ProjectSettings.GlobalizePath("user://rooms-save.json");
        if(!_testMode&&System.IO.File.Exists(quay)&&(!System.IO.File.Exists(rooms)||System.IO.File.GetLastWriteTimeUtc(quay)>System.IO.File.GetLastWriteTimeUtc(rooms)))
        {
            try{if(SaveStore.Read(quay).InRooms){_doorSlot=_roomsSlot=_portSlot=_atlandSlot=false;ResumeSave();return;}}
            catch(System.Exception e){GD.PushWarning("Kunde inte bedöma äldre expedition: "+e.Message);}
        }
        StartRooms();
    }
    private void StartRooms()
    {
        _uppsalaSlot=false;_shipTime=0;_pendingStoryFilm="";_radioBreath=0;_foundrySlot=false;_mineSlot=false;_regimentSlot=false;_doorSlot=false;
        LoadWaterArt();
        _roomsSlot=true;_atlandSlot=_portSlot=false;
        if(!_testMode&&System.IO.File.Exists(SavePath)){ResumeSave();return;}
        _game=Combat.NewRooms(_order);if(!_testMode)_game.EnableConnectedWorld();ApplyDeveloperSettings();_particles.Clear();_floating.Clear();_radioQueue.Clear();_radio="";_sound.StopVoice();
        _revealTime=0;_camera=G(_game.Player)+new Vector2(0,-60);RememberRenderPositions();ChangeScreen(Screen.Game);
        foreach(var cue in _game.Events)HandleCue(cue);Save();
    }
    private void DrawRoomMarkers()
    {
        var run=_game.Rooms!;DrawRootwayMarkers();
        foreach(var link in (_game.InConnectedWorld?Array.Empty<RoomLink>():RoomLinks.From(run.Current)))
        {
            var at=link.At(run.Current);if(!_game.CanSeeRoomPoint(at))continue;
            string label=RoomLinks.Open(run,link)?"TILL "+PortRooms.Name(link.Other(run.Current)).ToUpperInvariant():link.Gate==PassageGate.Water?"VATTENFYLLD TRAPPA":link.Gate==PassageGate.Shortcut?"REGLAD LUCKA":"LÅST PASSAGE";
            Text(label,G(at)+new Vector2(-70,38),11,Gold);
        }
        if(run.Current==PortRooms.Archive)
        {
            if(_game.CanSeeRoomPoint(ArchiveRoom.Desk))Text("LIGGAREN OCH VITTNESMÅLET",G(ArchiveRoom.Desk)+new Vector2(-90,35),12,Gold);
            if(_game.CanSeeRoomPoint(ArchiveRoom.Seal))Text(run.ArchiveSecured?"GRINDEN ÄR SÄKRAD":"KOLLEGIETS KONTROLL",G(ArchiveRoom.Seal)+new Vector2(-85,35),12,Gold);
        }
        if(run.Current==PortRooms.Pump)
        {
            if(_game.CanSeeRoomPoint(PortRooms.Pressure))Text(run.PressureReleased?"AVLASTAD":"TRYCKAVLASTNING",G(PortRooms.Pressure)+new Vector2(-65,35),12,Gold);
            if(_game.CanSeeRoomPoint(PortRooms.Wheel))Text(run.WaterLowered?"PUMPEN ÄR TÖMD":"MATARHJUL",G(PortRooms.Wheel)+new Vector2(-50,35),12,Gold);
        }
        if(run.Current==PortRooms.Gallery&&_game.CanSeeRoomPoint(PortRooms.Witness))Text(run.WitnessRead?"EDEN OCH STENEN":"VITTNESBOKEN",G(PortRooms.Witness)+new Vector2(-55,35),12,Gold);
        if(run.Current==PortRooms.Chamber&&_game.CanSeeRoomPoint(PortRooms.OathExit))Text(run.Completed?"PORTEN ÄR ÖPPEN":run.OathDefeated?"BRYT PORTENS SIGILL":"EDSFÖRSEGLAD PORT",G(PortRooms.OathExit)+new Vector2(-70,35),12,Gold);
        if(run.Current==PortRooms.Cistern&&!run.RelicTaken&&_game.CanSeeRoomPoint(PortRooms.Relic))Text("SALTETS VITTNESMÅL",G(PortRooms.Relic)+new Vector2(-65,35),12,Gold);
        if(run.Current==PortRooms.Court&&!run.KeyTaken&&_game.CanSeeRoomPoint(PortRooms.Key))
        {var p=G(PortRooms.Key);DrawCircle(p,17,new Color(.03f,.04f,.03f,.9f));DrawArc(p,7,0,Mathf.Tau,20,Gold,2,true);DrawLine(p+new Vector2(5,5),p+new Vector2(17,17),Gold,3,true);Text("VÄKTARENS PACKNING",p+new Vector2(-80,38),12,Gold);}
        if(run.Current==PortRooms.Lodge&&!run.CacheTaken&&_game.CanSeeRoomPoint(PortRooms.Cache))Text("FÖRSEGLAD KISTA",G(PortRooms.Cache)+new Vector2(-60,38),12,Gold);
    }
    private void AddRoomLayers(List<(float Depth,Action Draw)> layers)
    {
        if(_game.InConnectedWorld)return;
        var run=_game.Rooms!;var door=G(PortRooms.Door(run.Current));
        // Use the architectural doorways already present in the paintings.
        // The bronze gate has an explicit lock indicator; final moving leaves are a later art pass.
        if(run.Current==PortRooms.Court&&_game.CanSeeRoomPoint(PortRooms.CourtDoor))
        {
            layers.Add((door.Y-100,()=>
            {
                var lockAt=new Vector2(490,158);
                DrawCircle(lockAt,13,new Color(.035f,.04f,.03f,.92f));
                DrawArc(lockAt+new Vector2(0,-5),6,Mathf.Pi,run.DoorOpen?Mathf.Pi*1.75f:Mathf.Tau,18,run.DoorOpen?Teal:Gold,2,true);
                DrawRect(new Rect2(lockAt+new Vector2(-6,-5),new Vector2(12,12)),run.DoorOpen?Teal:Gold,false,2);
            }));
        }
        if(run.Current==PortRooms.Lodge&&_game.CanSeeRoomPoint(PortRooms.Cache))
        {
            var at=G(PortRooms.Cache);layers.Add((at.Y,()=>
            {var source=new Rect2(671,895,225,212);var size=source.Size*(55/source.Size.Y);
                DrawTextureRectRegion(_portProps,new Rect2(at-new Vector2(size.X/2,size.Y-3),size),source,run.CacheTaken?new Color(.5f,.5f,.46f):Colors.White);
            }));
        }
    }
    private void DrawRoomPrompt()
    {
        if(DrawUppsalaPrompt()||DrawFoundryPrompt()||DrawMinePrompt()||DrawRegimentPrompt())return;
        var r=_game.Rooms!;string prompt="";
        bool Near(System.Numerics.Vector2 p)=>System.Numerics.Vector2.Distance(_game.Player,p)<72&&_game.ClearPath(_game.Player,p);
        var link=(_game.InConnectedWorld?Array.Empty<RoomLink>():RoomLinks.From(r.Current)).FirstOrDefault(l=>Near(l.At(r.Current)));
        if(_game.InConnectedWorld&&DrawConnectedPrompt())return;
        if(link?.Gate==PassageGate.Archive&&!r.ArchiveSecured&&_game.ArchiveChoice!=0)prompt="E / B · Säkra arkivets grind";
        else if(link?.Gate==PassageGate.InnerPort&&!r.Completed&&r.OathDefeated)prompt="E / B · Öppna den inre porten";
        else if(link!=null)prompt=RoomLinks.Open(r,link)?"E / B · Gå till "+PortRooms.Name(link.Other(r.Current)):link.Gate==PassageGate.Key&&r.KeyTaken?"E / B · Lås upp":link.Gate==PassageGate.Shortcut&&r.Current==PortRooms.Cistern?"E / B · Lyft regeln":RoomLinks.LockedReason(link);
        else if(r.Current==PortRooms.Court&&!r.KeyTaken&&Near(PortRooms.Key))prompt="E / B · Sök väktarens packning";
        else if(r.Current==PortRooms.Lodge&&!r.CacheTaken&&Near(PortRooms.Cache))prompt="E / B · Undersök kistan";
        else if(r.Current==PortRooms.Pump&&Near(PortRooms.Pressure)&&!r.PressureReleased)prompt="E / B · Släpp övertrycket";
        else if(r.Current==PortRooms.Pump&&Near(PortRooms.Wheel)&&!r.WaterLowered)prompt=r.PressureReleased?"E / B · Vrid matarhjulet":"Matarhjulet är trycklåst";
        else if(r.Current==PortRooms.Cistern&&Near(PortRooms.Relic)&&!r.RelicTaken)prompt="E / B · Undersök altaret";
        else if(r.Current==PortRooms.Gallery&&Near(PortRooms.Witness))prompt=r.WitnessRead?"Eden löses mot de ringmärkta pelarna":"E / B · Läs vittnesboken";
        else if(r.Current==PortRooms.Chamber&&Near(PortRooms.OathExit)&&r.OathDefeated)prompt=r.Completed?"Rutten är säkrad · återvänd och hämta fynd":"E / B · Öppna den inre porten";
        else if(r.Current==PortRooms.Roots&&Near(Rootway.Winch))prompt=r.RootGateOpen?"Spärren är lossad · följ kedjan österut":"E / B · Lossa motviktens spärr";
        else if(r.Current==PortRooms.Grove&&Near(Rootway.Memorial))prompt=r.GroveSecured?"Namnen är återfunna":r.GroveWave>0?"E / B · Ta vittnessigillet":$"E / B · Gör avtrycket · {_game.GroveWaves} eftertrupp{(_game.GroveWaves==1?"":"er")}";
        else if(r.Current==PortRooms.Archive&&Near(ArchiveRoom.Desk))prompt="E / B · Jämför handlingarna";
        else if(r.Current==PortRooms.Archive&&Near(ArchiveRoom.Seal))prompt=r.ArchiveSecured?"Arkivet säkrat · återvägen är öppen":_game.ArchiveChoice==0?"Läs handlingarna vid bordet":"E / B · Säkra arkivets grind";
        if(prompt=="")return;
        if(_game.Enemies.Any(e=>!e.Dead))prompt=_game.Enemies.Any(e=>!e.Dead&&_game.CanSeeRoomPoint(e.Position))?"Slå tillbaka rummets väktare":"Området behöver säkras först";
        Panel(new Rect2(390,412,500,59),.94f);Centered(prompt,640,442,17,Gold);
    }
    private void DrawRoomJournal()
    {
        if(_game.InDoorTrial){DrawDoorJournal();return;}
        if(_game.InConnectedWorld){DrawConnectedJournal();return;}
        DrawRect(new Rect2(0,0,1280,720),new Color(.025f,.023f,.019f,.96f));
        Text("ATLAND · PORTEN OCH ROTVÄGEN",new Vector2(95,100),26,Pale,true);
        Wrapped("Från den dränkta porten, genom arkivet och ut under de stora rötterna. Cisternen döljer en valfri genväg. Alla besökta rum går att återvända till.",new Vector2(95,170),1000,21,Muted,34);
        var r=_game.Rooms!;
        int row=0;
        foreach(var id in PortRooms.Ids)
        {Text(r.Rooms[id].Visited?PortRooms.Name(id):"Oundersökt rum",new Vector2(95+(row/5)*520,265+(row++%5)*38),20,id==r.Current?Gold:Muted);}
        Text($"Nyckel: {(r.KeyTaken?"säkrad":"saknas")} · Vatten: {(r.WaterLowered?"sänkt":"högt")} · Genväg: {(r.ShortcutOpen?"öppen":"reglad")}",new Vector2(95,495),18,Pale);
        Wrapped(r.GroveSecured?"Lundens namn är återfunna. Sigillet och kvarlämnade fynd kan hämtas via återvägen. Nästa expedition följer avtrycket mot berget.":r.Rooms[PortRooms.Grove].Visited?_game.GroveClue:r.RootGateOpen?"Motvikten håller porten öppen. Följ kedjan österut mot De namnlösas lund.":r.ArchiveRead?_game.ArchiveResult:r.Completed?"Den inre porten är öppen. Fortsätt till Minnets arkiv. Kvarlämnade fynd och cisternens genväg går fortfarande att besöka.":r.WitnessRead?"Locka Edsväktarens sköldrus mot en ringmärkt edspelare. Kliv undan när riktningen låsts. Angrip när skölden faller; vanlig kraft biter svagt genom eden.":r.CacheTaken?"Ritningen visar pumpens ordning: avlasta trycket på östra sidan, vrid sedan västra matarhjulet. Cisternen kan dölja en äldre väg tillbaka till förgården.":"Sök väktarens nyckel och undersök logementets kista. Återbesök minns fiender, fynd och utforskade ytor.",new Vector2(95,540),1000,18,Muted,27);
        Button(new Rect2(830,614,340,49),"Tillbaka","back",true);
    }

    private async void RunRoomChecks()
    {
        try
        {
            void Check(bool ok,string message){if(!ok)throw new InvalidOperationException(message);}
            async Task Capture(string name)
            {
                _camera=G(_game.Player)+new Vector2(0,-50);RememberRenderPositions();_bannerTime=_noticeTime=_campaignTextTime=0;
                QueueRedraw();await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);
                await ToSignal(RenderingServer.Singleton,RenderingServer.SignalName.FramePostDraw);
                using var image=GetViewport().GetTexture().GetImage();
                var path=ProjectSettings.GlobalizePath($"res://artifacts/{name}.png");System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path)!);
                Check(image.SavePng(path)==Error.Ok,"Save room screenshot");
            }
            void Use(System.Numerics.Vector2 at)
            {
                _game.Player=at;_game.Step(default);_game.Step(new(default,System.Numerics.Vector2.UnitY,false,false,false,false,false,false,false,true));
                foreach(var cue in _game.Events)HandleCue(cue);
            }
            foreach(var e in _game.Enemies)e.Health=0;
            Use(PortRooms.CourtDoor+new System.Numerics.Vector2(0,45));Check(!_game.Rooms!.DoorOpen,"Locked before key");await Capture("rooms-locked");
            Use(PortRooms.Key);Check(_game.Rooms.KeyTaken,"Key found");
            Use(PortRooms.CourtDoor+new System.Numerics.Vector2(0,45));Check(_game.Rooms.DoorOpen,"Door opens");await Capture("rooms-open");
            Use(PortRooms.CourtDoor+new System.Numerics.Vector2(0,45));Check(_game.Rooms.Current==PortRooms.Lodge,"Arrive in lodge");await Capture("rooms-lodge");
            foreach(var e in _game.Enemies)e.Health=0;
            Use(PortRooms.Cache);Check(_game.Rooms.CacheTaken&&_game.LocalDrops.Any(),"Chest drops a room-local item");await Capture("rooms-cache");
            Use(PortRooms.LodgeDoor+new System.Numerics.Vector2(-25,35));Check(_game.Rooms.Current==PortRooms.Court&&!_game.LocalDrops.Any(),"Return hides other room loot");
            Use(PortRooms.CourtDoor+new System.Numerics.Vector2(0,45));Check(_game.Enemies.All(e=>e.Dead)&&_game.LocalDrops.Any(),"Revisit preserves dead guards and loot");
            OpenInventory(2);await Capture("rooms-stats");Back();
            GD.Print("ROOM CHECK PASS: locked/open door, key, passage, distinct rooms, cache, revisit, inventory");GetTree().Quit();
        }
        catch(Exception e){GD.PushError(e.ToString());GetTree().Quit(1);}
    }
}
