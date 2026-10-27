local pc = app.pixelColor
local clear = pc.rgba(0, 0, 0, 0)
local red = pc.rgba(214, 32, 52, 255)
local hot = pc.rgba(255, 105, 82, 255)
local white = pc.rgba(255, 220, 170, 255)
local source = Image{ fromFile="Assets/Art/Imported/Enemies/skycaller.png" }
local sprite = Sprite(32, 32, ColorMode.RGB)
sprite.layers[1].name = "Skycaller"

local definitions = {
  {"Idle",4}, {"Move",4}, {"Cast Start",3}, {"Cast Hold",4},
  {"Cast Release",3}, {"Recovery",3}, {"Hurt",2}, {"Stagger",2}, {"Death",5}
}
local frameIndex = 0
local tagRanges = {}

local function addFrame(state, localFrame)
  frameIndex = frameIndex + 1
  local image = Image(sprite.spec)
  image:clear(clear)
  local x, y = 0, 0
  if state == "Idle" then y = (localFrame == 2 or localFrame == 4) and -1 or 0 end
  if state == "Move" then x = (localFrame == 2 and 1) or (localFrame == 4 and -1) or 0; y = localFrame % 2 end
  if state == "Cast Start" then y = -math.min(1, localFrame - 1) end
  if state == "Cast Hold" or state == "Cast Release" then y = -1 end
  if state == "Hurt" then x = -localFrame end
  if state == "Stagger" then x = -2 + localFrame end
  if state == "Death" then y = localFrame - 1; x = -math.floor(localFrame / 2) end
  image:drawImage(source, Point(x, y))

  if state == "Cast Start" or state == "Cast Hold" or state == "Cast Release" then
    local grow = state == "Cast Start" and localFrame or (state == "Cast Hold" and 3 or 4 - localFrame)
    image:drawPixel(23 + x, 15 + y, hot)
    image:drawPixel(24 + x, 15 + y, red)
    if grow >= 2 then image:drawPixel(23 + x, 14 + y, red) end
    if grow >= 3 then image:drawPixel(25 + x, 15 + y, hot); image:drawPixel(22 + x, 16 + y, white) end
  end
  if state == "Hurt" then
    image:drawPixel(20, 11, white); image:drawPixel(21, 10, red)
  end
  if state == "Death" and localFrame >= 3 then
    for py=0,31 do for px=0,31 do
      if (px + py + localFrame) % 4 == 0 then image:drawPixel(px, py, clear) end
    end end
  end

  if frameIndex == 1 then sprite.cels[1].image = image else
    local f = sprite:newEmptyFrame()
    sprite:newCel(sprite.layers[1], f, image, Point(0,0))
  end
  sprite.frames[frameIndex].duration = state == "Cast Hold" and 0.12 or 0.1
end

for _, definition in ipairs(definitions) do
  local name, count = definition[1], definition[2]
  local first = frameIndex + 1
  for i=1,count do addFrame(name, i) end
  table.insert(tagRanges, {name, first, frameIndex})
end
for _, range in ipairs(tagRanges) do
  local tag = sprite:newTag(range[2], range[3])
  tag.name = range[1]
end
sprite:saveAs("Assets/Art/Source/Enemies/skycaller_animated.aseprite")
sprite:close()
