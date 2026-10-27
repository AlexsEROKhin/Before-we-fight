local pc=app.pixelColor
local clear=pc.rgba(0,0,0,0)
local dark=pc.rgba(53,34,72,230)
local purple=pc.rgba(142,48,207,255)
local bright=pc.rgba(213,106,255,255)
local source=Image{fromFile="Assets/Art/Imported/Enemies/shadowrazer.png"}
local sprite=Sprite(32,32,ColorMode.RGB)
sprite.layers[1].name="Shadowrazer"
local defs={{"Idle",4},{"Move",4},{"Close Cast",4},{"Medium Cast",4},{"Long Cast",4},{"Recovery",3},{"Hurt",2},{"Stagger",2},{"Death",4}}
local index=0
local ranges={}
local function add(state,n)
 index=index+1
 local img=Image(sprite.spec);img:clear(clear)
 local x,y=0,0
 if state=="Idle" then y=(n==2 or n==4) and -1 or 0 end
 if state=="Move" then x=(n==2 and 1) or (n==4 and -1) or 0;y=n%2-1 end
 if state=="Close Cast" then x=-1;y=-n+1 end
 if state=="Medium Cast" then x=0;y=-n+1 end
 if state=="Long Cast" then x=1;y=-n+1 end
 if state=="Recovery" then y=1;x=n%2 end
 if state=="Hurt" or state=="Stagger" then x=-n end
 if state=="Death" then y=n*2-1;x=-n end
 img:drawImage(source,Point(x,y))
 if state=="Close Cast" or state=="Medium Cast" or state=="Long Cast" then
  local gx=(state=="Close Cast" and 21) or (state=="Medium Cast" and 24) or 27
  img:drawPixel(gx,15,bright);img:drawPixel(gx+1,15,purple)
  if n>=3 then img:drawPixel(gx,14,dark) end
 end
 if index==1 then sprite.cels[1].image=img else local f=sprite:newEmptyFrame();sprite:newCel(sprite.layers[1],f,img,Point(0,0)) end
 sprite.frames[index].duration=0.1
end
for _,d in ipairs(defs) do local first=index+1;for i=1,d[2] do add(d[1],i) end;table.insert(ranges,{d[1],first,index}) end
for _,r in ipairs(ranges) do local t=sprite:newTag(r[2],r[3]);t.name=r[1] end
sprite:saveAs("Assets/Art/Source/Enemies/shadowrazer_animated.aseprite")
sprite:close()

local fx=Sprite(64,24,ColorMode.RGB)
fx.layers[1].name="Shadow Ground Bursts"
for f=1,9 do
 local img=Image(fx.spec);img:clear(clear)
 local variant=math.floor((f-1)/3)
 local phase=((f-1)%3)+1
 local center=12+variant*18
 if phase==1 then
  for x=center-5,center+5 do img:drawPixel(x,20,dark) end
 elseif phase==2 then
  for x=center-7,center+7 do img:drawPixel(x,20,purple) end
  img:drawPixel(center,16,bright);img:drawPixel(center-3,18,dark);img:drawPixel(center+3,18,dark)
 else
  for p=-6,6,2 do
   local h=8-math.abs(p)
   for y=20-h,20 do img:drawPixel(center+p,y,(y%2==0) and bright or purple) end
  end
 end
 if f==1 then fx.cels[1].image=img else local nf=fx:newEmptyFrame();fx:newCel(fx.layers[1],nf,img,Point(0,0)) end
 fx.frames[f].duration=0.1
end
local t1=fx:newTag(1,3);t1.name="Close Burst"
local t2=fx:newTag(4,6);t2.name="Medium Burst"
local t3=fx:newTag(7,9);t3.name="Long Burst"
fx:saveAs("Assets/Art/Source/Enemies/shadowrazer_vfx.aseprite")
fx:close()
