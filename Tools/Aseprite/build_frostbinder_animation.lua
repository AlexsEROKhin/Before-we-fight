local pc=app.pixelColor
local clear=pc.rgba(0,0,0,0)
local ice=pc.rgba(101,210,239,255)
local pale=pc.rgba(205,246,255,255)
local blue=pc.rgba(64,126,190,230)
local source=Image{fromFile="Assets/Art/Imported/Enemies/frostbinder.png"}
local sprite=Sprite(32,32,ColorMode.RGB)
sprite.layers[1].name="Frostbinder"
local defs={{"Idle",4},{"Walk",4},{"Cast Start",3},{"Cast Hold",4},{"Cast Release",3},{"Recovery",3},{"Hurt",2},{"Stagger",2},{"Death",4}}
local index=0
local ranges={}
local function add(state,n)
 index=index+1
 local img=Image(sprite.spec);img:clear(clear)
 local x,y=0,0
 if state=="Idle" then y=(n==2 or n==4) and -1 or 0 end
 if state=="Walk" then x=(n==2 and 1) or (n==4 and -1) or 0;y=n%2 end
 if state=="Cast Start" then y=-n+1 end
 if state=="Cast Hold" or state=="Cast Release" then y=-1 end
 if state=="Recovery" then y=1;x=n%2 end
 if state=="Hurt" or state=="Stagger" then x=-n end
 if state=="Death" then y=n*2-1;x=-n end
 img:drawImage(source,Point(x,y))
 if state=="Cast Start" or state=="Cast Hold" or state=="Cast Release" then
  img:drawPixel(24+x,15+y,pale);img:drawPixel(25+x,15+y,ice)
  if n>=2 then img:drawPixel(24+x,14+y,blue) end
 end
 if index==1 then sprite.cels[1].image=img else local f=sprite:newEmptyFrame();sprite:newCel(sprite.layers[1],f,img,Point(0,0)) end
 sprite.frames[index].duration=0.11
end
for _,d in ipairs(defs) do local first=index+1;for i=1,d[2] do add(d[1],i) end;table.insert(ranges,{d[1],first,index}) end
for _,r in ipairs(ranges) do local t=sprite:newTag(r[2],r[3]);t.name=r[1] end
sprite:saveAs("Assets/Art/Source/Enemies/frostbinder_animated.aseprite")
sprite:close()

local fx=Sprite(32,24,ColorMode.RGB)
fx.layers[1].name="Ice Sigil and Eruption"
for f=1,7 do
 local img=Image(fx.spec);img:clear(clear)
 if f<=3 then
  for x=7,24 do img:drawPixel(x,19,(f%2==0) and pale or blue) end
  img:drawPixel(9,18,ice);img:drawPixel(15,17,pale);img:drawPixel(22,18,ice)
 else
  local h=(f-3)*3
  for x=9,23,4 do for y=19-h,19 do img:drawPixel(x,y,(y%2==0) and pale or ice) end end
  img:drawPixel(7,20,blue);img:drawPixel(25,20,blue)
 end
 if f==1 then fx.cels[1].image=img else local nf=fx:newEmptyFrame();fx:newCel(fx.layers[1],nf,img,Point(0,0)) end
 fx.frames[f].duration=0.1
end
local ft=fx:newTag(1,7);ft.name="Freeze Ground"
fx:saveAs("Assets/Art/Source/Enemies/frostbinder_vfx.aseprite")
fx:close()
