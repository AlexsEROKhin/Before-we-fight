-- Repair only the two opening frames of the user's chest. The closed frame and
-- original palette stay untouched. Run with Aseprite -b --script this-file.
local root = assert(app.params.project, "Missing --script-param project")
root = root:gsub("\\", "/")
local art = root .. "/Assets/Art/ChestLoot/"
local source = root .. "/Assets/Art/Source/Pickups/"

local closed = Image { fromFile = art .. "chest_closed.png" }
local oldOpen = Image { fromFile = art .. "chest_open.png" }
assert(closed.width == 64 and closed.height == 64)
assert(oldOpen.width == 64 and oldOpen.height == 64)

local pc = app.pixelColor
local clear = pc.rgba(0, 0, 0, 0)
local outline = pc.rgba(0, 0, 0, 255)
local darkWood = pc.rgba(73, 48, 44, 255)
local wood = pc.rgba(102, 57, 49, 255)
local brass = pc.rgba(183, 123, 62, 255)

local function blank()
  local image = Image(64, 64, ColorMode.RGB)
  image:clear(clear)
  return image
end

local function rect(image, left, top, right, bottom, color)
  for y = top, bottom do
    for x = left, right do image:drawPixel(x, y, color) end
  end
end

local function body()
  local image = blank()
  -- Keep the player's original front panel exactly as painted.
  for y = 28, 36 do
    for x = 24, 39 do image:drawPixel(x, y, oldOpen:getPixel(x, y)) end
  end
  return image
end

local function lid(openAmount)
  local image = blank()
  local lidY = openAmount == 1 and 21 or 19
  local lidBottom = lidY + 4

  -- Reuse the original lid pixels; move it as one rigid panel, not as a
  -- disconnected decoration above the chest.
  for oldY = 16, 20 do
    for x = 24, 39 do
      image:drawPixel(x, lidY + oldY - 16, oldOpen:getPixel(x, oldY))
    end
  end

  -- Visible inside wall and two continuous side hinges join the lid to the
  -- chest rim. The final frame exposes a deeper, dark opening.
  for y = lidBottom + 1, 28 do
    rect(image, 24, y, 39, y, outline)
    rect(image, 25, y, 38, y, darkWood)
    rect(image, 26, y, 37, y, (y >= 27) and outline or wood)
    image:drawPixel(25, y, brass)
    image:drawPixel(38, y, brass)
  end
  rect(image, 27, 27, 36, 28, outline)
  return image
end

local sprite = Sprite(64, 64, ColorMode.RGB)
local bodyLayer = sprite.layers[1]
bodyLayer.name = "Original chest body"
local lidLayer = sprite:newLayer()
lidLayer.name = "Attached lid and hinges"

local states = {
  { body = closed, lid = blank(), duration = 0.10 },
  { body = body(), lid = lid(1), duration = 0.10 },
  { body = body(), lid = lid(2), duration = 0.12 },
}

for index, state in ipairs(states) do
  local frame = (index == 1) and sprite.frames[1] or sprite:newEmptyFrame(index)
  sprite:newCel(bodyLayer, frame, state.body, Point(0, 0))
  sprite:newCel(lidLayer, frame, state.lid, Point(0, 0))
  frame.duration = state.duration
  if index > 1 then
    local flat = blank()
    flat:drawImage(state.body, Point(0, 0))
    flat:drawImage(state.lid, Point(0, 0))
    flat:saveAs(art .. (index == 2 and "chest_opening.png" or "chest_open.png"))
  end
end

local tag = sprite:newTag(1, 3)
tag.name = "Open"
sprite:saveAs(source .. "Chest_Opening_Fixed.aseprite")
sprite:close()
print("Saved connected chest lid and editable Aseprite source")
