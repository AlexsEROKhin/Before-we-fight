local source = "Assets/Art/Source/SuperPuperHero/super puper new sprite_polished.aseprite"
local output = "Assets/Art/Source/SuperPuperHero/UserHit/"
local sprite = assert(app.open(source), "Saved character source not found")
local hit
for _, tag in ipairs(sprite.tags) do
  if tag.name:lower() == "hit" then hit = tag; break end
end
assert(hit, "The user-drawn hit tag is missing")
assert(hit.toFrame.frameNumber - hit.fromFrame.frameNumber + 1 == 5, "Expected five user-drawn hit frames")
local preview = Image(160, 40, ColorMode.RGB)
preview:clear(app.pixelColor.rgba(39, 34, 38, 255))
for index = 0, 4 do
  local image = Image(sprite.width, sprite.height, ColorMode.RGB)
  image:drawSprite(sprite, hit.fromFrame.frameNumber + index)
  image:saveAs(output .. "hit_" .. index .. ".png")
  preview:drawImage(Image(image, Rectangle(50, 16, 32, 40)), Point(index * 32, 0))
end
preview:resize(1280, 320)
preview:saveAs("ArtReview/SuperPuperHero/user_hit_preview_8x.png")
sprite:close()
