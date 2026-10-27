"""Hand-polish the user's one-frame Io wisp into a compact four-frame pixel sprite.

The original Aseprite document is read only. This writes a separate editable
Aseprite document plus a Unity-ready sheet; no generative image model is used.
"""

from pathlib import Path
import struct
import zlib

from PIL import Image, ImageDraw


ROOT = Path(__file__).resolve().parents[2]
SOURCE = Path(r"D:\Новая папка\anime3.aseprite")
ASE_OUT = ROOT / "Assets/Art/Source/Enemies/anime3_io_polished_v2.aseprite"
PNG_OUT = ROOT / "Assets/Art/Imported/anime3_io_idle_v2.png"
PREVIEW_OUT = ROOT / "ArtReview/anime3_io_polished_v2_preview.png"

INK = (18, 31, 56, 255)
SHADOW = (28, 69, 135, 255)
BLUE = (58, 122, 222, 255)
LIGHT = (105, 174, 255, 255)
CYAN = (122, 223, 244, 255)
PALE = (193, 250, 255, 255)
WHITE = (247, 254, 255, 255)
DEEP = (25, 48, 93, 255)


def source_image():
    data = SOURCE.read_bytes()
    assert struct.unpack_from("<H", data, 4)[0] == 0xA5E0
    frame_offset = 128
    chunk = frame_offset + 16
    for _ in range(struct.unpack_from("<H", data, frame_offset + 6)[0]):
        size, kind = struct.unpack_from("<IH", data, chunk)
        if kind == 0x2005:
            layer, x, y, opacity, cel_type = struct.unpack_from("<HhhBH", data, chunk + 6)
            if layer == 0 and cel_type == 2:
                w, h = struct.unpack_from("<HH", data, chunk + 22)
                raw = zlib.decompress(data[chunk + 26:chunk + size])
                image = Image.new("RGBA", (64, 64))
                image.alpha_composite(Image.frombytes("RGBA", (w, h), raw), (x, y))
                return image
        chunk += size
    raise RuntimeError("No editable RGBA cel found in anime3")


def make_frame(original, index):
    # Keep the original asymmetric flame silhouette, scaled to the same
    # 32-pixel footprint as the game's small enemies.
    crop = original.crop(original.getbbox())
    crop = crop.resize((17, 27), Image.Resampling.NEAREST)
    mask = crop.getchannel("A").point(lambda a: 255 if a else 0)
    bob = (0, -1, 0, 1)[index]
    x0, y0 = 7, 2 + bob

    image = Image.new("RGBA", (32, 32))
    silhouette = Image.new("L", (32, 32))
    silhouette.paste(mask, (x0, y0))
    pixels = silhouette.load()
    draw = ImageDraw.Draw(image)
    for y in range(32):
        for x in range(32):
            if pixels[x, y]:
                # Upper-left light; the right/lower flank gives the orb depth.
                color = LIGHT if x < 18 and y < 19 + bob else BLUE
                if x < 10 or y > 25 + bob:
                    color = SHADOW
                draw.point((x, y), fill=color)

    # Deliberate one-pixel outline follows the source shape, but leaves the
    # little orbit motes disconnected and readable.
    opaque = image.getchannel("A")
    for y in range(1, 31):
        for x in range(1, 31):
            if not opaque.getpixel((x, y)):
                continue
            if any(not opaque.getpixel((x + dx, y + dy))
                   for dx, dy in ((-1, 0), (1, 0), (0, -1), (0, 1))):
                draw.point((x, y), fill=INK)

    # The rear arc is visible above the orb, while the lower cyan arc crosses
    # its foreground. The orbit gives this wisp a readable Io-like character.
    cy = 17 + bob
    draw.point((8, cy - 6), fill=DEEP)
    draw.line([(9, cy - 7), (12, cy - 9), (16, cy - 10)], fill=SHADOW)
    draw.line([(21, cy - 10), (25, cy - 8), (27, cy - 5)], fill=LIGHT)

    # Large outlined energy core, kept deliberately off-centre like the
    # original eye. Clusters have a fixed upper-left lighting direction.
    draw.polygon([(17, cy - 7), (21, cy - 6), (24, cy - 3), (25, cy + 1),
                  (23, cy + 5), (20, cy + 7), (16, cy + 6), (13, cy + 3),
                  (12, cy - 1), (14, cy - 5)], fill=DEEP)
    draw.polygon([(17, cy - 6), (21, cy - 5), (23, cy - 2), (24, cy + 1),
                  (22, cy + 4), (19, cy + 6), (16, cy + 5), (14, cy + 2),
                  (13, cy - 1), (15, cy - 4)], fill=CYAN)
    draw.rectangle((16, cy - 4, 20, cy - 3), fill=PALE)
    draw.rectangle((15, cy - 2, 21, cy + 2), fill=PALE)
    draw.rectangle((17, cy - 3, 20, cy + 1), fill=WHITE)
    draw.point((21, cy - 3), fill=WHITE)
    draw.point((22, cy + 2), fill=LIGHT)
    draw.point((14, cy + 1), fill=LIGHT)

    # A small elliptical front arc makes the core feel suspended rather than
    # pasted onto the flame. It never hides the brightest central pixels.
    draw.line([(6, cy + 2), (7, cy + 5), (10, cy + 7), (14, cy + 8)], fill=DEEP)
    draw.line([(15, cy + 8), (20, cy + 8), (24, cy + 6), (27, cy + 3)], fill=DEEP)
    draw.line([(8, cy + 5), (11, cy + 7), (14, cy + 7)], fill=BLUE)
    draw.line([(16, cy + 8), (20, cy + 7), (24, cy + 5), (26, cy + 3)], fill=LIGHT)
    draw.point((25, cy + 4), fill=PALE)

    # Orbiting pixel clusters rotate over the four-frame loop. These are not
    # smooth particles or gradients: each cluster is 1–3 intentional pixels.
    orbits = [((5, 12), (27, 10)), ((4, 15), (28, 14)),
              ((5, 20), (27, 19)), ((8, 23), (26, 12))]
    (lx, ly), (rx, ry) = orbits[index]
    draw.point((lx, ly), fill=PALE)
    draw.point((lx + 1, ly), fill=CYAN)
    draw.point((rx, ry), fill=WHITE)
    draw.point((rx, ry + 1), fill=CYAN)
    if index in (1, 3):
        draw.point((24, 4), fill=LIGHT)
    return image


