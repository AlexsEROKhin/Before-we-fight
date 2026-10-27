-- Hand-authored pixel maps. Every symbol is one pixel in the user's palette.
-- Opens the user's edited .aseprite, duplicates it, and appends new cels.
-- No generated image, resizing, or old PNG animation rebuild is used.
local root=app.fs.currentPath
local sourcePath=root.."/Assets/Art/Source/Knight/Knight_All_Animations_Editable.aseprite"
local outputPath=root.."/Assets/Art/Source/Knight/Knight_Manual_UpDown_v2.aseprite"
local framesDir=root.."/Assets/Art/Source/Knight/Directional_Manual"
local reviewDir=root.."/ArtReview/KnightManual"
app.fs.makeAllDirectories(framesDir)
app.fs.makeAllDirectories(reviewDir)
local source=assert(app.open(sourcePath))
local sprite=Sprite(source)
local originalCount=#source.frames
local pc=app.pixelColor
local rgba=pc.rgba
local hex={
 ['0']=0x000000, ['1']=0x0B5E65, ['2']=0x45293F, ['3']=0x4D65B4,
 ['4']=0x4D9BE6, ['5']=0x61495A, ['6']=0x7F708A, ['7']=0x8FD3FF,
 ['8']=0x9BABB2, ['9']=0x9E4539, A=0xC7DCD0, B=0xCD683D,
 C=0xF9C22B, D=0xFFFFFF
}
local palette={}
for key,value in pairs(hex) do
 palette[key]=rgba((value>>16)&255,(value>>8)&255,value&255,255)
end
local sourceColors={}
local original=Image(source.width,source.height,ColorMode.RGB)
original:drawSprite(source,1)
for it in original:pixels() do if pc.rgbaA(it())>0 then sourceColors[it()]=true end end
for _,color in pairs(palette) do assert(sourceColors[color],"Pixel color not present in current source") end

