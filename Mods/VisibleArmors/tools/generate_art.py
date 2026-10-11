#!/usr/bin/env python3
"""
Visible Armors art generator.

Writes the editable armor art used by the mod (pure Python 3, no packages needed):

  GameResources/visible_armors/materials/<id>.png  8x1 palettes: outline, shadow, dark, mid, light,
                                                     highlight, accent, dark accent
  GameResources/visible_armors/helmets/<style>.png  5x5 helmet templates (9-slice, see README)
  GameResources/visible_armors/armor/<style>.png    5x4 torso templates (9-slice, see README)
  icon.png                                          the mod icon (128x128)
  docs/preview.png                                  every material and style on a stand-in body

Usage:  python tools/generate_art.py            (writes everything, overwriting the generated PNGs)
        python tools/generate_art.py --preview  (only rebuilds docs/preview.png from the PNGs on disk)

The PNGs are the source of truth for the game: once generated you can edit them in any pixel editor
(Aseprite, GIMP, Paint.NET...) and re-run with --preview to see the result. Re-running without
--preview overwrites your edits with the definitions below, so either edit this file or the PNGs.
appearances.json is hand written and never touched by this script.
"""
import os
import struct
import sys
import zlib

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
ART = os.path.join(ROOT, "GameResources", "visible_armors")

# --------------------------------------------------------------------------------------------- palettes
# 0 outline, 1 shadow, 2 dark, 3 mid, 4 light, 5 highlight, 6 accent, 7 dark accent.
PALETTES = {
    # Hues follow the game's own item icons (leather reddish brown, copper peach, bronze olive and gold,
    # silver mint, iron dark grey, steel lavender grey, mythril navy to cyan, adamantine charcoal with a lava glow).
    "leather":    ["#1e1414", "#331f1d", "#4a2a26", "#653833", "#844c3c", "#a86a4a", "#d1a25a", "#2a1712"],
    "copper":     ["#4e2c24", "#704631", "#93633b", "#b98655", "#dba66f", "#f8c697", "#4fa58c", "#2b5f50"],
    "bronze":     ["#262c14", "#3e4823", "#566530", "#72823c", "#a98a2c", "#dcb84c", "#8a3a20", "#4e1e10"],
    "silver":     ["#1f3d37", "#2f5a51", "#437e71", "#5fa293", "#86c4b5", "#c0ebe1", "#f2fffb", "#2d5f73"],
    "iron":       ["#1a1a1c", "#2a2a2e", "#3c3c41", "#505056", "#68686f", "#8a8a92", "#6a4228", "#33201a"],
    "steel":      ["#33323f", "#4b4a5c", "#656479", "#82819a", "#aeadc0", "#e4e4ec", "#d1a94a", "#7a5c2a"],
    "mythril":    ["#11203a", "#193052", "#244678", "#2f62a6", "#2b93cc", "#3fd6f6", "#d2f8ff", "#1d5a8a"],
    "adamantine": ["#121111", "#1d1b1b", "#292727", "#3a3737", "#4f4c4c", "#6c6868", "#ff7a1a", "#b8360a"],
    "gold":       ["#5a3d1c", "#7a5520", "#a8740f", "#cf990c", "#f2c40c", "#fff27a", "#c0262f", "#6e0f16"],
    "bone":       ["#3b3529", "#655c49", "#8f8468", "#b8ad8c", "#dcd3b4", "#fbf6e6", "#9b2b2b", "#561414"],
    "ice":        ["#12304a", "#21577e", "#3586b4", "#5ab3dc", "#9adcf3", "#e8fbff", "#ffffff", "#4a8fc0"],
    "scale":      ["#0b2616", "#164227", "#226038", "#33814d", "#52a868", "#94d69a", "#e3b62c", "#7a5a10"],
    "wood":       ["#2a1a0c", "#4a3016", "#6b4722", "#8c6232", "#ad8048", "#cfa56a", "#3f7a2c", "#24481a"],
    "stone":      ["#25272a", "#43474c", "#62676d", "#80868d", "#a1a7ad", "#c7ccd1", "#6b8a9a", "#3a4d57"],
}

