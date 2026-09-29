"""Tiles 16x16 da masmorra: piso, parede (topo e face), decoração."""
from pixel import Canvas, noise, FLAME


def piso(variant):
    c = Canvas(16, 16)
    for y in range(16):
        for x in range(16):
            n = noise(x, y, variant)
            col = 'amb2'
            if n > 0.93:
                col = 'amb3'
            elif n < 0.05:
                col = 'amb1'
            c.set(x, y, col)
    # juntas das lajes: cada variação tem só algumas, para o piso não virar grade
    bottom = variant in (0, 1)
    right = variant in (0, 2)
    for i in range(16):
        if bottom and noise(i, 15, variant) > 0.15:
            c.set(i, 15, 'amb1')
        if right and noise(15, i, variant) > 0.15:
            c.set(15, i, 'amb1')
    if bottom:
        for i in range(15):
            if noise(i, 0, variant + 9) > 0.5:
                c.set(i, 0, 'amb3')
    if variant == 1:
        for (x, y) in [(3, 5), (4, 6), (5, 6), (6, 7), (6, 8), (7, 9)]:
            c.set(x, y, 'amb1')
    if variant == 2:
        for (x, y) in [(10, 3), (11, 3), (12, 4), (4, 11), (5, 11)]:
            c.set(x, y, 'amb3')
    if variant == 3:
        for y in range(7, 15):
            c.set(7, y, 'amb1')
    return c


def parede_topo():
    c = Canvas(16, 16)
    for y in range(16):
        for x in range(16):
            n = noise(x, y, 21)
            c.set(x, y, 'amb5' if n > 0.9 else 'amb4')
    for x in range(16):
        c.set(x, 0, 'amb5')
    return c


def parede_face():
    c = Canvas(16, 16)
    for y in range(16):
        for x in range(16):
            c.set(x, y, 'amb4' if y < 5 else 'amb3')
    for x in range(16):
        c.set(x, 0, 'amb5')
        c.set(x, 5, 'amb1')
        c.set(x, 10, 'amb1')
        c.set(x, 15, 'amb0')
        c.set(x, 14, 'amb2')
        c.set(x, 6, 'amb4')
        c.set(x, 11, 'amb4')
    for y in range(6, 10):
        c.set(3, y, 'amb1')
        c.set(11, y, 'amb1')
    for y in range(11, 14):
        c.set(7, y, 'amb1')
        c.set(15, y, 'amb1')
    return c


def cranio():
    c = Canvas(16, 16)
    c.stamp([
        '..kkkk..',
        '.kwwwwk.',
        'kwwwwwwk',
        'kweewewk',
        'kwwwwwwk',
        '.kwkwkk.',
        '..kkkk..',
    ], 4, 6, {'k': 'amb0', 'w': 'wx1', 'e': 'amb0'})
    c.set(5, 8, 'wx2')
    return c


def vela_chao():
    c = Canvas(16, 16)
    c.stamp(['.f.', '.y.', 'kwk', 'kwk', 'kwk', 'kkk'], 6, 7, {'f': 'f3', 'y': 'f4', 'k': 'wx0', 'w': 'wx2'})
    c.stamp(['.f', 'kw', 'kk'], 10, 10, {'f': 'f4', 'k': 'wx0', 'w': 'wx1'})
    return c


def build():
    return {
        'piso0': piso(0), 'piso1': piso(1), 'piso2': piso(2), 'piso3': piso(3),
        'parede_topo': parede_topo(), 'parede_face': parede_face(),
        'cranio': cranio(), 'vela_chao': vela_chao(),
    }
