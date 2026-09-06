"""Dress the approved motion rig and render deterministic 2D sprite trials.
blender -b -t 4 --python tools/build_karl_sprite_model.py -- --study
blender -b -t 4 --python tools/build_karl_sprite_model.py -- --sprites
"""
import bpy,math,json,sys,random
from pathlib import Path
from mathutils import Vector,Matrix
ROOT=Path(__file__).resolve().parents[1];OUT=ROOT/'assets/source/animation';PREVIEW=ROOT/'artifacts/karl-model';PREVIEW.mkdir(parents=True,exist_ok=True)
bpy.ops.wm.open_mainfile(filepath=str(OUT/'karl-walk-reference.blend'))
scene=bpy.context.scene;scene.frame_set(1)
rig=bpy.data.objects['Karl · motion reference'];rig.animation_data.action.name='Karl · continuous walk'
for o in list(bpy.data.objects):
 if o.type not in {'ARMATURE','CAMERA'}:bpy.data.objects.remove(o,do_unlink=True)
scene.render.engine='CYCLES';scene.cycles.samples=24;scene.cycles.use_denoising=True
scene.cycles.device='CPU';scene.render.film_transparent=True
scene.view_settings.view_transform='AgX';scene.view_settings.look='AgX - Medium High Contrast'
scene.world.use_nodes=True;scene.world.node_tree.nodes['Background'].inputs[0].default_value=(.20,.24,.29,1);scene.world.node_tree.nodes['Background'].inputs[1].default_value=.35
random.seed(3020)
def mat(name,color,rough=.9,noise=40):
 m=bpy.data.materials.new(name);m.use_nodes=True;n=m.node_tree.nodes;l=m.node_tree.links;p=n.get('Principled BSDF')
 p.inputs['Roughness'].default_value=rough;p.inputs['Specular IOR Level'].default_value=.14
 tex=n.new('ShaderNodeTexNoise');tex.inputs['Scale'].default_value=noise;tex.inputs['Detail'].default_value=3;tex.inputs['Roughness'].default_value=.75
 coord=n.new('ShaderNodeTexCoord');l.new(coord.outputs['Generated'],tex.inputs['Vector'])
 ramp=n.new('ShaderNodeValToRGB');ramp.color_ramp.elements[0].position=.18;ramp.color_ramp.elements[1].position=.82
 ramp.color_ramp.elements[0].color=(*(v*.36 for v in color),1);ramp.color_ramp.elements[1].color=(*color,1)
 l.new(tex.outputs['Fac'],ramp.inputs[0]);l.new(ramp.outputs[0],p.inputs['Base Color'])
 bump=n.new('ShaderNodeBump');bump.inputs['Strength'].default_value=.28;bump.inputs['Distance'].default_value=.009
 l.new(tex.outputs['Fac'],bump.inputs['Height']);l.new(bump.outputs[0],p.inputs['Normal'])
 return m
navy=mat('Worn indigo wool',(.055,.092,.145));leather=mat('Dry dark leather',(.075,.054,.036),.96,26)
brass=mat('Tarnished brass',(.30,.215,.095),.84,24);linen=mat('Soiled collar linen',(.25,.225,.18))
skin=mat('Restrained painted skin',(.39,.255,.17),.89,9);hair=mat('Short dark brown hair',(.08,.048,.026),.96,18)
shadow=mat('Eye sockets and mouth',(.04,.026,.017));steel=mat('Worn steel',(.23,.25,.255),.7,35)

def bind(o,bone,material):
 o.data.materials.append(material)
 g=o.vertex_groups.new(name=bone);g.add(list(range(len(o.data.vertices))),1,'REPLACE')
 mod=o.modifiers.new('Karl skeletal skin','ARMATURE');mod.object=rig;o.parent=rig
 for p in o.data.polygons:p.use_smooth=True
 return o

def mesh(name,verts,faces,bone,material):
 data=bpy.data.meshes.new(name);data.from_pydata(verts,[],faces);data.update()
 o=bpy.data.objects.new(name,data);scene.collection.objects.link(o);return bind(o,bone,material)

def ell(name,p,scale,bone,material):
 bpy.ops.mesh.primitive_uv_sphere_add(segments=24,ring_count=16,location=p);o=bpy.context.object;o.name=name;o.scale=scale
 bpy.ops.object.transform_apply(location=False,rotation=False,scale=True);return bind(o,bone,material)

def rod(name,a,b,r1,r2,bone,material):
 a,b=Vector(a),Vector(b);d=b-a
 bpy.ops.mesh.primitive_cone_add(vertices=20,radius1=r1,radius2=r2,depth=d.length,location=(a+b)/2)
 o=bpy.context.object;o.name=name;o.rotation_euler=d.to_track_quat('Z','Y').to_euler()
 bpy.ops.object.transform_apply(location=False,rotation=True,scale=True);return bind(o,bone,material)

