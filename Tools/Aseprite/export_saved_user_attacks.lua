-- Export all three attack tags from the user's saved 57-frame Aseprite file.
-- The Unity clips reference these PNG paths, so their GUIDs remain stable.
local root = "Assets/Art/Source/SuperPuperHero/"
local sprite = assert(app.open(root .. "super puper new sprite_polished.aseprite"))
local destinations = {
  {tag="hit", prefix=root .. "UserHit/hit_"},
  {tag="hit up", prefix=root .. "DirectionalHit/hit_up_"},
  {tag="hit down", prefix=root .. "DirectionalHit/hit_down_"},
}
local preview = Image(160, 150, ColorMode.RGB)
preview:clear(app.pixelColor.rgba(39, 34, 38, 255))
for row, destination in ipairs(destinations) do
  local matched
  for _, tag in ipairs(sprite.tags) do
    if tag.name:lower() == destination.tag then matched = tag; break end
  end
  assert(matched, "Missing saved attack tag: " .. destination.tag)
  assert(matched.toFrame.frameNumber - matched.fromFrame.frameNumber + 1 == 5,
    "Expected five frames in " .. destination.tag)
  for index=0,4 do
    local image = Image(sprite.width, sprite.height, ColorMode.RGB)
    image:drawSprite(sprite, matched.fromFrame.frameNumber + index)
    image:saveAs(destination.prefix .. index .. ".png")
    preview:drawImage(Image(image, Rectangle(50, 10, 32, 50)),
      Point(index * 32, (row - 1) * 50))
  end
end
sprite:saveAs(root .. "DirectionalHit/super_puper_with_directional_hits.aseprite")
preview:resize(1280, 1200)
preview:saveAs("ArtReview/SuperPuperHero/user_all_attacks_preview_8x.png")
sprite:close()
