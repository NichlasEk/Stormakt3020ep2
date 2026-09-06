"""Author a real, fixed-length 3D walk reference. Run with Blender, not game runtime.
blender -b -t 4 --python tools/build_walk_reference.py -- --render
This is a motion maquette, deliberately not a replacement for Karl's approved art.
"""
import bpy, math, json, sys
from pathlib import Path
from mathutils import Vector, Matrix, Quaternion
ROOT=Path(__file__).resolve().parents[1]
OUT=ROOT/'assets/source/animation'
PREVIEW=ROOT/'artifacts/walk-reference'
OUT.mkdir(parents=True,exist_ok=True);PREVIEW.mkdir(parents=True,exist_ok=True)
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
scene=bpy.context.scene
scene.render.engine='BLENDER_WORKBENCH'
scene.render.resolution_x=960;scene.render.resolution_y=720;scene.render.resolution_percentage=100
scene.render.image_settings.file_format='PNG';scene.render.fps=30
scene.display.shading.light='STUDIO';scene.display.shading.color_type='MATERIAL'
scene.display.shading.show_shadows=True;scene.display.shading.show_cavity=True
scene.display.shading.cavity_type='BOTH';scene.display.shading.show_specular_highlight=False
scene.display.shading.background_type='WORLD';scene.world.color=(.045,.05,.047)
scene.view_settings.view_transform='Standard'
FRAMES=30;STRIDE=1.0;STANCE=.62;THIGH=.41;SHIN=.41
scene.frame_start=1;scene.frame_end=FRAMES

def material(name,color):
 m=bpy.data.materials.new(name);m.diffuse_color=(*color,1);return m
cloth=material('Motion study · indigo',(.075,.105,.135))
leather=material('Motion study · worn leather',(.065,.055,.043))
clay=material('Motion study · neutral clay',(.30,.27,.22))
brass=material('Motion study · joint markers',(.40,.30,.13))
steel=material('Motion study · blade',(.22,.24,.25))
ground=material('Ground',(.12,.135,.125))
line_mat=material('Contact grid',(.24,.255,.23))

# The support trajectory has exactly -STRIDE derivative. Lift and return join smoothly.
def footpath(phase):
 p=phase%1;half=STRIDE*STANCE/2
 if p<STANCE:return half-STRIDE*p,0,0
 t=(p-STANCE)/(1-STANCE);t2=t*t;t3=t2*t
 m=-STRIDE*(1-STANCE)
 along=(2*t3-3*t2+1)*-half+(t3-2*t2+t)*m+(-2*t3+3*t2)*half+(t3-t2)*m
 return along,.085*math.sin(math.pi*t)**2,t

def solve_leg(hip,foot):
 d=foot-hip;length=d.length
 assert abs(THIGH-SHIN)+.00001<length<THIGH+SHIN-.00001,('unreachable foot',length)
 axis=d.normalized();forward=Vector((0,-1,0));bend=(forward-axis*forward.dot(axis)).normalized()
 x=(THIGH**2-SHIN**2+length**2)/(2*length)
 return hip+axis*x+bend*math.sqrt(max(0,THIGH**2-x*x))

def pose(phase):
 angle=phase*math.tau
 # Center of mass shifts over the supporting leg; vertical excursion is restrained.
 pelvis=Vector((.012*math.sin(angle),-.012,.812+.009*math.cos(angle*2)))
 lean=Matrix.Rotation(.035,3,'X')@Matrix.Rotation(.018*math.sin(angle),3,'Y')
 chest=pelvis+lean@Vector((0,0,.52));neck=chest+Vector((0,0,.07));head=neck+Vector((0,0,.17))
 joints={'pelvis':(pelvis,pelvis+Vector((0,0,.13))), 'spine':(pelvis,chest),'head':(neck,head)}
 for i,side in enumerate(('L','R')):
  sign=-1 if i==0 else 1;p=(phase+i*.5)%1
  along,lift,swing=footpath(p)
  hip=pelvis+Vector((sign*.092,0,0));ankle=Vector((sign*.095,-along,.083+lift))
  knee=solve_leg(hip,ankle)
  toe_angle=0 if p<STANCE else -.23*math.sin(math.pi*swing)
  toe=ankle+Matrix.Rotation(toe_angle,3,'X')@Vector((0,-.19,-.024))
  joints['thigh.'+side]=(hip,knee);joints['shin.'+side]=(knee,ankle);joints['foot.'+side]=(ankle,toe)
  shoulder=chest+Vector((sign*.205,0,-.022))
  swing_angle=math.sin(angle+i*math.pi)
  elbow=shoulder+Vector((sign*.026,.065*swing_angle,-.265)).normalized()*.28
  # Weapon arm stays controlled; free arm supplies counter-rotation.
  wrist=elbow+Vector((sign*.018,-.12+(.018 if side=='R' else .055)*swing_angle,-.225)).normalized()*.255
  joints['upperarm.'+side]=(shoulder,elbow);joints['forearm.'+side]=(elbow,wrist)
  joints['hand.'+side]=(wrist,wrist+Vector((0,-.015,-.085)))
 return joints

