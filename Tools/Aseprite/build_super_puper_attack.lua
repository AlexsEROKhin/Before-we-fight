-- A small hand-drawn punch based on the user's existing side-walk sprites.
-- Keeps the original character and all 42 source frames untouched.
local pc = app.pixelColor
local clear = pc.rgba(0, 0, 0, 0)
local outline = pc.rgba(0, 0, 0, 255)
local armor = pc.rgba(91, 110, 225, 255)
local armorLight = pc.rgba(130, 143, 222, 255)
local armorShade = pc.rgba(61, 84, 228, 255)
local glove = pc.rgba(217, 160, 102, 255)
local gloveLight = pc.rgba(238, 195, 154, 255)
local root = "Assets/Art/Source/SuperPuperHero/"
local output = root .. "Attack/"
local source = root .. "Frames/"

local function loadFrame(number)
  local image = Image{fromFile = source .. "frame_" .. number .. ".png"}
  assert(image.width == 128 and image.height == 128, "Unexpected character frame size")
  return image
end

local function rect(image, x0, y0, x1, y1, color)
  for y = y0, y1 do
    for x = x0, x1 do image:drawPixel(x, y, color) end
  end
end

local function punch(base, reach)
  local image = Image(128, 128, ColorMode.RGB)
  image:clear(clear)
  image:drawImage(base, Point(0, 0))
  -- Replace only the near arm. The head, torso, legs and palette remain original.
  rect(image, 71, 35, 77, 42, clear)
  local fistStart = 77 + reach
  rect(image, 70, 36, fistStart - 1, 41, outline)
  rect(image, 71, 37, fistStart - 1, 39, armor)
  rect(image, 71, 37, fistStart - 2, 37, armorLight)
  rect(image, 72, 39, fistStart - 1, 39, armorShade)
  rect(image, fistStart, 36, fistStart + 4, 40, outline)
  rect(image, fistStart + 1, 37, fistStart + 3, 39, glove)
  rect(image, fistStart + 1, 37, fistStart + 2, 37, gloveLight)
  return image
end

local frames = {
  loadFrame(10),              -- raised fist / windup
  punch(loadFrame(12), 2),   -- forward punch with a stepping leg
  punch(loadFrame(13), 3),   -- one-pixel follow-through
  loadFrame(9),               -- recovery to the normal side pose
}
local durations = {0.07, 0.08, 0.07, 0.08}
local sprite = Sprite(128, 128, ColorMode.RGB)
sprite.layers[1].name = "Knight punch"
for index, image in ipairs(frames) do
  if index == 1 then sprite.cels[1].image = image
  else
    local frame = sprite:newEmptyFrame()
    sprite:newCel(sprite.layers[1], frame, image, Point(0, 0))
  end
  sprite.frames[index].duration = durations[index]
  local export = Sprite(128, 128, ColorMode.RGB)
  export.cels[1].image = image
  export:saveAs(output .. "attack_" .. (index - 1) .. ".png")
  export:close()
end
local tag = sprite:newTag(1, #frames)
tag.name = "Punch right"
sprite:saveAs(output .. "knight_punch_right.aseprite")
local preview = Image(160, 40, ColorMode.RGB)
preview:clear(pc.rgba(39, 34, 38, 255))
for index, image in ipairs(frames) do
  preview:drawImage(Image(image, Rectangle(50, 16, 40, 40)), Point((index - 1) * 40, 0))
end
preview:resize(1280, 320)
preview:saveAs("ArtReview/SuperPuperHero/knight_punch_preview_8x.png")
sprite:close()