def box(name,p,size,bone,material,bevel=.008):
 bpy.ops.mesh.primitive_cube_add(size=1,location=p);o=bpy.context.object;o.name=name;o.scale=size
 bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
 if bevel:
  mod=o.modifiers.new('Worn edges','BEVEL');mod.width=bevel;mod.segments=2
 return bind(o,bone,material)

def bone(name):return rig.data.bones[name].head_local.copy(),rig.data.bones[name].tail_local.copy()

def sleeve(name,a,b,radii,bname,material):
 a,b=Vector(a),Vector(b);axis=(b-a).normalized();u=axis.cross(Vector((0,1,0))).normalized();v=axis.cross(u);verts=[];faces=[];count=24
 for k,r in enumerate(radii):
  t=k/(len(radii)-1)
  for j in range(count):
   angle=j*math.tau/count;fold=1+.05*math.sin(angle*7+t*21)+.025*math.sin(angle*11-t*14)
   verts.append(tuple(a+(b-a)*t+(u*math.cos(angle)+v*math.sin(angle))*r*fold))
 for k in range(len(radii)-1):
  for j in range(count):faces.append((k*count+j,k*count+(j+1)%count,(k+1)*count+(j+1)%count,(k+1)*count+j))
 faces.extend([tuple(reversed(range(count))),tuple(range((len(radii)-1)*count,len(radii)*count))])
 return mesh(name,verts,faces,bname,material)

# Articulated limbs have overlapping rounded joint geometry, not cut-up image patches.
for side in ('L','R'):
 hip,knee=bone('thigh.'+side);_,ankle=bone('shin.'+side);shoulder,elbow=bone('upperarm.'+side);_,wrist=bone('forearm.'+side)
 sleeve('Breeches '+side,hip,knee,[.085,.091,.082,.072,.069],'thigh.'+side,leather)
 ell('Knee '+side,knee,(.067,.068,.068),'shin.'+side,leather)
 sleeve('Creased boot shaft '+side,knee,ankle,[.07,.074,.059,.047,.049],'shin.'+side,leather)
 cuff=knee+(ankle-knee)*.06;sleeve('Turned boot cuff '+side,knee,cuff,[.075,.078,.072],'shin.'+side,leather)
 ell('Ankle '+side,ankle,(.045,.049,.042),'foot.'+side,leather)
 ell('Boot vamp '+side,ankle+Vector((0,-.054,-.046)),(.056,.129,.039),'foot.'+side,leather)
 box('Boot sole '+side,ankle+Vector((0,-.05,-.075)),(.107,.225,.012),'foot.'+side,leather,.006)
 sleeve('Upper sleeve '+side,shoulder,elbow,[.075,.078,.068,.059,.057],'upperarm.'+side,navy)
 ell('Elbow wool '+side,elbow,(.057,.056,.055),'forearm.'+side,navy)
 sleeve('Lower sleeve '+side,elbow,wrist,[.058,.06,.054,.047,.043],'forearm.'+side,navy)
 sleeve('Worn leather cuff '+side,wrist+(elbow-wrist)*.25,wrist,[.052,.056,.053],'forearm.'+side,brass)
 ell('Shoulder wool '+side,shoulder,(.073,.078,.073),'upperarm.'+side,navy)
 ell('Glove '+side,wrist+Vector((0,-.01,-.037)),(.036,.039,.055),'hand.'+side,leather)
 for j in range(4):
  x=(j-1.5)*.013;rod('Gloved fingers '+side,wrist+Vector((x,-.033,-.042)),wrist+Vector((x,-.018,-.077)),.008,.009,'hand.'+side,leather)

pelvis=bone('pelvis')[0];chest=bone('spine')[1]
# Fitted waistcoat and open-front long coat, with folded/layered hems.
def garment(name,rings,start,end,material,bname='spine',segments=48):
 verts=[];faces=[]
 for k,(z,rx,ry) in enumerate(rings):
  for j in range(segments+1):
   theta=start+(end-start)*j/segments
   ripple=(.002+.005*k/max(1,len(rings)-1))*math.cos(theta*12+k*.9)
   verts.append(tuple(Vector((pelvis.x+(rx+ripple)*math.cos(theta),pelvis.y+(ry+ripple)*math.sin(theta),z))))
 for k in range(len(rings)-1):
  for j in range(segments):
   a=k*(segments+1)+j;faces.append((a,a+1,a+segments+2,a+segments+1))
 o=mesh(name,verts,faces,bname,material);solid=o.modifiers.new('Cloth thickness','SOLIDIFY');solid.thickness=.007
 return o
