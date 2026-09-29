"""Peças de UI: painel e botão 9-slice, ícones de chama, segmentos de Calor, tecla."""
from pixel import Canvas
import pavio


def painel(border='amb5'):
    c = Canvas(24, 24)
    for y in range(24):
        for x in range(24):
            c.set(x, y, 'amb1')
    for i in range(24):
        c.set(i, 0, 'amb0')
        c.set(i, 23, 'amb0')
        c.set(0, i, 'amb0')
        c.set(23, i, 'amb0')
        c.set(i, 1, border)
        c.set(1, i, border)
        c.set(i, 22, 'amb3')
        c.set(22, i, 'amb3')
    for (x, y) in [(0, 0), (23, 0), (0, 23), (23, 23)]:
        c.set(x, y, None)
    for (x, y) in [(1, 1), (22, 1), (1, 22), (22, 22)]:
        c.set(x, y, 'amb0')
    for (x, y) in [(3, 3), (20, 3), (3, 20), (20, 20)]:
        c.set(x, y, 'f2')
    return c


def botao(hover=False):
    c = Canvas(24, 24)
    fill = 'amb4' if hover else 'amb3'
    for y in range(24):
        for x in range(24):
            c.set(x, y, fill)
    for i in range(24):
        c.set(i, 0, 'amb0')
        c.set(i, 23, 'amb0')
        c.set(0, i, 'amb0')
        c.set(23, i, 'amb0')
        c.set(i, 1, 'f3' if hover else 'amb5')
        c.set(i, 21, 'amb1')
        c.set(i, 22, 'amb1')
    if hover:
        for i in range(24):
            c.set(1, i, 'f2')
            c.set(22, i, 'f2')
    for (x, y) in [(0, 0), (23, 0), (0, 23), (23, 23)]:
        c.set(x, y, None)
    return c


def icone_chama(cheia):
    big = pavio.draw_flame(R=8, H=14, wob=1.0, face=None, embers=0)
    c = Canvas(12, 12)
    # reduz 32 -> 12 pegando a região central da chama
    for y in range(12):
        for x in range(12):
            col = big.get(4 + int(x * 2), 4 + int(y * 2.1))
            if col is None or isinstance(col, tuple):
                continue
            if not cheia:
                col = {'f0': 'amb0', 'f1': 'amb3', 'f2': 'amb3', 'f3': 'amb4', 'f4': 'amb4', 'f5': 'amb5'}.get(col, col)
            c.set(x, y, col)
    return c


def segmento(estado):
    c = Canvas(6, 10)
    fills = {'off': ('amb2', 'amb3'), 'on': ('f2', 'f3'), 'max': ('b2', 'b4')}
    base, hi = fills[estado]
    for y in range(10):
        for x in range(6):
            edge = x in (0, 5) or y in (0, 9)
            c.set(x, y, 'amb0' if edge else (hi if y == 1 else base))
    return c


def tecla():
    c = Canvas(16, 16)
    for y in range(1, 15):
        for x in range(1, 15):
            c.set(x, y, 'amb4')
    for i in range(1, 15):
        c.set(i, 0, 'amb0')
        c.set(i, 15, 'amb0')
        c.set(0, i, 'amb0')
        c.set(15, i, 'amb0')
        c.set(i, 1, 'amb5')
        c.set(i, 13, 'amb2')
        c.set(i, 14, 'amb2')
    return c


def build():
    return {
        'painel': painel(), 'painel_chama': painel('f2'),
        'botao': botao(), 'botao_hover': botao(True),
        'chama_cheia': icone_chama(True), 'chama_vazia': icone_chama(False),
        'calor_off': segmento('off'), 'calor_on': segmento('on'), 'calor_max': segmento('max'),
        'tecla': tecla(),
    }
