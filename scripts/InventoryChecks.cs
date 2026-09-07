using Godot;
using Atland;
using System;
using System.Linq;
using System.Threading.Tasks;

public partial class Main
{
    private async void RunInventoryChecks()
    {
        try
        {
            void Check(bool condition,string message){if(!condition)throw new InvalidOperationException(message);}
            async Task Capture(string name)
            {
                QueueRedraw();await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);
                await ToSignal(RenderingServer.Singleton,RenderingServer.SignalName.FramePostDraw);
                using var image=GetViewport().GetTexture().GetImage();
                var path=System.IO.Path.GetFullPath(ProjectSettings.GlobalizePath($"res://artifacts/{name}.png"));
                System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path)!);
                Check(image.SavePng(path)==Error.Ok,"Screenshot must save");GD.Print("INVENTORY CAPTURE "+name);
            }
            _game.Enemies.Clear();_game.AttackTime=_game.DodgeTime=0;
            _Input(new InputEventKey{PhysicalKeycode=Key.I,Pressed=true});Check(_screen==Screen.Inventory,"I opens inventory");
            Check(_inventoryItemArt.Count==Items.All.Length&&_inventoryItemArt.Values.All(t=>t!=null&&t.GetWidth()>0),"Every catalog item has generated art");
            long tick=_game.Tick;
            foreach(var item in Items.All.Skip(6))_game.Inventory.Bag.Add(_game.Inventory.Create(item.Id));
            await Capture("inventory-bag");Check(_game.Tick==tick,"Inventory pauses combat");
            Activate("inv-item-4");Activate("inv-equip");Check(_game.EquipmentArmor==4,"Equip action changes protection");
            Activate("inv-item-5");Activate("inv-equip");Activate("inv-item-6");Activate("inv-equip");
            var weapon=_game.Inventory.Bag.First(i=>i.Definition=="atland-saber");Activate("inv-item-"+weapon.Id);Activate("inv-equip");
            _Input(new InputEventKey{PhysicalKeycode=Key.C,Pressed=true});Check(_inventoryTab==2,"C selects stats");await Capture("inventory-stats");
            _Input(new InputEventKey{PhysicalKeycode=Key.C,Pressed=true});Check(_screen==Screen.Game,"C again closes stats");
            _Input(new InputEventKey{PhysicalKeycode=Key.C,Pressed=true});Check(_screen==Screen.Inventory&&_inventoryTab==2,"C reopens stats from game");
            Back();ChangeScreen(Screen.Pause);_Input(new InputEventKey{PhysicalKeycode=Key.C,Pressed=true});
            _Input(new InputEventKey{PhysicalKeycode=Key.C,Pressed=true});Check(_screen==Screen.Pause,"C restores pause when opened from pause");
            ChangeScreen(Screen.Game);OpenInventory();
            Activate("inv-tab-0");int stored=_game.Inventory.Bag[0].Id;Activate("inv-item-"+stored);Activate("inv-store");Check(_game.Inventory.Stash.Any(i=>i.Id==stored),"Deposit retains identity");
            for(int i=0;i<30;i++)_game.Inventory.Stash.Add(_game.Inventory.Create(Items.All[6+i%8].Id));
            Activate("inv-tab-1");Activate("inv-item-"+stored);await Capture("inventory-stash");
            Activate("inv-next");await Capture("inventory-stash-page-2");
            Activate("inv-take");Check(_game.Inventory.Bag.Any(i=>i.Id==stored),"Withdraw works through UI");
            _game.Spawn(EnemyKind.Guard,_game.Player+new System.Numerics.Vector2(200,0));
            Activate("inv-tab-0");Activate("inv-item-"+stored);Activate("inv-store");Check(_game.Inventory.Bag.Any(i=>i.Id==stored),"Combat prevents deposit");
            await Capture("inventory-stash-locked");
            Back();Check(_screen==Screen.Game&&!_attack,"Back returns without an attack press");
            _game.Enemies.Clear();_game.DropItem("memory",_game.Player+new System.Numerics.Vector2(30,0));_game.Events.Clear();_noticeTime=0;
            await Capture("inventory-ground-loot");
            GD.Print("INVENTORY CHECK PASS: keyboard, pause, equipment, stats, stash, pages, combat lock, return, drops");GetTree().Quit();
        }
        catch(Exception e){GD.PushError(e.ToString());GetTree().Quit(1);}
    }
}
