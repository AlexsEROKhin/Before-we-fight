local sprite = assert(app.activeSprite)
local outline = app.pixelColor.rgba(14, 17, 24, 255)
local shadow = app.pixelColor.rgba(36, 41, 52, 255)
local iron = app.pixelColor.rgba(69, 76, 91, 255)
local light = app.pixelColor.rgba(126, 135, 149, 255)
local redShadow = app.pixelColor.rgba(122, 19, 26, 255)
local red = app.pixelColor.rgba(226, 39, 46, 255)
local redCore = app.pixelColor.rgba(255, 139, 120, 255)

app.transaction(function()
  for _, cel in ipairs(sprite.cels) do
    local image = cel.image
    for y = 0, image.height - 1 do
      for x = 0, image.width - 1 do
        local pixel = image:getPixel(x, y)
        local a = app.pixelColor.rgbaA(pixel)
        if a < 160 then
          image:drawPixel(x, y, 0)
        else
          local r = app.pixelColor.rgbaR(pixel)
          local g = app.pixelColor.rgbaG(pixel)
          local b = app.pixelColor.rgbaB(pixel)
          local brightness = (r + g + b) / 3
          if r > g * 1.4 and r > b * 1.3 and r > 65 then
            image:drawPixel(x, y, brightness > 135 and redCore or (r > 140 and red or redShadow))
          elseif brightness < 42 then
            image:drawPixel(x, y, outline)
          elseif brightness < 75 then
            image:drawPixel(x, y, shadow)
          elseif brightness < 112 then
            image:drawPixel(x, y, iron)
          else
            image:drawPixel(x, y, light)
          end
        end
      end
    end
  end
end)
