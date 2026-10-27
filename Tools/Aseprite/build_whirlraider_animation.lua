local pc=app.pixelColor
local clear=pc.rgba(0,0,0,0)
local trail=pc.rgba(176,185,196,210)
local spark=pc.rgba(255,205,105,255)
local source=Image{fromFile="Assets/Art/Imported/Enemies/whirlraider.png"}
local sprite=Sprite(32,32,ColorMode.RGB)
sprite.layers[1].name="Whirlraider"
local defs={{"Idle",4},{"Run",4},{"Spin Windup",3},{"Spin Start",3},{"Spin Loop",6},{"Spin Ending",3},{"Exhausted",4},{"Hurt",2},{"Stagger",2},{"Death",4}}
local index=0
local ranges={}
local function add(state,n)
 index=index+1
 local img=Image(sprite.spec);img:clear(clear)
 local x,y=0,0
 if state=="Idle" then y=(n==2 or n==4) and 1 or 0 end
 if state=="Run" then x=(n==2 and 1) or (n==4 and -1) or 0;y=n%2 end
 if state=="Spin Windup" then y=n; x=-math.floor(n/2) end
 if state=="Spin Start" then x=n-2 end
 if state=="Spin Loop" then x=(n%3)-1;y=n%2 end
 if state=="Spin Ending" then x=2-n end
 if state=="Exhausted" then y=2;x=(n%2) end
 if state=="Hurt" or state=="Stagger" then x=-n end
 if state=="Death" then y=n*2-2;x=-n end
 img:drawImage(source,Point(x,y))
 if state=="Spin Start" or state=="Spin Loop" or state=="Spin Ending" then
   local row=15+(n%3)
   for px=2,29 do if (px+n)%3~=0 then img:drawPixel(px,row,trail) end end
   img:drawPixel(28-n%4,row-1,spark)
 end
 if index==1 then sprite.cels[1].image=img else local f=sprite:newEmptyFrame();sprite:newCel(sprite.layers[1],f,img,Point(0,0)) end
 sprite.frames[index].duration=(state=="Spin Loop") and 0.08 or 0.1
end
for _,d in ipairs(defs) do local first=index+1;for i=1,d[2] do add(d[1],i) end;table.insert(ranges,{d[1],first,index}) end
for _,r in ipairs(ranges) do local t=sprite:newTag(r[2],r[3]);t.name=r[1] end
sprite:saveAs("Assets/Art/Source/Enemies/whirlraider_animated.aseprite")
sprite:close()

local fx=Sprite(32,32,ColorMode.RGB)
fx.layers[1].name="Spin Trail"
for f=1,6 do
 local img=Image(fx.spec);img:clear(clear)
 local yy=14+(f%3)
 for x=2,29 do if (x+f)%4~=0 then img:drawPixel(x,yy,trail) end end
 img:drawPixel(4+f*4,yy-1,spark);img:drawPixel(28-f*3,yy+1,trail)
 if f==1 then fx.cels[1].image=img else local nf=fx:newEmptyFrame();fx:newCel(fx.layers[1],nf,img,Point(0,0)) end
 fx.frames[f].duration=0.08
end
local tag=fx:newTag(1,6);tag.name="Spin Trail"
fx:saveAs("Assets/Art/Source/Enemies/whirlraider_spin_vfx.aseprite")
fx:close()
