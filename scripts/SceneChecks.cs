using Godot;
using Atland;
using System;
using System.Linq;
using System.Threading.Tasks;

public partial class Main
{
    private bool _sceneChecks;
    private async void RunSceneChecks()
    {
        try
        {
            async Task Show(string name,Region region,Vector2 position)
            {
                _game=Combat.New(Order.Medicine,true);_game.Enemies.Clear();_game.Seals.Clear();
                _game.Region=region;_game.Phase=region==Region.Warehouse?Phase.Warehouse:region==Region.Shore?Phase.Shore:Phase.Quay;
                _game.Player=N(position);_game.Facing=new(1,1);_radioQueue.Clear();_radioTime=0;_radio="";_bannerTime=0;
                _screen=Screen.Game;_camera=position+new Vector2(0,-60);RememberRenderPositions();
                for(int i=0;i<35;i++)await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);
                await ToSignal(RenderingServer.Singleton,RenderingServer.SignalName.FramePostDraw);
                using var image=GetViewport().GetTexture().GetImage();image.SavePng($"res://artifacts/{name}.png");GD.Print("SCENE CHECK "+name);
            }
            await Show("specialist-walks",Region.Quay,new(720,755));
            _game.Weapon=Weapon.Hammer;_game.Moving=true;_game.MoveDirection=new(1,-1);_game.Walk=2;
            _game.Spawn(EnemyKind.Collector,new(925,666));var collector=_game.Enemies[0];collector.Moving=true;collector.Facing=new(-1,1);collector.Walk=2;
            RememberRenderPositions();QueueRedraw();await ToSignal(RenderingServer.Singleton,RenderingServer.SignalName.FramePostDraw);
            using(var walkImage=GetViewport().GetTexture().GetImage())walkImage.SavePng("res://artifacts/specialist-walks.png");
            await Show("danish-walks",Region.Shore,new(768,690));
            foreach(var kind in new[]{EnemyKind.Guard,EnemyKind.Pikeman,EnemyKind.Gunner})_game.Spawn(kind,new(480+(int)kind*245,650));
            var directions=new[]{new System.Numerics.Vector2(1,1),new System.Numerics.Vector2(1,-1),new System.Numerics.Vector2(-1,-1),new System.Numerics.Vector2(-1,1)};
            for(int direction=0;direction<4;direction++)for(int step=0;step<4;step++)
            {
                foreach(var actor in _game.Enemies){actor.Moving=true;actor.MoveDirection=actor.Facing=directions[direction];actor.Walk=step;}
                RememberRenderPositions();QueueRedraw();await ToSignal(RenderingServer.Singleton,RenderingServer.SignalName.FramePostDraw);
                using var danishImage=GetViewport().GetTexture().GetImage();danishImage.SavePng($"res://artifacts/danish-walks-{direction}-{step}.png");
            }
            await Show("depth-crate-behind",Region.Warehouse,new(760,480));
            await Show("depth-crate-front",Region.Warehouse,new(790,660));
            await Show("depth-wall",Region.Warehouse,new(1070,696));
            await Show("depth-shore",Region.Shore,new(666,857));
            await Show("depth-quay",Region.Quay,new(1090,821));
            await Show("scene-reveal",Region.Shore,new(768,620));
            _game.AtlandRevealed=true;_game.Surveyed=new[]{true,true,true};_game.Phase=Phase.Reveal;_revealTime=9;
            for(int i=0;i<100;i++)await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);
            await ToSignal(RenderingServer.Singleton,RenderingServer.SignalName.FramePostDraw);
            using(var image=GetViewport().GetTexture().GetImage())image.SavePng("res://artifacts/scene-atland.png");
            GD.Print("SCENE CHECK PASS");GetTree().Quit();
        }
        catch(Exception e){GD.PushError(e.ToString());GetTree().Quit(1);}
    }
}
