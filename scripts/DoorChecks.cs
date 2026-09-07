using Godot;
using Atland;
using System;
using System.Threading.Tasks;
public partial class Main
{
 private async void RunDoorChecks()
 {
  try
  {
   async Task Capture(string name){System.IO.Directory.CreateDirectory(ProjectSettings.GlobalizePath("res://artifacts"));QueueRedraw();await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);await ToSignal(RenderingServer.Singleton,RenderingServer.SignalName.FramePostDraw);using var im=GetViewport().GetTexture().GetImage();if(im.SavePng(ProjectSettings.GlobalizePath("res://artifacts/"+name+".png"))!=Error.Ok)throw new Exception("Door capture failed");}
   void Check(bool ok,string text){if(!ok)throw new Exception(text);}
   var g=_game;g.Enemies.Clear();g.Player=DoorTrialLayout.Center+new System.Numerics.Vector2(-35,65);_camera=G(g.Player)+new Vector2(0,-80);RememberRenderPositions();g.UpdateRoomSight(true);await Capture("door-closed");
   g.DoorTest!.KeyTaken=true;g.DoorTest.Door.Locked=false;g.DoorTest.Door.TargetOpen=true;
   for(int i=0;i<50;i++){g.Step(new());await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);}
   Check(g.DoorTest.Door.Openness==1,"Door reaches open position");await Capture("door-open");
   g.Player=new(950,540);g.Step(new());_camera=G(g.Player)+new Vector2(0,-80);RememberRenderPositions();g.UpdateRoomSight(true);await Capture("door-inside");
   g.DoorTest.Door.Health=0;g.UpdateRoomSight(true);await Capture("door-broken");
   ChangeScreen(Screen.Title);await Capture("door-standard-title");
   Check(_buttons.Exists(b=>b.Id=="expedition"),"Standard room expedition is a primary menu action");
   var trip=Combat.NewAtland(Order.Medicine);trip.CampaignStage=-1;trip.Region=Region.Shore;trip.Phase=Phase.Complete;trip.PreferRoomRoute=true;trip.Health=39;trip.Potions=1;_game=trip;
   Activate("journey");Check(_game.InRooms&&_game.Health==39&&_game.Potions==1,"Real continuation handler preserves resources into rooms");await Capture("door-standard-continuation");
   GD.Print("DOOR NATIVE PASS: rendered closed/open/inside/broken, live animation, fog and primary expedition menu");GetTree().Quit();
  }
  catch(Exception e){GD.PushError(e.ToString());GetTree().Quit(1);}
 }
}
