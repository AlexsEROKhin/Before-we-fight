local pc=app.pixelColor
local clear=pc.rgba(0,0,0,0)
local pulse=pc.rgba(205,78,55,220)
local dust=pc.rgba(153,119,82,210)
local trail=pc.rgba(205,193,157,230)
local source=Image{fromFile="Assets/Art/Imported/Enemies/warcry_brute.png"}
local sprite=Sprite(32,32,ColorMode.RGB)
sprite.layers[1].name="Warcry Brute"
local defs={{"Idle",4},{"Heavy Walk",4},{"Roar Start",3},{"War Cry",4},{"Heavy Windup",4},{"Heavy Swing",4},{"Recovery",4},{"Hurt",2},{"Stagger",3},{"Death",5}}
local index=0
local ranges={}
local function add(state,n)
 index=index+1
 local img=Image(sprite.spec);img:clear(clear)
 local x,y=0,0
 if state=="Idle" then y=(n==3) and 1 or 0 end
 if state=="Heavy Walk" then x=(n==2 and 1) or (n==4 and -1) or 0;y=n%2 end
 if state=="Roar Start" then y=-n+1;x=-1 end
 if state=="War Cry" then y=-1;x=(n%2)-1 end
 if state=="Heavy Windup" then x=-n+1;y=-math.floor(n/2) end
 if state=="Heavy Swing" then x=n-1;y=(n==3 and 1 or 0) end
 if state=="Recovery" then y=2;x=n%2 end
 if state=="Hurt" or state=="Stagger" then x=-n end
 if state=="Death" then y=n*2-1;x=-n end
 img:drawImage(source,Point(x,y))
 if state=="War Cry" then
  img:drawPixel(27,10+n,pulse);img:drawPixel(29,9+n,pulse)
 end
 if state=="Heavy Swing" then
  for p=0,8 do img:drawPixel(21+p,7+math.floor(p/2),trail) end
 end
 if index==1 then sprite.cels[1].image=img else local f=sprite:newEmptyFrame();sprite:newCel(sprite.layers[1],f,img,Point(0,0)) end
 sprite.frames[index].duration=(state=="Heavy Windup") and 0.14 or 0.11
end
for _,d in ipairs(defs) do local first=index+1;for i=1,d[2] do add(d[1],i) end;table.insert(ranges,{d[1],first,index}) end
for _,r in ipairs(ranges) do local t=sprite:newTag(r[2],r[3]);t.name=r[1] end
sprite:saveAs("Assets/Art/Source/Enemies/warcry_brute_animated.aseprite")
sprite:close()

local fx=Sprite(48,32,ColorMode.RGB)
fx.layers[1].name="War Cry and Heavy Impact"
for f=1,7 do
 local img=Image(fx.spec);img:clear(clear)
 if f<=4 then
  local radius=5+f*4
  for x=24-radius,24+radius do
   if x>=0 and x<48 and (x+f)%3~=0 then img:drawPixel(x,19+(x%2),pulse) end
  end
 else
  for x=8,40 do if (x+f)%4==0 then img:drawPixel(x,23,dust) end end
  for p=0,10 do img:drawPixel(24+p,9+p,trail) end
 end
 if f==1 then fx.cels[1].image=img else local nf=fx:newEmptyFrame();fx:newCel(fx.layers[1],nf,img,Point(0,0)) end
 fx.frames[f].duration=0.1
end
local ft=fx:newTag(1,7);ft.name="War Cry Impact"
fx:saveAs("Assets/Art/Source/Enemies/warcry_brute_vfx.aseprite")
fx:close()
