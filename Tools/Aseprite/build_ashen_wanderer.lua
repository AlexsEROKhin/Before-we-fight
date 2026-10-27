local sourcePath="C:/Users/okyer/OneDrive/Документы/ChatGPT/создание игры/Assets/Art/Source/Heroes/AshenWanderer_FullAnimation.png"
local outputPath="C:/Users/okyer/OneDrive/Документы/ChatGPT/создание игры/Assets/Art/Source/Heroes/AshenWanderer_Animated.aseprite"
local source=Image{fromFile=sourcePath}
local columns,rows=8,8
local cellWidth=math.ceil(source.width/columns)
local cellHeight=math.ceil(source.height/rows)
local sprite=Sprite(cellWidth,cellHeight,ColorMode.RGB)
sprite.layers[1].name="Ashen Wanderer"
local clear=app.pixelColor.rgba(0,0,0,0)
for row=0,rows-1 do
 for column=0,columns-1 do
  local frameIndex=row*columns+column+1
  local x=math.floor(column*source.width/columns)
  local y=math.floor(row*source.height/rows)
  local image=Image(sprite.spec)
  image:clear(clear)
  image:drawImage(source,Point(-x,-y))
  if frameIndex==1 then
   sprite.cels[1].image=image
  else
   local frame=sprite:newEmptyFrame()
   sprite:newCel(sprite.layers[1],frame,image,Point(0,0))
  end
  sprite.frames[frameIndex].duration=0.1
 end
end
local tags={{"Idle",1,8},{"Walk",9,16},{"Run",17,24},{"Attack",25,32},{"Dodge",33,40},{"Dash",41,48},{"Jump",49,56},{"Death",57,64}}
for _,definition in ipairs(tags) do
 local tag=sprite:newTag(definition[2],definition[3])
 tag.name=definition[1]
end
sprite:saveAs(outputPath)
app.command.ExportSpriteSheet{
 ui=false,
 askOverwrite=false,
 type=SpriteSheetType.ROWS,
 columns=8,
 textureFilename="C:/Users/okyer/OneDrive/Документы/ChatGPT/создание игры/Assets/Art/Imported/ashen_wanderer_sheet.png",
 dataFilename="C:/Users/okyer/OneDrive/Документы/ChatGPT/создание игры/Assets/Art/Imported/ashen_wanderer_sheet.json",
 dataFormat=SpriteSheetDataFormat.JSON_ARRAY
}
sprite:close()
