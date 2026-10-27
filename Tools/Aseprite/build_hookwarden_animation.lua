local pc=app.pixelColor
local clear=pc.rgba(0,0,0,0)
local spark=pc.rgba(255,205,105,255)
local source=Image{fromFile="Assets/Art/Imported/Enemies/hookwarden.png"}
local sprite=Sprite(32,32,ColorMode.RGB)
sprite.layers[1].name="Hookwarden"
local defs={{"Idle",4},{"Walk",4},{"Hook Aim",3},{"Hook Throw",3},{"Chain Extended",2},{"Pull",3},{"Heavy Attack",4},{"Recovery",3},{"Hurt",2},{"Stagger",2},{"Death",4}}
local index=0
local ranges={}
local function frame(state,n)
 index=index+1
 local img=Image(sprite.spec);img:clear(clear)
 local x,y=0,0
 if state=="Idle" then y=(n==2 or n==4) and 1 or 0 end
 if state=="Walk" then x=(n==2 and 1) or (n==4 and -1) or 0;y=n%2 end
 if state=="Hook Aim" then x=n-1 end
 if state=="Hook Throw" then x=n end
 if state=="Pull" then x=-n end
 if state=="Heavy Attack" then x=(n<3 and 1 or -1);y=(n==3 and -1 or 0) end
 if state=="Hurt" or state=="Stagger" then x=-n end
 if state=="Death" then y=n*2-2;x=-n end
 img:drawImage(source,Point(x,y))
 if state=="Hook Throw" or state=="Chain Extended" then img:drawPixel(29,14,spark);img:drawPixel(30,14,spark) end
 if index==1 then sprite.cels[1].image=img else local f=sprite:newEmptyFrame();sprite:newCel(sprite.layers[1],f,img,Point(0,0)) end
 sprite.frames[index].duration=0.11
end
for _,d in ipairs(defs) do local first=index+1;for i=1,d[2] do frame(d[1],i) end;table.insert(ranges,{d[1],first,index}) end
for _,r in ipairs(ranges) do local t=sprite:newTag(r[2],r[3]);t.name=r[1] end
sprite:saveAs("Assets/Art/Source/Enemies/hookwarden_animated.aseprite")
sprite:close()
