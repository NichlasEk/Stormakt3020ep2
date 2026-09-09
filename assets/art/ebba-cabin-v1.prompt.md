# Ebba ombord · generation 2026-09-09

Built-in imagegen with `ebba-radio-v3.png` as identity/costume reference. Final `ebba-cabin-v1.png` is a 1024×1536 chroma-green source. The first two transparency requests produced baked checkerboards, so the final tool edit replaced the backdrop with green. The existing game-side `SpriteCutout` now supports green as well as magenta, removes enclosed gaps and despills edges before texture filtering. No Blender/3D model; original portrait unchanged.

## Initial prompt

Use case identity-preserve. Reference image is approved adult Admiral Ebba Grip. Create ONE full body 2D isometric game sprite of this exact woman standing, facing lower left in three-quarter view, looking attentively toward a visitor. Genuine transparent background, no floor or shadow baked in. Preserve her mature face, blonde braided crown updo, confident expression, voluptuous adult physique and approved deep neckline, royal dark blue long officer coat and antique gold shoulder epaulettes and crowned gorget. Add practical dark officer trousers and knee high worn leather boots. Full body head to soles visible with padding. Natural human anatomy and proportions, dignified straight posture, one hand holding folded paper at waist, other relaxed. Fixed elevated orthographic isometric camera 30 degrees down. Serious dark matte historical oil painted 2D sprite with clear silhouette and readable shapes at 170px tall. Not 3D, not Blender, not plastic, not doll, no cartoon, no chibi, no exaggerated head, no white outline, no text. One figure only, centered, transparent PNG.

## Final tool edit

Change ONLY the background behind this exact full-body Ebba sprite to perfectly flat pure chroma green RGB(0,255,0). No checkerboard anywhere, no shadows or gradients on green. Preserve woman completely including boots and face and full body. This solid green background is required for the game's runtime chroma-key shader. No green spill or outlines on figure.

Implementation uses the existing CPU cutout loader before uploading the texture, rather than a separate runtime shader. The final source above is preserved unchanged.
