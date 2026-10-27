local pc=app.pixelColor
local clear=pc.rgba(0,0,0,0)
local shadow=pc.rgba(67,48,88,210)
local purple=pc.rgba(137,77,190,255)
local slash=pc.rgba(213,190,235,240)
local source=Image{fromFile="Assets/Art/Imported/Enemies/blinkshade.png"}
local sprite=Sprite(32,32,ColorMode.RGB)
sprite.layers[1].name="Blinkshade"
local defs={{"Idle",4},{"Stalk",4},{"Run",4},{"Vanish Start",3},{"Vanish",3},{"Reappear",3},{"Blink Strike",4},{"Retreat",3},{"Recovery",3},{"Hurt",2},{"Stagger",2},{"Death",4}}
local index=0
local ranges={}
local function add(state,n)
 index=index+1
 local img=Image(sprite.spec);img:clear(clear)
 local x,y=0,0
 if state=="Idle" then y=(n==2 or n==4) and 1 or 0 end
 if state=="Stalk" then x=(n==2 and 1) or 0;y=n%2 end
 if state=="Run" then x=(n%3)-1;y=n%2 end
 if state=="Vanish Start" then x=n-1 end
 if state=="Reappear" then x=3-n end
 if state=="Blink Strike" then x=n*2-3;y=(n==3 and -1 or 0) end
 if state=="Retreat" then x=-n end
 if state=="Recovery" then y=1;x=(n%2) end
 if state=="Hurt" or state=="Stagger" then x=-n end
 if state=="Death" then y=n*2-1;x=-n end
 if state~="Vanish" then img:drawImage(source,Point(x,y)) end
 if state=="Vanish Start" or state=="Vanish" or state=="Reappear" then
  for p=1,5 do img:drawPixel(8+p*3,(p*5+n*2)%24+4,(p%2==0) and purple or shadow) end
 end
 if state=="Blink Strike" then
  for p=0,9 do img:drawPixel(19+p,9+math.floor(p/2),slash) end
 end
 if index==1 then sprite.cels[1].image=img else local f=sprite:newEmptyFrame();sprite:newCel(sprite.layers[1],f,img,Point(0,0)) end
 sprite.frames[index].duration=(state=="Blink Strike") and 0.07 or 0.09
end
for _,d in ipairs(defs) do local first=index+1;for i=1,d[2] do add(d[1],i) end;table.insert(ranges,{d[1],first,index}) end
for _,r in ipairs(ranges) do local t=sprite:newTag(r[2],r[3]);t.name=r[1] end
sprite:saveAs("Assets/Art/Source/Enemies/blinkshade_animated.aseprite")
sprite:close()

local fx=Sprite(32,32,ColorMode.RGB)
fx.layers[1].name="Vanish and Slash VFX"
for f=1,6 do
 local img=Image(fx.spec);img:clear(clear)
 for p=1,7 do
  local px=5+((p*4+f*3)%23);local py=5+((p*7-f*2)%20)
  img:drawPixel(px,py,(p%2==0) and purple or shadow)
 end
 if f>=4 then for p=0,8 do img:drawPixel(15+p,8+p,slash) end end
 if f==1 then fx.cels[1].image=img else local nf=fx:newEmptyFrame();fx:newCel(fx.layers[1],nf,img,Point(0,0)) end
 fx.frames[f].duration=0.08
end
local ft=fx:newTag(1,6);ft.name="Vanish Slash"
fx:saveAs("Assets/Art/Source/Enemies/blinkshade_vfx.aseprite")
fx:close()
