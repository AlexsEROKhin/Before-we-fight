local path = "Assets/Art/Source/SuperPuperHero/super puper new sprite_polished.aseprite"
local sprite = assert(app.open(path), "Could not open saved user sprite")
local lines = {"file=" .. path, "size=" .. sprite.width .. "x" .. sprite.height, "frames=" .. #sprite.frames}
for _, tag in ipairs(sprite.tags) do
  lines[#lines + 1] = string.format("tag=%s|%d|%d", tag.name, tag.fromFrame.frameNumber, tag.toFrame.frameNumber)
end
for _, layer in ipairs(sprite.layers) do
  lines[#lines + 1] = "layer=" .. layer.name
end
for index, frame in ipairs(sprite.frames) do
  lines[#lines + 1] = string.format("duration=%d|%.3f", index, frame.duration)
end
local file = assert(io.open("ArtReview/SuperPuperHero/inspect_user_attack.txt", "w"))
file:write(table.concat(lines, "\n"))
file:close()
sprite:close()
