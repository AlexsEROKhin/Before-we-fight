-- A compact, editable sword for the player's 32px palette.
-- Drawn pixel by pixel; no resampling, blur, or generated artwork.
local output = "Assets/Art/Source/PlayerSword/"
local pc = app.pixelColor
local clear = pc.rgba(0, 0, 0, 0)
local outline = pc.rgba(16, 20, 31, 255)
local steelDark = pc.rgba(59, 79, 101, 255)
local steel = pc.rgba(121, 149, 165, 255)
local steelLight = pc.rgba(195, 210, 208, 255)
local goldDark = pc.rgba(112, 68, 39, 255)
local gold = pc.rgba(186, 119, 51, 255)
local grip = pc.rgba(74, 40, 40, 255)

local function blank()
  local image = Image(16, 16, ColorMode.RGB)
  image:clear(clear)
  return image
end

local function rect(image, x0, y0, x1, y1, color)
  for y = y0, y1 do
    for x = x0, x1 do image:drawPixel(x, y, color) end
  end
end

local sprite = Sprite(16, 16, ColorMode.RGB)
sprite.layers[1].name = "Blade"
local hiltLayer = sprite:newLayer()
hiltLayer.name = "Hilt"
local blade = blank()
local hilt = blank()

-- A short straight blade, narrow enough to remain readable beside the knight.
rect(blade, 7, 1, 8, 1, outline)
rect(blade, 6, 2, 9, 9, outline)
rect(blade, 7, 2, 8, 9, steel)
rect(blade, 7, 2, 7, 8, steelLight)
rect(blade, 8, 3, 8, 9, steelDark)
blade:drawPixel(7, 1, steelLight)

-- Separate guard and grip layers make small palette/shape edits easy in Aseprite.
rect(hilt, 4, 10, 11, 11, outline)
rect(hilt, 5, 10, 10, 10, gold)
rect(hilt, 6, 11, 9, 11, goldDark)
rect(hilt, 7, 12, 8, 14, outline)
rect(hilt, 7, 12, 8, 13, grip)
rect(hilt, 6, 15, 9, 15, outline)
rect(hilt, 7, 15, 8, 15, gold)

sprite.cels[1].image = blade
sprite:newCel(hiltLayer, sprite.frames[1], hilt, Point(0, 0))
sprite:saveAs(output .. "PlayerSword16px.aseprite")

local flat = blank()
flat:drawImage(blade, Point(0, 0))
flat:drawImage(hilt, Point(0, 0))
flat:saveAs(output .. "PlayerSword16px.png")
sprite:close()
