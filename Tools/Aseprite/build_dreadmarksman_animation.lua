local pc=app.pixelColor
local clear=pc.rgba(0,0,0,0)
local aim=pc.rgba(211,61,70,190)
local flash=pc.rgba(255,221,145,255)
local trail=pc.rgba(213,198,164,210)
local source=Image{fromFile="Assets/Art/Imported/Enemies/dreadmarksman.png"}
local sprite=Sprite(32,32,ColorMode.RGB)
sprite.layers[1].name="Dreadmarksman"
local defs={{"Idle",4},{"Walk",4},{"Retreat",3},{"Aim Start",3},{"Aim Hold",4},{"Fire",3},{"Recoil",3},{"Reload",4},{"Recovery",3},{"Hurt",2},{"Stagger",2},{"Death",4}}
local index=0
local ranges={}
local function add(state,n)
 index=index+1
 local img=Image(sprite.spec);img:clear(clear)
 local x,y=0,0
 if state=="Idle" then y=(n==3) and 1 or 0 end
 if state=="Walk" then x=(n==2 and 1) or (n==4 and -1) or 0;y=n%2 end
 if state=="Retreat" then x=-n;y=n%2 end
 if state=="Aim Start" then x=n-1 end
 if state=="Aim Hold" then x=2;y=(n%2) end
 if state=="Fire" then x=3-n end
 if state=="Recoil" then x=-n end
 if state=="Reload" then y=(n==2 or n==3) and 1 or 0;x=-1 end
 if state=="Recovery" then y=1 end
 if state=="Hurt" or state=="Stagger" then x=-n end
 if state=="Death" then y=n*2-1;x=-n end
 img:drawImage(source,Point(x,y))
 if state=="Fire" and n==1 then img:drawPixel(29,14,flash);img:drawPixel(30,14,flash) end
 if index==1 then sprite.cels[1].image=img else local f=sprite:newEmptyFrame();sprite:newCel(sprite.layers[1],f,img,Point(0,0)) end
 sprite.frames[index].duration=(state=="Aim Hold") and 0.13 or 0.09
end
for _,d in ipairs(defs) do local first=index+1;for i=1,d[2] do add(d[1],i) end;table.insert(ranges,{d[1],first,index}) end
for _,r in ipairs(ranges) do local t=sprite:newTag(r[2],r[3]);t.name=r[1] end
sprite:saveAs("Assets/Art/Source/Enemies/dreadmarksman_animated.aseprite")
sprite:close()

local fx=Sprite(64,16,ColorMode.RGB)
fx.layers[1].name="Aim Projectile Impact"
for f=1,7 do
 local img=Image(fx.spec);img:clear(clear)
 if f<=3 then
  for x=2,61 do if (x+f)%3~=0 then img:drawPixel(x,8,aim) end end
 elseif f<=5 then
  local head=12+(f-4)*26
  for x=math.max(2,head-14),math.min(61,head) do img:drawPixel(x,8,trail) end
  img:drawPixel(math.min(61,head),8,flash)
 else
  local cx=55
  for p=-4,4 do img:drawPixel(cx+p,8+math.abs(p)%3,flash) end
 end
 if f==1 then fx.cels[1].image=img else local nf=fx:newEmptyFrame();fx:newCel(fx.layers[1],nf,img,Point(0,0)) end
 fx.frames[f].duration=0.08
end
local ft=fx:newTag(1,7);ft.name="Precision Shot"
fx:saveAs("Assets/Art/Source/Enemies/dreadmarksman_vfx.aseprite")
fx:close()
