"""Pavio, a chaminha: corpo rasterizado por parâmetros + rostos em grade de texto."""
import math
from pixel import Canvas, depth_map, outline, shadow, FLAME, SMOKE, FLAME_TO_BLUE

SIZE = 32
CX = 16
BASE = 28

# Rostos 9 px de largura. 'k' tinta, 'w' brilho, 'r' boca, 'b' suor.
# A linha 0 fica 1 px acima dos olhos (sobrancelhas).
FACES = {
    'normal': [
        '.........',
        '.kw...kw.',
        '.kk...kk.',
        '.kk...kk.',
        '.........',
        '..k...k..',
        '...kkk...',
    ],
    'piscando': [
        '.........',
        '.........',
        '.........',
        '.kk...kk.',
        '.........',
        '..k...k..',
        '...kkk...',
    ],
    'feliz': [
        '.........',
        '.........',
        '.k.....k.',
        'k.k...k.k',
        '.........',
        '..kkkkk..',
        '...krk...',
    ],
    'medo': [
        'kk.....kk',
        '.........',
        '.kk...kk.',
        '.wk...kw.',
        '.kk...kk.',
        '..k.k.k..',
        '.k.k.k.k.',
    ],
    'bravo': [
        'kk.....kk',
        '.kk...kk.',
        '.kk...kk.',
        '.kw...wk.',
        '.........',
        '..kkkkk..',
        '.........',
    ],
    'dano': [
        '.........',
        'k.k...k.k',
        '.k.....k.',
        'k.k...k.k',
        '.........',
        '...kkk...',
        '...krk...',
    ],
    'apagado': [
        '.........',
        '.........',
        '.........',
        'kk.....kk',
        '.........',
        '...kkk...',
        '..k...k..',
    ],
}
FACE_COLORS = {'k': 'face', 'w': 'f5', 'r': 'f1', 'b': 'sweat'}
EXPRESSIONS = ['normal', 'piscando', 'feliz', 'medo', 'bravo', 'dano']


def flame_mask(R, H, lean=0.0, wob=0.0, phase=0.0, sx=1.0, base=BASE, cx=CX, tongue=True):
    by = base - R
    mask = set()
    for y in range(SIZE):
        for x in range(SIZE):
            px, py = x + 0.5, y + 0.5
            if py >= by:
                ddx = (px - cx) / sx
                if ddx * ddx + (py - by) ** 2 <= R * R:
                    mask.add((x, y))
                continue
            t = (by - py) / H
            if t <= 1:
                off = lean * t * t + wob * math.sin(t * math.pi * 1.6 + phase) * t
                hw = R * (1 - t) ** 1.25
                if abs((px - cx - off) / sx) <= hw:
                    mask.add((x, y))
            if tongue:
                t2 = (by - R * 0.15 - py) / (H * 0.5)
                if 0 <= t2 <= 1:
                    off2 = -R * 0.62 * sx + lean * 0.4 * t2 - wob * 0.8 * math.sin(phase + 1) * t2
                    hw2 = R * 0.38 * (1 - t2) ** 1.3
                    if abs((px - cx - off2) / sx) <= hw2:
                        mask.add((x, y))
    return mask, by


def draw_flame(R, H, lean=0.0, wob=0.0, phase=0.0, sx=1.0, dy=0, ramp=FLAME, face='normal', embers=0, flash=False):
    base = BASE + dy
    c = Canvas(SIZE, SIZE)
    shadow(c, CX, BASE + 2, max(3, R * sx * 0.8))
    mask, by = flame_mask(R, H, lean, wob, phase, sx, base=base)
    d = depth_map(mask, SIZE, SIZE)
    top = by - H
    for (x, y) in mask:
        depth = d[y][x]
        lvl = {1: 1, 2: 2, 3: 3, 4: 4, 5: 4}.get(depth, 5)
        right = (x + 0.5 - CX) > R * 0.3 * sx
        if right and depth <= 2:
            lvl -= 1
        if y < by - H * 0.45 and depth >= 2:
            lvl += 1
        lvl = max(1, min(5, lvl))
        c.set(x, y, 'white' if flash else ramp[lvl])

    def rim(x, y):
        # contorno seletivo: mais claro no alto à esquerda, escuro embaixo à direita
        if y < by - 2 and x <= CX:
            return ramp[1]
        return ramp[0]
    outline(c, mask, rim)

    if face and R >= 5:
        rows = FACES[face]
        fx = int(round(CX + lean * 0.2)) - 4
        fy = by - 4 + dy * 0
        cmap = dict(FACE_COLORS)
        if ramp is SMOKE:
            cmap['w'] = 'sm2'
            cmap['r'] = 'sm0'
        c.stamp(rows, fx, fy, cmap)
        if face == 'medo':
            c.set(CX + 7, fy + 1, 'sweat')
            c.set(CX + 7, fy + 2, 'sweat')
            c.set(CX + 8, fy + 2, 'sweat')

    for i in range(embers):
        ex = int(CX + lean + math.sin(phase * 2 + i * 2.1) * 3)
        ey = int(top - 2 - ((phase * 3 + i * 3) % 5))
        c.set(ex, ey, ramp[4] if i % 2 == 0 else ramp[3])
    return c


