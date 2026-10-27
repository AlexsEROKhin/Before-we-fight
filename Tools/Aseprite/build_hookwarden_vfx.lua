local pc=app.pixelColor
local clear=pc.rgba(0,0,0,0)
local metal=pc.rgba(154,153,148,255)
local light=pc.rgba(218,210,185,255)
local dark=pc.rgba(45,40,39,255)
local spark=pc.rgba(255,193,78,255)
local spr=Sprite(16,16,ColorMode.RGB)
spr.layers[1].name="Hook"
for f=1,3 do
 local img=Image(spr.spec);img:clear(clear)
 for x=2,10 do img:drawPixel(x,8,metal) end
 img:drawPixel(10,7,light);img:drawPixel(11,8,metal);img:drawPixel(11,9,metal)
 img:drawPixel(10,10,metal);img:drawPixel(9,11,dark);img:drawPixel(8,11,dark)
 if f==3 then img:drawPixel(12,7,spark);img:drawPixel(13,6,spark) end
 if f==1 then spr.cels[1].image=img else local nf=spr:newEmptyFrame();spr:newCel(spr.layers[1],nf,img,Point(0,0)) end
 spr.frames[f].duration=0.08
end
local t=spr:newTag(1,3);t.name="Hook"
spr:saveAs("Assets/Art/Source/Enemies/hookwarden_hook_vfx.aseprite")
spr:close()
