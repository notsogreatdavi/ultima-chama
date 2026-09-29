#!/usr/bin/env python3
"""Gera a pixel art de Última Chama.

Uso:
  python3 build_sprites.py --unity ../../Assets/Resources/Sprites   (um PNG por quadro)
  python3 build_sprites.py --atlas <pasta>                           (atlas + cenas de prévia)
"""
import argparse
import json
import math
from pathlib import Path

from PIL import Image

import breus
import itens
import pavio
import tiles
import ui
from pixel import Canvas, COLORS, hexc

FPS = {
    'idle': 8, 'run': 12, 'shoot': 14, 'dash': 20, 'hurt': 12, 'death': 10,
    'float': 8, 'lunge': 12, 'dissolve': 12,
    'lumen': 8, 'vela': 8, 'faisca': 16, 'labareda': 18, 'impacto': 18, 'surgimento': 10, 'fumaca': 10,
}


def fps_for(name):
    for key, v in FPS.items():
        if key in name:
            return v
    return 10


def all_groups():
    return {
        'pavio': pavio.build(),
        'breus': breus.build(),
        'itens': itens.build(),
    }


# ---------- Saída para o Unity ----------

def export_unity(out):
    out = Path(out)
    meta = []
    for group, anims in all_groups().items():
        for name, frames in anims.items():
            d = out / group / name
            d.mkdir(parents=True, exist_ok=True)
            for old in d.glob('*.png'):
                old.unlink()
            for i, f in enumerate(frames):
                f.image().save(d / f'{i:02d}.png')
            meta.append({'key': f'{group}/{name}', 'frames': len(frames), 'fps': fps_for(name)})
    for sub, pieces in (('tiles', tiles.build()), ('ui', ui.build())):
        d = out / sub
        d.mkdir(parents=True, exist_ok=True)
        for name, c in pieces.items():
            c.image().save(d / f'{name}.png')
    # formato de lista para o JsonUtility do Unity
    (out / 'meta.json').write_text(json.dumps({'anims': meta}, indent=1))
    print('unity:', len(meta), 'animações em', out)


# ---------- Atlas e cenas para o canvas ----------

def atlas(rows, cell, cols=10):
    img = Image.new('RGBA', (cell * cols, cell * len(rows)), (0, 0, 0, 0))
    meta = []
    for r, (name, frames) in enumerate(rows):
        for i, f in enumerate(frames):
            ox = i * cell + (cell - f.w) // 2
            oy = r * cell + (cell - f.h)
            img.alpha_composite(f.image(), (ox, oy))
        meta.append({'name': name, 'row': r, 'n': len(frames), 'fps': fps_for(name)})
    return img, meta


def blend(c, target, a):
    return tuple(int(c[k] + (target[k] - c[k]) * a) for k in range(3)) + (255,)


def room(w_tiles, h_tiles, pillars, seed=0):
    t = tiles.build()
    img = Image.new('RGBA', (w_tiles * 16, h_tiles * 16), COLORS['amb0'])
    solid = set()
    for x in range(w_tiles):
        solid.add((x, 0))
        solid.add((x, 1))
        solid.add((x, h_tiles - 1))
    for y in range(h_tiles):
        solid.add((0, y))
        solid.add((w_tiles - 1, y))
    for (px, py) in pillars:
        solid.update({(px, py), (px + 1, py), (px, py + 1), (px + 1, py + 1)})
    for y in range(h_tiles):
        for x in range(w_tiles):
            if (x, y) in solid:
                exposed = (x, y + 1) not in solid and y + 1 < h_tiles
                tile = t['parede_face'] if exposed else t['parede_topo']
            else:
                n = math.sin(x * 3.1 + y * 7.7 + seed) * 1000
                tile = t['piso%d' % int(abs(n) % 4)]
            img.alpha_composite(tile.image(), (x * 16, y * 16))
    return img, t


def light(img, lights, bands=(0.0, 0.38, 0.62, 0.84)):
    """Iluminação em faixas (sem gradiente suave), típica de pixel art."""
    px = img.load()
    emissive = {COLORS[k] for k in ('eye0', 'eye1', 'f2', 'f3', 'f4', 'f5', 'lu1', 'lu2', 'b2', 'b3', 'b4', 'b5')}
    dark = COLORS['amb0']
    warm = COLORS['f3']
    for y in range(img.height):
        for x in range(img.width):
            best = 3
            glow = 0
            for (lx, ly, r) in lights:
                d = math.hypot(x - lx, y - ly) / r
                band = 0 if d < 1 else 1 if d < 1.3 else 2 if d < 1.6 else 3
                best = min(best, band)
                if d < 0.55 and r > 30:
                    glow = 1
            c = px[x, y]
            if c[3] == 0 or c in emissive:
                continue
            c = blend(c, dark, bands[best])
            if glow:
                c = blend(c, warm, 0.07)
            px[x, y] = c


def place(img, canvas, x, y):
    img.alpha_composite(canvas.image(), (int(x), int(y)))


