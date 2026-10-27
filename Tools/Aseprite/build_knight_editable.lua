local root = "C:/Users/okyer/OneDrive/Документы/ChatGPT/создание игры"
local framesRoot = root .. "/Assets/Art/Source/Knight/frames"
local directionalRoot = root .. "/Assets/Art/Source/Knight/Directional"
local outputPath = root .. "/Assets/Art/Source/Knight/Knight_All_Animations_Directional.aseprite"

local animations = {
  { tag="Idle", folder="idle", prefix="idle", count=9, duration=0.111 },
  { tag="Run", folder="run", prefix="run", count=8, duration=0.125 },
  { tag="Stop", folder="stop", prefix="stop", count=3, duration=0.100 },
  { tag="Jump", folder="jump", prefix="jump", count=8, duration=0.091 },
  { tag="Land", folder="land", prefix="land", count=2, duration=0.100 },
  { tag="Attack", folder="attack1", prefix="attack1", count=6, duration=0.067 },
  { tag="Attack VFX", folder="attack1_VFX", prefix="attack1_VFX", count=6, duration=0.067 },
  { tag="Air Attack", folder="attack1_jump", prefix="attack1_jump", count=6, duration=0.067 },
  { tag="Air Attack VFX", folder="attack1_jump_VFX", prefix="attack1_jump_VFX", count=6, duration=0.067 },
  { tag="Dash", folder="dash", prefix="dash", count=3, duration=0.067 },
  { tag="Hurt 1", folder="hurt1", prefix="hurt1", count=4, duration=0.083 },
  { tag="Hurt 2", folder="hurt2", prefix="hurt2", count=6, duration=0.083 },
  { tag="Death", folder="death", prefix="death", count=12, duration=0.100 }
}

local sprite = Sprite(80, 80, ColorMode.RGB)
sprite.filename = outputPath
sprite.layers[1].name = "Knight"
local frameNumber = 0

local function appendFrame(filename, duration)
  local image = Image{ fromFile=filename }
  frameNumber = frameNumber + 1
  if frameNumber == 1 then
    sprite.cels[1].image = image
  else
    local frame = sprite:newEmptyFrame()
    sprite:newCel(sprite.layers[1], frame, image, Point(0, 0))
  end
  sprite.frames[frameNumber].duration = duration
end

for _, animation in ipairs(animations) do
  local firstFrame = frameNumber + 1
  for index = 0, animation.count - 1 do
    local filename = string.format("%s/%s/%s_%02d.png", framesRoot, animation.folder, animation.prefix, index)
    appendFrame(filename, animation.duration)
  end
  local tag = sprite:newTag(firstFrame, frameNumber)
  tag.name = animation.tag
end

local directions = {
  { name="Down", key="down" },
  { name="Up", key="up" }
}
for _, direction in ipairs(directions) do
  local idleFirst = frameNumber + 1
  appendFrame(string.format("%s/idle_%s.png", directionalRoot, direction.key), 0.1)
  local idleTag = sprite:newTag(idleFirst, frameNumber)
  idleTag.name = "Facing " .. direction.name

  local walkFirst = frameNumber + 1
  appendFrame(string.format("%s/idle_%s.png", directionalRoot, direction.key), 0.11)
  for index = 2, 8 do
    appendFrame(string.format("%s/walk_%s_%02d.png", directionalRoot, direction.key, index), 0.11)
  end
  local walkTag = sprite:newTag(walkFirst, frameNumber)
  walkTag.name = "Walk " .. direction.name
end

sprite:saveAs(outputPath)
sprite:close()
