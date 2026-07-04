#!/usr/bin/env python3
"""Generate simple pixel-art placeholder textures for Knight Run."""

from pathlib import Path
from PIL import Image, ImageDraw

OUT = Path(__file__).resolve().parent.parent / "Assets" / "KnightRun" / "Resources" / "KnightRun" / "Textures"
SIZE = 64


def rgb(r, g, b):
    return (r, g, b, 255)


def checker(size, c1, c2, step=8):
    img = Image.new("RGBA", (size, size), c1)
    draw = ImageDraw.Draw(img)
    for y in range(0, size, step):
        for x in range(0, size, step):
            if ((x // step) + (y // step)) % 2 == 0:
                draw.rectangle([x, y, x + step - 1, y + step - 1], fill=c2)
    return img


def noise_tiles(size, base, alt, step=4):
    img = Image.new("RGBA", (size, size), base)
    draw = ImageDraw.Draw(img)
    for y in range(0, size, step):
        for x in range(0, size, step):
            shade = alt if (x + y) % (step * 2) == 0 else base
            draw.rectangle([x, y, x + step - 1, y + step - 1], fill=shade)
    return img


def stripes(size, c1, c2, horizontal=False, step=6):
    img = Image.new("RGBA", (size, size), c1)
    draw = ImageDraw.Draw(img)
    for i in range(0, size, step * 2):
        if horizontal:
            draw.rectangle([0, i, size - 1, i + step - 1], fill=c2)
        else:
            draw.rectangle([i, 0, i + step - 1, size - 1], fill=c2)
    return img


def knight_armor(size):
    img = checker(size, rgb(45, 78, 160), rgb(58, 95, 180), 8)
    draw = ImageDraw.Draw(img)
    draw.rectangle([20, 8, 43, 55], outline=rgb(190, 200, 220), width=2)
    draw.line([31, 8, 31, 55], fill=rgb(190, 200, 220), width=2)
    draw.rectangle([26, 22, 37, 34], fill=rgb(120, 130, 150))
    return img


def knight_helmet(size):
    img = Image.new("RGBA", (size, size), rgb(130, 135, 145))
    draw = ImageDraw.Draw(img)
    draw.ellipse([12, 10, 51, 45], fill=rgb(170, 175, 185))
    draw.rectangle([10, 28, 53, 38], fill=rgb(90, 95, 105))
    draw.rectangle([28, 18, 35, 30], fill=rgb(50, 55, 65))
    return img


def ground_forest(size):
    img = noise_tiles(size, rgb(55, 120, 45), rgb(45, 100, 38), 4)
    draw = ImageDraw.Draw(img)
    for x in range(4, size, 16):
        draw.line([x, 0, x + 3, size - 1], fill=rgb(35, 85, 30), width=1)
    return img


def ground_cave(size):
    return noise_tiles(size, rgb(95, 88, 78), rgb(75, 70, 62), 5)


def ground_mine(size):
    img = stripes(size, rgb(110, 75, 45), rgb(90, 60, 35), horizontal=True, step=5)
    draw = ImageDraw.Draw(img)
    for y in range(10, size, 14):
        draw.line([0, y, size - 1, y], fill=rgb(70, 48, 28), width=2)
    return img


def wall_forest(size):
    return stripes(size, rgb(35, 70, 30), rgb(28, 55, 24), step=8)


def wall_cave(size):
    return noise_tiles(size, rgb(55, 50, 48), rgb(40, 36, 34), 6)


def tree_trunk(size):
    return stripes(size, rgb(95, 60, 30), rgb(75, 48, 22), step=4)


def tree_leaves(size):
    img = Image.new("RGBA", (size, size), rgb(40, 100, 35))
    draw = ImageDraw.Draw(img)
    draw.ellipse([8, 8, 55, 55], fill=rgb(55, 130, 45))
    draw.ellipse([18, 18, 45, 45], fill=rgb(70, 150, 55))
    return img


def rock_obstacle(size):
    img = Image.new("RGBA", (size, size), rgb(110, 110, 115))
    draw = ImageDraw.Draw(img)
    draw.polygon([(10, 50), (20, 15), (45, 12), (58, 40), (48, 58), (15, 55)], fill=rgb(140, 140, 148))
    draw.polygon([(22, 40), (30, 22), (42, 25), (38, 45)], fill=rgb(170, 170, 178))
    return img


def log_obstacle(size):
    img = stripes(size, rgb(120, 75, 35), rgb(95, 58, 28), horizontal=True, step=5)
    draw = ImageDraw.Draw(img)
    draw.ellipse([4, 24, 14, 40], fill=rgb(80, 50, 25))
    draw.ellipse([50, 24, 60, 40], fill=rgb(65, 40, 20))
    return img


def coin(size):
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    draw.ellipse([8, 8, 55, 55], fill=rgb(230, 190, 40))
    draw.ellipse([16, 16, 47, 47], fill=rgb(255, 220, 60))
    draw.text((22, 22), "$", fill=rgb(180, 130, 20))
    return img


def stalactite(size):
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    draw.polygon([(31, 4), (42, 58), (20, 58)], fill=rgb(130, 125, 120))
    draw.line([(31, 4), (31, 58)], fill=rgb(160, 155, 150), width=2)
    return img


def mine_cart(size):
    return checker(size, rgb(130, 82, 38), rgb(105, 65, 30), 6)


def mine_rail(size):
    img = stripes(size, rgb(70, 72, 78), rgb(110, 112, 120), horizontal=True, step=3)
    draw = ImageDraw.Draw(img)
    draw.rectangle([0, 28, size - 1, 35], fill=rgb(150, 152, 160))
    return img


def save(name, image):
    path = OUT / f"{name}.png"
    image.save(path)
    print(f"  {path.name}")


def main():
    OUT.mkdir(parents=True, exist_ok=True)
    print(f"Generating textures in {OUT}...")
    textures = {
        "knight_armor": knight_armor,
        "knight_helmet": knight_helmet,
        "ground_forest": ground_forest,
        "ground_cave": ground_cave,
        "ground_mine": ground_mine,
        "wall_forest": wall_forest,
        "wall_cave": wall_cave,
        "tree_trunk": tree_trunk,
        "tree_leaves": tree_leaves,
        "rock_obstacle": rock_obstacle,
        "log_obstacle": log_obstacle,
        "coin": coin,
        "stalactite": stalactite,
        "mine_cart": mine_cart,
        "mine_rail": mine_rail,
    }
    for name, fn in textures.items():
        save(name, fn(SIZE))
    print(f"Done — {len(textures)} textures.")


if __name__ == "__main__":
    main()
