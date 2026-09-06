"""Run with blender -b -t 4 --python tools/render_actors.py. Original authored geometry."""
import bpy, math, os
from mathutils import Vector
ROOT=os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
scene=bpy.context.scene
scene.render.engine='CYCLES'
scene.cycles.device='CPU';scene.cycles.samples=8;scene.cycles.use_denoising=True
scene.render.resolution_x=256;scene.render.resolution_y=320;scene.render.resolution_percentage=100
scene.render.image_settings.file_format='PNG';scene.render.image_settings.color_mode='RGBA';scene.render.film_transparent=True
scene.display.shading.light='STUDIO';scene.display.shading.studiolight_rotate_z=0.4
scene.display.shading.color_type='MATERIAL';scene.display.shading.show_shadows=True
scene.display.shading.show_cavity=True;scene.display.shading.cavity_type='BOTH'
scene.display.shading.show_specular_highlight=True
scene.view_settings.view_transform='AgX'
scene.world.color=(.045,.045,.05)
def light(name,loc,power,color,size):
 bpy.ops.object.light_add(type='AREA',location=loc);o=bpy.context.object;o.name=name;o.data.energy=power;o.data.color=color;o.data.shape='DISK';o.data.size=size
 o.rotation_euler=(Vector((0,0,1.1))-o.location).to_track_quat('-Z','Y').to_euler()
light('Warm lantern key',(-3,-4,6),450,(1.0,.75,.48),4)
light('Cold sky fill',(3,2,5),260,(.48,.61,.77),5)
bpy.ops.object.camera_add(location=(0,-8,10));camera=bpy.context.object
camera.rotation_euler=(Vector((0,0,1.15))-camera.location).to_track_quat('-Z','Y').to_euler()
camera.data.type='ORTHO';camera.data.ortho_scale=3.85;scene.camera=camera
def mat(name,c):
 m=bpy.data.materials.new(name);m.diffuse_color=(*c,1);m.use_nodes=True
 nodes=m.node_tree.nodes;links=m.node_tree.links;shader=nodes.get('Principled BSDF')
 shader.inputs['Roughness'].default_value=.88
 shader.inputs['Specular IOR Level'].default_value=.18
 noise=nodes.new('ShaderNodeTexNoise');noise.inputs['Scale'].default_value=28;noise.inputs['Detail'].default_value=3
 ramp=nodes.new('ShaderNodeValToRGB');ramp.color_ramp.elements[0].position=.18;ramp.color_ramp.elements[1].position=.82
 ramp.color_ramp.elements[0].color=(*(v*.35 for v in c),1);ramp.color_ramp.elements[1].color=(*c,1)
 links.new(noise.outputs['Fac'],ramp.inputs[0]);links.new(ramp.outputs['Color'],shader.inputs['Base Color'])
 bump=nodes.new('ShaderNodeBump');bump.inputs['Strength'].default_value=.25;bump.inputs['Distance'].default_value=.025
 links.new(noise.outputs['Fac'],bump.inputs['Height']);links.new(bump.outputs['Normal'],shader.inputs['Normal'])
 return m
navy=mat('Indigo wool',(0.022,0.053,0.095));gold=mat('Old brass',(0.42,0.27,0.10));skin=mat('Skin',(0.45,0.29,0.20));iron=mat('Iron',(0.12,0.14,0.15));black=mat('Boot leather',(0.022,0.025,0.030));silver=mat('Blade',(0.38,0.43,0.45));white=mat('Bone linen',(0.47,0.43,0.34));red=mat('Oxblood',(0.19,0.045,0.031));teal=mat('Silver light',(0.14,0.38,0.34));brown=mat('Walnut',(0.10,0.065,0.031))
objects=[]
def finish(o,name,material):
 o.name=name;o.data.materials.append(material);objects.append(o);return o
def sphere(name,p,s,material):
 bpy.ops.mesh.primitive_uv_sphere_add(segments=12,ring_count=8,location=p);o=bpy.context.object;o.scale=s;return finish(o,name,material)
def box(name,p,s,material):
 bpy.ops.mesh.primitive_cube_add(size=1,location=p);o=bpy.context.object;o.scale=s;return finish(o,name,material)
def rod(name,a,b,r,material):
 a,b=Vector(a),Vector(b);d=b-a
 bpy.ops.mesh.primitive_cylinder_add(vertices=8,radius=r,depth=d.length,location=(a+b)/2)
 o=bpy.context.object;o.rotation_euler=d.to_track_quat('Z','Y').to_euler();return finish(o,name,material)
