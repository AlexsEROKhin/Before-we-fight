local root = app.fs.currentPath
local sourcePath = root .. "/Assets/Art/Source/Knight/Knight_All_Animations_Editable.aseprite"
local out = root .. "/ArtReview/KnightManual"
app.fs.makeAllDirectories(out)
local s = app.open(sourcePath)
assert(s, "Cannot open current knight document")
local data={filename=sourcePath,width=s.width,height=s.height,frames=#s.frames,tags={},layers={}}
for _,t in ipairs(s.tags) do
  table.insert(data.tags,{name=t.name,first=t.fromFrame.frameNumber,last=t.toFrame.frameNumber})
end
for _,l in ipairs(s.layers) do table.insert(data.layers,{name=l.name}) end
for _,n in ipairs({1,2,10,11,12,14}) do
  local im=Image(s.width,s.height,ColorMode.RGB)
  im:drawSprite(s,n)
  im:saveAs(out .. "/source_"..n..".png")
  local crop=Image(im,Rectangle(24,20,36,36))
  crop:resize(432,432)
  crop:saveAs(out .. "/source_"..n.."_large.png")
end
local file=assert(io.open(out .. "/source-info.json","w"))
file:write(json.encode(data))
file:close()
s:close()