def to_blue(canvas):
    b = canvas.copy()
    b.recolor(FLAME_TO_BLUE)
    return b


def idle(face='normal', ramp=FLAME):
    frames = []
    for i in range(6):
        ph = i * math.pi / 3
        frames.append(draw_flame(R=8 + (0.4 if i in (4, 5) else 0), H=14 + [0, 1, 2, 1, 0, -1][i],
                                 wob=1.4, phase=ph, face=face, ramp=ramp, embers=1))
    return frames


def run(face='normal', ramp=FLAME):
    frames = []
    hs = [14, 16, 15, 13, 14, 16, 15, 13]
    rs = [8.5, 8, 8, 8.6, 8.5, 8, 8, 8.6]
    dys = [0, -1, -2, -1, 0, -1, -2, -1]
    for i in range(8):
        f = draw_flame(R=rs[i], H=hs[i], lean=-4.5, wob=1.6, phase=i * math.pi / 4, dy=dys[i], face=face, ramp=ramp, embers=2)
        if i in (0, 4):
            f.set(5, BASE + 1, 'amb4')
            f.set(4, BASE, 'amb5')
        frames.append(f)
    return frames


def shoot(ramp=FLAME):
    ps = [(9.5, 12, 1.1), (10, 10, 1.2), (8.2, 17, 0.9), (8, 15, 1.0)]
    return [draw_flame(R=r, H=h, sx=s, wob=1.0, phase=i, face='bravo', ramp=ramp) for i, (r, h, s) in enumerate(ps)]


def dash(ramp=FLAME):
    frames = []
    sxs = [1.3, 1.8, 1.6, 1.3, 1.0]
    hs = [11, 8, 9, 12, 14]
    leans = [-6, -9, -8, -5, -2]
    trails = [4, 10, 8, 4, 1]
    for i in range(5):
        f = draw_flame(R=8, H=hs[i], sx=sxs[i], lean=leans[i], wob=0.6, phase=i, face='bravo', ramp=ramp)
        row = BASE - 6
        for k in range(trails[i]):
            x = int(CX - 8 * sxs[i] - 2 - k)
            col = ramp[4] if k < 2 else ramp[3] if k < 5 else ramp[2]
            f.set(x, row, col)
            if k % 2 == 0:
                f.set(x, row + 2, ramp[2])
        frames.append(f)
    return frames


def hurt(ramp=FLAME):
    return [
        draw_flame(R=8.5, H=13, face='dano', ramp=ramp, flash=True),
        draw_flame(R=10, H=10, sx=1.1, face='dano', ramp=ramp),
        draw_flame(R=8.5, H=15, wob=1.2, face='dano', ramp=ramp),
    ]


def death():
    frames = []
    for i in range(10):
        k = i / 9
        R = 8 - 5.5 * k
        H = max(0.5, 14 * (1 - k * 1.1))
        ramp = FLAME if i < 4 else SMOKE
        face = 'apagado' if i < 6 else None
        f = draw_flame(R=R, H=H, wob=1.0, phase=i, face=face, ramp=ramp) if R > 1.5 else Canvas(SIZE, SIZE)
        if i >= 3:
            for p in range(3):
                py = BASE - 10 - (i - 3) * 2 - p * 4
                px = CX + int(math.sin(i * 0.9 + p * 2) * 3)
                rad = 2 if p == 0 else 1
                col = 'sm2' if p == 0 else 'sm1'
                for yy in range(-rad, rad + 1):
                    for xx in range(-rad, rad + 1):
                        if xx * xx + yy * yy <= rad * rad + 1:
                            f.set(px + xx, py + yy, col)
        frames.append(f)
    return frames


def build():
    """Retorna {anim: [Canvas]} com todas as combinações usadas no jogo."""
    anims = {}
    for e in EXPRESSIONS:
        anims['idle_' + e] = idle(e)
        anims['run_' + e] = run(e)
    anims['shoot'] = shoot()
    anims['dash'] = dash()
    anims['hurt'] = hurt()
    anims['death'] = death()
    for name in list(anims):
        if name != 'death':
            anims['azul_' + name] = [to_blue(f) for f in anims[name]]
    return anims
