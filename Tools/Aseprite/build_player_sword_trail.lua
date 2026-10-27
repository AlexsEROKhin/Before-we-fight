-- Editable, hard-edged 32 px sword arc. The hilt pivot is at (16, 30).
local output = "Assets/Art/Source/PlayerSword/"
local pc = app.pixelColor
local clear = pc.rgba(0, 0, 0, 0)
local dark = pc.rgba(59, 79, 101, 255)
local mid = pc.rgba(121, 149, 165, 255)
local light = pc.rgba(195, 210, 208, 255)
local white = pc.rgba(234, 235, 217, 255)

local function blank()
  local image = Image(32, 32, ColorMode.RGB)
  image:clear(clear)
  return image
end

local function point(image, x, y, color)
  if x >= 0 and x < 32 and y >= 0 and y < 32 then
    image:drawPixel(x, y, color)
  end
end

local sprite = Sprite(32, 32, ColorMode.RGB)
sprite.layers[1].name = "Outer sweep"
local innerLayer = sprite:newLayer()
innerLayer.name = "Inner glint"
local outer = blank()
local inner = blank()

-- An arc swept by the sword's tip from -57 to +57 degrees.
-- Separate small clusters keep it readable at the native resolution.
for angle = -56, 56, 5 do
  local radians = math.rad(angle)
  local x = 16 + math.floor(math.sin(radians) * 13 + 0.5)
  local y = 30 - math.floor(math.cos(radians) * 13 + 0.5)
  point(outer, x, y, angle < -10 and dark or mid)
  if angle >= -12 and angle <= 42 then
    point(inner, x, y - 1, light)
  end
end

-- Three small broken highlights behind the leading edge, no blur or gradient.
for angle = 20, 50, 10 do
  local radians = math.rad(angle)
  local x = 16 + math.floor(math.sin(radians) * 11 + 0.5)
  local y = 30 - math.floor(math.cos(radians) * 11 + 0.5)
  point(inner, x, y, white)
end

sprite.cels[1].image = outer
sprite:newCel(innerLayer, sprite.frames[1], inner, Point(0, 0))
sprite:saveAs(output .. "PlayerSwordTrail32px.aseprite")

local flat = blank()
flat:drawImage(outer, Point(0, 0))
flat:drawImage(inner, Point(0, 0))
flat:saveAs(output .. "PlayerSwordTrail32px.png")
sprite:close()
