Hello, 
This is a Post Proccessing Effect Crepuscular Ray Script and shader, 
try the Example scene if you want to get an idea of the look.

Documentation: 
	Add Component Effects/CrepuscularRays.cs to scene Camera or click drag from the CrepuscularRays Folder.
	With the CrepuscularRays.cs Attached to the Camera Drag the Scene Directional Light into the "light" Slot of the script.

With that done you should be able to press play and see the rays effect.

I plan on improving this right now the rays are only in view when facing toward the sun. 
I had it working when facing away but there was a dpeth issue which I almost have fixed but no quite yet.
Also when facing Perpendicular to the sun the sunPosition ends up going to Infinity.