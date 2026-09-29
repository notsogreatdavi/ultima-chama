"""Itens (Lumen, Vela) e efeitos (Faísca, Labareda, impacto, surgimento, fumaça)."""
import math
from pixel import Canvas, depth_map, outline, shadow, noise, FLAME


def lumen():
    frames = []
    for i in range(6):
        c = Canvas(16, 16)
        shadow(c, 8, 14, 3)
        hw = max(1.0, 4.5 * abs(math.cos(i * math.pi / 6)))
        mask = set()
        for y in range(16):
            for x in range(16):
                px, py = x + 0.5, y + 0.5
                dy = py - 7
                lim = hw * (1 - (dy / 6 if dy > 0 else -dy / 4.5))
                if -4.5 <= dy <= 6 and abs(px - 8) <= lim:
                    mask.add((x, y))
        for (x, y) in mask:
            left = x + 0.5 < 8
            c.set(x, y, 'lu2' if (left and y < 8) else 'lu1')
        outline(c, mask, lambda x, y: 'lu0')
        if i in (0, 3):
            c.set(11, 3, 'lu2')
            c.set(12, 2, (191, 251, 255, 150))
        frames.append(c)
    return frames


def vela():
    frames = []
    for i in range(4):
        c = Canvas(16, 16)
        shadow(c, 8, 14, 3)
        for y in range(8, 14):
            for x in range(6, 10):
                c.set(x, y, 'wx2' if x == 6 else 'wx1' if x < 9 else 'wx0')
        c.set(9, 9, 'wx2')
        c.set(9, 10, 'wx2')
        for x in range(5, 11):
            c.set(x, 14, 'wx0')
        for y in range(8, 14):
            c.set(5, y, 'wx0')
            c.set(10, y, 'wx0')
        for x in range(6, 10):
            c.set(x, 7, 'wx0')
        c.set(8, 6, 'face')
        h = [4, 5, 4, 3][i]
        sway = [0, 1, 0, -1][i]
        for k in range(h):
            y = 5 - k
            c.set(8 + (sway if k >= h - 2 else 0), y, 'f4' if k < h - 1 else 'f3')
        c.set(7, 5, 'f2')
        c.set(9, 5, 'f2')
        frames.append(c)
    return frames


def faisca():
    shapes = [
        ['..o..', '.oyo.', 'oywyo', '.oyo.', '..o..'],
        ['o...o', '.oyo.', '.ywy.', '.oyo.', 'o...o'],
        ['..o..', '.oyo.', 'oywyo', '.oyo.', '..o..'],
        ['.o.o.', 'oyyyo', '.ywy.', 'oyyyo', '.o.o.'],
    ]
    cmap = {'o': 'f2', 'y': 'f4', 'w': 'f5'}
    frames = []
    for s in shapes:
        c = Canvas(8, 8)
        c.stamp(s, 2, 1, cmap)
        c.set(0, 3, (255, 166, 48, 140))
        frames.append(c)
    return frames


def labareda():
    frames = []
    size = 96
    for i in range(9):
        c = Canvas(size, size)
        r = 6 + i * 5
        th = max(1.2, 7 - i * 0.7)
        for y in range(size):
            for x in range(size):
                d = math.hypot(x + 0.5 - 48, y + 0.5 - 48)
                k = (d - (r - th)) / th
                if 0 <= k <= 1:
                    if i >= 5 and noise(x // 2, y // 2, i) < (i - 4) * 0.18:
                        continue
                    lvl = 5 - int(k * 3) - (1 if i >= 4 else 0) - (1 if i >= 7 else 0)
                    c.set(x, y, FLAME[max(1, min(5, lvl))])
        for a in range(12):
            ang = a * math.pi / 6 + i * 0.15
            rr = r + 3 + (a % 3)
            c.set(48 + math.cos(ang) * rr, 48 + math.sin(ang) * rr, 'f4' if i < 5 else 'f2')
        frames.append(c)
    return frames


def impacto():
    frames = []
    for i in range(5):
        c = Canvas(16, 16)
        if i == 0:
            c.stamp(['.w.', 'www', '.w.'], 7, 7, {'w': 'f5'})
        else:
            rr = 1 + i * 1.6
            for a in range(8):
                ang = a * math.pi / 4
                for s in range(2):
                    col = ['f5', 'f4', 'f3', 'f2'][min(3, i - 1 + s)]
                    c.set(8 + math.cos(ang) * (rr + s), 8 + math.sin(ang) * (rr + s), col)
        frames.append(c)
    return frames


def surgimento():
    frames = []
    for i in range(6):
        c = Canvas(32, 32)
        rx = 3 + i * 2
        ry = max(1, rx / 3)
        for y in range(32):
            for x in range(32):
                v = ((x + 0.5 - 16) / rx) ** 2 + ((y + 0.5 - 26) / ry) ** 2
                if v <= 1:
                    c.set(x, y, 'br1' if v > 0.55 else 'br0')
                elif v <= 1.35:
                    c.set(x, y, 'br3')
        for p in range(i + 1):
            px = 16 + math.sin(p * 2.3 + i) * rx * 0.7
            py = 24 - (p * 3 + i * 2) % 16
            c.set(px, py, 'eye0' if p % 3 == 0 else 'br3')
        frames.append(c)
    return frames


def fumaca():
    frames = []
    for i in range(5):
        c = Canvas(16, 16)
        rad = 2 + i * 0.8
        cy = 10 - i
        for y in range(16):
            for x in range(16):
                d = math.hypot(x + 0.5 - 8, y + 0.5 - cy)
                if d <= rad and noise(x, y, i) > i * 0.15:
                    c.set(x, y, 'sm2' if y < cy else 'sm1')
        frames.append(c)
    return frames


def build():
    return {
        'lumen': lumen(),
        'vela': vela(),
        'faisca': faisca(),
        'labareda': labareda(),
        'impacto': impacto(),
        'surgimento': surgimento(),
        'fumaca': fumaca(),
    }
