local pc=app.pixelColor
local clear=pc.rgba(0,0,0,0)
local foam=pc.rgba(170,220,218,220)
local water=pc.rgba(57,116,132,220)
local debris=pc.rgba(133,119,88,255)
local source=Image{fromFile="Assets/Art/Imported/Enemies/deeptide_colossus.png"}
local sprite=Sprite(48,48,ColorMode.RGB)
sprite.layers[1].name="DeepTide Colossus"
local defs={{"Idle",4},{"Slow Walk",4},{"Charge Start",4},{"Charge Hold",4},{"Ground Slam",4},{"Shockwave Release",3},{"Exhausted",5},{"Hurt",2},{"Stagger",3},{"Death",6}}
local index=0
local ranges={}
local function add(state,n)
 index=index+1
 local img=Image(sprite.spec);img:clear(clear)
 local x,y=0,0
 if state=="Idle" then y=(n==3) and 1 or 0 end
 if state=="Slow Walk" then x=(n==2 and 1) or (n==4 and -1) or 0;y=n%2 end
 if state=="Charge Start" then y=-n;x=-math.floor(n/2) end
 if state=="Charge Hold" then y=-4;x=(n%2)-1 end
 if state=="Ground Slam" then y=(n<3 and -4+n or 3);x=n-2 end
 if state=="Shockwave Release" then y=3;x=n-2 end
 if state=="Exhausted" then y=3;x=n%2 end
 if state=="Hurt" or state=="Stagger" then x=-n end
 if state=="Death" then y=n*3-2;x=-n end
 img:drawImage(source,Point(x,y))
 if state=="Ground Slam" or state=="Shockwave Release" then
  for px=5,43 do if (px+n)%5==0 then img:drawPixel(px,42,foam) end end
 end
 if index==1 then sprite.cels[1].image=img else local f=sprite:newEmptyFrame();sprite:newCel(sprite.layers[1],f,img,Point(0,0)) end
 sprite.frames[index].duration=(state=="Charge Hold") and 0.15 or 0.12
end
for _,d in ipairs(defs) do local first=index+1;for i=1,d[2] do add(d[1],i) end;table.insert(ranges,{d[1],first,index}) end
for _,r in ipairs(ranges) do local t=sprite:newTag(r[2],r[3]);t.name=r[1] end
sprite:saveAs("Assets/Art/Source/Enemies/deeptide_colossus_animated.aseprite")
sprite:close()

local fx=Sprite(64,40,ColorMode.RGB)
fx.layers[1].name="Radial Shockwave"
for f=1,8 do
 local img=Image(fx.spec);img:clear(clear)
 local radius=4+f*3
 for x=32-radius,32+radius do
  if x>=0 and x<64 and (x+f)%3~=0 then img:drawPixel(x,31+((x+f)%2),water) end
 end
 img:drawPixel(math.max(1,32-radius),28,foam);img:drawPixel(math.min(62,32+radius),28,foam)
 for d=1,math.floor(f/2) do
  local dx=32-radius+d*7
  if dx>1 and dx<62 then img:drawPixel(dx,27-d,debris);img:drawPixel(dx+1,28-d,debris) end
 end
 if f==1 then fx.cels[1].image=img else local nf=fx:newEmptyFrame();fx:newCel(fx.layers[1],nf,img,Point(0,0)) end
 fx.frames[f].duration=0.1
end
local ft=fx:newTag(1,8);ft.name="Tidal Shockwave"
fx:saveAs("Assets/Art/Source/Enemies/deeptide_colossus_vfx.aseprite")
fx:close()
