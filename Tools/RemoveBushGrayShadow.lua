local sprite = app.activeSprite
assert(sprite, "Open a bush sprite before running this script")
local gray = app.pixelColor.rgba(99, 100, 97, 255)
local cleared = 0

app.transaction(function()
  for _, cel in ipairs(sprite.cels) do
    if cel.frame.frameNumber == 1 then
      local image = cel.image
      for y = 0, image.height - 1 do
        for x = 0, image.width - 1 do
          if image:getPixel(x, y) == gray then
            image:drawPixel(x, y, 0)
            cleared = cleared + 1
          end
        end
      end
    end
  end
end)

print("Removed gray bush-shadow pixels: " .. cleared)
