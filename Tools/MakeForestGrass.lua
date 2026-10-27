local sprite = assert(app.activeSprite, "Open forest_floor.aseprite first")
local oldSoil = app.pixelColor.rgba(87, 75, 66, 255)
local oldFleck = app.pixelColor.rgba(98, 80, 69, 255)
local grass = app.pixelColor.rgba(79, 96, 69, 255)
local grassFleck = app.pixelColor.rgba(96, 116, 77, 255)
local grassShadow = app.pixelColor.rgba(61, 79, 56, 255)
local grassTip = app.pixelColor.rgba(116, 133, 86, 255)

-- The existing floor is a single seamless 512-pixel tile. Keep its small marks,
-- shift the two original colors to grass, and add sparse hand-sized clusters.
app.transaction(function()
  for _, cel in ipairs(sprite.cels) do
    local image = cel.image
    for y = 0, image.height - 1 do
      for x = 0, image.width - 1 do
        local pixel = image:getPixel(x, y)
        if pixel == oldSoil then
          image:drawPixel(x, y, grass)
        elseif pixel == oldFleck then
          image:drawPixel(x, y, grassFleck)
        end
      end
    end

    local function dot(x, y, color)
      if x >= 2 and x < image.width - 2 and y >= 2 and y < image.height - 2 then
        image:drawPixel(x, y, color)
      end
    end

    math.randomseed(22616)
    for i = 1, 170 do
      local x = math.random(5, image.width - 6)
      local y = math.random(5, image.height - 6)
      local form = i % 4
      dot(x, y, grassShadow)
      dot(x, y - 1, grassShadow)
      if form == 0 then
        dot(x - 1, y - 2, grassShadow)
        dot(x + 2, y - 1, grassFleck)
      elseif form == 1 then
        dot(x + 1, y - 2, grassShadow)
        dot(x - 2, y - 1, grassFleck)
      elseif form == 2 then
        dot(x - 1, y - 1, grassFleck)
        dot(x + 1, y - 1, grassTip)
      else
        dot(x + 1, y, grassFleck)
        dot(x - 1, y - 1, grassTip)
      end
    end
  end
end)
