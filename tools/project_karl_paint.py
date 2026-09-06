"""Register a single painted atlas on the rigged surface, then render 2D frames.
The painted view projections stay on the mesh across the entire walk cycle.
"""
import bpy,sys,json,math
from pathlib import Path
from mathutils import Vector
from bpy_extras.object_utils import world_to_camera_view
ROOT=Path(__file__).resolve().parents[1];SOURCE=ROOT/'assets/source/animation';OUT=ROOT/'artifacts/karl-painted';OUT.mkdir(exist_ok=True)
bpy.ops.wm.open_mainfile(filepath=str(SOURCE/'karl-costume-v1.blend'))
scene=bpy.context.scene;scene.frame_set(1);rig=bpy.data.objects['Karl · motion reference'];rig.location=(0,0,0)
bpy.context.view_layer.update();camera=scene.camera;target=Vector((0,0,.84))
texture=bpy.data.images.load(str(SOURCE/'karl-paint-v1.png'));texture.pack()
materials={}
def painted_material(original):
 if original.name in materials:return materials[original.name]
 material=original.copy();material.name=original.name+' · registered paint';nodes=material.node_tree.nodes;links=material.node_tree.links
 shader=nodes.get('Principled BSDF');base=shader.inputs['Base Color'].links[0].from_socket
 paint=nodes.new('ShaderNodeTexImage');paint.image=texture
 separate=nodes.new('ShaderNodeSeparateColor');links.new(paint.outputs['Color'],separate.inputs[0])
 def mathnode(op,a,b):
  n=nodes.new('ShaderNodeMath');n.operation=op
  for index,value in enumerate((a,b)):
   if isinstance(value,(int,float)):n.inputs[index].default_value=value
   else:links.new(value,n.inputs[index])
  return n.outputs[0]
 rgb=[separate.outputs[i] for i in range(3)]
 high=mathnode('MAXIMUM',mathnode('MAXIMUM',rgb[0],rgb[1]),rgb[2]);low=mathnode('MINIMUM',mathnode('MINIMUM',rgb[0],rgb[1]),rgb[2])
 # Gray template margins are not costume. Exclude them instead of painting gray holes.
 chroma=mathnode('SUBTRACT',high,low);colored=mathnode('GREATER_THAN',chroma,.015)
 dark=mathnode('LESS_THAN',high,.029);valid=mathnode('MAXIMUM',colored,dark)
 fallback=nodes.new('ShaderNodeMixRGB');fallback.blend_type='MULTIPLY';fallback.inputs[0].default_value=1;fallback.inputs[2].default_value=(.18,.18,.18,1);links.new(base,fallback.inputs[1])
 mix=nodes.new('ShaderNodeMixRGB');links.new(valid,mix.inputs[0]);links.new(fallback.outputs[0],mix.inputs[1]);links.new(paint.outputs['Color'],mix.inputs[2])
 emission=nodes.new('ShaderNodeEmission');links.new(mix.outputs[0],emission.inputs[0]);emission.inputs[1].default_value=1
 links.new(emission.outputs[0],nodes.get('Material Output').inputs[0]);materials[original.name]=material
 return material
projections=[Vector((3,-5,4.5)),Vector((3,5,4.5)),Vector((-3,5,4.5)),Vector((-3,-5,4.5))]
camera_matrices=[]
for view in projections:
 camera.location=target+view;camera.rotation_euler=(-view).to_track_quat('-Z','Y').to_euler();bpy.context.view_layer.update();camera_matrices.append(camera.matrix_world.inverted().copy())
