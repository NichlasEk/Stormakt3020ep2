using Godot;
using Atland;
using System;
using System.Threading.Tasks;

public partial class Main
{
    private async void RunPortChecks()
    {
        try
        {
            async Task Capture(string name,System.Numerics.Vector2 position)
            {
                _game.Player=position;_game.Facing=new(1,1);_camera=G(position)+new Vector2(0,-60);
                _bannerTime=_campaignTextTime=_radioTime=0;_radioQueue.Clear();_radio="";RememberRenderPositions();
                for(int i=0;i<12;i++)await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);
                QueueRedraw();await ToSignal(RenderingServer.Singleton,RenderingServer.SignalName.FramePostDraw);
                using var frame=GetViewport().GetTexture().GetImage();
                var path=System.IO.Path.GetFullPath(ProjectSettings.GlobalizePath($"res://artifacts/{name}.png"));
                System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path)!);
                if(frame.SavePng(path)!=Error.Ok)throw new Exception("Cannot capture port");
                GD.Print("PORT CAPTURE "+name);
            }
            await Capture("port-entry",Expedition.Entry);
            _game.Enemies.Clear();
            await Capture("port-courtyard",new System.Numerics.Vector2(510,612));
            await Capture("port-cache",PortLayout.Cache);
            await Capture("port-objectives-locked",Expedition.Exit+new System.Numerics.Vector2(0,90));
            _game.CampaignProgress=3;_game.CampaignMask=7;
            await Capture("port-objectives-complete",Expedition.Exit+new System.Numerics.Vector2(0,90));
            GD.Print("PORT CHECK PASS: painted entry/courtyard, 2D cache, locked/completed objectives");GetTree().Quit();
        }
        catch(Exception ex){GD.PushError(ex.ToString());GetTree().Quit(1);}
    }
}