# --------------------------------------------------------------------------------------------- templates
# Tokens: "." nothing, "K" keep the unit's pixel, "V" visor (darkest shade, also over the face),
# "0".."5" material shade, "A" accent, "a" dark accent, "M" kingdom colour, "m" dark kingdom colour.
# A trailing "*" makes the pixel semi-transparent in the PNG: it only recolours pixels the unit already has.
#
# Helmets, 5x5. Columns: front margin | head front | head middle (stretches) | head back | back margin.
#               Rows:    2 above head | 1 above head | head top | head middle (stretches) | head bottom.
# Units face left, so the front (face) is on the left.
HELMETS = {
    "cap": [
        ". . . . .",
        ". . . . .",
        "2 4 4 3 .",
        ". 3 3 2 .",
        ". K K 2 .",
    ],
    "helm": [
        ". . . . .",
        ". 3 4 3 .",
        ". 4 5 3 .",
        ". 3 4 2 .",
        ". K K 2 .",
    ],
    "nasal": [
        ". . . . .",
        ". 3 4 3 .",
        ". 4 5 3 2",
        ". 3 4 2 2",
        ". K K 2 1",
    ],
    "great_helm": [
        ". . . . .",
        ". 4 5 4 .",
        ". 5 5 4 .",
        ". 4 4 3 .",
        ". V V 2 .",
    ],
    "plumed": [
        ". . . M .",
        ". 3 4 3 M",
        ". 4 5 3 m",
        ". 3 4 2 .",
        ". K K 2 .",
    ],
    "winged": [
        ". . . . 5",
        ". 3 4 3 4",
        ". 4 5 3 3",
        ". 3 4 2 .",
        ". K K 2 .",
    ],
    "crested": [
        ". . . A .",
        ". 3 4 3 A",
        ". 4 5 3 .",
        ". 3 4 2 .",
        ". K K 2 .",
    ],
    "horned": [
        "A . . . A",
        "4 3 4 3 4",
        ". 4 5 3 .",
        ". 3 4 2 .",
        ". K K 2 .",
    ],
    "headband": [
        ". . . . .",
        ". . . . .",
        ". 4 5 4 3",
        ". . . . .",
        ". . . . 2",
    ],
}

# Torso pieces, 5x4. Columns: front arm | torso front | torso middle (stretches) | torso back | back arm.
#                    Rows:    shoulders (row above the torso) | torso top | torso middle (stretches) | torso bottom.
# The arm columns should use "*" pixels so only existing arm pixels become sleeves.
TORSOS = {
    "vest": [
        ". . . . .",
        ". 4 4 3 .",
        ". 3 3 2 .",
        ". a A a .",
    ],
    "mail": [
        ". . . . .",
        "3* 4 5 3 2*",
        "2* 3 4 2 1*",
        ". 2 3 1 .",
    ],
    "scale": [
        ". . . . .",
        "3* 5 4 3 2*",
        "2* 3 4 2 1*",
        ". 4 3 2 .",
    ],
    "breastplate": [
        ". 3 . 2 .",
        "3* 5 4 3 2*",
        "2* 4 3 2 1*",
        ". a a a .",
    ],
    "plate": [
        "3 4 . 3 2",
        "3* 5 4 3 2*",
        "2* 4 3 2 1*",
        ". 3 2 1 .",
    ],
    "plate_ornate": [
        "A 4 . 3 A",
        "3* 5 A 3 2*",
        "2* 4 3 2 1*",
        ". a A a .",
    ],
    "cloak": [
        ". . . . .",
        ". 4 3 2 .",
        ". 3 3 2 .",
        ". 2 2 1 .",
    ],
}


def hex_rgb(value):
    value = value.lstrip("#")
    return int(value[0:2], 16), int(value[2:4], 16), int(value[4:6], 16)


def encode_token(token):
    clip = token.endswith("*")
    t = token.rstrip("*")
    alpha = 128 if clip else 255
    if t == ".":
        return (0, 0, 0, 0)
    if t == "K":
        return (0, 255, 0, 255)
    if t == "V":
        return (0, 255, 255, alpha)
    if t == "A":
        return (255, 0, 0, alpha)
    if t == "a":
        return (128, 0, 0, alpha)
    if t == "M":
        return (255, 0, 255, alpha)
    if t == "m":
        return (128, 0, 128, alpha)
    if t.isdigit():
        v = int(t) * 51
        return (v, v, v, alpha)
    raise ValueError("unknown token " + token)


def parse_grid(rows):
    return [[encode_token(tok) for tok in row.split()] for row in rows]


