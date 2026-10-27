Arena map dressing

The user's original Aseprite files are user_bushes.aseprite (12 frames) and
user_stones.aseprite (5 frames). Each frame is also exported unchanged as an
individual PNG in this folder. Unity imports those PNGs as point-filtered
sprites without mipmaps or compression.

All placed instances are saved in Assets/Scenes/Arena.unity under
WORLD - edit positions here > User Bushes and Stones (editable).
Bushes and stones are separate children. Select any instance in the Unity
Hierarchy to move, resize, disable, duplicate, or delete it.

They are decorative and have no colliders. The initial placement leaves open
space around the player start, trees, camp centers, and reward objects.
The menu command Tools > Before We Fight > Place User Bushes and Stones is for
initial placement only; it will not overwrite the existing editable group.

The placed set uses two darker green bush variants and two neutral stone
variants. The brighter original frames remain in this folder for later use.
Tools > Before We Fight > Harmonize User Map Dressing reapplies the coherent
palette, sizes, and shared lighting direction while keeping instance positions.

Bush_01.png is exported from user_bushes_no_shadow.aseprite. Its gray ground
oval was removed pixel for pixel; the original multi-frame user_bushes.aseprite
is kept unchanged as a backup.

Every placed bush and stone in Arena has an individually editable Box Collider 2D.
Bush colliders use the small lower strip from Bush 156, which was set by hand;
stone colliders cover a wider area near the rock base. The menu command Tools >
Before We Fight > Add Bush and Stone Colliders fills in missing colliders and
leaves existing hand-adjusted ones untouched.
