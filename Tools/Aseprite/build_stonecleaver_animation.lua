local pc=app.pixelColor
local clear=pc.rgba(0,0,0,0)
local rock=pc.rgba(115,108,102,255)
local dark=pc.rgba(42,38,39,255)
local glow=pc.rgba(224,150,38,255)
local dust=pc.rgba(151,126,94,210)
local source=Image{fromFile="Assets/Art/Imported/Enemies/stonecleaver.png"}
local sprite=Sprite(32,32,ColorMode.RGB)
sprite.layers[1].name="Stonecleaver"
local defs={{"Idle",4},{"Slow Walk",4},{"Slam Windup",4},{"Slam Impact",3},{"Recovery",4},{"Hurt",2},{"Stagger",3},{"Death",5}}
local index=0
local ranges={}
local function add(state,n)
 index=index+1
 local img=Image(sprite.spec);img:clear(clear)
 local x,y=0,0
 if state=="Idle" then y=(n==3) and 1 or 0 end
 if state=="Slow Walk" then x=(n==2 and 1) or (n==4 and -1) or 0;y=(n%2) end
 if state=="Slam Windup" then y=-n;x=math.floor(n/2) end
 if state=="Slam Impact" then y=(n==1 and -2 or 2);x=-n end
 if state=="Recovery" then y=2;x=(n%2) end
 if state=="Hurt" or state=="Stagger" then x=-n end
 if state=="Death" then y=n*2-2;x=-n end
 img:drawImage(source,Point(x,y))
 if state=="Slam Impact" then
  for px=4,28 do if (px+n)%4==0 then img:drawPixel(px,28,dust) end end
  img:drawPixel(25,27,glow);img:drawPixel(27,28,rock)
 end
 if index==1 then sprite.cels[1].image=img else local f=sprite:newEmptyFrame();sprite:newCel(sprite.layers[1],f,img,Point(0,0)) end
 sprite.frames[index].duration=(state=="Slam Windup") and 0.14 or 0.11
end
for _,d in ipairs(defs) do local first=index+1;for i=1,d[2] do add(d[1],i) end;table.insert(ranges,{d[1],first,index}) end
for _,r in ipairs(ranges) do local t=sprite:newTag(r[2],r[3]);t.name=r[1] end
sprite:saveAs("Assets/Art/Source/Enemies/stonecleaver_animated.aseprite")
sprite:close()

local fissure=Sprite(64,24,ColorMode.RGB)
fissure.layers[1].name="Traveling Fissure"
for f=1,6 do
 local img=Image(fissure.spec);img:clear(clear)
 local length=8+f*8
 local yy=18
 for x=2,length do
  local y=yy-((x+f)%3)
  img:drawPixel(x,y,dark)
  if x%5==0 then img:drawPixel(x,y-1,glow) end
 end
 for s=1,f do
  local sx=5+s*8
  if sx<length then
   img:drawPixel(sx,yy-3,rock);img:drawPixel(sx,yy-4,rock);img:drawPixel(sx+1,yy-2,dark)
  end
 end
 img:drawPixel(math.min(62,length+2),14,dust)
 if f==1 then fissure.cels[1].image=img else local nf=fissure:newEmptyFrame();fissure:newCel(fissure.layers[1],nf,img,Point(0,0)) end
 fissure.frames[f].duration=0.1
end
local ft=fissure:newTag(1,6);ft.name="Fissure"
fissure:saveAs("Assets/Art/Source/Enemies/stonecleaver_fissure_vfx.aseprite")
fissure:close()
