-- Small, hand-placed pixel icons for the arena minimap. Each frame is also
-- exported as a transparent PNG; the .aseprite keeps every icon editable.
local root = assert(app.params.project, "Missing --script-param project")
root = root:gsub("\\", "/")
local output = root .. "/Assets/Art/Minimap/"
local source = root .. "/Assets/Art/Source/Minimap/"
local pc = app.pixelColor
local colors = {
  ["."] = pc.rgba(0, 0, 0, 0),
  K = pc.rgba(17, 16, 23, 255), -- common hard outline
  B = pc.rgba(34, 75, 137, 255),
  L = pc.rgba(81, 139, 206, 255),
  T = pc.rgba(23, 30, 47, 255),
  R = pc.rgba(141, 43, 50, 255),
  r = pc.rgba(224, 98, 83, 255),
  G = pc.rgba(61, 155, 155, 255),
  g = pc.rgba(142, 230, 218, 255),
  P = pc.rgba(93, 70, 135, 255),
  p = pc.rgba(192, 151, 222, 255),
  O = pc.rgba(158, 98, 52, 255),
  o = pc.rgba(244, 179, 95, 255),
  W = pc.rgba(245, 224, 176, 255),
}

local icons = {
  { name = "hero_helmet", rows = {
    "...........", "...KKKKK...", "..KLLLLLK..", ".KLLBBBL LK.",
    ".KLBTTTBLK.", ".KBBBTBBBK.", ".KBBBTBBBK.", ".KBBB KBBBK.",
    "..KBBBBBK..", "...KKKKK...", "..........." } },
  { name = "enemy_mage", rows = {
    "...........", "....KKK....", "...Koo oK...", "..KOOOOOK..",
    ".KKKKKKKKK.", "...KRRRK...", "..KRRrRRK..", "..KRRRRRK..",
    "...KRRRK...", "....KKK....", "..........." } },
  { name = "enemy_ring", rows = {
    "...........", "...KKKKK...", "..KRrrrRK..", ".KRK...KRK.",
    ".KrK...KrK.", ".KrK...KrK.", ".KrK...KrK.", ".KRK...KRK.",
    "..KRrrrRK..", "...KKKKK...", "..........." } },
  { name = "enemy_io", rows = {
    ".....g.....", "....gGg....", "...gGKGg...", "..gGgGgGg..",
    ".gGgGGGgGg.", "gGgGgWgGgGg", ".gGgGGGgGg.", "..gGgGgGg..",
    "...gGKGg...", "....gGg....", ".....g....." } },
  { name = "enemy_trapper", rows = {
    "...........", "..KK...KK..", ".KPPK.KPPK.", ".KPpPKPpPK.",
    "..KPpPpPK..", "...KPPPPK..", "...KPpPK...", "..KPpPpPK..",
    ".KPPK.KPPK.", "..KK...KK..", "..........." } },
  { name = "enemy_other", rows = {
    "...........", "...KKKKK...", "..KrrrrrK..", ".KRRRRRRRK.",
    ".KRKRRRKRK.", ".KRKRRRKRK.", ".KRRRKRRRK.", "..KRRRRRK..",
    "...KRRRK...", "....KKK....", "..........." } },
}

local sprite = Sprite(11, 11, ColorMode.RGB)
sprite.layers[1].name = "Minimap icon pixels"
for index, icon in ipairs(icons) do
  local image = Image(11, 11, ColorMode.RGB)
  image:clear(colors["."])
  assert(#icon.rows == 11, icon.name)
  for y, row in ipairs(icon.rows) do
    row = row:gsub(" ", "")
    assert(#row == 11, icon.name .. " row " .. y .. " length " .. #row)
    for x = 1, 11 do
      local color = assert(colors[row:sub(x, x)], icon.name .. " color")
      image:drawPixel(x - 1, y - 1, color)
    end
  end
  local frame = index == 1 and sprite.frames[1] or sprite:newEmptyFrame(index)
  sprite:newCel(sprite.layers[1], frame, image, Point(0, 0))
  frame.duration = 0.1
  local tag = sprite:newTag(index, index)
  tag.name = icon.name
  image:saveAs(output .. icon.name .. ".png")
end
sprite:saveAs(source .. "Minimap_Icons_Editable.aseprite")
sprite:close()
print("Saved six minimap icons and editable Aseprite source")
