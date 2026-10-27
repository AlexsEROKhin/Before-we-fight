local input = app.params.input
local output = app.params.output
local canvasSize = tonumber(app.params.canvas) or 32
local requestedHeight = tonumber(app.params.height) or 24
if not input or not output then error("Pass input and output") end

local source = Image{ fromFile=input }
local pc = app.pixelColor
local minX, minY, maxX, maxY = source.width, source.height, -1, -1
for y = 0, source.height - 1 do
  for x = 0, source.width - 1 do
    if pc.rgbaA(source:getPixel(x, y)) > 8 then
      if x < minX then minX = x end
      if y < minY then minY = y end
      if x > maxX then maxX = x end
      if y > maxY then maxY = y end
    end
  end
end
if maxX < minX then error("Source has no visible pixels") end

local crop = Image(source, Rectangle(minX, minY, maxX - minX + 1, maxY - minY + 1))
local targetHeight = requestedHeight
local targetWidth = math.max(1, math.floor(crop.width * targetHeight / crop.height + 0.5))
if targetWidth > canvasSize - 4 then
  targetWidth = canvasSize - 4
  targetHeight = math.max(1, math.floor(crop.height * targetWidth / crop.width + 0.5))
end
crop:resize{ width=targetWidth, height=targetHeight, method="nearest-neighbor" }

local sprite = Sprite(canvasSize, canvasSize, ColorMode.RGB)
sprite.layers[1].name = "Creep"
local canvas = Image(sprite.spec)
canvas:clear(pc.rgba(0, 0, 0, 0))
canvas:drawImage(crop, Point(math.floor((canvasSize - targetWidth) / 2), canvasSize - targetHeight - 3))
sprite.cels[1].image = canvas
sprite:saveAs(output)
sprite:close()
