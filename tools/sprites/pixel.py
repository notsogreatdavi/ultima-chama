"""Base da pixel art: paleta, canvas de pixels, contorno e sombreamento por profundidade."""
import math
from PIL import Image


def hexc(h, a=255):
    h = h.lstrip('#')
    return (int(h[0:2], 16), int(h[2:4], 16), int(h[4:6], 16), a)


PALETTE = {
    # ambiente
    'amb0': '#0E0B16', 'amb1': '#1B1528', 'amb2': '#2A2140', 'amb3': '#3A2E55', 'amb4': '#4B3F6B', 'amb5': '#6A5C8F',
    # chama
    'f0': '#5A1A1A', 'f1': '#A8321E', 'f2': '#E8631C', 'f3': '#FFA630', 'f4': '#FFE08A', 'f5': '#FFF8E0',
    # chama azul (Calor máximo)
    'b0': '#1B2F6B', 'b1': '#2F5FD0', 'b2': '#4F86F0', 'b3': '#6FA8FF', 'b4': '#CFE8FF', 'b5': '#F4FAFF',
    # breu
    'br0': '#1A1030', 'br1': '#2E1B4F', 'br2': '#4C2C78', 'br3': '#6E48A8',
    'eye0': '#7FF5E8', 'eye1': '#E0FFFB',
    # caçadora
    'ca0': '#3A0E24', 'ca1': '#7A1C44', 'ca2': '#C23A6E', 'ca3': '#F07AA0',
    # brutamontes (breu mais fechado)
    'bt0': '#120A22', 'bt1': '#231540', 'bt2': '#382363', 'bt3': '#533A8A',
    # lumen e cera
    'lu0': '#1E6F80', 'lu1': '#3FC7D9', 'lu2': '#BFFBFF',
    'wx0': '#8C7A62', 'wx1': '#C9B79A', 'wx2': '#EDE3C8',
    # fumaça
    'sm0': '#4A4458', 'sm1': '#6E6A80', 'sm2': '#9A96AC',
    # rosto
    'face': '#2A1020', 'white': '#FFFFFF', 'sweat': '#7FC8FF',
}
COLORS = {k: hexc(v) for k, v in PALETTE.items()}

FLAME = ['f0', 'f1', 'f2', 'f3', 'f4', 'f5']
BLUE = ['b0', 'b1', 'b2', 'b3', 'b4', 'b5']
SMOKE = ['sm0', 'sm0', 'sm1', 'sm1', 'sm2', 'sm2']
FLAME_TO_BLUE = dict(zip(FLAME, BLUE))


class Canvas:
    """Grade de pixels com nomes de cores da paleta (ou tuplas RGBA)."""

    def __init__(self, w, h):
        self.w, self.h = w, h
        self.px = [[None] * w for _ in range(h)]

    def inside(self, x, y):
        return 0 <= x < self.w and 0 <= y < self.h

    def set(self, x, y, c):
        x, y = int(x), int(y)
        if self.inside(x, y):
            self.px[y][x] = c

    def get(self, x, y):
        return self.px[y][x] if self.inside(x, y) else None

    def stamp(self, rows, ox, oy, cmap):
        for j, row in enumerate(rows):
            for i, ch in enumerate(row):
                if ch != '.':
                    self.set(ox + i, oy + j, cmap[ch])

    def paste(self, other, ox, oy):
        for y in range(other.h):
            for x in range(other.w):
                c = other.px[y][x]
                if c is not None:
                    self.set(ox + x, oy + y, c)

    def recolor(self, mapping):
        for y in range(self.h):
            for x in range(self.w):
                c = self.px[y][x]
                if c in mapping:
                    self.px[y][x] = mapping[c]

    def copy(self):
        c = Canvas(self.w, self.h)
        c.px = [row[:] for row in self.px]
        return c

    def image(self):
        img = Image.new('RGBA', (self.w, self.h), (0, 0, 0, 0))
        data = []
        for y in range(self.h):
            for x in range(self.w):
                c = self.px[y][x]
                if c is None:
                    data.append((0, 0, 0, 0))
                elif isinstance(c, tuple):
                    data.append(c)
                else:
                    data.append(COLORS[c])
        img.putdata(data)
        return img


def depth_map(mask, w, h):
    """Distância (Chebyshev) de cada pixel da máscara até a borda."""
    INF = 999
    d = [[INF if (x, y) in mask else 0 for x in range(w)] for y in range(h)]
    frontier = []
    for (x, y) in mask:
        for nx, ny in ((x + 1, y), (x - 1, y), (x, y + 1), (x, y - 1)):
            if (nx, ny) not in mask:
                d[y][x] = 1
                frontier.append((x, y))
                break
    while frontier:
        nxt = []
        for (x, y) in frontier:
            for dx in (-1, 0, 1):
                for dy in (-1, 0, 1):
                    nx, ny = x + dx, y + dy
                    if 0 <= nx < w and 0 <= ny < h and d[ny][nx] > d[y][x] + 1:
                        d[ny][nx] = d[y][x] + 1
                        nxt.append((nx, ny))
        frontier = nxt
    return d


def outline(canvas, mask, color_fn):
    """Pinta os pixels vazios vizinhos da máscara (contorno de 1 px)."""
    for y in range(canvas.h):
        for x in range(canvas.w):
            if (x, y) in mask:
                continue
            for nx, ny in ((x + 1, y), (x - 1, y), (x, y + 1), (x, y - 1)):
                if (nx, ny) in mask:
                    canvas.set(x, y, color_fn(x, y))
                    break


def noise(x, y, seed=0):
    """Ruído determinístico 0..1 por pixel."""
    n = math.sin(x * 12.9898 + y * 78.233 + seed * 37.719) * 43758.5453
    return n - math.floor(n)


def shadow(canvas, cx, y, rx):
    """Sombra de contato achatada sob o personagem."""
    col = (14, 11, 22, 150)
    for x in range(int(cx - rx), int(cx + rx) + 1):
        canvas.set(x, y, col)
    for x in range(int(cx - rx + 2), int(cx + rx - 1)):
        canvas.set(x, y + 1, col)