waist=pelvis.z+.13
rings=[(pelvis.z-.07,.16,.106),(waist,.138,.102),(chest.z-.12,.189,.115),(chest.z+.005,.18,.107)]
garment('Buttoned waistcoat',rings,0,math.tau,leather)
coat=garment('Long split indigo coat',[(.53,.23,.175),(.66,.225,.168),(.80,.186,.133),(waist,.148,.116),(chest.z-.13,.202,.13),(chest.z+.01,.199,.117)],-math.pi/2+.45,math.pi*1.5-.45,navy)
# Hem follows a small, smooth fraction of each thigh. Upper coat stays on spine.
spinegroup=coat.vertex_groups.get('spine');groups={s:coat.vertex_groups.new(name='thigh.'+s) for s in ('L','R')}
for vertex in coat.data.vertices:
 z=vertex.co.z;w=max(0,min(.20,(.84-z)*.6));side='L' if vertex.co.x<0 else 'R'
 spinegroup.add([vertex.index],1-w,'REPLACE');groups[side].add([vertex.index],w,'REPLACE')
# Brass edge piping and two rows of small buttons, aged rather than bright gold.
for z in [waist+.04,waist+.115,waist+.19,waist+.265,waist+.34]:
 for x in [-.051,.051]:ell('Waistcoat button',(x,pelvis.y-.12,z),(.008,.006,.008),'spine',brass)
for sign in (-1,1):
 rod('Coat front seam',(sign*.11,pelvis.y-.15,.56),(sign*.065,pelvis.y-.127,chest.z),.003,.003,'spine',brass)
 box('Raised indigo collar',(sign*.058,pelvis.y-.045,chest.z+.045),(.033,.14,.07),'spine',navy,.006)
box('Linen neckcloth',(0,pelvis.y-.04,chest.z+.035),(.075,.095,.061),'spine',linen,.014)
garment('Waist belt',[(waist-.025,.154,.124),(waist+.025,.154,.124)],0,math.tau,leather)
box('Belt buckle',(0,pelvis.y-.132,waist),(.064,.014,.045),'spine',brass,.005)
box('Buckle inset',(0,pelvis.y-.141,waist),(.044,.006,.026),'spine',leather,.002)
box('Leather ammunition pouch',(-.153,-.025,waist-.08),(.09,.075,.10),'spine',leather,.01)

# Adult head: narrow temples, defined jaw, nose bridge, brows and short swept hair.
neck,head=bone('head');center=neck+Vector((0,0,.092))
rod('Neck',neck-Vector((0,0,.04)),neck+Vector((0,0,.06)),.040,.043,'head',skin)
verts=[];faces=[];profile=[(-.107,.036,.047),(-.085,.053,.061),(-.038,.067,.073),(.025,.071,.079),(.075,.065,.069),(.105,.043,.046)]
for z,rx,ry in profile:
 for j in range(32):
  a=j*math.tau/32;verts.append(tuple(center+Vector((rx*math.cos(a),ry*math.sin(a),z))))
for k in range(len(profile)-1):
 for j in range(32):faces.append((k*32+j,k*32+(j+1)%32,(k+1)*32+(j+1)%32,(k+1)*32+j))
faces.extend([tuple(reversed(range(32))),tuple(range(160,192))]);mesh('Karl face',verts,faces,'head',skin)
for sign in (-1,1):
 ell('Ear',center+Vector((sign*.070,.005,-.013)),(.011,.018,.027),'head',skin)
 ell('Eye socket',center+Vector((sign*.030,-.070,.015)),(.021,.009,.009),'head',shadow)
 ell('Muted eye',center+Vector((sign*.030,-.077,.015)),(.012,.003,.004),'head',linen)
 ell('Iris',center+Vector((sign*.030,-.080,.015)),(.004,.002,.004),'head',shadow)
 rod('Brow',center+Vector((sign*.014,-.077,.029)),center+Vector((sign*.048,-.068,.025)),.006,.007,'head',hair)
# Nose is a small wedge rather than a round primitive.
mesh('Nose bridge',[tuple(center+Vector(p)) for p in [(-.010,-.073,.027),(.010,-.073,.027),(-.013,-.077,-.018),(.013,-.077,-.018),(0,-.101,-.013)]],[(0,1,4),(1,3,4),(3,2,4),(2,0,4),(0,2,3,1)],'head',skin)
rod('Mouth line',center+Vector((-.019,-.072,-.045)),center+Vector((.019,-.072,-.045)),.002,.002,'head',shadow)
ell('Chin',center+Vector((0,-.045,-.074)),(.033,.025,.022),'head',skin)
# Scalp cap only covers the crown; expose the forehead and face.
verts=[];faces=[]
for k in range(9):
 a=.02+k*1.4/8
 for j in range(40):
  theta=j*math.tau/40;z=.105*math.cos(a)+.019
  verts.append(tuple(center+Vector((.074*math.sin(a)*math.cos(theta),.08*math.sin(a)*math.sin(theta)+.01,z))))
