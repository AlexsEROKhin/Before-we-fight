-- Editable, limited-palette environment art for the large Arena.
local pc = app.pixelColor
local function rgba(hex)
  return pc.rgba(tonumber(hex:sub(1,2),16),tonumber(hex:sub(3,4),16),tonumber(hex:sub(5,6),16),255)
end
local clear = pc.rgba(0,0,0,0)
local soil = rgba("574B42")
local soilDark = rgba("4D443D")
local soilWarm = rgba("625045")
local moss = rgba("56604B")
local mossLight = rgba("68725A")
local stone = rgba("77746A")
local outline = rgba("292B29")
local leafDark = rgba("344936")
local leaf = rgba("4F6844")
local leafLight = rgba("78905C")
local bark = rgba("634A37")
local rockDark = rgba("414642")
local rockMid = rgba("626961")
local rockLight = rgba("85877A")

local function rect(im,x0,y0,x1,y1,color)
  for y=y0,y1 do for x=x0,x1 do im:drawPixel(x,y,color) end end
end
local function export(name,im)
  local sprite=Sprite(im.width,im.height,ColorMode.RGB)
  sprite.layers[1].name=name
  sprite.cels[1].image=im
  sprite:saveAs("Assets/Art/Environment/"..name..".aseprite")
  sprite:saveAs("Assets/Art/Environment/"..name..".png")
  sprite:close()
end

local floor=Image(512,512,ColorMode.RGB)
floor:clear(soil)
math.randomseed(71027)
local function wrappedPixel(x,y,color)
  floor:drawPixel((x+512)%512,(y+512)%512,color)
end
-- Match the user's quiet brown reference: only rare two-pixel-high soil marks.
-- The marks wrap at the edge so the existing tiled Ground prefab stays seamless.
for i=1,135 do
  local x,y=math.random(0,511),math.random(0,511)
  local width=math.random(4,10)
  for dx=0,width-1 do
    wrappedPixel(x+dx,y,soilWarm)
    wrappedPixel(x+dx,y+1,soilWarm)
  end
end
export("forest_floor",floor)
if app.params and app.params.onlyGround == "true" then return end

local bush=Image(32,24,ColorMode.RGB)
bush:clear(clear)
local bushRows={
  {11,20},{8,23},{6,25},{4,27},{3,28},{2,29},{2,29},
  {1,30},{1,30},{2,29},{3,28},{4,27},{5,26},{7,24},
  {9,22},{11,20}
}
for row,range in ipairs(bushRows) do
  local y=row+2
  rect(bush,range[1],y,range[2],y,outline)
  if range[2]-range[1]>2 then rect(bush,range[1]+1,y,range[2]-1,y,leafDark) end
end
rect(bush,7,8,14,10,leaf)
rect(bush,17,7,24,9,leaf)
rect(bush,5,12,11,14,leaf)
rect(bush,15,11,26,14,leaf)
rect(bush,10,16,21,18,leaf)
rect(bush,9,8,12,8,leafLight)
rect(bush,19,7,22,7,leafLight)
rect(bush,6,12,9,12,leafLight)
rect(bush,17,11,21,11,leafLight)
rect(bush,13,19,18,21,bark)
rect(bush,11,22,21,22,outline)
export("forest_bush",bush)

local rock=Image(28,18,ColorMode.RGB)
rock:clear(clear)
local rockRows={{8,17},{5,21},{3,23},{2,24},{1,25},{1,25},{1,25},{2,24},{3,23},{4,22},{6,20}}
for row,range in ipairs(rockRows) do
  local y=row+3
  rect(rock,range[1],y,range[2],y,outline)
  if range[2]-range[1]>2 then rect(rock,range[1]+1,y,range[2]-1,y,rockDark) end
end
rect(rock,6,6,18,8,rockMid)
rect(rock,4,9,21,11,rockMid)
rect(rock,9,5,17,6,rockLight)
rect(rock,6,8,12,8,rockLight)
rect(rock,17,10,23,11,rockMid)
rect(rock,7,14,20,14,soilDark)
export("forest_rock",rock)

local grass=Image(16,12,ColorMode.RGB)
grass:clear(clear)
rect(grass,2,9,13,9,leafDark)
rect(grass,4,7,5,8,leaf)
rect(grass,7,4,8,8,leaf)
rect(grass,10,6,11,8,leaf)
grass:drawPixel(3,6,leafLight)
grass:drawPixel(7,3,leafLight)
grass:drawPixel(12,5,leafLight)
rect(grass,5,10,10,10,moss)
export("forest_grass",grass)
