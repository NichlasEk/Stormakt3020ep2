using Godot;
using Atland;
using System;
using System.Linq;

public partial class Main
{
    private bool _inventoryChecks,_inventoryMouseRelease;
    private int _inventoryTab,_inventoryPage,_selectedItem=4;
    private Screen _inventoryReturn=Screen.Game;
    private void OpenInventory(int tab=0)
    {
        if(_screen is not (Screen.Game or Screen.Pause or Screen.Inventory))return;
        if(_screen!=Screen.Inventory)_inventoryReturn=_screen;
        _inventoryTab=tab;_inventoryPage=0;ChangeScreen(Screen.Inventory);
    }
    private bool InventoryAction(string id)
    {
        if(id=="inventory"){OpenInventory();return true;}
        if(!id.StartsWith("inv-"))return false;
        string result="";bool mutation=false;
        if(id.StartsWith("inv-tab-")){_inventoryTab=int.Parse(id[8..]);_inventoryPage=0;_selectedItem=-1;}
        else if(id.StartsWith("inv-item-"))_selectedItem=int.Parse(id[9..]);
        else if(id=="inv-next")_inventoryPage++;
        else if(id=="inv-prev")_inventoryPage=Math.Max(0,_inventoryPage-1);
        else if(id=="inv-equip"){result=_game.EquipItem(_selectedItem);mutation=true;}
        else if(id=="inv-remove")
        {
            var pair=_game.Inventory.Equipped.FirstOrDefault(e=>e.Value.Id==_selectedItem);
            result=pair.Value is null?"Ingen utrustning vald.":_game.UnequipItem(pair.Key);mutation=true;
        }
        else if(id is "inv-store" or "inv-take")
        {result=_game.TransferItem(_selectedItem,id=="inv-store");mutation=true;if(result=="")_selectedItem=-1;}
        if(result!="")Notice(result);
        else if(mutation){Save();_sound.Play("paper");}
        return true;
    }
    // Small engraved UI symbols; actor sprites and their appearance are unchanged.
    private void ItemIcon(GearSlot slot,Vector2 p,float size,Color color)
    {
        void Line(float x,float y,float a,float b,float width=2)=>DrawLine(p+new Vector2(x,y)*size,p+new Vector2(a,b)*size,color,width,true);
        switch(slot)
        {
            case GearSlot.Saber:Line(-.28f,.34f,.28f,-.36f,3);Line(-.3f,.1f,-.04f,.32f);Line(-.29f,.35f,-.39f,.46f,3);break;
            case GearSlot.Hammer:Line(-.16f,.42f,.16f,-.19f,4);DrawColoredPolygon(new[]{p+new Vector2(-.17f,-.35f)*size,p+new Vector2(.36f,-.13f)*size,p+new Vector2(.44f,-.33f)*size,p+new Vector2(-.08f,-.54f)*size},color);break;
            case GearSlot.Armor:
                var shape=new[]{new Vector2(-.18f,-.4f),new Vector2(-.4f,-.25f),new Vector2(-.27f,.03f),new Vector2(-.2f,.4f),new Vector2(.2f,.4f),new Vector2(.27f,.03f),new Vector2(.4f,-.25f),new Vector2(.18f,-.4f),new Vector2(0,-.22f)};
                DrawPolyline(shape.Append(shape[0]).Select(v=>p+v*size).ToArray(),color,2,true);Line(0,-.18f,0,.33f);break;
            case GearSlot.Helmet:DrawArc(p,size*.32f,Mathf.Pi,Mathf.Tau,24,color,3,true);Line(-.4f,0,.4f,0);Line(-.23f,0,-.18f,.25f);Line(.23f,0,.18f,.25f);Line(0,-.34f,0,.2f);break;
            case GearSlot.Sigil:DrawArc(p,size*.32f,0,Mathf.Tau,24,color,2,true);Line(0,-.24f,0,.24f);Line(-.17f,.12f,.17f,-.12f);Line(-.1f,.32f,-.17f,.52f);Line(.1f,.32f,.17f,.52f);break;
        }
    }
    private static string ItemStats(ItemDefinition d)
    {
        var parts=new System.Collections.Generic.List<string>();
        if(d.Damage!=0)parts.Add($"+{d.Damage} grundskada");
        if(d.Armor!=0)parts.Add($"+{d.Armor} % skadeskydd");
        if(d.Recovery!=0)parts.Add($"+{d.Recovery} uthållighet/s");
        return parts.Count==0?"Standardutrustning":string.Join("\n",parts);
    }
    private void DrawInventory()
    {
        DrawRect(new Rect2(0,0,1280,720),new Color(.025f,.035f,.033f,.97f));
        Text("KARL CCLV · FÄLTUTRUSTNING",new Vector2(40,55),24,Pale,true);
        Text("Spelet är pausat",new Vector2(40,84),13,Muted);
        Button(new Rect2(375,96,151,43),"Inventarium","inv-tab-0",_inventoryTab==0);
        Button(new Rect2(537,96,151,43),"Stash","inv-tab-1",_inventoryTab==1);
        Button(new Rect2(699,96,151,43),"Stats","inv-tab-2",_inventoryTab==2);
        Text("UTRUSTAT",new Vector2(40,135),13,Gold);
        int row=0;
        foreach(var slot in Enum.GetValues<GearSlot>())
        {
            var r=new Rect2(40,153+row*85,291,76);Panel(r,.97f);
            _game.Inventory.Equipped.TryGetValue(slot,out var item);bool selected=item!=null&&item.Id==_selectedItem;
            if(selected)DrawRect(r,Gold,false,2);
            ItemIcon(slot,r.Position+new Vector2(34,39),43,item==null?Muted:Gold);
            Text(Items.SlotName(slot)+(slot==_game.ActiveWeaponSlot?" · AKTIV":""),r.Position+new Vector2(68,24),12,Gold);
            Wrapped(item?.Data.Name??"Tom plats",r.Position+new Vector2(68,47),211,14,Pale,18);
            if(item!=null)
            {
                _buttons.Add((r,"inv-item-"+item.Id));
                if(((_controller||_menuKeyboard)&&_menuSelection==_buttons.Count-1)||r.HasPoint(GetGlobalMousePosition()))DrawRect(r,Pale,false,1);
            }
            row++;
        }
        Text($"Tinkturer: {_game.Potions}   ·   TAB byter aktivt vapen i strid",new Vector2(40,609),13,Muted);
        if(_inventoryTab==2)DrawInventoryStats();else DrawItemGrid();
        Button(new Rect2(945,638,295,45),"Tillbaka","back",true);
        Text("I Inventarium · C Stats · ESC Tillbaka",new Vector2(40,675),14,Muted);
    }
    private void DrawItemGrid()
    {
        bool stash=_inventoryTab==1;var source=stash?_game.Inventory.Stash:_game.Inventory.Bag;
        int pages=Math.Max(1,(source.Count+23)/24);_inventoryPage=Math.Clamp(_inventoryPage,0,pages-1);
        Text($"{(stash?"EXPEDITIONSFÖRRÅD":"VÄSKA")} · {source.Count}/{(stash?Items.StashCapacity:Items.BagCapacity)}",new Vector2(375,167),13,Gold);
        for(int index=0;index<24;index++)
        {
            var r=new Rect2(375+index%6*79,185+index/6*89,70,80);Panel(r,.98f);
            int actual=_inventoryPage*24+index;if(actual>=source.Count)continue;
            var item=source[actual];bool selected=item.Id==_selectedItem;
            DrawRect(r,selected?Gold:new Color("4b4c3f"),false,selected?2:1);
            ItemIcon(item.Data.Slot,r.Position+new Vector2(35,32),44,Gold);
            Text(Items.SlotName(item.Data.Slot),r.Position+new Vector2(5,70),11,Muted);
            _buttons.Add((r,"inv-item-"+item.Id));
            if(((_controller||_menuKeyboard)&&_menuSelection==_buttons.Count-1)||r.HasPoint(GetGlobalMousePosition()))DrawRect(r,new Color(Pale,.6f),false,1);
        }
        Text(_game.CanUseStash?"Förrådet är tillgängligt.":"Säkra området för att flytta föremål till/från stash.",new Vector2(375,570),13,Muted);
        if(stash&&pages>1)
        {
            if(_inventoryPage>0)Button(new Rect2(375,587,143,36),"Föregående","inv-prev");
            Text($"{_inventoryPage+1}/{pages}",new Vector2(570,613),14,Gold);
            if(_inventoryPage+1<pages)Button(new Rect2(699,587,151,36),"Nästa","inv-next");
        }
        Panel(new Rect2(892,153,348,464),.98f);
        var selectedItem=_game.Inventory.Bag.Concat(_game.Inventory.Stash).Concat(_game.Inventory.Equipped.Values).FirstOrDefault(i=>i.Id==_selectedItem);
        if(selectedItem is null){Wrapped("Välj ett föremål för att se egenskaper, jämföra och flytta det.",new Vector2(914,206),300,18,Muted,28);return;}
        var d=selectedItem.Data;Wrapped(d.Name,new Vector2(914,186),300,21,Pale,27);
        Text(Items.SlotName(d.Slot),new Vector2(914,251),13,Gold);
        Wrapped(d.Description,new Vector2(914,285),300,15,Muted,23);
        Wrapped(ItemStats(d),new Vector2(914,364),300,17,Gold,26);
        _game.Inventory.Equipped.TryGetValue(d.Slot,out var equipped);
        bool worn=equipped?.Id==selectedItem.Id;
        if(!worn&&equipped!=null)
            Wrapped($"Nu: {equipped.Data.Name}\nSkillnad: skada {d.Damage-equipped.Data.Damage:+0;-0;0} · skydd {d.Armor-equipped.Data.Armor:+0;-0;0} % · återhämtning {d.Recovery-equipped.Data.Recovery:+0;-0;0}/s",new Vector2(914,448),300,13,Muted,20);
        if(worn)
        {
            if(d.Slot is not (GearSlot.Saber or GearSlot.Hammer))Button(new Rect2(914,552,304,43),"Ta av","inv-remove");
            else Text("Utrustat · byt via väskan",new Vector2(914,578),14,Gold);
        }
        else if(_game.Inventory.Bag.Any(i=>i.Id==selectedItem.Id))
        {
            Button(new Rect2(914,508,304,40),"Utrusta","inv-equip");
            Button(new Rect2(914,555,304,40),"Flytta till stash","inv-store");
        }
        else Button(new Rect2(914,555,304,40),"Flytta till väskan","inv-take");
    }
    private void DrawInventoryStats()
    {
        Panel(new Rect2(375,153,865,465),.98f);
        Text("KARLS EGENSKAPER",new Vector2(401,187),17,Gold);
        var rows=new (string,string)[]{("Liv",$"{Math.Ceiling(_game.Health)} / 100"),("Uthållighet",$"{Math.Ceiling(_game.Stamina)} / 100"),("Aktivt vapen",_game.WeaponName),("Grundhugg",$"{_game.AttackDamage:0.#} skada"),("Tungt hugg",$"{_game.AttackDamage*1.8f:0.#} skada"),("Skadeskydd",$"{_game.EquipmentArmor:0} %"),("Återhämtning i vila",$"{29+_game.EquipmentRecovery:0} uthållighet/s"),("Besegrade / parader",$"{_game.Kills} / {_game.Parries}")};
        for(int i=0;i<rows.Length;i++){float y=229+i*40;Text(rows[i].Item1,new Vector2(401,y),17,Muted);Text(rows[i].Item2,new Vector2(768,y),17,Pale);}
        Wrapped("Skadan visas före kombinationer, ripost och fiendens gard. Skadeskyddet gäller även kulor och områdesskada.",new Vector2(401,575),810,13,Muted,19);
    }
    private void DrawDroppedItems()
    {
        foreach(var drop in _game.LocalDrops)
        {
            var p=G(drop.Position);DrawCircle(p,14,new Color(.04f,.05f,.04f,.85f));ItemIcon(drop.Item.Data.Slot,p,22,Gold);
            if(System.Numerics.Vector2.Distance(_game.Player,drop.Position)<85)
            {Text(drop.Item.Data.Name,p+new Vector2(-60,32),12,Gold);Text(_game.Inventory.Bag.Count>=Items.BagCapacity?"Väskan är full":"E / B · Plocka upp",p+new Vector2(-60,49),11,Muted);}
        }
    }
}
