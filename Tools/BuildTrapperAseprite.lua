-- Run with Aseprite -b --script Tools/BuildTrapperAseprite.lua
-- Supply --script-param project=<absolute project directory>.
local root = app.params.project
assert(root and root ~= "", "Missing --script-param project")
root = root:gsub("\\", "/")
local framesRoot = root .. "/Assets/Art/Imported/Enemies/Trapper/"
local sourceRoot = root .. "/Assets/Art/Source/Trapper/"

local groups = {
  { name="Idle", stem="idle", count=4, duration=0.125 },
  { name="Walk", stem="walk", count=6, duration=0.125 },
  { name="Cast", stem="cast", count=5, duration=0.140 },
  { name="Hit", stem="hit", count=2, duration=0.120 },
  { name="Death", stem="death", count=4, duration=0.100 }
}

local sprite = Sprite(32, 32, ColorMode.RGB)
local layer = sprite.layers[1]
layer.name = "Trapper pixels"
local index = 1
for _, group in ipairs(groups) do
  local first = index
  for number = 0, group.count - 1 do
    local frame = (index == 1) and sprite.frames[1] or sprite:newEmptyFrame(index)
    local path = framesRoot .. "trapper_" .. group.stem .. "_" .. number .. ".png"
    local image = Image { fromFile = path }
    assert(image and image.width == 32 and image.height == 32, "Invalid sprite " .. path)
    sprite:newCel(layer, frame, image, Point(0, 0))
    frame.duration = group.duration
    index = index + 1
  end
  local tag = sprite:newTag(first, index - 1)
  tag.name = group.name
end

sprite:saveAs(sourceRoot .. "Trapper.aseprite")
print("Saved " .. #sprite.frames .. " frames to Trapper.aseprite")
