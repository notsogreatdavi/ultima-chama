"""Breus: fantasmas de sombra com olhos que brilham."""
import math
from pixel import Canvas, depth_map, outline, shadow, noise

RAMPS = {
    'comum': ['br0', 'br1', 'br2', 'br3'],
    'cacadora': ['ca0', 'ca1', 'ca2', 'ca3'],
    'brutamontes': ['bt0', 'bt1', 'bt2', 'bt3'],
}
SIZES = {'comum': 32, 'cacadora': 32, 'brutamontes': 48}

EYES = {
    'comum': (['.e.', 'eWe', 'eWe', '.e.'], ['.e.', 'eWe', 'eWe', '.e.'], 5, 2),
    'cacadora': (['ee..', '.eWe', '..ee'], ['..ee', 'eWe.', 'ee..'], 6, 2),
    'brutamontes': (['eee', 'eWe'], ['eee', 'eWe'], 7, 3),
}
MOUTHS = {
    'comum': ['k.k.k', '.k.k.'],
    'cacadora': ['kkkkkkk', 'kWkWkWk', '.kkkkk.'],
    'brutamontes': ['kkkkkkkkk', 'kWkWkWkWk', 'kkkkkkkkk'],
}


def ghost_mask(size, cx, cy, rx, ry, bottom, phase, sx=1.0, horns=False):
    mask = set()
    for y in range(size):
        for x in range(size):
            px, py = x + 0.5, y + 0.5
            dx = (px - cx) / sx
            if py <= cy:
                if (dx / rx) ** 2 + ((py - cy) / ry) ** 2 <= 1:
                    mask.add((x, y))
            else:
                wave = 1.6 * math.sin(x * 0.85 + phase)
                taper = 1 - 0.1 * (py - cy) / max(1, bottom - cy)
                if py <= bottom + wave and abs(dx) <= rx * taper:
                    mask.add((x, y))
            if horns:
                for side in (-1, 1):
                    hx0 = cx + side * rx * 0.55 * sx
                    top = cy - ry
                    t = (top + 2 - py) / 6
                    if 0 <= t <= 1:
                        hcx = hx0 + side * t * 3
                        if abs(px - hcx) <= 2.2 * (1 - t) + 0.3:
                            mask.add((x, y))
    return mask


def draw(variant, phase=0.0, dy=0, sx=1.0, sy=1.0, flash=False, look=0):
    size = SIZES[variant]
    ramp = RAMPS[variant]
    big = size == 48
    cx = size / 2
    rx = (16 if big else 10.5) * 1.0
    ry = (13 if big else 9.5) * sy
    cy = (size * 0.42) + dy + (1 - sy) * 6
    bottom = size - (6 if big else 5) + dy
    c = Canvas(size, size)
    shadow(c, cx, size - 3, rx * sx * 0.8)
    mask = ghost_mask(size, cx, cy, rx, ry, bottom, phase, sx, horns=(variant == 'cacadora'))
    d = depth_map(mask, size, size)
    for (x, y) in mask:
        depth = d[y][x]
        lvl = 1 if depth == 1 else 2
        if depth >= 2 and (x + 0.5 - cx) < -rx * 0.15 * sx and (y + 0.5 - cy) < -ry * 0.1:
            lvl = 3
        if y + 0.5 > cy + (bottom - cy) * 0.5 and lvl > 1:
            lvl -= 1
        c.set(x, y, 'eye1' if flash else ramp[lvl])
    outline(c, mask, lambda x, y: ramp[0])

    left, right, spread, ey = EYES[variant]
    cmap = {'e': 'eye0', 'W': 'eye1', 'k': ramp[0]}
    ox = int(cx + look)
    eye_y = int(cy - (1 if not big else 3) + ey - 2)
    lw = len(left[0])
    c.stamp(left, ox - spread - lw // 2, eye_y, cmap)
    c.stamp(right, ox + spread - lw // 2 + (0 if lw % 2 else 1), eye_y, cmap)
    # brilho dos olhos: pixels soltos de 1 px
    glow = (127, 245, 232, 110)
    c.set(ox - spread - 2, eye_y - 1, glow)
    c.set(ox + spread + 2, eye_y - 1, glow)

    mouth = MOUTHS[variant]
    mw = len(mouth[0])
    c.stamp(mouth, ox - mw // 2, eye_y + len(left) + (2 if big else 1), cmap)

    if variant == 'brutamontes':
        # sobrancelha pesada e rachadura na cabeça
        for i in range(6):
            c.set(ox - spread - 3 + i, eye_y - 2 + (i // 3), ramp[0])
            c.set(ox + spread + 3 - i, eye_y - 2 + (i // 3), ramp[0])
        crack = [(0, 0), (1, 1), (1, 2), (2, 3), (1, 4)]
        for (ccx, ccy) in crack:
            c.set(int(cx + 7 + ccx), int(cy - ry + 4 + ccy), ramp[0])
    return c


def float_anim(variant):
    return [draw(variant, phase=i * math.pi / 3, dy=round(math.sin(i * math.pi / 3) * 1.3)) for i in range(6)]


def hurt(variant):
    return [draw(variant, flash=True), draw(variant, sx=1.15, sy=0.85, phase=1)]


def lunge(variant):
    ps = [(0.85, 1.1, -1), (1.35, 0.8, 2), (1.45, 0.75, 3), (1.1, 0.95, 1)]
    return [draw(variant, sx=s, sy=y, look=l, phase=i) for i, (s, y, l) in enumerate(ps)]


def walk(variant):
    ps = [(1.0, 1.0, 0), (1.08, 0.92, 1), (1.0, 1.0, 0), (0.95, 1.05, -1), (1.0, 1.0, 0), (1.08, 0.92, 1)]
    return [draw(variant, sx=s, sy=y, dy=d, phase=i * 1.05) for i, (s, y, d) in enumerate(ps)]


def dissolve(variant, frames=8):
    """Some de cima para baixo em blocos, soltando poucas partículas que sobem."""
    base = draw(variant)
    size = base.w
    ramp = RAMPS[variant]
    out = []
    for k in range(frames):
        t = (k + 1) / frames
        c = Canvas(size, size)
        for y in range(size):
            for x in range(size):
                col = base.px[y][x]
                if col is None or isinstance(col, tuple):
                    continue
                # erosão em blocos 2x2: mais pixel art, menos chuvisco
                n = noise(x // 2, y // 2, 7) * 0.7 + (1 - y / size) * 0.3
                keep_eye = col in ('eye0', 'eye1') and k < frames - 2
                if n >= t or keep_eye:
                    c.set(x, y, col)
                elif n >= t - 1.0 / frames and noise(x, y, 3) < 0.18:
                    c.set(x, y - 3, ramp[3] if noise(x, y, 5) < 0.8 else 'eye0')
                elif n >= t - 2.0 / frames and noise(x, y, 4) < 0.08:
                    c.set(x, y - 7, ramp[2])
        out.append(c)
    return out


def build():
    anims = {}
    for v in ('comum', 'cacadora', 'brutamontes'):
        anims[v + '_float'] = walk(v) if v == 'brutamontes' else float_anim(v)
        anims[v + '_hurt'] = hurt(v)
        anims[v + '_dissolve'] = dissolve(v, 10 if v == 'brutamontes' else 8)
    anims['cacadora_lunge'] = lunge('cacadora')
    return anims
