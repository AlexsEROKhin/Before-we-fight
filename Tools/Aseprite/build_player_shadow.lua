local pc = app.pixelColor
local transparent = pc.rgba(0, 0, 0, 0)
local outer = pc.rgba(32, 23, 28, 100)
local inner = pc.rgba(25, 18, 24, 165)

local sprite = Sprite(24, 9, ColorMode.RGB)
sprite.layers[1].name = "Contact shadow"
local image = Image(sprite.spec)
image:clear(transparent)

local outerRows = {
  {9, 14}, {5, 18}, {3, 20}, {1, 22}, {0, 23},
  {1, 22}, {3, 20}, {5, 18}, {9, 14},
}
for y = 0, 8 do
  for x = outerRows[y + 1][1], outerRows[y + 1][2] do
    image:drawPixel(x, y, outer)
  end
end

local innerRows = {
  {7, 16}, {5, 18}, {4, 19}, {5, 18}, {7, 16},
}
for y = 2, 6 do
  for x = innerRows[y - 1][1], innerRows[y - 1][2] do
    image:drawPixel(x, y, inner)
  end
end

sprite.cels[1].image = image
sprite:saveAs("Assets/Art/Source/player_contact_shadow.aseprite")
sprite:saveAs("Assets/Art/Imported/player_contact_shadow.png")