for k in range(8):
 for j in range(40):faces.append((k*40+j,k*40+(j+1)%40,(k+1)*40+(j+1)%40,(k+1)*40+j))
mesh('Short swept hair cap',verts,faces,'head',hair)
for j in range(28):
 x=random.uniform(-.053,.053);y=random.uniform(-.039,.055)
 a=center+Vector((x,y,.085+random.uniform(0,.023)));b=a+Vector((.014,.02,-.018))
 ell('Swept hair lock',(a+b)/2,(.012,.023,.008),'head',hair)
# Headband and small brass forehead lamp.
rod('Headband front',center+Vector((-.059,-.054,.061)),center+Vector((.059,-.054,.061)),.006,.006,'head',leather)
ell('Lamp housing',center+Vector((.007,-.078,.061)),(.018,.010,.018),'head',brass)
ell('Unlit lamp lens',center+Vector((.007,-.087,.061)),(.011,.004,.011),'head',linen)

wrist=bone('hand.R')[0];h=wrist+Vector((0,-.01,-.053));tip=h+Vector((.025,-.40,-.54))
rod('Saber grip',h+Vector((0,.025,.05)),h,.012,.012,'hand.R',leather)
# Flat, complete blade geometry; no adjacent atlas frame can crop the tip.
axis=(tip-h).normalized();edge=Vector((.013,0,0));verts=[tuple(h-edge),tuple(h+edge),tuple(tip)]
blade=mesh('Saber blade',verts,[(0,1,2)],'hand.R',steel);solid=blade.modifiers.new('Blade thickness','SOLIDIFY');solid.thickness=.003
rod('Saber crossguard',h+Vector((-.055,0,0)),h+Vector((.055,0,0)),.006,.006,'hand.R',brass)

# Broad lighting reveals matte painted-like material breakup without plastic shine.
def light(name,loc,power,color,size):
 bpy.ops.object.light_add(type='AREA',location=loc);o=bpy.context.object;o.name=name;o.data.energy=power;o.data.color=color;o.data.size=size
 o.rotation_euler=(Vector((0,0,.9))-o.location).to_track_quat('-Z','Y').to_euler()
light('Warm overcast key',(-3,-4,6),450,(1,.84,.65),5)
light('Cool sky fill',(4,2,5),330,(.62,.73,.88),6)
camera=scene.camera;camera.data.ortho_scale=2.3
scene.render.resolution_x=384;scene.render.resolution_y=384;scene.render.resolution_percentage=100
scene.render.image_settings.file_format='PNG';scene.render.image_settings.color_mode='RGBA'
views=[('se',Vector((3,-5,4.5))),('ne',Vector((3,5,4.5))),('nw',Vector((-3,5,4.5))),('sw',Vector((-3,-5,4.5)))]
# Anchor is the projected world origin, invariant across all frames and all directions.
from bpy_extras.object_utils import world_to_camera_view
manifest={'status':'costume and 2D integration trial; not final approved Karl likeness','frames':30,'cycle_m':1.0,'size':384,'views':[],'generator':'tools/build_karl_sprite_model.py','base':'karl-walk-reference.blend'}
for direction,view in views:
 target=Vector((0,0,.84));camera.location=target+view;camera.rotation_euler=(target-camera.location).to_track_quat('-Z','Y').to_euler()
 rig.location=Vector((0,0,0));bpy.context.view_layer.update();anchor=world_to_camera_view(scene,camera,Vector((0,0,0)))
 manifest['views'].append({'direction':direction,'anchor_px':[anchor.x*384,(1-anchor.y)*384]})
scene.frame_set(1);rig.location=(0,0,0)
bpy.ops.wm.save_as_mainfile(filepath=str(OUT/'karl-costume-v1.blend'))
(OUT/'karl-costume-v1.json').write_text(json.dumps(manifest,indent=2)+'\n')
if '--study' in sys.argv or '--sprites' in sys.argv:
 for direction,view in views:
  folder=PREVIEW/direction;folder.mkdir(exist_ok=True);target=Vector((0,0,.84));camera.rotation_euler=(target-(target+view)).to_track_quat('-Z','Y').to_euler()
  for frame in (range(1,31) if '--sprites' in sys.argv else [1,8,16,23]):
   scene.frame_set(frame);camera.location=target+view+rig.location;scene.render.filepath=str(folder/f'{frame:03}.png');bpy.ops.render.render(write_still=True)
 print('KARL COSTUME SPRITES COMPLETE',flush=True)
