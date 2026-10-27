-- Editable, native-resolution replacement for the InvokerMage sky spell.
-- Run from the Unity project root: Aseprite.exe -b --script Tools/Aseprite/build_invoker_spell_polished.lua
local root = "Assets/Art/Source/Enemies/InvokerMage/PolishedSpell_2026-10-13/"
local pc = app.pixelColor
local C = {
  clear = pc.rgba(0, 0, 0, 0),
  shadow = pc.rgba(47, 9, 23, 255),
  ground = pc.rgba(115, 13, 35, 130),
  dark = pc.rgba(99, 14, 34, 255),
  red = pc.rgba(195, 30, 57, 255),
  hot = pc.rgba(248, 69, 81, 255),
  rose = pc.rgba(255, 139, 131, 255),
  cyan = pc.rgba(107, 224, 232, 255),
  ice = pc.rgba(183, 250, 241, 255),
  white = pc.rgba(255, 247, 211, 255),
}

local function px(im, x, y, color)
  if x >= 0 and x < 64 and y >= 0 and y < 64 then
    im:drawPixel(math.floor(x), math.floor(y), color)
  end
end
local function rect(im, x1, y1, x2, y2, color)
  for y = y1, y2 do for x = x1, x2 do px(im, x, y, color) end end
end
local function line(im, x1, y1, x2, y2, color)
  local dx, dy = math.abs(x2-x1), math.abs(y2-y1)
  local sx, sy = x1 < x2 and 1 or -1, y1 < y2 and 1 or -1
  local error = dx-dy
  while true do
    px(im, x1, y1, color)
    if x1 == x2 and y1 == y2 then break end
    local e2 = 2*error
    if e2 > -dy then error=error-dy; x1=x1+sx end
    if e2 < dx then error=error+dx; y1=y1+sy end
  end
end
local function ring(im, rx, ry, color, thick)
  -- Continuous flattened ground circle; no antialiased/subpixel strokes.
  for y = -ry, ry do
    for x = -rx, rx do
      local outer = (x*x)/(rx*rx) + (y*y)/(ry*ry)
      local innerX, innerY = math.max(1, rx-thick), math.max(1, ry-thick)
      local inner = (x*x)/(innerX*innerX) + (y*y)/(innerY*innerY)
      if outer <= 1 and inner >= 1 then px(im, 32+x, 47+y, color) end
    end
  end
end
local function diamond(im, cx, cy, radius, color)
  for y=-radius,radius do
    for x=-radius,radius do
      if math.abs(x)+math.abs(y) <= radius then px(im,cx+x,cy+y,color) end
    end
  end
end
local function marker(im, phase)
  local rim = phase < 2 and C.red or (phase < 5 and C.hot or C.rose)
  for y=-4,4 do for x=-16,16 do
    if (x*x)/(16*16)+(y*y)/(4*4) <= 1 then px(im,32+x,47+y,C.ground) end
  end end
  ring(im, 19, 6, C.shadow, 2)
  ring(im, 17, 5, rim, 1)
  -- Broken inner sigil and four pointed bearings, all attached to the ground circle.
  local detail = phase < 3 and C.dark or (phase < 6 and C.red or C.rose)
  line(im, 27,47,30,45,detail); line(im,34,45,37,47,detail)
  line(im,27,48,30,50,detail); line(im,34,50,37,48,detail)
  if phase >= 1 then
    diamond(im,12,47,2,detail); diamond(im,52,47,2,detail)
  end
  if phase >= 2 then
    diamond(im,32,39,2,detail); diamond(im,32,55,2,detail)
  end
  if phase >= 4 then
    px(im,23,44,C.rose); px(im,41,50,C.rose)
    rect(im,30,47,33,47,C.hot)
  end
  if phase >= 5 then
    px(im,18,38,C.hot); px(im,42,35,C.red)
    px(im,25,33,C.rose); px(im,48,41,C.hot)
  end
  if phase >= 6 then
    ring(im,14,4,C.hot,1)
    rect(im,31,45,32,49,C.rose)
  end
end
local function bolt(im, bottom, width)
  -- Offset dark/red casing, cyan body, and a narrow ivory core.
  for y=0,bottom do
    local bend = y < 13 and -1 or (y < 27 and 0 or (y < 39 and 1 or 0))
    local cx=32+bend
    rect(im,cx-width-2,y,cx+width+1,y,C.shadow)
    rect(im,cx-width-1,y,cx+width,y,C.hot)
    rect(im,cx-width+1,y,cx+width-2,y,C.cyan)
    if width >= 3 then rect(im,cx-1,y,cx,y,C.white) end
  end
  if bottom >= 40 then
    px(im,24,28,C.hot); px(im,40,34,C.hot)
    rect(im,26,17,27,18,C.rose); px(im,39,12,C.cyan)
  end
end
local function burst(im, large)
  local extent = large and 15 or 10
  diamond(im,32,47,large and 7 or 5,C.shadow)
  diamond(im,32,47,large and 5 or 4,C.hot)
  diamond(im,32,47,large and 3 or 2,C.white)
  line(im,32-extent,47,26,47,C.hot)
  line(im,38,47,32+extent,47,C.hot)
  line(im,32,47-extent,32,42,C.cyan)
  line(im,32,52,32,47+(large and 9 or 6),C.hot)
  line(im,25,40,21,36,C.rose); line(im,39,40,43,36,C.cyan)
  if large then
    line(im,24,52,20,55,C.hot); line(im,40,52,44,55,C.hot)
    rect(im,15,39,16,40,C.red); rect(im,47,38,48,39,C.cyan)
    px(im,21,30,C.rose); px(im,43,29,C.ice)
  end
end

local sprite=Sprite(64,64,ColorMode.RGB)
sprite.layers[1].name="Sky spell - warning, strike, impact"
for frame=1,12 do
  local im=Image(sprite.spec)
  im:clear(C.clear)
  if frame <= 7 then
    marker(im,frame-1)
  elseif frame == 8 then
    marker(im,6); bolt(im,19,2)
  elseif frame == 9 then
    marker(im,6); bolt(im,38,3)
  elseif frame == 10 then
    marker(im,6); bolt(im,47,4); burst(im,false)
  elseif frame == 11 then
    ring(im,19,6,C.red,1); burst(im,true)
  else
    ring(im,18,5,C.dark,1)
    diamond(im,32,47,2,C.rose)
    px(im,19,40,C.red); px(im,44,37,C.hot)
    px(im,25,31,C.rose); px(im,39,30,C.cyan)
  end
  if frame == 1 then sprite.cels[1].image=im else
    local f=sprite:newEmptyFrame()
    sprite:newCel(sprite.layers[1],f,im,Point(0,0))
  end
  sprite.frames[frame].duration=frame <= 7 and 0.18 or 0.1
  im:saveAs(root..string.format("spell_%02d.png",frame-1))
end
local warning=sprite:newTag(1,7);warning.name="Warning"
local strike=sprite:newTag(8,12);strike.name="Strike"
sprite:saveAs(root.."InvokerSkySpell_Polished.aseprite")
sprite:close()