def scene_gameplay(groups):
    img, t = room(20, 12, [(4, 5), (14, 3)], seed=1)
    place(img, t['cranio'], 48, 144)
    place(img, t['vela_chao'], 272, 48)
    p, b, it = groups['pavio'], groups['breus'], groups['itens']
    place(img, it['lumen'][0], 118, 74)
    place(img, it['lumen'][2], 196, 128)
    place(img, it['vela'][1], 74, 118)
    place(img, b['comum_float'][2], 206, 62)
    place(img, b['cacadora_lunge'][2], 92, 100)
    place(img, b['brutamontes_float'][1], 232, 96)
    place(img, b['comum_float'][4], 40, 64)
    place(img, it['faisca'][1], 188, 86)
    place(img, it['impacto'][2], 200, 80)
    place(img, p['run_bravo'][2], 148, 78)
    light(img, [(164, 100, 52), (126, 82, 10), (204, 136, 10), (80, 124, 16), (280, 58, 22)])
    return img


def scene_menu():
    img, t = room(20, 12, [(15, 7)], seed=4)
    place(img, t['vela_chao'], 40, 136)
    b = breus.build()
    place(img, b['comum_float'][0], 262, 40)
    place(img, b['cacadora_float'][3], 34, 40)
    light(img, [(212, 112, 46), (48, 144, 18)])
    return img


def scene_gameover():
    img, t = room(20, 12, [(4, 5), (14, 3)], seed=1)
    fum = itens.build()['fumaca']
    place(img, fum[1], 152, 84)
    place(img, fum[3], 160, 70)
    light(img, [(160, 100, 20)], bands=(0.2, 0.55, 0.75, 0.9))
    return img


def export_atlas(out):
    out = Path(out)
    out.mkdir(parents=True, exist_ok=True)
    g = all_groups()
    p, b, it = g['pavio'], g['breus'], g['itens']
    meta = {}

    rows = ['idle_normal', 'idle_piscando', 'idle_feliz', 'idle_medo', 'idle_bravo', 'idle_dano',
            'run_normal', 'shoot', 'dash', 'hurt', 'death', 'azul_idle_feliz', 'azul_run_feliz']
    img, m = atlas([(r, p[r]) for r in rows], 32)
    img.save(out / 'atlas_pavio.png')
    meta['pavio'] = {'cell': 32, 'cols': 10, 'w': img.width, 'h': img.height, 'rows': m}

    rows = ['comum_float', 'comum_hurt', 'comum_dissolve', 'cacadora_float', 'cacadora_lunge', 'cacadora_dissolve']
    img, m = atlas([(r, b[r]) for r in rows], 32)
    img.save(out / 'atlas_breus.png')
    meta['breus'] = {'cell': 32, 'cols': 10, 'w': img.width, 'h': img.height, 'rows': m}

    rows = ['brutamontes_float', 'brutamontes_hurt', 'brutamontes_dissolve']
    img, m = atlas([(r, b[r]) for r in rows], 48)
    img.save(out / 'atlas_bruta.png')
    meta['bruta'] = {'cell': 48, 'cols': 10, 'w': img.width, 'h': img.height, 'rows': m}

    rows = ['lumen', 'vela', 'faisca', 'impacto', 'fumaca']
    img, m = atlas([(r, it[r]) for r in rows], 16)
    img.save(out / 'atlas_itens.png')
    meta['itens'] = {'cell': 16, 'cols': 10, 'w': img.width, 'h': img.height, 'rows': m}

    img, m = atlas([('labareda', it['labareda'])], 96, cols=9)
    img.save(out / 'atlas_labareda.png')
    meta['labareda'] = {'cell': 96, 'cols': 9, 'w': img.width, 'h': img.height, 'rows': m}

    img, m = atlas([('surgimento', it['surgimento'])], 32, cols=6)
    img.save(out / 'atlas_surgimento.png')
    meta['surgimento'] = {'cell': 32, 'cols': 6, 'w': img.width, 'h': img.height, 'rows': m}

    tl = tiles.build()
    sheet = Image.new('RGBA', (16 * len(tl), 16), (0, 0, 0, 0))
    for i, c in enumerate(tl.values()):
        sheet.alpha_composite(c.image(), (i * 16, 0))
    sheet.save(out / 'tiles.png')
    meta['tiles'] = list(tl)

    for name, c in ui.build().items():
        c.image().save(out / f'ui_{name}.png')

    scene_gameplay(g).save(out / 'cena_gameplay.png')
    scene_menu().save(out / 'cena_menu.png')
    scene_gameover().save(out / 'cena_gameover.png')

    (out / 'atlas_meta.json').write_text(json.dumps(meta, indent=1))
    print('atlas em', out)


if __name__ == '__main__':
    ap = argparse.ArgumentParser()
    ap.add_argument('--unity')
    ap.add_argument('--atlas')
    a = ap.parse_args()
    if a.unity:
        export_unity(a.unity)
    if a.atlas:
        export_atlas(a.atlas)
