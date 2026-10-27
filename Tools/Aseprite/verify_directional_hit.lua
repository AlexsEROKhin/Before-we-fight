local root = "Assets/Art/Source/SuperPuperHero/"
local sprite = assert(app.open(root .. "DirectionalHit/super_puper_with_directional_hits.aseprite"))
assert(#sprite.frames == 57, "Expected 47 original frames plus ten directional hit frames")
local expected = {hit=5, ["hit down"]=5, ["hit up"]=5}
for _, tag in ipairs(sprite.tags) do
  if expected[tag.name] then
    assert(tag.toFrame.frameNumber - tag.fromFrame.frameNumber + 1 == expected[tag.name],
      "Incorrect tag length: " .. tag.name .. " " .. tag.fromFrame.frameNumber .. "-" .. tag.toFrame.frameNumber)
    expected[tag.name] = nil
  end
end
assert(next(expected) == nil, "Missing hit tag in saved document")
local checked = 0
local function compare(frameNumber, path)
  local rendered = Image(sprite.width, sprite.height, ColorMode.RGB)
  rendered:drawSprite(sprite, frameNumber)
  local exported = Image{fromFile=path}
  for y=0,127 do
    for x=0,127 do
      assert(rendered:getPixel(x,y) == exported:getPixel(x,y),
        string.format("Saved frame mismatch at %d, %d, %d",frameNumber,x,y))
    end
  end
  checked = checked + 1
end
for i=0,4 do
  compare(43+i,root .. "UserHit/hit_" .. i .. ".png")
  compare(48+i,root .. "DirectionalHit/hit_up_" .. i .. ".png")
  compare(53+i,root .. "DirectionalHit/hit_down_" .. i .. ".png")
end
local file = assert(io.open("ArtReview/SuperPuperHero/directional_hit_verification.txt","w"))
file:write("verified 57-frame Aseprite document, all hit tags and " .. checked .. " exported frames")
file:close()
sprite:close()