local function stamp(image,rows,x,y,mirror)
 local width=0
 for _,row in ipairs(rows) do width=math.max(width,#row) end
 for iy,row in ipairs(rows) do
  for ix=1,#row do
   local color=palette[row:sub(ix,ix)]
   if color then image:drawPixel(x+(mirror and width-ix or ix-1),y+iy-1,color) end
  end
 end
end
local frontHelmet={
 "..0000000..",
 ".034774430.",
 "03477744430",
 "034DD444430",
 "03CCCCCCC30",
 "03C00C00C30",
 "03C00C00C30",
 "03C00C00C30",
 ".03CCCC330.",
 "..0000000.."
}
local rearHelmet={
 "..0000000..",
 ".034774430.",
 "03477744430",
 "03477444430",
 "03444444430",
 "03344444430",
 "03334444330",
 ".033333330.",
 "..0CCCCC0..",
 "...00000..."
}
local frontTorso={
 "009666900",
 "056668650",
 "025566550",
 "025555550",
 ".09CCCC0.",
 "..00000.."
}
local rearTorso={
 "009666900",
 "056668B90",
 "025569B50",
 "02559B550",
 ".0CBCCC0.",
 "..00000.."
}
local leftShoulder={".000.","0BB90","0B990",".0950","..00."}
local rightShoulder={".000.","09690","09990","0590.",".00.."}
local leftHand={".00.","0880","08A0","0AA0",".00."}
local rightHand={".00.","0880","08A0","0A80",".00."}
-- The gold pommel, copper grip, and crossguard reproduce the source equipment.
local hilt={
 ".00.....",
 "0DC0....",
 "0CC0....",
 ".00B0...",
 "...0B0..",
 "....0BC0",
 "...00C0.",
 "..0CC0..",
 "...00..."
}
local legs={
 stand={"01110","01110","01110","00990","05500","05BB0"},
 reach={"01110","01110","01110","00990","05990","05BB0",".000."},
 lift={"01110","01990","05BB0",".000."},
 pass={"01110","01110","00990","05B00",".000."},
 push={"01110","01110","00990","05590",".0B0.","..0.."}
}
local rearLegs={
 stand={"01110","01110","01110","09900","0B550","0BB50"},
 reach={"01110","01110","01110","09900","0B590","0BB50",".000."},
 lift={"01110","09910","05550",".000."},
 pass={"01110","01110","09900","0BB50",".000."},
 push={"01110","01110","09900","09550",".0B0.","..0.."}
}
-- Eight individually specified poses: contact, down, passing, up; opposite foot.
local poses={
 {l="reach",r="push",bob=0,la=-1,ra=1},
 {l="stand",r="lift",bob=1,la=-1,ra=1},
 {l="pass",r="pass",bob=0,la=0,ra=0},
 {l="lift",r="stand",bob=-1,la=1,ra=-1},
 {l="push",r="reach",bob=0,la=1,ra=-1},
 {l="lift",r="stand",bob=1,la=1,ra=-1},
 {l="pass",r="pass",bob=0,la=1,ra=-1},
 {l="stand",r="lift",bob=-1,la=-1,ra=1}
}
local layers={}
for _,name in ipairs({"Vertical - equipment","Vertical - legs","Vertical - torso and arms","Vertical - helmet"}) do
 local layer=sprite:newLayer()
 layer.name=name
 layers[#layers+1]=layer
end
local function compose(direction,pose)
 local images={}
 for i=1,4 do images[i]=Image(80,80,ColorMode.RGB) end
 local rear=direction=="up"
 local bob=pose.bob
 local legset=rear and rearLegs or legs
 stamp(images[1],hilt,rear and 46 or 28,29+bob,rear)
 stamp(images[2],legset[pose.l],36,42)
 stamp(images[2],legset[pose.r],41,42)
 stamp(images[3],rear and rearTorso or frontTorso,36,37+bob)
 stamp(images[3],leftShoulder,33,36+bob)
 stamp(images[3],rightShoulder,43,36+bob)
 stamp(images[3],leftHand,32,39+bob+(rear and -pose.la or pose.la))
 stamp(images[3],rightHand,45,39+bob+(rear and -pose.ra or pose.ra))
 stamp(images[4],rear and rearHelmet or frontHelmet,35,27+bob)
 return images
end
local rendered={}
local tagged={}
local function add(direction,name,pose,duration)
 local frame=sprite:newEmptyFrame(#sprite.frames+1)
 frame.duration=duration
 local images=compose(direction,pose)
 for i,im in ipairs(images) do sprite:newCel(layers[i],frame,im,Point(0,0)) end
 local flat=Image(80,80,ColorMode.RGB)
 flat:drawSprite(sprite,frame.frameNumber)
 flat:saveAs(framesDir.."/"..name..".png")
 rendered[name]=flat
 return frame.frameNumber
end
for _,direction in ipairs({"down","up"}) do
 local n=add(direction,"idle_"..direction,{l="stand",r="stand",bob=0,la=0,ra=0},0.125)
 local tag=sprite:newTag(n,n); tag.name="idle "..direction
 tagged[#tagged+1]={name=tag.name,first=n,last=n}
 local first=#sprite.frames+1
 for i,pose in ipairs(poses) do add(direction,string.format("walk_%s_%02d",direction,i),pose,0.125) end
 tag=sprite:newTag(first,#sprite.frames);tag.name="walk "..direction
 tagged[#tagged+1]={name=tag.name,first=first,last=#sprite.frames}
end
-- Confirm the duplicate kept the user's original cels, timings, and tags exactly.
-- Aseprite extends the final tag on frame insertion; restore the source ranges.
for _,t in ipairs(tagged) do
 for _,copy in ipairs(sprite.tags) do
  if copy.name==t.name then
   copy.fromFrame=sprite.frames[t.first]
   copy.toFrame=sprite.frames[t.last]
  end
 end
end
for _,t in ipairs(source.tags) do
 for _,copy in ipairs(sprite.tags) do
  if copy.name==t.name then
   copy.fromFrame=sprite.frames[t.fromFrame.frameNumber]
   copy.toFrame=sprite.frames[t.toFrame.frameNumber]
  end
 end
end
for i=1,originalCount do
 local a,b=Image(80,80,ColorMode.RGB),Image(80,80,ColorMode.RGB)
 a:drawSprite(source,i);b:drawSprite(sprite,i)
 assert(a:isEqual(b),"Original pixels changed in frame "..i)
 assert(source.frames[i].duration==sprite.frames[i].duration,"Original timing changed")
end
for i,t in ipairs(source.tags) do
 local copy=sprite.tags[i]
 assert(t.name==copy.name and t.fromFrame.frameNumber==copy.fromFrame.frameNumber and t.toFrame.frameNumber==copy.toFrame.frameNumber,"Original tag changed")
end
sprite:saveAs(outputPath)
local function checker(w,h)
 local im=Image(w,h,ColorMode.RGB)
 for y=0,h-1 do for x=0,w-1 do
  local v=((math.floor(x/4)+math.floor(y/4))%2==0) and 78 or 87
  im:drawPixel(x,y,rgba(v,v,v,255))
 end end
 return im
end
local comparison=checker(96,36)
comparison:drawImage(Image(original,Rectangle(24,20,32,36)),Point(0,0))
comparison:drawImage(Image(rendered.idle_down,Rectangle(24,20,32,36)),Point(32,0))
comparison:drawImage(Image(rendered.idle_up,Rectangle(24,20,32,36)),Point(64,0))
comparison:resize(768,288)
comparison:saveAs(reviewDir.."/comparison.png")
local cycles=checker(256,72)
for row,d in ipairs({"down","up"}) do
 for i=1,8 do cycles:drawImage(Image(rendered[string.format("walk_%s_%02d",d,i)],Rectangle(24,20,32,36)),Point((i-1)*32,(row-1)*36)) end
end
cycles:resize(1024,288)
cycles:saveAs(reviewDir.."/cycles.png")
local record=assert(io.open(reviewDir.."/manual-art-report.json","w"))
record:write(json.encode({source=sourcePath,output=outputPath,originalFrames=originalCount,totalFrames=#sprite.frames,originalPreserved=true,paletteColors=14,tags=tagged}))
record:close()
sprite:close();source:close()
