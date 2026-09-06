"""Render four registered paint projections and assemble a lossless template."""
import bpy,json
from pathlib import Path
from mathutils import Vector
ROOT=Path(__file__).resolve().parents[1];OUT=ROOT/'artifacts/karl-paint';OUT.mkdir(exist_ok=True)
bpy.ops.wm.open_mainfile(filepath=str(ROOT/'assets/source/animation/karl-costume-v1.blend'))
scene=bpy.context.scene;scene.frame_set(1);rig=bpy.data.objects['Karl · motion reference'];rig.location=(0,0,0)
scene.render.resolution_x=512;scene.render.resolution_y=512;scene.render.resolution_percentage=100
scene.cycles.samples=16
views=[(3,-5,4.5),(3,5,4.5),(-3,5,4.5),(-3,-5,4.5)];target=Vector((0,0,.84));camera=scene.camera
for i,xyz in enumerate(views):
 view=Vector(xyz);camera.location=target+view;camera.rotation_euler=(-view).to_track_quat('-Z','Y').to_euler()
 scene.render.filepath=str(OUT/f'{i}.png');bpy.ops.render.render(write_still=True)
# Packing only: the actual painting is done by the image tool, not this script.
from PIL import Image
atlas=Image.new('RGB',(1024,1024),(65,64,61))
for i in range(4):
 im=Image.open(OUT/f'{i}.png');atlas.paste(im,(i%2*512,i//2*512),im.getchannel('A'))
atlas.save(OUT/'paint-template.png')