def actor(kind,pose):
 coat=navy if kind.startswith('karl') else red if kind=='gunner' else iron
 boss=kind=='collector';scale=1.25 if boss else 1
 sway=0.20 if pose==1 else -0.20 if pose==2 else 0
 for side in [-1,1]:
  hip=(side*.17,0,.94);knee=(side*.19,side*sway,.56);foot=(side*.20,side*sway*1.7,.18)
  rod('Breeches',hip,knee,.09,white if kind.startswith('karl') else coat)
  rod('Tall boot',knee,foot,.082,black);sphere('Toe',(foot[0],foot[1]-.07,.13),(.09,.19,.10),black)
 # Bell-shaped coat skirt, fitted torso, broad shoulder cape and brass breastplate.
 bpy.ops.mesh.primitive_cone_add(vertices=8,radius1=.30,radius2=.21,depth=.58,location=(0,0,1.02));finish(bpy.context.object,'Coat tails',coat)
 box('Coat',(0,0,1.43),(.47,.31,.59),coat)
 box('Belt',(0,-.02,1.16),(.47,.34,.06),black);box('Buckle',(0,-.205,1.16),(.07,.03,.075),gold)
 for side in [-1,1]:rod('Coat seam',(side*.22,-.165,1.65),(side*.24,-.20,.79),.013,brown)
 for z in [1.28,1.39,1.5,1.61]:sphere('Brass button',(.07,-.165,z),(.015,.014,.018),gold)
 box('Collar',(0,-.025,1.73),(.24,.23,.08),coat)
 sphere('Head',(0,-.012,1.91),(.135,.125,.185),skin if kind.startswith('karl') else silver)
 sphere('Nose',(0,-.135,1.92),(.026,.044,.043),skin if kind.startswith('karl') else iron)
 for x in [-.052,.052]:sphere('Eyes',(x,-.13,1.963),(.014,.012,.009),black if kind.startswith('karl') else teal)
 sphere('Hat crown',(0,.012,2.10),(.18,.155,.11),black)
 # Folded tricorn edges.
 brim=bpy.data.meshes.new('Folded leather brim');brim.from_pydata([(-.27,-.19,2.04),(.27,-.19,2.04),(0,.26,2.04),(0,0,2.10)],[],[(0,1,3),(1,2,3),(2,0,3)])
 o=bpy.data.objects.new('Tricorn',brim);bpy.context.collection.objects.link(o);finish(o,'Tricorn',black)
 for a,b in [((-.27,-.19,2.04),(.27,-.19,2.04)),((.27,-.19,2.04),(0,.26,2.04)),((0,.26,2.04),(-.27,-.19,2.04))]:rod('Tricorn edging',a,b,.014,brown)
 if boss:
  for x in [-.21,-.10,0,.10,.21]:rod('Crown prong',(x,0,2.2),(x,0,2.50-abs(x)),.033,gold)
  box('Ledger on back',(0,.26,1.49),(.66,.18,.72),brown)
 for side in [-1,1]:
  shoulder=(side*.33,0,1.63);elbow=(side*.45,-.06,1.34);hand=(side*.43,-.24,1.15)
  if side==1:
   if pose==3:elbow=(.46,.0,1.88);hand=(.32,.05,2.24)
   elif pose==4:elbow=(.35,-.38,1.52);hand=(.15,-.73,1.32)
   elif pose==5:elbow=(.35,-.22,1.46);hand=(.05,-.40,1.78)
  rod('Sleeve',shoulder,elbow,.083,coat);rod('Forearm',elbow,hand,.068,coat)
  box('Shoulder strap',shoulder,(.13,.17,.045),brown);sphere('Cuff',hand,(.078,.065,.075),gold);sphere('Glove',(hand[0],hand[1]-.035,hand[2]-.04),(.054,.05,.072),white)
  if side==1:
   h=Vector(hand)
   d=Vector((.10,-.14,1)) if pose==3 else Vector((-.25,-1,.05)) if pose==4 else Vector((.05,-.14,.85)) if pose==5 else Vector((.10,-.22,-.9))
   tip=h+d
   if kind in ['karl-hammer','collector']:
    rod('Hammer shaft',h,tip,.032,brown);box('Hammer head',tip,(.39,.18,.21),iron);box('Hammer band',tip,(.08,.19,.22),gold)
   elif kind=='pikeman':
    tip=h+Vector((0,-.6,1.6));rod('Pike',h-Vector((0,-.4,1.0)),tip,.033,brown)
    rod('Pike blade',tip,tip+Vector((0,-.13,.33)),.068,silver)
   elif kind=='gunner':
    tip=h+Vector((0,-.65,.25));rod('Carbine',h,tip,.07,brown);rod('Barrel',tip,tip+Vector((0,-.28,.1)),.042,iron)
   else:
    rod('Saber',h,tip,.036,silver);rod('Guard',h+Vector((-.15,0,0)),h+Vector((.15,0,0)),.032,gold)
  elif kind.startswith('karl'):rod('Holstered pistol',(-.29,-.04,1.1),(-.31,-.06,.83),.055,brown)
 return scale
os.makedirs(ROOT+'/assets/source/actor-frames',exist_ok=True)
for kind in ['karl-saber','karl-hammer','guard','pikeman','gunner','collector']:
 for pose in range(7):
  for o in objects:bpy.data.objects.remove(o,do_unlink=True)
  objects=[];scale=actor(kind,pose)
  from mathutils import Matrix
  if pose==6:
   fallen=Matrix.Rotation(math.pi/2,4,'X')
   for o in objects:
    o.location=fallen@(o.location-Vector((0,0,1.0)))+Vector((0,0,.18))
    o.rotation_euler=(fallen.to_3x3()@o.rotation_euler.to_matrix()).to_euler()
  originals=[(o,o.location.copy(),o.rotation_euler.copy(),o.scale.copy()) for o in objects]
  for direction in range(8):
   angle=direction*math.pi/4;rot=Matrix.Rotation(angle,4,'Z')
   for o,p,r,s in originals:
    o.location=(rot@p)*scale;o.rotation_euler=(rot.to_3x3()@r.to_matrix()).to_euler();o.scale=s*scale
   scene.render.filepath=f'{ROOT}/assets/source/actor-frames/{kind}-{pose}-{direction}.png';bpy.ops.render.render(write_still=True)
 print('FINISHED',kind,flush=True)
