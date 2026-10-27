local sprite = assert(app.activeSprite)
app.command.SpriteSize { width = 32, height = 32, method = "nearest" }
assert(sprite.width == 32 and sprite.height == 32, "SpriteSize did not make 32x32 art")
app.transaction(function()
  for _, cel in ipairs(sprite.cels) do
    local image = cel.image
    for y = 0, image.height - 1 do
      for x = 0, image.width - 1 do
        local pixel = image:getPixel(x, y)
        if app.pixelColor.rgbaA(pixel) < 128 then image:drawPixel(x, y, 0) end
      end
    end
  end
end)
