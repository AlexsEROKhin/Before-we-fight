local output = app.params.output
if output == nil or output == "" then
  error("Pass --script-param output=<path>")
end

local sprite = Sprite(128, 64, ColorMode.RGB)
sprite.filename = output
sprite:setPalette(Palette())
local layer = sprite.layers[1]
layer.name = "MENU Button"

local transparent = app.pixelColor.rgba(0, 0, 0, 0)
local outline = app.pixelColor.rgba(24, 16, 22, 255)
local shadow = app.pixelColor.rgba(42, 20, 28, 255)
local rimDark = app.pixelColor.rgba(91, 49, 56, 255)
local rim = app.pixelColor.rgba(178, 120, 102, 255)
local fill = app.pixelColor.rgba(103, 25, 39, 255)
local fillLight = app.pixelColor.rgba(137, 37, 49, 255)
local letterShadow = app.pixelColor.rgba(74, 18, 29, 255)
local letter = app.pixelColor.rgba(245, 192, 126, 255)
local shine = app.pixelColor.rgba(255, 226, 166, 255)

local glyphs = {
  M = {"10001","11011","10101","10101","10001","10001","10001"},
  E = {"11111","10000","10000","11110","10000","10000","11111"},
  N = {"10001","11001","11001","10101","10011","10011","10001"},
  U = {"10001","10001","10001","10001","10001","10001","01110"}
}

local function rect(image, x, y, w, h, color)
  for py = y, y + h - 1 do
    for px = x, x + w - 1 do image:drawPixel(px, py, color) end
  end
end

local function chamfered(image, x, y, w, h, color)
  rect(image, x + 2, y, w - 4, h, color)
  rect(image, x, y + 2, w, h - 4, color)
  image:drawPixel(x + 1, y + 1, color)
  image:drawPixel(x + w - 2, y + 1, color)
  image:drawPixel(x + 1, y + h - 2, color)
  image:drawPixel(x + w - 2, y + h - 2, color)
end

local function text(image, value, x, y, scale, color)
  local cursor = x
  for i = 1, #value do
    local glyph = glyphs[value:sub(i, i)]
    for row = 1, 7 do
      for col = 1, 5 do
        if glyph[row]:sub(col, col) == "1" then
          rect(image, cursor + (col - 1) * scale, y + (row - 1) * scale, scale, scale, color)
        end
      end
    end
    cursor = cursor + 6 * scale
  end
end

for frame = 1, 5 do
  local image = Image(sprite.spec)
  image:clear(transparent)
  local bob = (frame == 2 or frame == 4) and -1 or 0
  local x, y, w, h = 7, 11 + bob, 114, 40

  chamfered(image, x + 2, y + 4, w, h, shadow)
  chamfered(image, x, y, w, h, outline)
  chamfered(image, x + 2, y + 2, w - 4, h - 4, rimDark)
  chamfered(image, x + 3, y + 3, w - 6, h - 6, rim)
  chamfered(image, x + 5, y + 5, w - 10, h - 10, fill)
  rect(image, x + 7, y + 6, w - 14, 2, fillLight)

  -- Uneven little notches keep the hand-drawn, rough arena style.
  rect(image, x - 2, y + 9, 3, 5, outline)
  rect(image, x + w - 1, y + 23, 3, 5, outline)
  image:drawPixel(x + 8 + frame * 13, y + 3, shine)
  image:drawPixel(x + 80 - frame * 11, y + h - 4, rimDark)

  text(image, "MENU", 30, y + 14, 2, letterShadow)
  text(image, "MENU", 29, y + 13, 2, letter)
  if frame == 3 then
    image:drawPixel(49, y + 12, shine)
    image:drawPixel(73, y + 12, shine)
  end

  if frame == 1 then
    sprite.cels[1].image = image
  else
    local newFrame = sprite:newEmptyFrame()
    sprite:newCel(layer, newFrame, image, Point(0, 0))
  end
  sprite.frames[frame].duration = 0.12
end

sprite:saveAs(output)
sprite:close()