# --------------------------------------------------------------------------------------------- PNG I/O
def write_png(path, pixels):
    """pixels: list of rows (top row first) of (r, g, b, a)."""
    height = len(pixels)
    width = len(pixels[0])
    raw = bytearray()
    for row in pixels:
        raw.append(0)
        for r, g, b, a in row:
            raw += bytes((r, g, b, a))

    def chunk(tag, data):
        return struct.pack(">I", len(data)) + tag + data + struct.pack(">I", zlib.crc32(tag + data) & 0xFFFFFFFF)

    png = b"\x89PNG\r\n\x1a\n"
    png += chunk(b"IHDR", struct.pack(">IIBBBBB", width, height, 8, 6, 0, 0, 0))
    png += chunk(b"IDAT", zlib.compress(bytes(raw), 9))
    png += chunk(b"IEND", b"")
    os.makedirs(os.path.dirname(path), exist_ok=True)
    with open(path, "wb") as f:
        f.write(png)


def read_png(path):
    """Minimal reader for the 8-bit RGBA / RGB non-interlaced PNGs this tool and most editors write."""
    with open(path, "rb") as f:
        data = f.read()
    pos = 8
    width = height = 0
    color_type = 6
    idat = b""
    palette = None
    trns = None
    while pos < len(data):
        length = struct.unpack(">I", data[pos:pos + 4])[0]
        tag = data[pos + 4:pos + 8]
        body = data[pos + 8:pos + 8 + length]
        pos += 12 + length
        if tag == b"IHDR":
            width, height, depth, color_type, _, _, interlace = struct.unpack(">IIBBBBB", body)
            if depth != 8 or interlace != 0:
                raise ValueError(path + ": only 8-bit non-interlaced PNGs are supported by the preview")
        elif tag == b"PLTE":
            palette = [tuple(body[i:i + 3]) for i in range(0, len(body), 3)]
        elif tag == b"tRNS":
            trns = body
        elif tag == b"IDAT":
            idat += body
    channels = {6: 4, 2: 3, 3: 1, 0: 1, 4: 2}[color_type]
    raw = zlib.decompress(idat)
    stride = width * channels
    rows = []
    prev = bytearray(stride)
    i = 0
    for _ in range(height):
        ftype = raw[i]
        line = bytearray(raw[i + 1:i + 1 + stride])
        i += 1 + stride
        for x in range(stride):
            left = line[x - channels] if x >= channels else 0
            up = prev[x]
            ul = prev[x - channels] if x >= channels else 0
            if ftype == 1:
                line[x] = (line[x] + left) & 0xFF
            elif ftype == 2:
                line[x] = (line[x] + up) & 0xFF
            elif ftype == 3:
                line[x] = (line[x] + (left + up) // 2) & 0xFF
            elif ftype == 4:
                p = left + up - ul
                pa, pb, pc = abs(p - left), abs(p - up), abs(p - ul)
                pred = left if pa <= pb and pa <= pc else up if pb <= pc else ul
                line[x] = (line[x] + pred) & 0xFF
        prev = line
        row = []
        for x in range(width):
            px = line[x * channels:(x + 1) * channels]
            if color_type == 6:
                row.append(tuple(px))
            elif color_type == 2:
                row.append((px[0], px[1], px[2], 255))
            elif color_type == 3:
                r, g, b = palette[px[0]]
                a = trns[px[0]] if trns is not None and px[0] < len(trns) else 255
                row.append((r, g, b, a))
            elif color_type == 4:
                row.append((px[0], px[0], px[0], px[1]))
            else:
                row.append((px[0], px[0], px[0], 255))
        rows.append(row)
    return rows


# --------------------------------------------------------------------------------------------- preview
# A stand-in body drawn for this preview only (not the game's art): same proportions as WorldBox
# civilians (2x2 head over a 4x3 torso, trousers and legs), facing left. Labels: S skin, C cloth,
# D dark cloth, H hair, F face.
MANNEQUIN = [
    "..........",
    "..........",
    "..........",
    "....HH....",
    "....FH....",
    "...CSCC...",
    ".SSCSCCS..",
    "...CSCC.S.",
    "...DDDD.S.",
    "...D..D...",
    "...S..S...",
]
SKIN = (233, 186, 140, 255)
CLOTH = (200, 60, 60, 255)
CLOTH_DARK = (120, 30, 30, 255)
HAIR = (110, 70, 30, 255)
KINGDOM = [(240, 70, 70, 255), (214, 56, 56, 255), (170, 40, 40, 255), (120, 28, 28, 255), (90, 20, 20, 255)]


def preview_unit(helmet, torso, boots_height, palette, cape):
    pad = 2
    base = [list(r) for r in MANNEQUIN]
    h = len(base) + pad
    w = len(base[0]) + pad * 2
    canvas = [[(0, 0, 0, 0)] * w for _ in range(h)]
    labels = [[""] * w for _ in range(h)]
    color_of = {"S": SKIN, "C": CLOTH, "D": CLOTH_DARK, "H": HAIR, "F": SKIN}
    for y, row in enumerate(base):
        for x, ch in enumerate(row):
            if ch == ".":
                continue
            cy = y + pad  # canvas rows: top-down, padding on top
            canvas[cy][x + pad] = color_of[ch]
            labels[cy][x + pad] = ch
    pal = [hex_rgb(c) + (255,) for c in palette]

    def resolve(px):
        r, g, b, a = px
        if (r, g, b) == (0, 255, 255):
            return pal[0]
        if (r, g, b) == (255, 0, 0):
            return pal[6]
        if (r, g, b) == (128, 0, 0):
            return pal[7]
        if (r, g, b) == (255, 0, 255):
            return KINGDOM[0]
        if (r, g, b) == (128, 0, 128):
            return KINGDOM[3]
        return pal[min(5, round(((r + g + b) / 3) / 51))]

    def slice_col(off, size):
        if off < 0:
            return 0
        if off >= size:
            return 4
        if size == 1:
            return 2
        if off == 0:
            return 1
        if off == size - 1:
            return 3
        return 2

    # Landmarks of the mannequin (top-down rows in canvas space).
    head_x, head_top, head_w, head_h = 4 + pad, 3 + pad, 2, 2
    core_l, core_r, torso_top, torso_bottom = 3 + pad, 6 + pad, 5 + pad, 7 + pad
    bottom = len(base) - 1 + pad

    if torso:
        rows = torso
        for r_idx, y in enumerate(range(torso_top - 1, torso_bottom + 1)):
            trow = 0 if y == torso_top - 1 else 1 if y == torso_top else 3 if y == torso_bottom else 2
            for x in range(core_l - 1, core_r + 2):
                px = rows[trow][slice_col(x - core_l, core_r - core_l + 1)]
                if px[3] == 0 or px[:3] == (0, 255, 0):
                    continue
                if labels[y][x] in ("H", "F"):
                    continue
                if px[3] < 250 and canvas[y][x][3] == 0:
                    continue
                canvas[y][x] = resolve(px)
                labels[y][x] = labels[y][x] or "P"
        if cape:
            x = core_r + 1
            for y in range(torso_top, bottom):
                for xx in ((x,) if y < bottom - 2 else (x, x + 1)):
                    if canvas[y][xx][3] == 0:
                        canvas[y][xx] = KINGDOM[1] if y > torso_top else KINGDOM[3]
    if boots_height:
        for x in range(w):
            for y in range(bottom, torso_bottom, -1):
                if labels[y][x] in ("S", "D") and (labels[y][x] == "S" or y == bottom):
                    for k in range(boots_height):
                        yy = y - k
                        if yy <= torso_bottom or labels[yy][x] not in ("S", "D", "C"):
                            break
                        canvas[yy][x] = pal[5] if k == boots_height - 1 and boots_height > 1 else pal[3]
                    break
    if helmet:
        for dy in range(-2, head_h):
            y = head_top + dy
            trow = dy + 2 if dy < 1 else (4 if dy == head_h - 1 else 3)
            for dx in range(-1, head_w + 1):
                x = head_x + dx
                px = helmet[trow][slice_col(dx, head_w)]
                if px[3] == 0 or px[:3] == (0, 255, 0):
                    continue
                if labels[y][x] in ("S", "C", "D"):
                    continue
                if labels[y][x] == "F" and px[:3] != (0, 255, 255):
                    continue
                if px[3] < 250 and canvas[y][x][3] == 0:
                    continue
                canvas[y][x] = resolve(px)
    return canvas


def build_preview():
    pal_dir = os.path.join(ART, "materials")
    palettes = {}
    for name in sorted(os.listdir(pal_dir)):
        if name.endswith(".png"):
            row = read_png(os.path.join(pal_dir, name))[0]
            palettes[name[:-4]] = ["#%02x%02x%02x" % px[:3] for px in row[:8]]
    helmets = {n[:-4]: read_png(os.path.join(ART, "helmets", n)) for n in sorted(os.listdir(os.path.join(ART, "helmets"))) if n.endswith(".png")}
    torsos = {n[:-4]: read_png(os.path.join(ART, "armor", n)) for n in sorted(os.listdir(os.path.join(ART, "armor"))) if n.endswith(".png")}
    # One row per material: its default helmet+armor+boots set, then each helmet and torso style in that material.
    defaults = {
        "leather": ("cap", "vest", 1), "copper": ("helm", "mail", 2), "bronze": ("plumed", "scale", 2),
        "silver": ("winged", "breastplate", 2), "iron": ("nasal", "breastplate", 2), "steel": ("great_helm", "plate", 2),
        "mythril": ("crested", "plate", 2), "adamantine": ("horned", "plate_ornate", 2),
    }
    cell_w, cell_h = 14, 13
    scale = 6
    names = list(palettes)
    cols = 1 + len(helmets) + len(torsos)
    sheet = [[(40, 44, 52, 255)] * (cols * cell_w) for _ in range(len(names) * cell_h)]
    for r, mat in enumerate(names):
        pal = palettes[mat]
        units = []
        hd, td, bh = defaults.get(mat, ("helm", "breastplate", 2))
        units.append(preview_unit(helmets.get(hd), torsos.get(td), bh, pal, td == "plate_ornate"))
        for name in helmets:
            units.append(preview_unit(helmets[name], None, 0, pal, False))
        for name in torsos:
            units.append(preview_unit(None, torsos[name], 0, pal, name in ("cloak", "plate_ornate")))
        for c, unit in enumerate(units):
            for y, row in enumerate(unit):
                for x, px in enumerate(row):
                    if px[3]:
                        sheet[r * cell_h + y][c * cell_w + x + 1] = px
    big = []
    for row in sheet:
        line = []
        for px in row:
            line.extend([px] * scale)
        for _ in range(scale):
            big.append(line)
    write_png(os.path.join(ROOT, "docs", "preview.png"), big)
    print("preview: %d materials x %d columns -> docs/preview.png" % (len(names), cols))


# --------------------------------------------------------------------------------------------- icon
ICON = [
    "................................",
    "................................",
    "...........aaaa.................",
    "..........aAAAAa................",
    "...........aAAAAa...............",
    "............aAAAAa..............",
    "..........0000AAAa..............",
    "........0044554Aa...............",
    ".......04455554300..............",
    "......0345555543210.............",
    "......0344555443210.............",
    ".....034444444432210............",
    ".....033333333332110............",
    ".....000000000033210............",
    ".....0VVVVVVV00332110...........",
    ".....0VVVVVVV0033211 0..........",
    ".....033333333332211 0..........",
    ".....0343343343322110...........",
    ".....0333333333322110...........",
    ".....0222222222221100...........",
    "......0222222222110.............",
    ".......00000000000..............",
    "................................",
]


def build_icon():
    pal = [hex_rgb(c) + (255,) for c in PALETTES["steel"]]
    accent = hex_rgb(PALETTES["adamantine"][6]) + (255,)
    accent_dark = hex_rgb(PALETTES["adamantine"][7]) + (255,)
    size = 32
    img = [[(0, 0, 0, 0)] * size for _ in range(size)]
    top = (size - len(ICON)) // 2
    for y, row in enumerate(ICON):
        row = row.replace(" ", ".")
        for x, ch in enumerate(row[:size]):
            if ch == ".":
                continue
            if ch == "A":
                c = accent
            elif ch == "a":
                c = accent_dark
            elif ch == "V":
                c = (10, 12, 16, 255)
            else:
                c = pal[int(ch)]
            img[top + y][x + 6] = c
    scale = 4
    big = []
    for row in img:
        line = []
        for px in row:
            line.extend([px] * scale)
        for _ in range(scale):
            big.append(line)
    write_png(os.path.join(ROOT, "icon.png"), big)


def main():
    if "--preview" not in sys.argv:
        for name, colors in PALETTES.items():
            write_png(os.path.join(ART, "materials", name + ".png"), [[hex_rgb(c) + (255,) for c in colors]])
        for name, rows in HELMETS.items():
            write_png(os.path.join(ART, "helmets", name + ".png"), parse_grid(rows))
        for name, rows in TORSOS.items():
            write_png(os.path.join(ART, "armor", name + ".png"), parse_grid(rows))
        build_icon()
        print("wrote %d palettes, %d helmets, %d torso pieces, icon.png" % (len(PALETTES), len(HELMETS), len(TORSOS)))
    build_preview()


if __name__ == "__main__":
    main()