for obj in [o for o in bpy.data.objects if o.type=='MESH']:
 # Calculate source-pose world positions before bevel/solidify changes topology.
 points=[]
 for vert in obj.data.vertices:
  rest=obj.matrix_world@vert.co;deformed=Vector((0,0,0));total=0
  for g in vert.groups:
   name=obj.vertex_groups[g.group].name
   if name not in rig.pose.bones:continue
   deformed+=(rig.pose.bones[name].matrix@rig.data.bones[name].matrix_local.inverted()@rest)*g.weight;total+=g.weight
  points.append(deformed/total if total else rest)
 uv=obj.data.uv_layers.new(name='RegisteredPaint')
 for polygon in obj.data.polygons:
  a,b,c=[points[i] for i in polygon.vertices[:3]];normal=(b-a).cross(c-a).normalized()
  direction=max(range(4),key=lambda i:normal.dot(projections[i].normalized()))
  projection=camera_matrices[direction]
  for loop_index in polygon.loop_indices:
   point=points[obj.data.loops[loop_index].vertex_index];local=projection@point;p=Vector((local.x/camera.data.ortho_scale+.5,local.y/camera.data.ortho_scale+.5,0))
   uv.data[loop_index].uv=((direction%2+p.x)/2,(1-direction//2+p.y)/2)
 original=obj.data.materials[0]
 if original.name not in {'Worn steel','Soiled collar linen','Eye sockets and mouth'}:
  replacement=painted_material(original);obj.data.materials.clear();obj.data.materials.append(replacement)
# Keep the approved 2D head identity; do not project a painted face onto primitive eyes/nose.
for obj in list(bpy.data.objects):
 if obj.type=='MESH' and obj.name!='Neck' and any(g.name=='head' for g in obj.vertex_groups):bpy.data.objects.remove(obj,do_unlink=True)
scene.render.resolution_x=384;scene.render.resolution_y=384;scene.cycles.samples=8
# Screen direction is calculated from projected travel, not guessed from camera names.
views=[('se',Vector((-3,-5,4.5))),('ne',Vector((-3,5,4.5))),('nw',Vector((3,5,4.5))),('sw',Vector((3,-5,4.5)))]
manifest={'status':'painted 2D sprite trial','frame_size':384,'frames':30,'cycle_m':1,'views':[]}
for name,view in views:
 camera.location=target+view;camera.rotation_euler=(-view).to_track_quat('-Z','Y').to_euler();bpy.context.view_layer.update()
 root=world_to_camera_view(scene,camera,Vector((0,0,0)));end=world_to_camera_view(scene,camera,Vector((0,-1,0)))
 crown=world_to_camera_view(scene,camera,Vector((0,0,1.64)))
 anchor=[root.x*384,(1-root.y)*384];delta=Vector(((end.x-root.x)*384,(root.y-end.y)*384))
 scale=150/((crown.y-root.y)*384)
 manifest['views'].append({'direction':name,'anchor_px':anchor,'scale':scale,'cycle_world_px':delta.length*scale,'travel_screen':[delta.x,delta.y],'head_centers_px':[]})
assert all(v['travel_screen'][0]*x>0 and v['travel_screen'][1]*y>0 for v,x,y in zip(manifest['views'],[1,1,-1,-1],[1,-1,-1,1]))
(SOURCE/'karl-baked-v1.json').write_text(json.dumps(manifest,indent=2)+'\n')
scene.frame_set(1);rig.location=(0,0,0);bpy.ops.wm.save_as_mainfile(filepath=str(SOURCE/'karl-painted-v1.blend'))
if '--validate' in sys.argv:
 print('PROJECTION VALIDATION PASS · four direction signs, anchors and scale');raise SystemExit(0)
for name,view in views:
 folder=OUT/name;folder.mkdir(exist_ok=True);camera.rotation_euler=(-view).to_track_quat('-Z','Y').to_euler()
 for frame in (range(1,31) if '--sprites' in sys.argv else [1,8,16,23]):
  scene.frame_set(frame);camera.location=target+view+rig.location;bpy.context.view_layer.update()
  head=rig.pose.bones['head'];center=rig.matrix_world@(head.head+(head.tail-head.head).normalized()*.092)
  head_uv=world_to_camera_view(scene,camera,center)
  next(v for v in manifest['views'] if v['direction']==name)['head_centers_px'].append([head_uv.x*384,(1-head_uv.y)*384])
  scene.render.filepath=str(folder/f'{frame:03}.png');bpy.ops.render.render(write_still=True)
(SOURCE/'karl-baked-v1.json').write_text(json.dumps(manifest,indent=2)+'\n')
print('PAINTED SPRITE RENDER COMPLETE',flush=True)