rest=pose(0)
arm_data=bpy.data.armatures.new('Karl motion skeleton')
rig=bpy.data.objects.new('Karl · motion reference',arm_data);scene.collection.objects.link(rig)
bpy.context.view_layer.objects.active=rig;rig.select_set(True)
bpy.ops.object.mode_set(mode='EDIT')
for name,(a,b) in rest.items():
 bone=arm_data.edit_bones.new(name);bone.head=a;bone.tail=b
 # Match the solver's roll convention. Otherwise a fitted coat rotates 180 degrees
 # around the spine even though the bone endpoints are numerically correct.
 bone.align_roll((b-a).to_track_quat('Y','Z').to_matrix().col[2])
# Pelvis-rooted hierarchy; the solver supplies coherent global joint transforms.
for name in rest:
 if name=='pelvis':continue
 parent='pelvis'
 if name=='head' or name.startswith('upperarm'):parent='spine'
 if name.startswith('shin.'):parent='thigh.'+name[-1]
 if name.startswith('foot.'):parent='shin.'+name[-1]
 if name.startswith('forearm.'):parent='upperarm.'+name[-1]
 if name.startswith('hand.'):parent='forearm.'+name[-1]
 arm_data.edit_bones[name].parent=arm_data.edit_bones[parent]
bpy.ops.object.mode_set(mode='OBJECT');rig.show_in_front=True

def bind(obj,name,mat):
 obj.data.materials.append(mat)
 group=obj.vertex_groups.new(name=name);group.add(list(range(len(obj.data.vertices))),1,'REPLACE')
 mod=obj.modifiers.new('Fixed-length skeletal deformation','ARMATURE');mod.object=rig
 obj.parent=rig
 return obj

def ellipsoid(name,location,scale,mat,bone):
 bpy.ops.mesh.primitive_uv_sphere_add(segments=12,ring_count=8,location=location)
 obj=bpy.context.object;obj.name=name;obj.scale=scale
 bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
 return bind(obj,bone,mat)

def segment(name,a,b,r1,r2,mat,bone):
 d=b-a
 bpy.ops.mesh.primitive_cone_add(vertices=12,radius1=r1,radius2=r2,depth=d.length,location=(a+b)/2)
 obj=bpy.context.object;obj.name=name;obj.rotation_euler=d.to_track_quat('Z','Y').to_euler()
 bpy.ops.object.transform_apply(location=False,rotation=True,scale=True)
 return bind(obj,bone,mat)

# Simple adult proportions, no painted-image warping and no production costume claims.
for side in ('L','R'):
 hip,knee=rest['thigh.'+side];_,ankle=rest['shin.'+side]
 segment('Breeches '+side,hip,knee,.075,.094,cloth,'thigh.'+side)
 segment('Boot shaft '+side,knee,ankle,.047,.062,leather,'shin.'+side)
 ellipsoid('Knee joint '+side,knee,(.062,.061,.06),brass,'shin.'+side)
 ellipsoid('Ankle joint '+side,ankle,(.046,.046,.044),leather,'foot.'+side)
 ellipsoid('Boot '+side,ankle+Vector((0,-.055,-.047)),(.052,.125,.038),leather,'foot.'+side)
 shoulder,elbow=rest['upperarm.'+side];_,wrist=rest['forearm.'+side]
 segment('Upper sleeve '+side,shoulder,elbow,.058,.073,cloth,'upperarm.'+side)
 ellipsoid('Elbow joint '+side,elbow,(.052,.052,.051),cloth,'forearm.'+side)
 segment('Lower sleeve '+side,elbow,wrist,.042,.054,cloth,'forearm.'+side)
 ellipsoid('Glove '+side,wrist+Vector((0,-.008,-.035)),(.038,.036,.055),leather,'hand.'+side)
 ellipsoid('Shoulder '+side,shoulder,(.075,.075,.077),cloth,'upperarm.'+side)

# Tailored torso built from oval cross-sections, not a stack of round body primitives.
pelvis=rest['pelvis'][0];rings=[(.02,.145,.105),(.16,.13,.095),(.40,.20,.115),(.51,.19,.105)]
verts=[];faces=[]
for z,rx,ry in rings:
 for j in range(12):
  a=j*math.tau/12;verts.append(tuple(pelvis+Vector((rx*math.cos(a),ry*math.sin(a),z))))
for k in range(3):
 for j in range(12):faces.append((k*12+j,k*12+(j+1)%12,(k+1)*12+(j+1)%12,(k+1)*12+j))