def chunk(kind, payload):
    return struct.pack("<IH", len(payload) + 6, kind) + payload


def cel_chunk(image):
    # Aseprite compressed image cel: layer, position, opacity/type, reserved,
    # dimensions, then zlib-compressed RGBA pixels.
    payload = struct.pack("<HhhBH", 0, 0, 0, 255, 2)
    payload += b"\x00" * 7
    payload += struct.pack("<HH", image.width, image.height)
    payload += zlib.compress(image.tobytes(), 9)
    return chunk(0x2005, payload)


def write_aseprite(frames):
    name = "Io - polished idle".encode("utf-8")
    layer_payload = struct.pack("<HHHHHHB", 1, 0, 0, 0, 0, 0, 255)
    layer_payload += b"\x00" * 3 + struct.pack("<H", len(name)) + name
    layer = chunk(0x2004, layer_payload)
    frame_data = []
    for number, image in enumerate(frames):
        chunks = ([layer] if number == 0 else []) + [cel_chunk(image)]
        body = b"".join(chunks)
        header = struct.pack("<IHHH", 16 + len(body), 0xF1FA, len(chunks), 140)
        header += b"\x00" * 2 + struct.pack("<I", len(chunks))
        frame_data.append(header + body)
    header = bytearray(128)
    struct.pack_into("<IHHHHH", header, 0, 128 + sum(map(len, frame_data)),
                     0xA5E0, len(frames), 32, 32, 32)
    struct.pack_into("<I", header, 14, 1)
    struct.pack_into("<H", header, 18, 140)
    header[32] = 0
    header[33] = 0
    struct.pack_into("<H", header, 34, 0)
    header[36:38] = b"\x01\x01"
    ASE_OUT.parent.mkdir(parents=True, exist_ok=True)
    ASE_OUT.write_bytes(bytes(header) + b"".join(frame_data))


def main():
    original = source_image()
    frames = [make_frame(original, i) for i in range(4)]
    write_aseprite(frames)
    sheet = Image.new("RGBA", (128, 32))
    for i, frame in enumerate(frames):
        sheet.alpha_composite(frame, (i * 32, 0))
    PNG_OUT.parent.mkdir(parents=True, exist_ok=True)
    sheet.save(PNG_OUT)
    PREVIEW_OUT.parent.mkdir(parents=True, exist_ok=True)
    sheet.resize((1024, 256), Image.Resampling.NEAREST).save(PREVIEW_OUT)
    print(ASE_OUT)
    print(PNG_OUT)
    print(PREVIEW_OUT)


if __name__ == "__main__":
    main()
