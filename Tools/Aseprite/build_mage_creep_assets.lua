local pc = app.pixelColor
local transparent = pc.rgba(0, 0, 0, 0)
local red = pc.rgba(220, 24, 46, 255)
local bright = pc.rgba(255, 92, 74, 255)
local pale = pc.rgba(255, 210, 145, 255)

local base = Image{ fromFile="Assets/Art/Imported/battle_mage_creep_32.png" }
local mage = Sprite(32, 32, ColorMode.RGB)
mage.layers[1].name = "Idle + Cast"
for frame = 1, 8 do
  local image = Image(mage.spec)
  image:clear(transparent)
  local bob = (frame == 2 or frame == 4) and -1 or 0
  image:drawImage(base, Point(0, bob))
  if frame >= 5 then
    local pulse = frame - 5
    image:drawPixel(25, 16 + bob, bright)
    image:drawPixel(26, 16 + bob, red)
    image:drawPixel(24, 15 + bob, pale)
    if pulse >= 1 then image:drawPixel(26, 15 + bob, bright) end
    if pulse >= 2 then image:drawPixel(25, 14 + bob, red) end
    if pulse >= 3 then image:drawPixel(27, 16 + bob, red) end
  end
  if frame == 1 then mage.cels[1].image = image else
    local f = mage:newEmptyFrame()
    mage:newCel(mage.layers[1], f, image, Point(0, 0))
  end
  mage.frames[frame].duration = 0.14
end
mage:saveAs("Assets/Art/Source/battle_mage_creep_anim.aseprite")
mage:close()

local marker = Sprite(32, 16, ColorMode.RGB)
marker.layers[1].name = "Warning Marker"
for frame = 1, 4 do
  local image = Image(marker.spec)
  image:clear(transparent)
  local c = frame % 2 == 0 and bright or red
  for x = 8, 23 do
    image:drawPixel(x, 12, c)
    if x >= 11 and x <= 20 then image:drawPixel(x, 13, c) end
  end
  image:drawPixel(6, 11, c); image:drawPixel(7, 10, c)
  image:drawPixel(24, 10, c); image:drawPixel(25, 11, c)
  image:drawPixel(12 + frame * 3, 7 - frame, bright)
  image:drawPixel(18 - frame * 2, 9 - frame, red)
  if frame == 1 then marker.cels[1].image = image else
    local f = marker:newEmptyFrame()
    marker:newCel(marker.layers[1], f, image, Point(0, 0))
  end
  marker.frames[frame].duration = 0.12
end
marker:saveAs("Assets/Art/Source/mage_target_marker.aseprite")
marker:close()

local beam = Sprite(16, 64, ColorMode.RGB)
beam.layers[1].name = "Sky Spell"
for frame = 1, 3 do
  local image = Image(beam.spec)
  image:clear(transparent)
  local half = frame == 1 and 1 or 2
  for y = 0, 53 do
    for x = 8 - half, 8 + half do image:drawPixel(x, y, frame == 3 and pale or bright) end
  end
  for x = 2, 14 do image:drawPixel(x, 55, red) end
  for x = 4, 12 do image:drawPixel(x, 56, bright) end
  image:drawPixel(1, 54, bright); image:drawPixel(15, 54, bright)
  image:drawPixel(4, 59, red); image:drawPixel(12, 59, red)
  if frame == 1 then beam.cels[1].image = image else
    local f = beam:newEmptyFrame()
    beam:newCel(beam.layers[1], f, image, Point(0, 0))
  end
  beam.frames[frame].duration = 0.06
end
beam:saveAs("Assets/Art/Source/mage_sky_spell.aseprite")
beam:close()
