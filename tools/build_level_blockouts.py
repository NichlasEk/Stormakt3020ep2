"""Run with Blender --background --python. Metres, shared orthographic camera.
Exports measured paint guides AND the projected gameplay layout from one scene.
"""
import bpy, math, json
from pathlib import Path
from mathutils import Vector
from bpy_extras.object_utils import world_to_camera_view

ROOT=Path(__file__).resolve().parents[1]
OUT=ROOT/'assets/source/journey';OUT.mkdir(parents=True,exist_ok=True)
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
scene=bpy.context.scene
scene.render.engine='CYCLES';scene.cycles.device='CPU';scene.cycles.samples=12
scene.render.resolution_x=1536;scene.render.resolution_y=1024;scene.render.resolution_percentage=100
scene.world.color=(.11,.11,.11)
def mat(name,color):
 m=bpy.data.materials.new(name);m.diffuse_color=(*color,1);return m
stone=mat('stone',(.25,.26,.27));wood=mat('wood',(.22,.13,.07));water=mat('water',(.08,.14,.17));brass=mat('marker',(.55,.4,.13));white=mat('human-scale',(.65,.65,.61))
def box(name,pos,size,material):
 bpy.ops.mesh.primitive_cube_add(size=1,location=pos);o=bpy.context.object;o.name=name;o.dimensions=size;bpy.ops.object.transform_apply(location=False,rotation=False,scale=True);o.data.materials.append(material);return o
bpy.ops.object.camera_add(location=(10,-10,11.28));camera=bpy.context.object
camera.rotation_euler=(Vector((0,0,1.28))-camera.location).to_track_quat('-Z','Y').to_euler()
camera.data.type='ORTHO';camera.data.ortho_scale=1536*math.sqrt(2/3)/(150/1.78)
scene.camera=camera
bpy.ops.object.light_add(type='AREA',location=(-3,-4,9));bpy.context.object.data.energy=1100;bpy.context.object.data.shape='DISK';bpy.context.object.data.size=9
def project(x,y,z=0):
 p=world_to_camera_view(scene,camera,Vector((x,y,z)));return [round(p.x*1536,2),round((1-p.y)*1024,2)]
def poly(x,y,w,h):return [project(x-w/2,y-h/2),project(x+w/2,y-h/2),project(x+w/2,y+h/2),project(x-w/2,y+h/2)]
def reference():
 x,y=-4.7,2.5
 box('1.78m adult torso',(x,y,1.12),(.4,.24,.55),white)
 for dx in [-.12,.12]:box('leg',(x+dx,y,.42),(.14,.17,.84),white)
 bpy.ops.mesh.primitive_uv_sphere_add(segments=12,ring_count=8,radius=.115,location=(x,y,1.665));bpy.context.object.data.materials.append(white)
def render(name):
 scene.render.filepath=str(OUT/(name+'-blockout.png'));bpy.ops.render.render(write_still=True)
 bpy.ops.wm.save_as_mainfile(filepath=str(OUT/(name+'-blockout.blend')))

layout={'metres_per_standing_adult':1.78,'standing_pixels':150,'image_size':[1536,1024],
 'ground':poly(0,0,8,8),'warehouseObstacle':poly(0,0,1.8,1.25),
 'warehouseEntry':project(-3,3),'warehouseExit':project(3,-3),
 'whetstone':project(-3,-.8),'manifest':project(-2.8,-2.8),'winch':project(2.8,1.4),
 'shoreEntry':project(-3,3),'survey':[project(-2,-1.6),project(2.5,-2),project(2,2)],
 'reveal':project(0,0),'shoreExit':project(3,-3),
 'warehouseSpawns':[project(-2,-1),project(2,-2),project(2,1.4)],
 'shoreSpawns':[project(-2,-2),project(2,-2),project(2,2)]}
(OUT/'layout.json').write_text(json.dumps(layout,indent=2)+'\n')
box('floor',(0,0,-.12),(8,8,.24),stone)
# Far walls only: roof and near walls cut away for action readability.
box('back wall',(0,4.12,1.25),(8.24,.24,2.5),stone)
box('side wall',(-4.12,0,1.25),(.24,8,2.5),stone)
box('crate island',(0,0,.35),(1.8,1.25,.7),wood)
for x,y in [(-3,-.8),(-2.8,-2.8),(2.8,1.4)]:box('interaction plinth',(x,y,.18),(.38,.38,.36),brass)
reference();render('warehouse')
for o in list(bpy.data.objects):
 if o.type=='MESH':bpy.data.objects.remove(o,do_unlink=True)
box('sea',(0,0,-.55),(35,35,.1),water);box('rock shelf',(0,0,-.22),(8,8,.44),stone)
for x,y in [(-4.5,-3),(3,-4.5),(4.5,3)]:box('2.3m standing stone',(x,y,1.15),(.45,.65,2.3),stone)
for x,y in [(-2,-1.6),(2.5,-2),(2,2)]:box('survey mark',(x,y,.035),(.38,.38,.07),brass)
reference();render('shore')
print('LAYOUT',json.dumps(layout))