faces.extend([tuple(reversed(range(12))),tuple(range(36,48))])
mesh=bpy.data.meshes.new('Torso mesh');mesh.from_pydata(verts,[],faces);mesh.update()
obj=bpy.data.objects.new('Tailored motion torso',mesh);scene.collection.objects.link(obj);bind(obj,'spine',cloth)
neck,head=rest['head'];ellipsoid('Head · featureless motion study',neck+Vector((0,0,.095)),(.077,.084,.112),clay,'head')
ellipsoid('Forehead direction marker',neck+Vector((0,-.081,.12)),(.027,.014,.018),brass,'head')
# Saber is rigidly attached to the right hand, so the entire weapon survives every frame.
wrist=rest['hand.R'][0]
segment('Practice blade',wrist+Vector((0,-.02,-.06)),wrist+Vector((0,-.35,-.58)),.006,.011,steel,'hand.R')
segment('Saber guard',wrist+Vector((-.06,-.02,-.06)),wrist+Vector((.06,-.02,-.06)),.009,.009,brass,'hand.R')

max_error=0;support_error=0;max_pose_error=0
for frame in range(1,FRAMES+2):
 phase=(frame-1)/FRAMES;joints=pose(phase)
 scene.frame_set(frame)
 for name,(a,b) in joints.items():
  pb=rig.pose.bones[name];pb.rotation_mode='QUATERNION'
  rotation=(b-a).to_track_quat('Y','Z')
  # No scaling at any frame. IK supplies constant-length thigh and shin segments.
  pb.matrix=Matrix.Translation(a)@rotation.to_matrix().to_4x4()
  bpy.context.view_layer.update()
  max_pose_error=max(max_pose_error,(pb.head-a).length,(pb.tail-b).length)
  pb.keyframe_insert('location',frame=frame);pb.keyframe_insert('rotation_quaternion',frame=frame)
  if name.startswith(('thigh','shin')):max_error=max(max_error,abs((b-a).length-(THIGH if name.startswith('thigh') else SHIN)))
 rig.location=(0,-STRIDE*phase,0);rig.keyframe_insert('location',frame=frame)
# Dense keys make preview independent of animation curve interpolation defaults.
for i in range(500):
 p=i/1000
 x1=footpath(p)[0]+STRIDE*p;x2=footpath(p+.0001)[0]+STRIDE*(p+.0001)
 support_error=max(support_error,abs(x2-x1))
assert max_error<.00001 and support_error<.00001 and max_pose_error<.00001,(max_error,support_error,max_pose_error)

# Contact grid gives a visible world-space reference for sliding, unlike a floating turntable.
bpy.ops.mesh.primitive_plane_add(size=200,location=(0,0,-.002));bpy.context.object.data.materials.append(ground)
for axis in (0,1):
 for i in range(-16,17):
  bpy.ops.mesh.primitive_cube_add(size=1,location=((i*.25,0,0) if axis==0 else (0,i*.25,0)))
  obj=bpy.context.object;obj.name='25 cm contact grid';obj.scale=(.003,8,.002) if axis==0 else (8,.003,.002);obj.data.materials.append(line_mat)

bpy.ops.object.camera_add();camera=bpy.context.object;scene.camera=camera;camera.data.type='ORTHO';camera.data.ortho_scale=2.65
# Track the moving root; the world grid scrolls past visibly planted feet.
view=Vector((3,-5,3.7));target=Vector((0,-STRIDE*.5,.83))
camera.location=target+view;camera.rotation_euler=(target-camera.location).to_track_quat('-Z','Y').to_euler()
scene.frame_set(1)
bpy.ops.wm.save_as_mainfile(filepath=str(OUT/'karl-walk-reference.blend'))
manifest={'status':'motion reference only; not approved production Karl art','frames':FRAMES,'fps':30,'cycle_distance_m':STRIDE,'support_fraction':STANCE,'thigh_m':THIGH,'shin_m':SHIN,'max_segment_length_error_m':max_error,'max_support_slide_error_m':support_error,'max_blender_joint_error_m':max_pose_error,'source':'tools/build_walk_reference.py','next':'Review motion before building the full textured and skinned Karl model. Reject 2D deformation and cut-apart atlas trials.'}
(OUT/'walk-reference.json').write_text(json.dumps(manifest,indent=2)+'\n')
print('RIG VALIDATION',manifest,flush=True)
if '--render' in sys.argv:
 for direction,view in [('se',Vector((3,-5,3.7))),('ne',Vector((3,5,3.7)))]:
  camera.location=target+view;camera.rotation_euler=(target-camera.location).to_track_quat('-Z','Y').to_euler()
  folder=PREVIEW/direction;folder.mkdir(exist_ok=True)
  for frame in range(1,FRAMES+1):
   scene.frame_set(frame);camera.location=target+view+rig.location;scene.render.filepath=str(folder/f'{frame:03}.png');bpy.ops.render.render(write_still=True)
 print('REFERENCE CAPTURES COMPLETE',flush=True)
